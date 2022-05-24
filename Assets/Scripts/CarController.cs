using UnityEngine;

public class CarController : MonoBehaviour
{
    public bool IsRecordingPerimeter { get; private set; } = false;

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
    [SerializeField] private GameObject inputTargetObj;
    [SerializeField] private GameObject targetObj;

    [Tooltip("The perimeter empty gameObject's transform that should be the parent of all walls and corners.")]
    [SerializeField] private Transform perimeter;

    [Header("Make sure the wall start object has a tag \"WallStart\"")]
    [Space]

    [SerializeField] private GameObject wallPrefab;
    [SerializeField] private GameObject wallCornerPrefab;
    [SerializeField] private Material wallTranslucentMat;
    [SerializeField] private Material wallRegularMat;

    [Space]
    [SerializeField] private GameObject obstaclePrefab;

    private float speed = 0f; // the current speed of the car (because it accelerates its not always maxSpeed)
    private float rotationSpeed = 0f; // same reason as above

    private bool wasRotating = true;

    private GameObject curWall; // the wall that is currently being "built".
    private GameObject curWallCorner; // the wall corner that is currently being "built".
    private Transform wallStart; // used to get the position from which to start building the wall.
    private Vector3 buildStartPos; // the position from which to start building the wall (set on record start).
    private bool isBuildingClosingWall = false; // true while the final wall to close the perimeter is being built (automatically).

    private Vector3 targetPos; // the position that the car is currently going to
    private Quaternion targetRot; // the target rotation (so the car is looking towards targetPos) before starting to move.
    private CarRotDirection rotToTargetDir = CarRotDirection.None;
    private bool isMovingToTarget = false;

#if UNITY_EDITOR
    [SerializeField]
#endif
    private bool obstacleDetected = false;
    private bool isMovingAroundObstacle = false;
    private ObstAvoidanceStep obstAvoidanceStep = ObstAvoidanceStep.None;
    private Transform obstacleStart;
    private GameObject curObstacle = null;

    private Vector3 inputtedPos; // the position that was inputted by the user (this will not be the same as the current target position when moving around an obstacle)

    private const float ultrasonicOffsetFromCenter = 0.8f;

    private enum CarRotDirection
    {
        None = 0,
        Right = 1,
        Left = -1,
    }

    private enum ObstAvoidanceStep
    {
        None = 0,
        One,
        Two,
        Three,
        Four,
        Five,
    }

    private void Awake()
    {
        wallStart = GameObject.FindWithTag("WallStart").transform;
        obstacleStart = GameObject.FindWithTag("ObstacleEnd").transform;
        inputTargetObj.SetActive(false);
        targetObj.SetActive(false);
    }

    private void Start()
    {
        GameObject.FindWithTag(ArduinoController.BTObjTag).GetComponent<ArduinoController>().OnBTMessageRecieved = OnBTMsgRecieved;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsRecordingPerimeter) return;

