using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class unit : MonoBehaviour {

	//==================
	//===== States =====
	//==================

	// Enums for Finite State Machine
	public enum State{interception,pursuit, evade, wander};
	// Track the current state
	public State state;

	// Always have a reference to the Game Controller
	protected gameController controller;
	
	//=================
	//===== Stats =====
	//=================

	// Declare all variables used by units, to be initialized in inherited classes
	// A number of variables in all scripts are public so that they can be observed in the inspector, not with intention to alter them there
	
	//=== Health ===
	public int health;
	
	//=== Speed Boost ===
	protected float speedBoost = 2f;
	protected float speedBoostCD = 5f;
	protected float speedBoostDuration = 5f;
	protected bool canSpeedBoost = false;

	//=== Enemies ===
	public List<GameObject> unitArray;
	public List<GameObject> pursuers;
	public GameObject target = null;
	public float targetThreat = 0;
	public float threat;
	public int targettingCount = 0;

	//=== Transformations ===
	// Speed
	protected float maxSpeed;
	protected float minSpeed;
	protected float speed;
	
	// Rotation
	protected float maxRot;
	protected float minRot;
	protected float rot;	//rot_speed is recalculated based on current speed, faster speed, slower turn, etc
	
	// Acceleration
	protected float accel;		//Rate of change of speed per second
	
	//=====================
	//===== Transform =====
	//=====================
	
	protected Vector3 Position;
	protected Vector3 Orientation;
	protected Vector3 Rotation;
	protected float seperationRange = 20f;
	
	//=== Shooting variables ===

	// Prefabs
	public GameObject laserPrefab;
	public GameObject missilePrefab;
	public GameObject torpedoPrefab;

	public Transform laserTip;
	protected float nextFire;
	public float shootingRange;
	protected float missileCD;  		// Missile cooldown
	protected bool canMissile;
	protected float torpedoCD; 			// Torpedo cooldown
	protected bool canTorpedo;

	// Global Gizmo Tickbozes
	public bool seperationGizmo;
	public bool laserRangeGizmo;
	public bool targetLineGizmo;
	
	public bool debugText;

	// Prototype State Functions
	protected abstract void interceptionState ();
	protected abstract void pursuitState ();
	protected abstract void evadeState ();
	protected abstract void wanderState ();
	/*protected void OnCollisionEnter(Collision collision)
	{
		if(collision.gameObject.tag == "Unit")
		{
			Debug.Log("Collided with Unit");
			Destroy(gameObject);
		}
		//else if(collision.gameObject.tag == "Projectile")
		//{
		//	Destroy(collision.gameObject);
		//}
	
	}*/
	protected virtual void Update()
	{
		switch(state)
		{
		case State.interception:
			//Debug.Log ("intercepting");
			interceptionState ();
			break;

		case State.pursuit:
			//Debug.Log ("pursuing");
			if (target)
			{
				pursuitState ();
			}
			else 
			{
				switchState(State.wander);
			}
			break;

		case State.evade:
			//Debug.Log ("Evading");
			evadeState ();
			break;

		case State.wander:
			//Debug.Log ("wandering");
			wanderState ();
			break;
		}
	}

	// Different for Fighters and Bombers
	protected abstract void EvaluateTargets();

	protected virtual void FixedUpdate()
	{
		Rotation = Vector3.zero;

		// Use certain steering hehaviours based on the unit's current state
		switch(state)
		{
		case State.interception:
			// Move to formation position
			break;
			
		case State.pursuit:
			if (target)
			{
				Rotation += seekTarget();
			}
			break;
			
		case State.evade:
			// Move away for each pursuer to determine the best route for escape
			foreach (GameObject pursuer in pursuers)
			{
				target = pursuer;
				Rotation += escape();
			}
			break;
			
		case State.wander:
			Rotation += wander();
			break;
		}
		
		// Always avoid nearby ships to avoid collision
		Rotation += seperation()*5f;

		// Adjust Speed and Rotation to better match current objective
		// Slow down to start making a tight corner, speed up when target is far away

		Rotation.Normalize();
		
		float step = rot/1000;	//1000 is an arbitrary value that works with whole values for rot_speed		*****
		Orientation = Vector3.RotateTowards(transform.forward, Rotation, step, 0.0f);
		
		transform.rotation = Quaternion.LookRotation(Orientation);
		
		//Always moving the fighter forward by max velocity
		if (canSpeedBoost)
		{
			rigidbody.AddForce(transform.forward*(speed + speedBoost));
			StartCoroutine( WaitForSpeedBoostCD());
		}
		else 
		{
			rigidbody.AddForce(transform.forward*speed);
		}
	}
	
	// Speed boost when available
	public IEnumerator WaitForSpeedBoostCD()
	{
		yield return new WaitForSeconds(speedBoostDuration);
		speedBoostCD = 5f;
		canSpeedBoost = false;
	}
	
	// Switch State function
	protected void switchState (State newState)
	{
		//Debug.Log ("Entering new state");
		state = newState;
	}

	protected void OnDrawGizmos()
	{
		//=== Direction ===
		Gizmos.color = Color.green;
		//Gizmos.DrawWireSphere(transform.position, cohesionRadius);
		Gizmos.DrawLine(transform.position, transform.position + transform.forward*5f);
		
		//=== Seperation Range ===
		if(controller.seperationGizmo || seperationGizmo)
		{
			Gizmos.color = Color.yellow;
			Gizmos.DrawWireSphere(transform.position, seperationRange);
		}
		
		//=== Laser Range ===
		if(controller.laserRangeGizmo || laserRangeGizmo)
		{
			Gizmos.color = Color.red;
			if(target != null)
			{
				if(Vector3.Distance(target.transform.position,transform.position) <= laser.range)
				{
					Gizmos.color = Color.green;
				}
			}
			
			Gizmos.DrawWireSphere (transform.position, laser.range);
		}
		
		//=== Target Line ===
		if(target != null && (controller.targetLineGizmo || targetLineGizmo))
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(transform.position, target.transform.position);
		}
	}
	
	// Generic Initilization function for Unit base class
	public virtual void InitUnit(Vector3 pos, Quaternion orient)
	{
		//Debug.Log("Init Fighter");
		Position = pos;
		transform.position = Position;
		
		return;
	}

	//==============================
	//===== Movement Behavious =====
	//==============================	

	// These are generic behaviours for all unit types

	protected Vector3 seekTarget()
	{
		Vector3 result = target.transform.position - transform.position;
		
		return result;
	}
	
	protected Vector3 wander()
	{
		Vector3 result;
		
		// Random values between -1 and 1, with results more likely around 0
		result.x = Utility.randomBinomial();
		result.y = Utility.randomBinomial();
		result.z = Utility.randomBinomial();
		//result.Normalize();
		
		return result;
	}
	
	protected Vector3 seperation()
	{
		// Begin with a zero vector...
		Vector3 result = Vector3.zero;
		
		// ...then get all colliders in range...
		Collider[] nearbyCollider = Physics.OverlapSphere (transform.position, seperationRange);

		// ...then attempt to move away from all colliders...
		foreach(Collider collider in nearbyCollider)												// Note *** treats units and missiles/torpedoes the same ***
		{
			result += (transform.position - collider.transform.position);// * (10f - Vector3.Distance(transform.position, collider.transform.position));
		}
		//result.Normalize();
		// ...and then return the result
		return result;
	}
	
	protected Vector3 escape()
	{
		Vector3 result;
		
		if(target == null)
		{
			result = wander();
		}
		else
		{
			result = transform.position - target.transform.position;
		}
		return result;
	}
}
