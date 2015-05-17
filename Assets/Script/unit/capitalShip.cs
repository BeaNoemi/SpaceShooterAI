using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class capitalShip : unit {

	public List<GameObject> turrets;


	void Awake () {
		health = 60;
		shootingRange = 15f;
		target = null;

		controller = GameObject.Find("Game Controller").GetComponent<gameController>();
	}


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Fighter"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Bomber"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Torpedo"));
		unitArray = unitArray.Distinct ().ToList ();
		unitArray = unitArray.Where(item => item != null).ToList();
		
		// Check each potential target
		foreach (GameObject unit in unitArray)
		{
				// If they are on a different team
				if(unit.transform.parent.GetInstanceID() != transform.parent.GetInstanceID() && unit != null)
				{
					float distance = Vector3.Distance(transform.position, unit.transform.position);
					if ( distance < shootingRange ) 
					{
						target = unit; 
					}
				}
		
		}
		unitArray.Clear ();
		if (target != null)
		{
			float closestDist = 20f;
			GameObject activeTurret = null;
			foreach (GameObject turret in turrets)
			{
				float distance = Vector3.Distance(turret.transform.position, target.transform.position);
				if (distance < closestDist)
				{
					closestDist = distance;
					activeTurret = turret;
				}
			}

			// firing
			if (Time.time > nextFire && activeTurret != null)
			{
				nextFire = Time.time + 1f;
				
				if(!laserPrefab)
				{
					Debug.Log("laserPrefab not found");
				}
				GameObject newLaser = (GameObject) Instantiate(laserPrefab, activeTurret.transform.position, Quaternion.identity);
				newLaser.transform.parent = this.transform.parent;
				laser laserScript = newLaser.GetComponent<laser>();
				laserScript.target = target;
				Debug.Log ("CS attacked");
			}
		}
	}
	
	// Override the implemention in Unit.cs as the Capital Ship doesn't move
	protected override void FixedUpdate() {	}
	
	// These need to be defined as they are from abstract functions, however are unused for the Capital Ship
	protected override void interceptionState () {}
	protected override void pursuitState () {}
	protected override void evadeState () {}
	protected override void wanderState () {}
	protected override void EvaluateTargets() {}
}
