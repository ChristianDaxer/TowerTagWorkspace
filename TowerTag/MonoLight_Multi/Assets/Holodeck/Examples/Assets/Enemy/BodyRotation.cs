using UnityEngine;
using System.Collections;

public class BodyRotation : MonoBehaviour {
    private Vector3 prevPosition;
    public float maxRotationSpeed = 180;
    public float SpeedOnMaxRotation = 2;

	// Use this for initialization
	void Start () {
        prevPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
        //claculate direction
        Vector3 movedir = transform.position - prevPosition;
        Vector3 movedirNorm = movedir.normalized;
        float movespeed = movedir.magnitude;
        float speedPercentage = Mathf.Abs(Mathf.Clamp(movespeed, 0, 2)/SpeedOnMaxRotation);
        float angle = 0;
        if (movespeed > 0.01)
        {
            angle = Vector3.Angle(transform.forward, movedirNorm);
        }
        if(angle > 180.0f)
        {
            angle -= 360.0f;
        }
        transform.Rotate(transform.up, Mathf.Clamp(angle, -maxRotationSpeed, maxRotationSpeed) * Time.deltaTime * speedPercentage);
        //Debug.Log("Angle: " + angle);
        //transform.Rotate(0.0f,angle, 0.0f);
    }
}
