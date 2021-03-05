public class ArduinoController {
    public enum PinMode {
        Output,
        Input,
        InputPullUp
    };

    enum ArduinoControllerFunction {
        SetPinMode,
        AnalogWrite,
        ReadLog,
        SetPinForShot
    };

    public static byte OldShotWriteValue;
    public static byte OldWriteValue;
    public static bool MuteCommunication = false;

    public static void Connect(string serialPortName, int baudRate) {
        ArduinoSerializer.Connect(serialPortName, baudRate);
    }

    public static void Disconnect() {
        ArduinoSerializer.Disconnect();
    }

    public static void SetPinMode(byte pin, PinMode mode) {
        if (MuteCommunication)
            return;

        //Debug.Log("Set Pinmode: " + mode.ToString() + " on pin " + pin);
        byte[] data = {(byte) ArduinoControllerFunction.SetPinMode, pin, 0, (byte) mode};
        ArduinoSerializer.Write(data);
    }

    public static void AnalogWrite(byte pin, byte value) {
        if (MuteCommunication)
            return;

        if (value != OldWriteValue) {
            //Debug.Log("Write Analog Value: " + value + " on pin " + pin);
            byte[] data = {(byte) ArduinoControllerFunction.AnalogWrite, pin, 0, value};
            ArduinoSerializer.Write(data);
            OldWriteValue = value;
        }
    }

    public static void SetPinForShot(byte pin, float shotDuration, byte value) {
        if (MuteCommunication)
            return;

        if (value != OldShotWriteValue) {
            byte[] data = {
                (byte) ArduinoControllerFunction.SetPinForShot, pin, (byte) (shotDuration * 100), value
            }; //Shot duration is transmitted in 10 ms steps
            ArduinoSerializer.Write(data);
            OldShotWriteValue = value;
        }
    }
}