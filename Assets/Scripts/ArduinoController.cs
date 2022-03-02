public static class ArduinoController
{
    private static bool isMoving = false;
    private static bool isRotating = false;

    public static void RotateLeft()
    {
        if (isRotating) return;

        UnityEngine.Debug.Log("Arduino car rotating left");

        isRotating = true;
    }

    public static void RotateRight()
    {
        if (isRotating) return;

        UnityEngine.Debug.Log("Arduino car rotating right");

        isRotating = true;
    }

    public static void StopRotating()
    {
        if (!isRotating) return;

        UnityEngine.Debug.Log("Arduino car stopped rotating");

        isRotating = false;
    }

    public static void MoveForward()
    {
        if (isMoving) return;

        UnityEngine.Debug.Log("Arduino car moving forward");

        isMoving = true;
    }

    public static void MoveBackward()
    {
        if (isMoving) return;

        UnityEngine.Debug.Log("Arduino car moving backward");

        isMoving = true;
    }

    public static void StopMoving()
    {
        if (!isMoving) return;

        UnityEngine.Debug.Log("Arduino car stopped moving");

        isMoving = false;
    }
}
