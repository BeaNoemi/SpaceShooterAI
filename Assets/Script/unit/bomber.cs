using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class bomber : unit {
	
	//===================================
	//===== Unity Defined Functions =====
	//===================================
	
	void Awake()
	{
		health = 8;
		
		maxSpeed = 10f;
		minSpeed = 2f;
		speed = 2f;
		
		maxRot = 20f;
		minRot = 2f;
		rot = 11f;
		
		accel = 0.1f;
		
		shootingRange = 20f;
		torpedoCD = 15f;
		canMissile = false;
		
		state = State.wander;
		controller = GameObject.Find("Game Controller").GetComponent<gameController>();
	}
	
	void Start()
	{
		Orientation = transform.rotation.eulerAngles;

	}
	
	protected override void Update()
	{
		base.Update ();

		// Creates a list of pursuers to see if they should evade or not
		targettingCount = 0;
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Fighter"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("CS"));
		unitArray = unitArray.Distinct ().ToList ();
		unitArray = unitArray.Where(item => item != null).ToList();
		foreach (GameObject unit in unitArray)
		{
			if (unit.transform.parent.GetInstanceID() != this.transform.parent.GetInstanceID() && unit != null)
			{
				switch (unit.tag) 
				{
				case "Fighter":
					fighter fighterScript = unit.GetComponent<fighter>();
					if (fighterScript.target == this.gameObject)
					{
						
						if (fighterScript.health > health)
						{
							pursuers.Add(unit);
							targettingCount += 1;
						}
					}
					break;
				case "CS":
					capitalShip CSScript = unit.GetComponent<capitalShip>();
					if (CSScript.target == this.gameObject )
					{
						if (CSScript.health > health)
						{
							pursuers.Add(unit);
							targettingCount += 1;
						}
					}
					break;
				}
			}

			pursuers = pursuers.Distinct ().ToList ();
			pursuers = pursuers.Where(item => item != null).ToList();
		}
		unitArray.Clear();
		
		//=== Cooldowns ===
		if (torpedoCD > 0) 
		{
			torpedoCD -= 1 * Time.deltaTime;
		} 
		else
		{
			canTorpedo = true;
		}
		
		if (speedBoostCD > 0)
		{
			speedBoostCD -= 1 * Time.deltaTime;
		}
		else 
		{
			canSpeedBoost = true;
		}
		
	}
	
	
	//==================================
	//===== FSM Function Overrides =====
	//==================================
	
	protected override void interceptionState ()
	{
		Debug.Log ("interceptionState() fighter override");
		switchState (State.pursuit);
		return;
	}
	
	protected override void pursuitState ()
	{
		// ...calculate angle to target...
		//***Always Returns Acute Angle**
		float targAngleTo = Vector3.Angle(transform.forward, target.transform.position-transform.position);
		
		// ...and distance...
		float distToTarg = Vector3.Distance (target.transform.position,transform.position);
		
		// ...then figure out if the target is in front or behind. As the previous angle is acute.
		bool targInFront;
		if( Mathf.Sign(Vector3.Dot(transform.forward, target.transform.position-transform.position)) == 1)
		{
			targInFront = true;
		}
		else
		{
			targInFront = false;
		}
		
		// If a torpedo is ready, target is in front, in the correct angle, and is in range...
		if (canTorpedo && targInFront && targAngleTo < torpedo.angle && distToTarg <= shootingRange )
		{
			// ...and you have a target...
			if (target != null)
			{
				// Check the torpedoPrefab has been correctly loaded
				if (!torpedoPrefab)
				{
					Debug.Log ("torpedoPrefab not found");
				}

				GameObject newTorpedo = (GameObject) Instantiate (torpedoPrefab, laserTip.position, Quaternion.identity);
				newTorpedo.transform.parent = this.transform.parent;
				torpedo torpedoScript = newTorpedo.GetComponent<torpedo>();
				torpedoScript.target = target;
				torpedoCD = 20f;
				canTorpedo = false;

				Debug.Log ("Torpedo Fired");
			}
		}
		
		if (target == null)
		{
			switchState (State.wander);
		}
		else if (targettingCount > 0)
		{
			switchState (State.evade);
		}
		return;
	}
	
	protected override void evadeState ()
	{
		target = null;
		if (targettingCount == 0)
		{
			switchState(State.wander);
		}
		else if (canTorpedo)
		{
			switchState(State.pursuit);
		}
	}
	
	protected override void wanderState ()
	{
		if (target == null)
		{
			EvaluateTargets();
		}
		else
		{
			switchState (State.pursuit);
		}
	}
	
	//==================================
	//===== User Defined Functions =====
	//==================================
	
	protected override void EvaluateTargets()
	{
		//Debug.Log ("Evaluate Targets");
		
		//============================
		//===== Evaluate Targets =====
		//============================
		unitArray.AddRange (GameObject.FindGameObjectsWithTag ("CS"));
		unitArray = unitArray.Distinct ().ToList ();
		unitArray = unitArray.Where(item => item != null).ToList();
		// Check each potential target
		foreach (GameObject unit in unitArray)
		{	
			// If they are on a different team...
			if((unit.transform.parent.GetInstanceID() != transform.parent.GetInstanceID()) && (unit != null))
			{
				target = unit;
			}
		}
		unitArray.Clear ();
		return;
	}
}
