using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static bool IsUpPressed { get; private set; }
    public static bool IsDownPressed { get; private set; }
    public static bool IsLeftPressed { get; private set; }
    public static bool IsRightPressed { get; private set; }

    [Header("Make sure to set the callbacks for the movement buttons\nin their event trigger components in the inspector.")]
    [Space]

    [SerializeField] private Image stopImage;

    [SerializeField] private CarController carScript;

    private GameObject movementButtons;

    private void Awake()
    {
        movementButtons = GameObject.FindWithTag("MovementButtons");
        movementButtons.SetActive(false);
        GameObject.FindWithTag("RecordButton").GetComponent<Button>().onClick.AddListener(OnRecordClick);
        GameObject.FindWithTag("ResetButton").GetComponent<Button>().onClick.AddListener(OnResetClick);
    }

    private void OnRecordClick()
    {
        if (carScript.IsRecordingPerimeter)
        {
            movementButtons.SetActive(false);
            carScript.StopRecording(!GameObject.FindWithTag("AutoCloseToggle").GetComponent<Toggle>().isOn);
            stopImage.enabled = false;
            return;
        }

        movementButtons.SetActive(true);
        carScript.StartRecording();
        stopImage.enabled = true;
    }

    private void OnResetClick()
    {
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnUpButtonChanged(bool val) => IsUpPressed = val;

    public void OnDownButtonChanged(bool val) => IsDownPressed = val;

    public void OnLeftButtonChanged(bool val) => IsLeftPressed = val;

    public void OnRightButtonChanged(bool val) => IsRightPressed = val;
}
