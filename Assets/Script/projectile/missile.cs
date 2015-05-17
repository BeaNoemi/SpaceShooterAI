using UnityEngine;
using System.Collections;

public class missile : projectile {
	
	// Static Variables for Child Class
	public static float angle = 30f;
	public static float range = 15f;
	
	void Start()
	{
		// Set Base Class Variables
		speed 	= 15f;
		damage 	= 5;
		
		// Look at target
		if(target != null)
		{
			transform.LookAt (target.transform);
		}
	}
	
	void OnTriggerEnter (Collider collision)
	{
		//Debug.Log ("it hit");
		if ((collision.gameObject == target) && (collision.gameObject.name != "missile(Clone)") && (collision.gameObject != null)) 
		{
			switch (collision.gameObject.tag)
			{
			case "Fighter":
				Destroy(target);
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
			};
		}
		else
		{
			// Error catching, just clean up the laser
			Destroy(gameObject);
		}
	}
}
