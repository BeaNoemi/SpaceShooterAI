using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class fighter : unit {
	
	//===================================
	//===== Unity Defined Functions =====
	//===================================

	void Awake()
	{
		health = 5;
		
		maxSpeed = 10f;
		minSpeed = 2f;
		speed = 6f;
		
		maxRot = 20f;
		minRot = 2f;
		rot = 11f;
		
		accel = 0.1f;

		shootingRange = 15f;
		missileCD = 20f;
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

		targettingCount = 0;
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Fighter"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("CS"));
		unitArray = unitArray.Distinct ().ToList ();
		unitArray = unitArray.Where(item => item != null).ToList();
		foreach (GameObject unit in unitArray)
		{
			if (unit.transform.parent.GetInstanceID() != transform.parent.GetInstanceID() && unit != null)
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
		if (missileCD > 0) 
		{
			missileCD -= 1 * Time.deltaTime;
		} 
		else
		{
			canMissile = true;
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

		// If a missile is ready, target is in front, in the correct angle, and is in range...
		if (canMissile && targInFront && targAngleTo < missile.angle && distToTarg <= shootingRange )
		{
			//Debug.Log ("Missile" );
			// ...and you have a target...
			if (target != null)
			{
				// Check the missilePrefab has been correctly loaded
				if (!missilePrefab)
				{
					Debug.Log ("missilePrefab not found");
				}
				
				GameObject newMissile = (GameObject) Instantiate (missilePrefab, laserTip.position, Quaternion.identity);
				newMissile.transform.parent = this.transform.parent;
				missile missileScript = newMissile.GetComponent<missile>();
				missileScript.target = target;

				missileCD = 20f;
				canMissile = false;
			}
		}
		//If the target is in front and in 60 degrees
		else if(targInFront && targAngleTo < laser.angle && distToTarg <= shootingRange)
		{
			//Debug.Log ("Fighter Laser Fired");
			if ((target != null) && (Time.time > nextFire))
			{
				nextFire = Time.time + 1f;
				
				if(!laserPrefab)
				{
					Debug.Log("laserPrefab not found");
				}
				GameObject newLaser = (GameObject) Instantiate(laserPrefab, laserTip.position, Quaternion.identity);
				newLaser.transform.parent = this.transform.parent;
				laser laserScript = newLaser.GetComponent<laser>();
				laserScript.target = target;
			}
		}

		if (target == null)
		{
			switchState (State.wander);
		}
		else if (targettingCount > 2 && !canMissile)
		{
			switchState (State.evade);
		}

		EvaluateTargets ();
		return;
	}

	protected override void evadeState ()
	{
		target = null;
		if (targettingCount == 0)
		{
			switchState(State.wander);
		}
		else if (canMissile)
		{
			switchState(State.pursuit);
		}
	}

	protected override void wanderState ()
	{
		if (target == null)
		{
			EvaluateTargets();
			
			if(target == null)
			{
				// No remaining enemies the fighter can target, reset simulation
				Application.LoadLevel(Application.loadedLevel);
			}
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
	
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Fighter"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag ("CS"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag ("Torpedo"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag ("Bomber"));
		unitArray = unitArray.Distinct ().ToList ();
		unitArray = unitArray.Where(item => item != null).ToList();
		// Check each potential target
		foreach (GameObject unit in unitArray)
		{	
			// If they are on a different team...
			if(unit.transform.parent.GetInstanceID() != transform.parent.GetInstanceID() && unit != null)
			{
				// Always calculate threat for current target
				if (unit.tag == "Fighter")
				{
					threat = EvalUnit(unit.GetComponent<fighter>());
				}
				// Else if the target is a bomber 
				else if (unit.tag == "Bomber")
				{
					threat = EvalUnit(unit.GetComponent<bomber>());
					threat += 30;
				}
				// Else if the target is a Capital ship
				else if(unit.tag == "CS")
				{
					threat = -70;
				}
				// Else if the target is a torpedo
				else if(unit.tag == "Torpedo")
				{
					threat = 30;
				}
				
				//If there is not current target
				if(target == null)
				{
					//Take the first available target and calculate threat for it
					target = unit;
					targetThreat = threat;
				}
				// If the evaluated target has a greater treat than current target, replace it
				else if( threat > targetThreat)
				{
					//If this unit has a greater threat than the current target, replace current target with it
					target = unit;
					targetThreat = threat;
				}
				// Else a higher threat has not been found
			/*	if (target.tag == "Torpedo")
				{
					Debug.Log("Torpedo targetted by " + this.name);
				}*/
				/*if (target.tag == "Bomber")
				{
					Debug.Log("Bomber targetted by " + this.name);
				}*/
			}
		}
		unitArray.Clear ();
		return;
	}

	float EvalUnit(unit target)
	{
	
		float threat = 0;
		
		//Always Acute Angle
		float targAngleTo = Vector3.Angle(transform.forward, target.transform.position-transform.position);
		
		float distance = Vector3.Distance(transform.position, target.transform.position);
		bool targInFront;
		
		if( Mathf.Sign(Vector3.Dot(transform.forward, target.transform.position-transform.position)) == 1)
		{
			targInFront = true;
		}
		else
		{
			targInFront = false;
		}
		
		// If target is in front, provide a threat bonus
		if(targInFront)
		{
			// If the target is also in laser range
			if(distance <= laser.range)
			{
				// Now check if target can immediately be fired upon
				if(targAngleTo < laser.angle)
				{
					threat += 60f;
				}
				else
				{
					threat += 30f;
				}
			}
			else
			{
				// Threat bonus for being in range
				threat += 10f;
			}
		}
		
		// Reduce threat for distant foes
		threat -= distance;
		
		// Increase threat relative to targetThreat of target
		threat += target.GetComponent<unit>().targetThreat*0.1f; 
		
		return threat;
	}
}
