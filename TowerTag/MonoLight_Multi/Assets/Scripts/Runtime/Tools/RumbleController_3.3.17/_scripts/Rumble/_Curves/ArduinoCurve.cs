using UnityEngine;
using UnityEngine.Serialization;

public class ArduinoCurve : RumbleCurve
{
    [FormerlySerializedAs("pin")] [SerializeField]
    private byte _pin;

    [FormerlySerializedAs("minValue")] [SerializeField]
    private byte _minValue;

    [FormerlySerializedAs("maxValue")] [SerializeField]
    private byte _maxValue = 255;

    [FormerlySerializedAs("curve")] [SerializeField]
    private AnimationCurve _curve;

    public override void Init()
    {
        // define pin for output
        ArduinoController.SetPinMode(_pin, ArduinoController.PinMode.Output);
        // init pin with zero value
        ArduinoController.AnalogWrite(_pin, 0);
    }

    public override void UpdateCurve(float delta)
    {
        byte discreteValue = (byte)Mathf.RoundToInt(Mathf.Lerp(_minValue, _maxValue, _curve.Evaluate(delta)));
        //print(delta);
        ArduinoController.AnalogWrite(_pin, discreteValue);
    }

    public override void Exit()
    {
        // set pin value back to zero
        ArduinoController.AnalogWrite(_pin, 0);
        ArduinoController.OldWriteValue = 0;
    }

    private void OnDestroy()
    {
        Exit();
        _curve = null;
    }
}
