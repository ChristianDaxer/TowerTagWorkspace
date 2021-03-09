using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Holodeck
{
    public class ControllerManager : MonoBehaviour
    {
        
        public List<ControllerPositioner> controllers;
        private List<ControllerPositioner> instances;

        private void Start()
        {
            instances = new List<ControllerPositioner>();
            foreach (ControllerPositioner controller in controllers)
            {
                GameObject go = Instantiate(controller.gameObject);
                go.transform.parent = Camera.main.transform;
                instances.Add(go.GetComponent<ControllerPositioner>());
            }
        }

        // Update is called once per frame
        void Update()
        {
            // call oculus functions
#if USEController
            OVRInput.Update();
            OVRInput.FixedUpdate();
#endif

            // check controllers
            foreach (ControllerPositioner controller in instances)
            {
                controller.checkControllerPresent();
            }
        }
    }
}

