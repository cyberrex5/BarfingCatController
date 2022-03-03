using AndroidBluetooth;

public static class ArduinoController
{
    private const string BTAdapterName = "HC-02";
    private static bool _isConnected = false;

    private static bool isMoving = false;
    private static bool isRotating = false;

    public static void Connect()
    {
        if (_isConnected) return;
        for (int i = 0; i < 10; ++i)
        {
            _isConnected = BluetoothService.StartBluetoothConnection(BTAdapterName);
            if (_isConnected) break;
        }
    }

    private static void Write(string data)
    {
        if (_isConnected && !string.IsNullOrWhiteSpace(data))
        {
            BluetoothService.WritetoBluetooth(data);
        }
    }

    public static void RotateLeft()
    {
        if (isRotating) return;

        //UnityEngine.Debug.Log("Arduino car rotating left");
        Write("a");

        isRotating = true;
    }

    public static void RotateRight()
    {
        if (isRotating) return;

        //UnityEngine.Debug.Log("Arduino car rotating right");
        Write("d");

        isRotating = true;
    }

    public static void StopRotating()
    {
        if (!isRotating) return;

        //UnityEngine.Debug.Log("Arduino car stopped rotating");
        Write("f");

        isRotating = false;
    }

    public static void MoveForward()
    {
        if (isMoving) return;

        //UnityEngine.Debug.Log("Arduino car moving forward");
        Write("w");

        isMoving = true;
    }

    public static void MoveBackward()
    {
        if (isMoving) return;

        //UnityEngine.Debug.Log("Arduino car moving backward");
        Write("s");

        isMoving = true;
    }

    public static void StopMoving()
    {
        if (!isMoving) return;

        //UnityEngine.Debug.Log("Arduino car stopped moving");
        Write("f");

        isMoving = false;
    }
}
