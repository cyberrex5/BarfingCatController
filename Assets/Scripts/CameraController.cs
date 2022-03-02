using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static bool WasControlling { get; private set; }

    private static bool camWasUpdated;

    private Camera cam;
    private Vector3 initialPos;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        if (WasControlling && Input.touchCount == 0)
        {
            WasControlling = false;
        }

        if (Utilities.IsTouchingUI())
        {
            initialPos = Utilities.GetPosInWorld(Input.mousePosition);
            return;
        }

        if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector3 touch1InitialPos = touch1.position - touch1.deltaPosition;
            Vector3 touch2InitialPos = touch2.position - touch2.deltaPosition;

            float delta = Vector2.Distance(touch1.position, touch2.position) - Vector2.Distance(touch1InitialPos, touch2InitialPos);

            float curFov = cam.fieldOfView;
            float newFov = Mathf.Clamp(curFov - (delta * 0.1f), 1f, 170f);

            camWasUpdated = curFov != newFov;

            cam.fieldOfView = newFov;
        }
        else
        {
            Vector3 newPos = transform.position;
            if (Input.GetMouseButtonDown(0))
            {
                initialPos = Utilities.GetPosInWorld(Input.mousePosition);
            }
            if (Input.GetMouseButton(0))
            {
                newPos -= Utilities.GetPosInWorld(Input.mousePosition) - initialPos;
            }

            camWasUpdated = transform.position != newPos;

            transform.position = newPos;
        }

        if (camWasUpdated)
        {
            WasControlling = true;
        }
    }
}
