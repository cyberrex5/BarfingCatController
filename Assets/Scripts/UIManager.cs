using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    [SerializeField] private GameObject settingsPanel;
    [Space]
    [SerializeField] private Slider speedSlider;
    [SerializeField] private TMP_InputField speedSliderInput;
    [Space]
    [SerializeField] private Slider minObjDistSlider;
    [SerializeField] private TMP_InputField minObjDistSliderInput;

    [Space]

    [SerializeField] private GameObject closeWarningPanel;

    [Space]

    [SerializeField] private Image stopImage;

    [SerializeField] private CarController carScript;

    private bool hasAppliedChanges { get => curNormalSpeed == ArduinoController.NormalSpeed && curMinObjDist == (byte)(ArduinoController.MinObjectDist * 10f); }

    private byte curNormalSpeed;
    private byte curMinObjDist;

    private void Awake()
    {
        RevertSettings();

        MovementButtons = GameObject.FindWithTag("MovementButtons");
        MovementButtons.SetActive(false);
        AutoCloseToggle = GameObject.FindWithTag("AutoCloseToggle");
        AutoCloseToggle.SetActive(false);
        GameObject.FindWithTag("RecordButton").GetComponent<Button>().onClick.AddListener(OnRecordClick);
        GameObject.FindWithTag("ResetButton").GetComponent<Button>().onClick.AddListener(OnResetClick);

        if (ArduinoController.IsConnected)
        {
            GameObject.FindWithTag("ConnectingPanel").SetActive(false);
            GameObject.FindWithTag(ArduinoController.BTObjTag).GetComponent<ArduinoController>().enabled = true;
            return;
        }

        GameObject.FindWithTag("ConnectButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameObject.FindWithTag("ConnectButton").GetComponent<Button>().interactable = false;
            GameObject.FindWithTag("SandboxButton").GetComponent<Button>().interactable = false;

            AndroidBluetooth.BluetoothService.CreateBluetoothObject();
            ArduinoController.Connect();

            GameObject.FindWithTag("ConnectingPanel").SetActive(false);
            carScript.enabled = true;
        });

        GameObject.FindWithTag("SandboxButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameObject.FindWithTag("ConnectingPanel").SetActive(false);
            carScript.enabled = true;
        });
    }

    private void RevertSettings()
    {
        curNormalSpeed = ArduinoController.NormalSpeed;
        speedSlider.value = curNormalSpeed;
        speedSliderInput.text = curNormalSpeed.ToString();

        curMinObjDist = (byte)(ArduinoController.MinObjectDist * 10f);
        minObjDistSlider.value = curMinObjDist;
        minObjDistSliderInput.text = curMinObjDist.ToString();
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

    public void OpenSettingsPanel() => settingsPanel.SetActive(true);

    //public void CloseSettingsPanel() => settingsPanel.SetActive(false);

    public void OnCloseSettingsClick()
    {
        if (hasAppliedChanges)
        {
            settingsPanel.SetActive(false);
            return;
        }

        closeWarningPanel.SetActive(true);
    }

    public void OnCloseWarningPanelConfirm()
    {
        RevertSettings();
        settingsPanel.SetActive(false);
        closeWarningPanel.SetActive(false);
    }

    public void OnCloseWarningPanelCancel()
    {
        closeWarningPanel.SetActive(false);
    }

    public void OnSpeedSliderChange()
    {
        curNormalSpeed = (byte)speedSlider.value;
        speedSliderInput.text = curNormalSpeed.ToString();
    }

    public void OnSpeedSliderTextChange()
    {
        byte newVal;
        if (!byte.TryParse(speedSliderInput.text, out newVal))
        {
            return;
        }

        int clampedVal = Mathf.Clamp(newVal, 0, 255);
        if (newVal != clampedVal)
        {
            speedSliderInput.text = clampedVal.ToString();
            newVal = (byte)clampedVal;
        }

        curNormalSpeed = newVal;
        speedSlider.value = newVal;
    }

    public void OnObjDistSliderChange()
    {
        curMinObjDist = (byte)minObjDistSlider.value;
        minObjDistSliderInput.text = curMinObjDist.ToString();
    }

    public void OnObjDistSliderTextChange()
    {
        byte newVal;
        if (!byte.TryParse(minObjDistSliderInput.text, out newVal))
        {
            return;
        }

        int clampedVal = Mathf.Clamp(newVal, 0, 255);
        if (newVal != clampedVal)
        {
            minObjDistSliderInput.text = clampedVal.ToString();
            newVal = (byte)clampedVal;
        }

        curMinObjDist = newVal;
        minObjDistSlider.value = newVal;
    }

    public void OnApplySettingsClick()
    {
        if (curNormalSpeed != ArduinoController.NormalSpeed) ArduinoController.SetNormalSpeed(curNormalSpeed);
        if (curMinObjDist != (byte)(ArduinoController.MinObjectDist * 10f)) ArduinoController.SetMinObjectDist(curMinObjDist);
    }
}
