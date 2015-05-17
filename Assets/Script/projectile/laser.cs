using UnityEngine;
using System.Collections;

public class laser : projectile {
	
	// Only the laser has a chance to miss
	private int hitChance = 0;
	
	// Static Variables for Child Class
	public static float angle = 30f;
	public static float range = 20f;
	
	void Start()
	{
		// Set Base Class Variables
		speed 	= 30f;
		damage 	= 1;
		
		// Look at target
		if(target != null)
		{
			transform.LookAt (target.transform);
		}
		
		// Generate a random number between 0 and 100 to determine hit changes
		hitChance = Random.Range(0,100);
		
		if (hitChance < 50) 						
		{
			//Debug.Log ("Should not hit it");
			rigidbody.velocity = transform.forward * speed;
			// If it is going to miss, start coroutine here
			StartCoroutine(WaitAndDestroy());
		}

	}
	
	void Update()
	{
		// If the shot is going to hit and the target is still alive
		if (hitChance >= 50)
		{
			base.Update();
		}
	}

	void OnTriggerEnter (Collider collision)
	{
		//Debug.Log ("Laser hit");
		// Figure out what was hit and damage it
		if ((collision.gameObject == target) && (collision.gameObject.name != "laser(Clone)") && (collision.gameObject != null))
		{
			switch (collision.gameObject.tag)
			{
			case "Fighter":
				fighter fighterScript = target.GetComponent<fighter>();
				fighterScript.health -= damage;
				if (fighterScript.health <= 0) 
				{
					Destroy(target);
				}
				Destroy(gameObject);
				break;
			case "CS":
				capitalShip CSScript = target.GetComponent<capitalShip>();
				CSScript.health -= damage;
				if (CSScript.health <= 0)
				{
					Destroy(target);
				}
				Destroy(gameObject);
				break;
			case "Bomber":
				bomber bomberScript = target.GetComponent<bomber>();
				bomberScript.health -= damage;
				if (bomberScript.health <= 0)
				{
					Destroy(target);
				}
				Destroy(gameObject);
				break;
			case "Torpedo":
				Destroy(target);
				Destroy(gameObject);
				break;
			}
		}
		else
		{
			// Error catching, just clean up the laser
			Destroy(gameObject);
		}

	}
}
