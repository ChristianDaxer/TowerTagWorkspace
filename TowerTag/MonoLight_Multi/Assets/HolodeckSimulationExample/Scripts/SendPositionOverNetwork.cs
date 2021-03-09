using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendPositionOverNetwork : MonoBehaviour {
    public int frequency = 120;
    private bool running = false;
    // Use this for initialization
    void Start()
    {
        Holodeck.DebugAPI.SimulatePositionserver(true);
        running = true;
        StartCoroutine(SendData());
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        running = false;
        Holodeck.DebugAPI.StopSimulatePositionserver();
    }

    private IEnumerator SendData()
    {
        while (running)
        {
            yield return new WaitForSecondsRealtime(1 / (float) frequency);
            Holodeck.DebugAPI.SendNetworkSimulationDataPackage();
        }
    }
}
