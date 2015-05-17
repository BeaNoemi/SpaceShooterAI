using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class gameController : MonoBehaviour {
	
	public int teamCount;
	public int teamFighters;
	public int teamBombers;
	public Vector3 pos;
	
	// Global Gizmo Tickbozes
	public bool seperationGizmo;
	public bool laserRangeGizmo;
	public bool targetLineGizmo;

	// Prefabs
	public GameObject fighterPrefab;
	public GameObject bomberPrefab;
	public GameObject capitalShipPrefab;
	

	
	public List<GameObject> unitArray;
	
	void Awake()
	{
		// Increase the speed of the simulation, feel free to change at will, no affect on the logic behind the simulation
		Time.timeScale = 10f;
		
		// Gameobject to store new units and teams in while initializing them
		GameObject newUnit;
		GameObject newTeam;
		
		// Create the teams...
		for(int i = 0; i < teamCount; i++)
		{
			// ...and create a variable to store each new team.
			newTeam = new GameObject();
			newTeam.name = "Team " + (i + 1);
			newTeam.tag = "Team";

			// Spawn Fighters
			for(int j = 0; j < teamFighters; j++)
			{
				// Instantiate from prefab
				newUnit = (GameObject)Instantiate(fighterPrefab);
				newUnit.name = "F" + (j + 1);
				newUnit.transform.parent = newTeam.transform;
				
				// Change colour based on which team they're in
				switch(i)
				{
				case 0:
					newUnit.renderer.material.color = Color.red;
					break;
				case 1:
					newUnit.renderer.material.color = Color.blue;
					break;
					
				default:
					// Error catching, just in case a mistake is made in the number of teams
					Debug.Log ("newUnit TeamColour Default \n Too few cases applied for switch-case statement");
					newUnit.renderer.material.color = Color.white;
					break;
				}
				
				// Spawn in a row with a slightly randomized y co-ordinate
				pos = new Vector3 (10*j, Random.Range(-2f,2f), 60*i);
				// And orientate, however this code seems incapable of actualyl changing rotation from the default for some reason
				Quaternion orient = new Quaternion(0f, 0f, 0f, 0f);
				newUnit.GetComponent<fighter>().InitUnit(pos, orient);
			}

			// Spawn Bombers
			for(int j = 0; j < teamBombers; j++)
			{
				newUnit = (GameObject)Instantiate(bomberPrefab);
				newUnit.name = "B" + (j + 1);
				newUnit.transform.parent = newTeam.transform;
				
				switch(i)
				{
				case 0:
					newUnit.renderer.material.color = Color.red;
					break;
				case 1:
					newUnit.renderer.material.color = Color.blue;
					break;
					
				default:
					Debug.Log ("newUnit TeamColour Default \n Too few cases applied for switch-case statement");
					newUnit.renderer.material.color = Color.white;
					break;
				}
				
				pos = new Vector3 (10*j, Random.Range(-2f,2f), 60*i - 10);
				//Vector3 orient = new Vector3(0f,0f,180f*i);
				Quaternion orient = Quaternion.Euler( new Vector3 (0f, 180f, 0f));
				newUnit.GetComponent<bomber>().InitUnit(pos, orient);
			}


			// Spawn Capital Ships
			newUnit = (GameObject)Instantiate(capitalShipPrefab);
			newUnit.name = "CS";
			newUnit.transform.parent = newTeam.transform;
			switch(i)
			{
			case 0:
				newUnit.renderer.material.color = Color.red;
				pos = new Vector3 ( 0, 0, -50f);
				break;
			case 1:
				newUnit.renderer.material.color = Color.blue;
			    pos = new Vector3 ( 0, 0, 100f);
				break;
			default:
				Debug.Log ("newUnit TeamColour Default \n Too few cases applied for switch-case statement");
				newUnit.renderer.material.color = Color.white;
				break;
			}
			newUnit.GetComponent<capitalShip>().InitUnit(pos, new Quaternion(0f, 0f, 0f, 0f));
		}
	}
	
	void Update()
	{
		// Manual Reset for application
		if (Input.GetKeyDown("r"))
		{
			Debug.Log("r key pressed");
			Application.LoadLevel(Application.loadedLevel);
		}
		
		// Check to see if all fighters and bombers have been destroyed
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Fighter"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag ("Bomber"));
		
		//Debug.Log ("Unit Count: " + unitArray.Count);
		
		if(unitArray.Count == 0)
		{
			//Debug.Log("No more units");
			// This ensures the simulation will reset, even if both capital ships remain
			Application.LoadLevel(Application.loadedLevel);
		}
		unitArray.Clear();
		
		// Now check to see if only bombers are left
		unitArray.AddRange (GameObject.FindGameObjectsWithTag("Fighter"));
		unitArray.AddRange (GameObject.FindGameObjectsWithTag ("CS"));
		
		if(unitArray.Count == 0)
		{
			//Debug.Log("No more units");
			// This ensures the simulation will reset, even if both capital ships remain
			Application.LoadLevel(Application.loadedLevel);
		}
		
		unitArray.Clear();
	}
}
