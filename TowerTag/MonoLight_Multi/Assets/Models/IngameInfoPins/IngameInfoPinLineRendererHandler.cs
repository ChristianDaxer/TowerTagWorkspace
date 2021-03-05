using System;
using UnityEngine;

public class IngameInfoPinLineRendererHandler : MonoBehaviour {
    private LineRenderer lineRenderer;
    //private float counter;
    private float distAB;

    public float lineWidthFactor = 1;
    public Transform origin;
    public Transform destination;
    public float progressAB;

    float cameraToLineRenderDistance;

    private Camera _mainCam;
    Camera MainCamera
    {
        get
        {
            if (_mainCam == null)
            {
                if (PlayerHeadBase.GetInstance(out var playerHeadBase))
                    return _mainCam = playerHeadBase.HeadCamera;
                return null;
            }

            return _mainCam;
        }
    }


    // Start is called before the first frame update
    void Start() {
        lineRenderer = GetComponent<LineRenderer>();
    }
    
    // Update is called once per frame
    void Update() {
        try
        {
            cameraToLineRenderDistance = Vector3.Distance(MainCamera.transform.position, this.transform.position);

            var position = origin.position;
            lineRenderer.SetPosition(0, position);
            distAB = Vector3.Distance(position, destination.position);
            float x = (distAB / 100) * progressAB;
            Vector3 pointAlongLine = x * Vector3.Normalize(destination.position - position) + position;
            lineRenderer.SetPosition(1, pointAlongLine);
            // set line width related to camera pos
            float lineWidth = cameraToLineRenderDistance / 100 * lineWidthFactor;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
        }
        catch (Exception e)
        {
            throw new Exception("IngameInfoPinLineRenderer Error: ",e);
        }
    }
}
