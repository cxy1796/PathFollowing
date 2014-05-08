using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Steer : MonoBehaviour {
	
	protected Vector3 velocity;
	protected Vector3 acceleration;
	public float r = 2.0f;
	public float maxForce = 30.0f;
	public float maxSpeed;
	private GameManager manager;
	
	
	Vector3 dv = Vector3.zero; // desired velocity
	CharacterController characterController;
	
	
	private Vector3 target = Vector3.zero;
	
	private List<GameObject> wayPts;
	
	public float gravity = 20.0f;
	
	
	protected Vector3 location;
	protected int curWP = -1;
	protected float radius = 2.0f;
	
	public float seekWt = 20.0f;
	public float avoidWt = 2.0f;
	
		
	public Vector3 Location()
	{
		return location;	
	}
	
	// Use this for initialization
	void Start () {
				
		maxSpeed = Random.Range (5, 20);
		characterController = gameObject.GetComponent <CharacterController>();
		
		GameObject main = GameObject.Find("MainGO");
		manager = main.GetComponent<GameManager>();
			
	
		acceleration = Vector3.zero;
		velocity = characterController.transform.forward;
		
		wayPts = new List<GameObject>();
		wayPts = manager.GetWaypoints();
		
		location = transform.position;
		
	}
	
	
	// Update is called once per frame
	void Update () 
	{
		//CalcSteeringForce ();
		
		ApplyBehaviors();
		//update velocity
		velocity += acceleration * Time.deltaTime;
		velocity.y = 0;	// we are staying in the x/z plane
		velocity = Vector3.ClampMagnitude (velocity, maxSpeed);
		
		//orient the transform to face where we going
		if (velocity != Vector3.zero)
			transform.forward = velocity.normalized;

		// keep us grounded
		velocity.y -= gravity * Time.deltaTime;

		// the CharacterController moves us subject to physical constraints
		characterController.Move (velocity * Time.deltaTime);
		
		location = transform.position;
		//reset acceleration for next cycle
		acceleration = Vector3.zero;
		
	}
	
//	void CalcSteeringForce()
//	{
//		Vector3 force = Vector3.zero;
//		
//		//seek target
//		force += seekWt * Follow ();
//		
//		
//		force += avoidWt * Separate (manager.GetFollowers());
//		force = Vector3.ClampMagnitude (force, maxForce);
//		
//		ApplyForce (force);
//	}

	Vector3 Follow()
	{
		Vector3 predict = characterController.velocity.normalized;
		Vector3 predictLoc = location + predict;
		
		Vector3 normal = Vector3.zero;
	//	Vector3 point = Vector3.zero;
		float distance = 100;
		//Debug.DrawRay(transform.position, predictLoc, Color.red);
		// Find first and last points
	//	int first;
	//	int last;
		
//		if(curWP >= 0)
//		{
//			first = curWP - 1;
//			
//			if(first == -1)
//			{
//				first = wayPts.Count - 1;
//				last = (first + 3);
//			}
//		}
//		else
//		{
//			first = 0;
//			last = wayPts.Count;
//		}
		
		// Loop through all points of path
		for(int i = 0; i < wayPts.Count; i++)
		{
			Vector3 a = wayPts[i % wayPts.Count].transform.position;	
			Vector3 b = wayPts[(i + 1) % wayPts.Count].transform.position;
			
			Vector3 normalPoint = GetNormalPoint(predictLoc, a, b);
			
			Vector3 dir = b - a;
			
			if(normalPoint.x < Mathf.Min(a.x, b.x) || normalPoint.x > Mathf.Max (a.x, b.x) || normalPoint.y < Mathf.Min(a.y, b.y) || normalPoint.y > Mathf.Max (a.y, b.y))
			{
				normalPoint = b;
				a = wayPts[(i + 1) % wayPts.Count].transform.position;
				b = wayPts[(i + 2) % wayPts.Count].transform.position;
				
				dir = b - a;
			}
			
			float dist  = Vector3.Distance(normalPoint, predictLoc);
		
			if(dist < distance)
			{
				distance = dist;
				normal = normalPoint;
				//curWP = wayPts[i % wayPts.Count];
				dir = (dir.normalized) * 12;
				
				target = normal + dir;
				
			}
		}
		//Debug.DrawRay(transform.position, target, Color.cyan);
		if(distance > radius)
		{
			return Seek (target);
		}
		else
		{
			return Vector3.zero;	
		}
		
	}
	
	Vector3 GetNormalPoint(Vector3 p, Vector3 a, Vector3 b)
	{
		Vector3 ap = p - a;
		Vector3 ab = b - a;
		
		ab = (ab.normalized) * Vector3.Dot (ap, ab);
		
		Vector3 normal = a + ab;
		return normal;
	}
	
	Vector3 Separate(List<GameObject> followers)
	{
		float desiredSeparation = r;
		Vector3 steer = Vector3.zero;
		int count = 0;
		
		for(int i = 0; i < followers.Count; i++)
		{
			GameObject other = followers[i];
			
			float d = Vector3.Distance (location, other.transform.position);
			
			if((d > 0) && (d < desiredSeparation))
			{
				Vector3 diff = 	location - other.transform.position;
				
				diff = (diff.normalized)/d;
				steer = steer + diff;
				count++;
			}
		}
		
		if(count > 0)
		{
			steer = steer/(float)count;	
		}
		
		if(steer.magnitude > 0)
		{
			steer = ((steer.normalized) * maxSpeed) - velocity;
		}
		
		return Vector3.ClampMagnitude(steer, maxForce);
	}
	
	
	public Vector3 Seek (Vector3 targetPos)
	{
		// Find desired velocity
		dv = targetPos - transform.position;
		dv = dv.normalized * maxSpeed;
		dv -= characterController.velocity;
		dv.y = 0;
		return dv;
	}
	
	
	void ApplyBehaviors()
	{
		Vector3 f = Follow ();
		Vector3 s = Separate (manager.GetFollowers ());
		
		f = f * 3;
		s = s * 1;
		//Debug.DrawRay(transform.position, f, Color.cyan);
		ApplyForce (f);
		ApplyForce (s);
		
	}
	
	
	void ApplyForce(Vector3 force)
	{
		acceleration = acceleration + force;	
	}
	
	
//	public Vector3 Alignment(Vector3 direction)
//	{
//		return Vector3.zero;	
//	}
//	
//	public Vector3 Cohesion (Vector3 centroid)
//	{
//		return Vector3.zero;	
//	}
	
//	public Vector3 Separation (ArrayList flocker)
//	{
//		return Vector3.zero;
//	}
	
//	public Vector3 StayInBounds (float radius, Vector3 center)
//	{
//		if (Vector3.Distance (transform.position, center) > radius)
//			return Seek (center);
//		else
//			return Vector3.zero;
//	}
	
//	
//	private void CalcSteeringForces()
//	{
//		Vector3 force = Vector3.zero;
//		
//	}

	
}