        other.gameObject.GetComponent<MeshRenderer>().material = wallTranslucentMat;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsRecordingPerimeter) return;

        other.gameObject.GetComponent<MeshRenderer>().material = wallRegularMat;
    }

    private void Update()
    {
        Vector3 direction = Vector3.zero;
        Vector3 rotation = Vector3.zero;

        if (IsRecordingPerimeter)
        {
            GetMovementFromUI(ref direction, ref rotation);
            RecordPerimeter(rotation.y);

            Move(ref direction, ref rotation);

            return;
        }

        if (ShouldSetTargetPoint())
        {
            Vector3 mouseWorldPos = Utilities.GetPosInWorld(Input.mousePosition, transform.position.y);
            if (IsPointInsidePerimeter(mouseWorldPos))
            {
                inputtedPos = mouseWorldPos;
                StartGoingToPoint(inputtedPos);
            }
        }

        if (isMovingToTarget)
        {
            if (IsAtTargetPos())
            {
                StopMovingToTarget();

                if (isMovingAroundObstacle)
                {
                    SetNextObstacleMovStep();
                }
            }
            else if (isMovingAroundObstacle)
            {
                bool isMovingAlongObstacleSide = obstAvoidanceStep == ObstAvoidanceStep.One || obstAvoidanceStep == ObstAvoidanceStep.Four;

                if (!obstacleDetected && isMovingAlongObstacleSide)
                {
                    // go to next step which will move away slightly from obstacle so that when rotating and moving along the next side, we dont hit the obstacle
                    SetNextObstacleMovStep();
                }
                else
                {
                    if (isMovingAlongObstacleSide)
                    {
                        UpdateObstacleGameObj();
                    }

                    direction = Vector3.forward;
                }
            }
            else if (obstacleDetected)
            {
                isMovingAroundObstacle = true;
                obstAvoidanceStep = ObstAvoidanceStep.One;

                CreateObstacleGameObj();

                StartGoingToPoint(transform.position + (1000f * transform.right), false);
            }
            else
            {
                direction = Vector3.forward;
            }
        }
        else if (rotToTargetDir != CarRotDirection.None) // if rotating to target
        {
            if (HasReachedTargetRot())
            {
                StopRotatingAndStartMovingToTarget();
            }
            else
            {
                rotation.y = (float)rotToTargetDir;
            }
        }

        if (isBuildingClosingWall)
        {
            RecordPerimeter(rotation.y);
        }

        Move(ref direction, ref rotation);
    }

    private void OnBTMsgRecieved(ArduinoMessage msg)
    {
        if (msg == ArduinoMessage.ObstacleDetected)
        {
            obstacleDetected = true;
        }
        else if (msg == ArduinoMessage.NoObstacle)
        {
            obstacleDetected = false;
        }
    }

    private void GetMovementFromUI(ref Vector3 dir, ref Vector3 rot)
    {
        if (UIManager.IsUpPressed) dir += Vector3.forward;
        if (UIManager.IsDownPressed) dir += Vector3.back;

        if (UIManager.IsLeftPressed) --rot.y;
        if (UIManager.IsRightPressed) ++rot.y;

        if (dir != Vector3.zero) rot = Vector3.zero;
    }

    private bool ShouldSetTargetPoint()
    {
        return !isBuildingClosingWall && !Utilities.IsTouchingUI() && !CameraController.WasControlling && Input.GetMouseButtonUp(0);
    }

    private void SetNextObstacleMovStep()
    {
        switch (obstAvoidanceStep)
        {
            case ObstAvoidanceStep.One:
                obstAvoidanceStep = ObstAvoidanceStep.Two;
                MoveAwayFromObstacle();
                break;

            case ObstAvoidanceStep.Two:
                obstAvoidanceStep = ObstAvoidanceStep.Three;
                StartGoingToPoint(transform.position + (ArduinoController.MinObjectDist * -1f * transform.right), false);
                return;

            case ObstAvoidanceStep.Three:
                obstAvoidanceStep = ObstAvoidanceStep.Four;
                curObstacle.GetComponent<WallBuilder>().SwitchWallSides();
                StartGoingToPoint(transform.position + (1000f * transform.forward), false);
                return;

            case ObstAvoidanceStep.Four:
                obstAvoidanceStep = ObstAvoidanceStep.Five;
                MoveAwayFromObstacle();
                break;

            case ObstAvoidanceStep.Five:
                obstAvoidanceStep = ObstAvoidanceStep.None;
                isMovingAroundObstacle = false;
                curObstacle.GetComponent<WallBuilder>().enabled = false;
                curObstacle = null;
                StartGoingToPoint(inputtedPos, false);
                return;
        }

        void MoveAwayFromObstacle()
        {
            StartGoingToPoint(transform.position + ((ArduinoController.MinObjectDist + ultrasonicOffsetFromCenter) * transform.forward), false);
        }
    }

    private void CreateObstacleGameObj()
    {
        Vector3 obstacleStartPos = transform.position + (ArduinoController.MinObjectDist * transform.forward) + (-0.5f * transform.right);
        Quaternion obstacleRot = Quaternion.LookRotation(0.5f * transform.right, Vector3.up);
        obstacleStart.position = obstacleStartPos;
        curObstacle = Instantiate(obstaclePrefab, obstacleStartPos, obstacleRot);
    }

    private void UpdateObstacleGameObj()
    {
        obstacleStart.position = transform.position
            + (ultrasonicOffsetFromCenter * transform.forward)
            + ((ArduinoController.MinObjectDist + (obstAvoidanceStep == ObstAvoidanceStep.Four ? (curObstacle.transform.localScale.x * 0.5f) : 0)) * -1f * transform.right);
    }

    private void RecordPerimeter(float curYRot)
    {
        if (curYRot == 0 && wasRotating)
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

    private void Move(ref Vector3 dir, ref Vector3 rot)
    {
        if (rot == Vector3.zero)
        {
            ArduinoController.StopRotating();

            rotationSpeed = 0;
        }
        else
        {
            rotationSpeed = Mathf.Clamp(rotationSpeed + (Time.deltaTime * MaxRotationSpeed / WheelTimeToMax), 0f, MaxRotationSpeed);
        }

        if (dir == Vector3.zero)
        {
            ArduinoController.StopMoving();

            speed = 0;
        }
        else
        {
            speed = Mathf.Clamp(speed + (Time.deltaTime * maxSpeed / WheelTimeToMax), 0f, maxSpeed);
        }

        if (rot.y == -1) ArduinoController.RotateLeft();
        else if (rot.y == 1) ArduinoController.RotateRight();

        if (dir == Vector3.forward) ArduinoController.MoveForward();
        else if (dir == Vector3.back) ArduinoController.MoveBackward();

        transform.Translate(Time.deltaTime * speed * dir);
        transform.Rotate(Time.deltaTime * rotationSpeed * rot);

        wasRotating = (rot.y != 0);
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

    private void StartGoingToPoint(Vector3 point, bool setInputTargetObj = true)
    {
        point.y = transform.position.y;
        Vector3 carToPtVec = point - transform.position;

        if (carToPtVec.magnitude < WheelTimeToMax * MaxSpeed * 0.5f)
        {
            isMovingToTarget = false;
            rotToTargetDir = CarRotDirection.None;
            return;
        }

        if (isMovingToTarget)
        {
            isMovingToTarget = false;
        }
        else if (rotToTargetDir != CarRotDirection.None)
        {
            rotToTargetDir = CarRotDirection.None;
        }

        if (setInputTargetObj)
        {
            inputTargetObj.transform.position = point;
            inputTargetObj.SetActive(true);
        }
        else
        {
            targetObj.transform.position = point;
            targetObj.SetActive(true);
        }

        targetPos = point;
        targetRot = Quaternion.LookRotation(carToPtVec, Vector3.up);

        // Calculate the degree delta to rotate,
        // and set rotation direction to left or right depending on which is shorter
        float degreeDeltaToRot = targetRot.eulerAngles.y - transform.rotation.eulerAngles.y;
        if (degreeDeltaToRot < 0)
        {
            degreeDeltaToRot = 360 - transform.rotation.eulerAngles.y + targetRot.eulerAngles.y;
        }

        rotToTargetDir = degreeDeltaToRot > 180 ? CarRotDirection.Left : CarRotDirection.Right;

        if (isMovingAroundObstacle)
        {
            ArduinoController.RotateServoLeft();
        }
        else
        {
            ArduinoController.RotateServoForward();
        }
    }

    private bool HasReachedTargetRot()
    {
        float degreeDeltaToRot = (targetRot.eulerAngles.y - transform.rotation.eulerAngles.y);
        if (degreeDeltaToRot < 0)
        {
            degreeDeltaToRot = 360 - transform.rotation.eulerAngles.y + targetRot.eulerAngles.y;
        }

        if (rotToTargetDir == CarRotDirection.Left)
        {
            return degreeDeltaToRot <= 180 && degreeDeltaToRot >= 0;
        }

        return rotToTargetDir == CarRotDirection.Right && degreeDeltaToRot > 180;
    }

    private void StopRotatingAndStartMovingToTarget()
    {
        rotToTargetDir = CarRotDirection.None;
        transform.rotation = targetRot;

        isMovingToTarget = true;
    }

    private bool IsAtTargetPos()
    {
        if (transform.position == targetPos)
        {
            return true;
        }

        float movDistSinceLastFrame = Time.deltaTime * speed;
        return (targetPos - transform.position).sqrMagnitude < movDistSinceLastFrame * movDistSinceLastFrame;
    }

    private void StopMovingToTarget()
    {
        isMovingToTarget = false;
        transform.position = targetPos;
        inputTargetObj.SetActive(false);
        targetObj.SetActive(false);

        if (isBuildingClosingWall)
        {
            curWall.GetComponent<WallBuilder>().ForceWallEndPosition(GameObject.FindWithTag("WallStart").transform.position);

            SetRecordUIInteractable(true);
            StopRecording(true);
            isBuildingClosingWall = false;
        }
    }

    private void EndCurWallBuild()
    {
        if (curWall != null)
        {
            if (!isBuildingClosingWall) curWall.GetComponent<MeshRenderer>().material = wallRegularMat;
            curWall.GetComponent<WallBuilder>().enabled = false;
            curWall.transform.parent = perimeter;
        }
        if (curWallCorner != null)
        {
            curWallCorner.GetComponent<MeshRenderer>().material = wallRegularMat;
            curWallCorner.transform.parent = perimeter;
        }
        curWall = null;
        curWallCorner = null;
    }

    public void StopRecording(bool skipDistCheck = false)
    {
        if (skipDistCheck)
        {
            EndCurWallBuild();

            IsRecordingPerimeter = false;
            return;
        }

        float cornerRadius = (wallCornerPrefab.transform.localScale.x / 2) - 0.01f;
        if ((wallStart.position - buildStartPos).sqrMagnitude > cornerRadius * cornerRadius)
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
        UIManager.AutoCloseToggle.GetComponent<UnityEngine.UI.Toggle>().interactable = val;

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
