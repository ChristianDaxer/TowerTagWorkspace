using UnityEditor;
using UnityEngine;

public class PreciseSceneTransform : MonoBehaviour {


    [MenuItem("Tools/Round position of selected objects")]
    private static void RoundAllScenePositions() {
        //var obj = FindObjectsOfType(typeof(GameObject));
        var obj = Selection.GetTransforms(SelectionMode.Unfiltered);
        foreach( var o in obj ) {
            var g = (GameObject) o.gameObject;
            RoundOffPositions(g);
        }
    }

    public static void RoundOffPositions(GameObject gObject) {
        // Handle rounding slightly differently for Transform vs. RectTransform
        string logMessage = "Round position of " + gObject.name;
        if( gObject.GetComponent<RectTransform>() != null ) {
            var rt = gObject.GetComponent<RectTransform>();
            logMessage += " from " + rt.position + " / " + rt.anchoredPosition;
            rt.anchoredPosition = RoundVector2(rt.anchoredPosition);
            rt.sizeDelta = RoundVector2(rt.sizeDelta);
            logMessage += " to " + rt.position + " / " + rt.anchoredPosition;
        } else {
            // Regular Transform
            var t = gObject.GetComponent<Transform>();
            logMessage += " from " + t.position;
            t.localPosition = RoundVector3(t.localPosition);
            logMessage += " to " + t.position;
        }
        Debug.Log(logMessage);
    }

    [MenuItem("Tools/Round rotation of selected objects")]
    private static void RoundAllSceneRotations() {
        //var obj = FindObjectsOfType(typeof(GameObject));
        var obj = Selection.GetTransforms(SelectionMode.Unfiltered);
        foreach( var o in obj ) {
            var g = (GameObject) o.gameObject;
            RoundOffRotations(g);
        }
    }

    public static void RoundOffRotations(GameObject gObject) {
        // Handle rounding slightly differently for Transform vs. RectTransform
        string logMessage = "Round rotation of " + gObject.name;
        if( gObject.GetComponent<RectTransform>() != null ) {
            var rt = gObject.GetComponent<RectTransform>();
            logMessage += " from " + rt.localEulerAngles;
            rt.localEulerAngles = RoundVector3(rt.localEulerAngles);
            logMessage += " to " + rt.localEulerAngles;
        } else {
            // Regular Transform
            var t = gObject.GetComponent<Transform>();
            logMessage += " from " + t.localEulerAngles;
            t.localEulerAngles = RoundVector3(t.localEulerAngles);
            //t. rotation = RoundQuaternion(t.rotation);
            logMessage += " to " + t.localEulerAngles;
        }
        Debug.Log(logMessage);
    }

    [MenuItem("Tools/Round scale of selected objects")]
    private static void RoundAllSceneScales() {
        //var obj = FindObjectsOfType(typeof(GameObject));
        var obj = Selection.GetTransforms(SelectionMode.Unfiltered);
        foreach( var o in obj ) {
            var g = (GameObject) o.gameObject;
            RoundOffScale(g);
        }
    }

    public static void RoundOffScale(GameObject gObject) {
        // Handle rounding slightly differently for Transform vs. RectTransform
        string logMessage = "Round scale of " + gObject.name;
        if( gObject.GetComponent<RectTransform>() != null ) {
            var rt = gObject.GetComponent<RectTransform>();
            logMessage += " from " + rt.localScale;
            rt.localScale = RoundVector3(rt.localScale);
            logMessage += " to " + rt.localScale;
        } else {
            // Regular Transform
            var t = gObject.GetComponent<Transform>();
            logMessage += " from " + t.localScale;
            t.localScale = RoundVector3(t.localScale);
            logMessage += " to " + t.localScale;
        }
        Debug.Log(logMessage);
    }

    private static Vector2 RoundVector2(Vector2 v) {
        return new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    }

    private static Vector3 RoundVector3(Vector3 v) {
        return new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
    }

    private static Quaternion RoundQuaternion(Quaternion q) {
        return new Quaternion(Mathf.Round(q.x), Mathf.Round(q.y), Mathf.Round(q.z), Mathf.Round(q.w));
    }
}
