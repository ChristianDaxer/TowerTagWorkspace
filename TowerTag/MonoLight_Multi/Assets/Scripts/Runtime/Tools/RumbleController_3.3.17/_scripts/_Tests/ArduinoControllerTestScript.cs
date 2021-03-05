using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class ArduinoControllerTestScript : MonoBehaviour {
    [FormerlySerializedAs("port")] [SerializeField] private string _port;

    [FormerlySerializedAs("baudRate")] [SerializeField] private int _baudRate;

    [FormerlySerializedAs("pin")] [SerializeField] private byte _pin;

    [FormerlySerializedAs("curve")] [SerializeField] private AnimationCurve _curve;

    [FormerlySerializedAs("minValue")] [SerializeField] private byte _minValue;

    [FormerlySerializedAs("maxValue")] [SerializeField] private byte _maxValue = 255;

    [FormerlySerializedAs("duration")] [SerializeField] private float _duration = 1f;

    private bool _pulse;

    private void Start() {
        // connect to Arduino
        ArduinoController.Connect(_port, _baudRate);
        // define pin for output
        ArduinoController.SetPinMode(_pin, ArduinoController.PinMode.Output);
        // init pin with zero value
        ArduinoController.AnalogWrite(_pin, 0);
    }

    private void Update() {
        if (_pulse) {
            float value = _curve.Evaluate((Time.realtimeSinceStartup % _duration) / _duration);
            SendValue(_pin, value);
        }
    }

    // pin: corresponding pin number
    // value: value between [0..1]
    private void SendValue(byte pin, float value) {
        var discreteValue = (byte) Mathf.RoundToInt(Mathf.Lerp(_minValue, _maxValue, value));
        ArduinoController.AnalogWrite(pin, discreteValue);
    }

    IEnumerator TriggerOneShot(float duration) {
        float timer = 0;
        while (timer <= duration) {
            SendValue(_pin, _curve.Evaluate(timer / duration));
            yield return new WaitForEndOfFrame();
            timer += Time.deltaTime;
        }
    }

    private void OnApplicationQuit() {
        // set pin value back to zero
        ArduinoController.AnalogWrite(_pin, 0);
        // cleanup connection & unmanaged resources
        ArduinoController.Disconnect();
    }

    private void OnGUI() {
        if (GUILayout.Button("TogglePulse")) {
            _pulse = !_pulse;
        }

        if (GUILayout.Button("Trigger OneShot")) {
            StopCoroutine(TriggerOneShot(_duration));
            StartCoroutine(TriggerOneShot(_duration));
        }
    }
}