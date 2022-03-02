using UnityEngine;

public static class Utilities
{
    private static Camera cam = null;

    public static Vector3 GetPosInWorld(Vector2 pos, float offset = 0.0f)
    {
        if (cam == null) cam = Camera.main;
        return cam.ScreenToWorldPoint(new Vector3(pos.x, pos.y, cam.gameObject.transform.position.y - offset));
    }

    public static bool IsTouchingUI()
    {
#if UNITY_EDITOR
        return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();
#else
        foreach (Touch touch in Input.touches)
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                return true;
            }
        }
        return false;
#endif
    }

    public static Vector3 AngleToVector(float angle)
    {
        float rad = angle * Mathf.PI / 180;
        return new Vector3(Mathf.Cos(rad), 0.0f, Mathf.Sin(rad));
    }
}
