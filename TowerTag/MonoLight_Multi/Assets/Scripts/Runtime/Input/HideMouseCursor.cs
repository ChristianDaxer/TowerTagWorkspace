using System;
using System.Collections;
using UnityEngine;

public class HideMouseCursor : MonoBehaviour
{

    private bool _coroutineIsRunning;
    private bool _coroutineCondition;
    private Vector3 _lastMouseCoordinate = Vector3.zero;
    private const int TimeToWait = 2;
    
    // Update is called once per frame
    void Update()
    {
        // Get Current mouse pos
        var mouseDelta = Input.mousePosition - _lastMouseCoordinate;
 
        // Check if Mouse Position is the same.
        if (Math.Abs(mouseDelta.x) < 0.01)
        {
            if (!_coroutineIsRunning)
            {
                _coroutineCondition = true;
                StartCoroutine(CountDownHideMouseCursor());
            }
        }
        else
        {
            _coroutineCondition = false;
            if (!CheckMouseCursor())
                ToggleMouseCursorVisibility(true);
        }
            
            
        // Update old MousePosition Value
        _lastMouseCoordinate = Input.mousePosition;
    }

    private IEnumerator CountDownHideMouseCursor()
    {
        _coroutineIsRunning = true;
        var time = 0f;
        while (_coroutineCondition && time < TimeToWait)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (!_coroutineCondition)
        {
            _coroutineIsRunning = false;
        }
        else
        { 
            ToggleMouseCursorVisibility(false);
            _coroutineCondition = false;
            _coroutineIsRunning = false;
        }
    }

    private static void ToggleMouseCursorVisibility(bool toggle)
    {
        Cursor.visible = toggle;
    }

    private static bool CheckMouseCursor()
    {
        return Cursor.visible;
    }
}

