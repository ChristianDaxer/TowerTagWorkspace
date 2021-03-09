using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDMove : MonoBehaviour
{
    public float moveSpeedFrontal = 1f;
    public float moveSpeedSide = 0.5f;

    private float height = 1.7f;

    // Start is called before the first frame update
    void Start()
    {
        height = transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        #if UNITY_EDITOR
        if (Input.GetKey(KeyCode.W))
        {
            //Move forward
            transform.Translate(Vector3.forward * moveSpeedFrontal * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            //Move backward
            transform.Translate(Vector3.back * moveSpeedFrontal * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.A))
        {
            //Move left
            transform.Translate(Vector3.left * moveSpeedSide * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            //Move right
            transform.Translate(Vector3.right * moveSpeedSide * Time.deltaTime);
        }

        transform.position = new Vector3(transform.position.x, height, transform.position.z);
        #endif
    }
}
