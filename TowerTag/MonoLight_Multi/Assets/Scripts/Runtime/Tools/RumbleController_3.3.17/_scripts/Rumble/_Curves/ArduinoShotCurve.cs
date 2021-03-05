using UnityEngine;
using UnityEngine.Serialization;

// Hannah
public class ArduinoShotCurve : RumbleCurve
{
    [FormerlySerializedAs("pin")] [SerializeField]
    private byte _pin;

    [FormerlySerializedAs("maxValue")] [SerializeField]
    private byte _maxValue = 255;

    [FormerlySerializedAs("vibrationDuration")] [SerializeField]
    public float _vibrationDuration = 0.02f;

    public override void Init()
    {
        // define pin for output
        ArduinoController.SetPinMode(_pin, ArduinoController.PinMode.Output);

        // init pin with zero value
        ArduinoController.AnalogWrite(_pin, 0);
        ArduinoController.OldShotWriteValue = 0;
        ArduinoController.SetPinForShot(_pin, _vibrationDuration, _maxValue);
    }

    public override void UpdateCurve(float delta)
    {
    }

    public override void Exit()
    {
        // set pin value back to zero
        ArduinoController.AnalogWrite(_pin, 0);
        ArduinoController.OldShotWriteValue = 0;
    }

    private void OnDestroy()
    {
        Exit();
    }
}
