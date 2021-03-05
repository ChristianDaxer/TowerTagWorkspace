using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class ArduinoControllerTestScriptMulti : MonoBehaviour
{
    [FormerlySerializedAs("port")] [SerializeField]
    private string _port;

    [FormerlySerializedAs("baudRate")] [SerializeField]
    private int _baudRate;

    [FormerlySerializedAs("pins")] [SerializeField]
    private byte[] _pins;

    [FormerlySerializedAs("curve")] [SerializeField]
    private AnimationCurve[] _curve;

    [FormerlySerializedAs("minValue")] [SerializeField]
    private byte _minValue;

    [FormerlySerializedAs("maxValue")] [SerializeField]
    private byte _maxValue = 255;

    [FormerlySerializedAs("duration")] [SerializeField]
    private float _duration = 1f;

    private bool _pulse;

    private void Start ()
    {
        // connect to Arduino
        ArduinoController.Connect(_port, _baudRate);

        foreach (byte pin in _pins)
        {
            // define pin for output
            ArduinoController.SetPinMode(pin, ArduinoController.PinMode.Output);
            // init pin with zero value
            ArduinoController.AnalogWrite(pin, 0);
        }
    }

    private void Update ()
    {
        if (_pulse)
        {
            for (var i = 0; i < _pins.Length; i++)
            {
                float value = _curve[i % _pins.Length].Evaluate((Time.realtimeSinceStartup % _duration) / _duration);
                SendValue(_pins[i], value);
            }
        }
    }

    // pin: corresponding pin number
    // value: value between [0..1]
    private void SendValue(byte pin, float value)
    {
        var discreteValue = (byte)Mathf.RoundToInt(Mathf.Lerp(_minValue, _maxValue, value));
        ArduinoController.AnalogWrite(pin, discreteValue);
    }

    private IEnumerator TriggerOneShot(float duration)
    {
        float timer = 0;
        while (timer <= duration)
        {
            for (var i = 0; i < _pins.Length; i++)
            {
                SendValue(_pins[i], _curve[i % _pins.Length].Evaluate(timer / duration));
            }
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
    }

    private void OnApplicationQuit()
    {
        foreach (byte pin in _pins)
        {
            // set pin value back to zero
            ArduinoController.AnalogWrite(pin, 0);
        }

        // cleanup connection & unmanaged resources
        ArduinoController.Disconnect();
    }

    private void OnGUI()
    {
        if (GUILayout.Button("TogglePulse"))
        {
            _pulse = !_pulse;
        }

        if (GUILayout.Button("Trigger OneShot"))
        {
            StopCoroutine(TriggerOneShot(_duration));
            StartCoroutine(TriggerOneShot(_duration));
        }
    }
}
