using UnityEngine;
using System.Collections;

public class torpedo : projectile {
	
	// Static Variables for Child Class
	public static float angle = 60f;
	public static float range = 15f;
	
	// Unique stat for the Torpedo as it can be shot down
	public int health = 1;
	
	void Start()
	{
		// Set Base Class Variables
		speed 	= 5f;
		damage 	= 15;
		
		// Look at target
		if(target != null)
		{
			transform.LookAt (target.transform);
		}
	}
	
	void OnTriggerEnter (Collider collision)
	{
		//Debug.Log ("it hit");
		if ((collision.gameObject == target) && (collision.gameObject.name != "torpedo(Clone)") && (collision.gameObject != null) && (collision.gameObject.tag == "CS")) 
		{
			Debug.Log("torpedo hit");
			capitalShip CSScript = target.GetComponent<capitalShip>();
			CSScript.health -= damage;
			if (CSScript.health <= 0)
			{
				Destroy(target);
			}
			Destroy(gameObject);
		}
		else if ((collision.gameObject == null) || (collision.gameObject.tag == "CS"))
		{
			Destroy(gameObject);
		}
	}
}
