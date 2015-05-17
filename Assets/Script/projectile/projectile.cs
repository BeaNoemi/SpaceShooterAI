using UnityEngine;
using System.Collections;

public abstract class projectile : MonoBehaviour {
	
	// Reference to the Game Controller
	protected GameObject controller;
	
	// Hold target
	public GameObject target;
	
	// Starting position
	protected Vector3 origin;
	
	// General Stats for projectiles
	protected int damage;
	protected float speed;
	protected float destroyTime = 5f;
	
	protected void Awake()
	{
		// Find the Game Controlle
		controller = GameObject.FindGameObjectWithTag("GameController");
		
		// Set Origin
		origin = transform.position;
	}
	
	protected void Update()
	{
		// If the target is still alive
		if (target != null) 
		{
			//Debug.Log ("Should be hitting it now");
			
			// Move towards target
			float step = speed * Time.deltaTime;
			transform.position = Vector3.MoveTowards(transform.position, target.transform.position, step);
		}
		else 
		{
			// Otherwise start coroutine
			StartCoroutine(WaitAndDestroy());
		}
	}
	
	// Destroy the projectile after a certain amount of time, called when hitting the target is no longer possible
	public IEnumerator WaitAndDestroy()
	{
		yield return new WaitForSeconds(destroyTime);
		
		Destroy(gameObject);
	}
}
