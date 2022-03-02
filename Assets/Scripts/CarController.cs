using UnityEngine;

public class CarController : MonoBehaviour
{
    [HideInInspector] public bool IsRecordingPerimeter = false;

    [Tooltip("Max car speed (m/sec)")]
    [Range(0, 5)]
    public float MaxSpeed = 1.5f;
    private float maxSpeed { get => MaxSpeed * 10f; }

    [Tooltip("Max rotation speed (degree/sec)")]
    public float MaxRotationSpeed = 200f;

    [Tooltip("Time (for wheels) to reach max speed (sec)")]
    [Range(0, 1)]
    public float WheelTimeToMax = 0.15f;

    [Space]
    [Tooltip("The GameObject that should appear at the point the car is (automatically) moving to.")]
    [SerializeField] private GameObject targetObj;

    [Tooltip("The perimeter empty gameObject's transform that should be the parent of all walls and corners.")]
    [SerializeField] private Transform perimeter;

    [Header("Make sure the wall start object has a tag \"WallStart\"")]
    [Space]

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wallCornerPrefab;
    //[SerializeField] private Material wallRegularMat;

    [Space]
    [Header("Don't set these")]
    [SerializeField] private float speed = 0f;
    [SerializeField] private float rotationSpeed = 0f;

    private Vector3 direction;
    private Vector3 rotation;
    private bool wasRotating = true;

    private GameObject curWall; // the wall that is currently being "built".
    private GameObject curWallCorner; // the wall corner that is currently being "built".
    private Transform wallStart; // used to get the position from which to start building the wall.
    private Vector3 buildStartPos; // the position from which to start building the wall (set on record start).
    private bool isBuildingClosingWall = false; // true while the final wall to close the perimeter is being built (automatically).

    private Vector3 targetPos; // the target position the car should be (automatically) moving to.
    private Quaternion targetRot; // the target rotation (so the car is looking towards targetPos) before starting to move.
    private CarRotDirection rotToTargetDir = CarRotDirection.None;
    private bool isMovingToTarget = false;

    private enum CarRotDirection
    {
        None = 0,
        Right = 1,
        Left = -1,
    }

    private void Awake()
    {
        wallStart = GameObject.FindWithTag("WallStart").transform;
        targetObj.SetActive(false);
    }

    private void Update()
    {
        direction = Vector3.zero;
        rotation = Vector3.zero;

        if (IsRecordingPerimeter)
        {
            if (UIManager.IsUpPressed) direction += Vector3.forward;
            if (UIManager.IsDownPressed) direction += Vector3.back;

            if (UIManager.IsLeftPressed) --rotation.y;
            if (UIManager.IsRightPressed) ++rotation.y;

            if (direction != Vector3.zero) rotation = Vector3.zero;

            RecordPerimeter();
        }
        else
        {
            if (isMovingToTarget)
            {
                direction = Vector3.forward;
                CheckIfAtTarget();
            }
            else if (rotToTargetDir == CarRotDirection.Right || rotToTargetDir == CarRotDirection.Left)
            {
                rotation.y = (float)rotToTargetDir;
            }
            else if (!isBuildingClosingWall && !Utilities.IsTouchingUI() && !CameraController.WasControlling && Input.GetMouseButtonUp(0))
            {
                Vector3 mouseWorldPos = Utilities.GetPosInWorld(Input.mousePosition, transform.position.y);
                if (IsPointInsidePerimeter(mouseWorldPos)) StartGoingToPoint(mouseWorldPos);
            }

            if (isBuildingClosingWall) RecordPerimeter();
        }

        Move();
        wasRotating = (rotation.y != 0);
    }

    private void RecordPerimeter()
    {
        if (rotation.y == 0 && wasRotating)
        {
            if (curWall != null)
            {
                if (curWall.transform.localScale.z == 0)
                {
                    Destroy(curWall);
                    if (curWallCorner != null) Destroy(curWallCorner);
                }
                else
                {
                    EndCurWallBuild();
                }
            }
            curWall = Instantiate(wallPrefab, wallStart.position, transform.rotation);
            curWallCorner = Instantiate(wallCornerPrefab, new Vector3(wallStart.position.x, curWall.transform.position.y, wallStart.position.z), Quaternion.identity);
        }
    }

    private void Move()
    {
        if (rotation == Vector3.zero)
        {
            ArduinoController.StopRotating();

            rotationSpeed = 0;
        }
        else
        {
            rotationSpeed = Mathf.Clamp(rotationSpeed + (Time.deltaTime * MaxRotationSpeed / WheelTimeToMax), 0f, MaxRotationSpeed);
        }
        if (direction == Vector3.zero)
        {
            ArduinoController.StopMoving();

            speed = 0;
        }
        else
        {
            speed = Mathf.Clamp(speed + (Time.deltaTime * maxSpeed / WheelTimeToMax), 0f, maxSpeed);
        }

        if (rotation.y == -1) ArduinoController.RotateLeft();
        else if (rotation.y == 1) ArduinoController.RotateRight();

        if (direction == Vector3.forward) ArduinoController.MoveForward();
        else if (direction == Vector3.back) ArduinoController.MoveBackward();

        transform.Translate(Time.deltaTime * speed * direction);
        transform.Rotate(Time.deltaTime * rotationSpeed * rotation);
    }

