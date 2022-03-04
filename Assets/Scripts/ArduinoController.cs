using AndroidBluetooth;

public static class ArduinoController
{
    private const string BTAdapterName = "HC-02";

    public static bool IsConnected { get; private set; } = false;

    private static bool isMoving = false;
    private static bool isRotating = false;

    public static void Connect()
    {
        if (IsConnected) return;
        IsConnected = BluetoothService.StartBluetoothConnection(BTAdapterName);
        if (!IsConnected) UnityEngine.Application.Quit(2);
    }

    private static void Write(string data)
    {
        if (IsConnected)
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
