using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowObjectId : MonoBehaviour
{
    public TextMesh text;
    public ObjectPositioner positioner;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = positioner.id.ToString();
    }
}
