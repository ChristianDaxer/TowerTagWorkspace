using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Holodeck;

public class EnemyControllerPositioner : MonoBehaviour {

    public ControllerType type;
    public ObjectPositioner op;
    bool registerd = false;

	// Use this for initialization
	void Start () {
	}

    private void Update()
    {
        if(op.id != 0 && !registerd)
        {
            ControllerAPI.registerController(op.id, type, this.gameObject);
            registerd = true;
        }
    }

    //cleanup after Gameobjects is removed
    private void OnDestroy()
    {

        ControllerAPI.unregisterController(op.id, type, this.gameObject);
        registerd = false;
    }
}