    private bool IsPointInsidePerimeter(Vector3 point)
    {
        const int rayCount = 12;

        bool isInsidePerimeter = true;
        for (int i = 0; i < rayCount; ++i)
        {
            Vector3 dir = Utilities.AngleToVector(i * 360f / rayCount);
            Debug.DrawRay(point, dir * 50, Color.green, 0.15f);
            if (!Physics.Raycast(point, dir, out _, Mathf.Infinity))
            {
                isInsidePerimeter = false;
                break;
            }
        }

        return isInsidePerimeter;
    }

    private void StartGoingToPoint(Vector3 point)
    {
        point.y = transform.position.y;
        if (transform.position == point)
        {
            isMovingToTarget = false;
            rotToTargetDir = CarRotDirection.None;
            return;
        }

        targetObj.transform.position = point;
        targetObj.SetActive(true);

        targetPos = point;
        targetRot = Quaternion.LookRotation(point - transform.position);

        // Calculate the degree delta to rotate,
        // and set rotation direction to left or right depending on which is shorter
        float degree = (targetRot.eulerAngles.y - transform.rotation.eulerAngles.y);
        if (degree < 0)
        {
            degree = (360 - transform.rotation.eulerAngles.y) + targetRot.eulerAngles.y;
        }
        if (degree > 180)
        {
            degree = 360 - degree;
            rotToTargetDir = CarRotDirection.Left;
        }
        else
        {
            rotToTargetDir = CarRotDirection.Right;
        }

        float maxRotSpeedAmount = 0.5f * WheelTimeToMax * MaxRotationSpeed; // the rotation in degrees until reaching max speed.
        Invoke(nameof(StopRotatingAndStartMovingToTarget),
               // time to rotate `degree`:
               degree < maxRotSpeedAmount
               ? Mathf.Sqrt(2 * degree * WheelTimeToMax / MaxRotationSpeed)
               : ((degree - maxRotSpeedAmount) / MaxRotationSpeed) + WheelTimeToMax);
    }

    private void StopRotatingAndStartMovingToTarget()
    {
        rotToTargetDir = CarRotDirection.None;
        transform.rotation = targetRot;

        isMovingToTarget = true;

        //float dist = (targetPos - transform.position).magnitude;

        //float maxSpeedDist = 0.5f * WheelTimeToMax * maxSpeed; // the distance that would be crossed until reaching max speed.
        //Invoke(nameof(StopMovingToTarget),
        //       // time to cross `dist`:
        //       dist < maxSpeedDist
        //       ? Mathf.Sqrt(2 * dist * WheelTimeToMax / maxSpeed)
        //       : ((dist - maxSpeedDist) / maxSpeed) + WheelTimeToMax);
    }

    private void CheckIfAtTarget()
    {
        if (transform.position == targetPos)
        {
            StopMovingToTarget();
            return;
        }

        float movDistSinceLastFrame = Time.deltaTime * speed;
        if ((targetPos - transform.position).sqrMagnitude < movDistSinceLastFrame * movDistSinceLastFrame)
        {
            StopMovingToTarget();
        }
    }

    private void StopMovingToTarget()
    {
        isMovingToTarget = false;
        transform.position = targetPos;
        targetObj.SetActive(false);

        if (isBuildingClosingWall)
        {
            isBuildingClosingWall = false;

            SetRecordUIInteractable(true);
            StopRecording(true);
        }
    }

    private void EndCurWallBuild()
    {
        if (curWall != null)
        {
            //curWall.GetComponent<MeshRenderer>().material = wallRegularMat;
            curWall.GetComponent<WallBuilder>().enabled = false;
            curWall.transform.parent = perimeter;
        }
        if (curWallCorner != null)
        {
            //curWallCorner.GetComponent<MeshRenderer>().material = wallRegularMat;
            curWallCorner.transform.parent = perimeter;
        }
        curWall = null;
        curWallCorner = null;
    }

    public void StopRecording(bool skipDistCheck = false)
    {
        float cornerRadius = (wallCornerPrefab.transform.localScale.x / 2) - 0.01f;
        if (!skipDistCheck && ((wallStart.position - buildStartPos).sqrMagnitude > cornerRadius * cornerRadius))
        {
            if (wallStart.localPosition.z == 0)
            {
                StartGoingToPoint(buildStartPos);
            }
            else
            {
                StartGoingToPoint(buildStartPos + ((transform.position - buildStartPos).normalized * wallStart.localPosition.z));
            }
            isBuildingClosingWall = true;
            SetRecordUIInteractable(false);
        }
        else
        {
            EndCurWallBuild();
        }
        IsRecordingPerimeter = false;
    }

    public void StartRecording()
    {
        buildStartPos = wallStart.position;
        wasRotating = true;
        IsRecordingPerimeter = true;
    }

    private void SetRecordUIInteractable(bool val)
    {
        GameObject.FindWithTag("AutoCloseToggle").GetComponent<UnityEngine.UI.Toggle>().interactable = val;

        GameObject recordButton = GameObject.FindWithTag("RecordButton");

        recordButton.GetComponent<UnityEngine.UI.Button>().interactable = val;

        UnityEngine.UI.Image[] images = recordButton.GetComponentsInChildren<UnityEngine.UI.Image>();
        if (!val)
        {
            foreach (UnityEngine.UI.Image image in images)
            {
                image.color = new Color(1, 1, 1, 0.5f);
            }
            return;
        }

        foreach (UnityEngine.UI.Image image in images)
        {
            image.color = new Color(1, 1, 1, 1f);
        }
    }
}
