using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static GameObject MovementButtons { get; private set; }
    public static GameObject AutoCloseToggle { get; private set; }

    public static bool IsUpPressed { get; private set; }
    public static bool IsDownPressed { get; private set; }
    public static bool IsLeftPressed { get; private set; }
    public static bool IsRightPressed { get; private set; }

    [Header("Make sure to set the callbacks for the movement buttons\nin their event trigger components in the inspector.")]
    [Space]

    [SerializeField] private Image stopImage;

    [SerializeField] private CarController carScript;

    private void Awake()
    {
        MovementButtons = GameObject.FindWithTag("MovementButtons");
        MovementButtons.SetActive(false);
        AutoCloseToggle = GameObject.FindWithTag("AutoCloseToggle");
        AutoCloseToggle.SetActive(false);
        GameObject.FindWithTag("RecordButton").GetComponent<Button>().onClick.AddListener(OnRecordClick);
        GameObject.FindWithTag("ResetButton").GetComponent<Button>().onClick.AddListener(OnResetClick);

#if !UNITY_EDITOR
        AndroidBluetooth.BluetoothService.CreateBluetoothObject();
        ArduinoController.Connect();
#endif
        GameObject.FindWithTag("ConnectingPanel").SetActive(false);
    }

    private void OnRecordClick()
    {
        if (carScript.IsRecordingPerimeter)
        {
            MovementButtons.SetActive(false);
            carScript.StopRecording(!AutoCloseToggle.GetComponent<Toggle>().isOn);
            AutoCloseToggle.SetActive(false);
            stopImage.enabled = false;
            return;
        }

        MovementButtons.SetActive(true);
        AutoCloseToggle.SetActive(true);
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
