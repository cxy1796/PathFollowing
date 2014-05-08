using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
	
	private List<GameObject> wayPts;
	private List<GameObject> followers;
	
	public GameObject follower;
	public GameObject followPrefab;
	
	int numPts = 36;
	int numFollow = 50;
	
	
	// Use this for initialization
	void Start () {
		
		Vector3 pos = new Vector3 (0, 1.0f, 0);
		
		wayPts = new List<GameObject>();
		followers = new List<GameObject>();
		
		
		for(int i = 0; i < numPts; i++)
		{
			wayPts.Add(GameObject.Find ("WP" + i));
		}
		
		for(int i = 0; i < numFollow; i++)
		{
			pos = wayPts[Random.Range (0, numPts)].transform.position;
			follower = (GameObject)GameObject.Instantiate (followPrefab, pos, Quaternion.identity);
			followers.Add (follower);
		}	
		
	}
	
	// Update is called once per frame
	void Update () {
		
	
	}
	
	public List<GameObject> GetWaypoints()
	{
		return wayPts;	
	}
	
	public List<GameObject>GetFollowers()
	{
		return followers;	
	}
}
