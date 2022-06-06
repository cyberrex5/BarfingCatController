#define LOG_ARDUINO

using AndroidBluetooth;

public enum ArduinoMessage : byte
{
    Forward = (byte)'w',
    Left = (byte)'a',
    Backward = (byte)'s',
    Right = (byte)'d',
    Stop = (byte)'f',
    ServoRight = (byte)'r',
    ServoLeft = (byte)'l',
    ServoForward = (byte)'z',
    ChangeNormalSpeed = (byte)'n',
    ChangeMinObjectDist = (byte)'m',
    ObstacleDetected = (byte)'o',
    NoObstacle = (byte)'p',
}

public class ArduinoController : UnityEngine.MonoBehaviour
{
    public const string BTObjTag = "BTObj";

    private const string BTAdapterName = "HC-02";

    public static bool IsConnected { get; private set; } = false;

    public static byte NormalSpeed { get; private set; } = 100;
    public static float MinObjectDist { get; private set; } = 1.5f; // in cm / 10 (unit used by this app in engine/scene)

    public System.Action<ArduinoMessage> OnBTMessageRecieved;

    private static bool isMoving = false;
    private static bool isRotating = false;
    private static Orientation servoOrientation = Orientation.Forward;

    private enum Orientation
    {
        Forward = 0,
        Left,
        Right,
    }

    private void Update()
    {
        string btMsg = BluetoothService.Instance.ReadFromBluetooth();
        if (btMsg.Length == 1)
        {
            ConditionalLog("Recieved bluetooth message");
            OnBTMessageRecieved?.Invoke((ArduinoMessage)btMsg[0]);
        }
    }

    private void OnApplicationQuit()
    {
        BluetoothService.Instance.StopBluetoothConnection();
    }

    public static void Connect()
    {
        if (IsConnected) return;

        IsConnected = BluetoothService.Instance.StartBluetoothConnection(BTAdapterName);

        if (!IsConnected)
        {
            UnityEngine.Application.Quit(2);
        }
        else
        {
            UnityEngine.GameObject.FindWithTag(BTObjTag).GetComponent<ArduinoController>().enabled = true;
            //DontDestroyOnLoad(UnityEngine.GameObject.FindWithTag(BTObjTag));
        }
    }

    private static void Write(ArduinoMessage msg)
    {
        if (IsConnected)
        {
            BluetoothService.Instance.WritetoBluetooth(((char)msg).ToString());
        }
    }

    private static void Write(string msg)
    {
        if (IsConnected)
        {
            BluetoothService.Instance.WritetoBluetooth(msg);
        }
    }

    public static void SetMinObjectDist(byte cm)
    {
        ConditionalLog("Changing min object dist to " + cm);
        MinObjectDist = cm * 0.1f;
        //Write($"{ArduinoMessage.ChangeMinObjectDist}{(char)cm}");
        Write(ArduinoMessage.ChangeMinObjectDist);
        Write(((char)cm).ToString());
    }

    public static void SetNormalSpeed(byte val)
    {
        ConditionalLog("Changing normal speed to " + val);
        NormalSpeed = val;
        //Write($"{ArduinoMessage.ChangeNormalSpeed}{(char)val}");
        Write(ArduinoMessage.ChangeNormalSpeed);
        Write(((char)val).ToString());
    }

    public static void RotateServoLeft()
    {
        if (servoOrientation == Orientation.Left) return;

        ConditionalLog("Rotating servo left");
        Write(ArduinoMessage.ServoLeft);

        servoOrientation = Orientation.Left;
    }

    public static void RotateServoRight()
    {
        if (servoOrientation == Orientation.Right) return;

        ConditionalLog("Rotating servo right");
        Write(ArduinoMessage.ServoRight);

        servoOrientation = Orientation.Right;
    }

    public static void RotateServoForward()
    {
        if (servoOrientation == Orientation.Forward) return;

        ConditionalLog("Rotating servo forward");
        Write(ArduinoMessage.ServoForward);

        servoOrientation = Orientation.Forward;
    }

    public static void RotateLeft()
    {
        if (isRotating) return;

        ConditionalLog("Arduino car rotating left");
        Write(ArduinoMessage.Left);

        isRotating = true;
    }

    public static void RotateRight()
    {
        if (isRotating) return;

        ConditionalLog("Arduino car rotating right");
        Write(ArduinoMessage.Right);

        isRotating = true;
    }

    public static void StopRotating()
    {
        if (!isRotating) return;

        ConditionalLog("Arduino car stopped rotating");
        Write(ArduinoMessage.Stop);

        isRotating = false;
    }

    public static void MoveForward()
    {
        if (isMoving) return;

        ConditionalLog("Arduino car moving forward");
        Write(ArduinoMessage.Forward);

        isMoving = true;
    }

    public static void MoveBackward()
    {
        if (isMoving) return;

        ConditionalLog("Arduino car moving backward");
        Write(ArduinoMessage.Backward);

        isMoving = true;
    }

    public static void StopMoving()
    {
        if (!isMoving) return;

        ConditionalLog("Arduino car stopped moving");
        Write(ArduinoMessage.Stop);

        isMoving = false;
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void ConditionalLog(object message)
    {
#if LOG_ARDUINO
        UnityEngine.Debug.Log(message);
#endif
    }
}
