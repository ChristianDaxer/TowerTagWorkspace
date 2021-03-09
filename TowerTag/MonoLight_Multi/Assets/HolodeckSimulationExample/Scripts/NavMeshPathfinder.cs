using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshPathfinder : MonoBehaviour {

    private NavMeshAgent myAgent;
    public float minX = 1f;
    public float maxX = 9f;
    public float minY = 1f;
    public float maxY = 9f;
    public Vector3 destination;
    public float waitOnTarget = 1f;
    public float waitUntil = 0f;
    public float currtime = 0f;

    // Use this for initialization
    void Start () {
        myAgent = GetComponent<NavMeshAgent>();
        myAgent.destination = new Vector3(Random.Range(minX, maxX), 0f, Random.Range(minY, maxY));
        destination = myAgent.destination;
	}
	
	// Update is called once per frame
	void Update () {
        if((myAgent.destination - this.transform.position).magnitude < 0.1)
        {


        }
        else
        {
            waitUntil = Time.time + waitOnTarget;
        }
        currtime = Time.time;
        if (Time.time > waitUntil)
        {
            myAgent.destination = new Vector3(Random.Range(minX, maxX), 0f, Random.Range(minY, maxY));
            destination = myAgent.destination;
        }


    }
}
