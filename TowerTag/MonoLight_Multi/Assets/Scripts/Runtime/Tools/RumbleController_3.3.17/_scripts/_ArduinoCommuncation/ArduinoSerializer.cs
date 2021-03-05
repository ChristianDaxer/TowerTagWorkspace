using System;
using System.IO;
using System.IO.Ports;

// TODO: Add failure handling & User Feedback (connect/disconnect failed, connection lost etc.)
public static class ArduinoSerializer
{
    private static SerialPort _serialPort;
    private const int ReadTimeout = 10;

    // handle Disconnect/cleanup when ArduinoSerializer is garbage collected
    // ReSharper disable once UnusedMember.Local
    private static readonly Disposer _destructor = new Disposer();
    private sealed class Disposer{ ~Disposer(){Disconnect();}}

    public static void Connect(string serialPortName, int baudRate)
    {
        Debug.Log("ArduinoSerializer: Try to connect to " + serialPortName + " (" + baudRate + ")!");

        if (_serialPort != null)
        {
            Disconnect();
        }

        _serialPort = new SerialPort(serialPortName, baudRate) {ReadTimeout = ReadTimeout};
        _serialPort.ErrorReceived += OnErrorReceived;
        _serialPort.Disposed += OnDisposed;

        if (_serialPort != null)
        {
            try
            {
                _serialPort.Open();
                Debug.Log("ArduinoSerializer: " + (_serialPort.IsOpen ? "Connected": "Could not connect" ) + " to " + serialPortName + " (" + baudRate + ")!");

            }
            catch (Exception e)
            {

                if (e is ArgumentException ||
                    e is UnauthorizedAccessException ||
                    e is InvalidOperationException ||
                    e is IOException)
                {
                    Debug.LogWarning("ArduinoSerializer: OpenSerialPort: " + e.Message);
                }
                else
                {
                    throw;
                }
            }
        }

    }

    public static void Disconnect()
    {
        Debug.Log("ArduinoSerializer: Try to disconnect!");
        if (_serialPort != null)
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.Close();
                }

            }
            catch (IOException e)
            {
                Debug.LogWarning("ArduinoSerializer: CloseSerialPort: " + e.Message);
            }

            _serialPort.Dispose();
        }
        else
        {
            Debug.Log("ArduinoSerializer: is already disconnected!");
        }

        if (_serialPort != null)
        {
            _serialPort.ErrorReceived -= OnErrorReceived;
            _serialPort.Disposed -= OnDisposed;
            _serialPort = null;
            Debug.Log("ArduinoSerializer: removed SerialPort!");
        }
    }

    public static void Write(byte[] data)
    {
        if (_serialPort != null && _serialPort.IsOpen)
        {
            try
            {
                _serialPort.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                if (e is ArgumentException ||
                    e is InvalidOperationException ||
                    e is TimeoutException)
                {
                    Debug.LogWarning("ArduinoSerializer: WriteToSerialPort: " + e.Message);
                }
                else
                {
                    throw;
                }
            }
        }
        else
        {
            Debug.LogWarning("ArduinoSerializer: Write: SerialPort is not Connected!");
        }
    }

    private static void OnErrorReceived(object sender, SerialErrorReceivedEventArgs args)
    {
        Debug.LogError("ArduinoSerializer: Received error -> " + args.EventType);
    }

    private static void OnDisposed(object sender, EventArgs args)
    {
        Debug.Log("ArduinoSerializer: OnDisposed -> " + args);
    }
}
