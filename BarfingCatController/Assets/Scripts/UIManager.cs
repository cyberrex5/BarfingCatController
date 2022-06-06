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

    [SerializeField] private GameObject calibrationPanel;
    [Space]
    [SerializeField] private Slider maxSpeedSlider;
    [SerializeField] private TMP_InputField maxSpeedSliderInput;
    [Space]
    [SerializeField] private Slider maxRotSpeedSlider;
    [SerializeField] private TMP_InputField maxRotSpeedSliderInput;
    [Space]
    [SerializeField] private Slider timeToMaxSlider;
    [SerializeField] private TMP_InputField timeToMaxSliderInput;

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

    private bool hasAppliedChanges
    {
        get => curNormalSpeed == ArduinoController.NormalSpeed
            && curMinObjDist == (byte)(ArduinoController.MinObjectDist * 10f)
            && curMaxSpeed == carScript.MaxSpeed
            && curMaxRotSpeed == carScript.MaxRotationSpeed
            && curTimeToMax == carScript.WheelTimeToMax;
    }

    private float curMaxSpeed;
    private float curMaxRotSpeed;
    private float curTimeToMax;

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

        //if (ArduinoController.IsConnected)
        //{
        //    GameObject.FindWithTag("ConnectingPanel").SetActive(false);
        //    GameObject.FindWithTag(ArduinoController.BTObjTag).GetComponent<ArduinoController>().enabled = true;
        //    return;
        //}

        GameObject.FindWithTag("ConnectButton").GetComponent<Button>().onClick.AddListener(() =>
        {
            GameObject.FindWithTag("ConnectButton").GetComponent<Button>().interactable = false;
            GameObject.FindWithTag("SandboxButton").GetComponent<Button>().interactable = false;

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

        curMaxSpeed = carScript.MaxSpeed;
        maxSpeedSlider.value = curMaxSpeed;
        maxSpeedSliderInput.text = curMaxSpeed.ToString();

        curMaxRotSpeed = carScript.MaxRotationSpeed;
        maxRotSpeedSlider.value = curMaxRotSpeed;
        maxRotSpeedSliderInput.text = curMaxRotSpeed.ToString();

        curTimeToMax = carScript.WheelTimeToMax;
        timeToMaxSlider.value = curTimeToMax;
        timeToMaxSliderInput.text = curTimeToMax.ToString();
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
        AndroidBluetooth.BluetoothService.Instance.StopBluetoothConnection();
        UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    public void OnUpButtonChanged(bool val) => IsUpPressed = val;

    public void OnDownButtonChanged(bool val) => IsDownPressed = val;

    public void OnLeftButtonChanged(bool val) => IsLeftPressed = val;

    public void OnRightButtonChanged(bool val) => IsRightPressed = val;

    public void OpenSettingsPanel() => settingsPanel.SetActive(true);

    //public void CloseSettingsPanel() => settingsPanel.SetActive(false);

    public void OpenCalibrationPanel() => calibrationPanel.SetActive(true);

    public void CloseCalibrationPanel() => calibrationPanel.SetActive(false);

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

    public void OnSpeedSliderChange() => UpdateSliderInputField(speedSlider, speedSliderInput, ref curNormalSpeed);

    public void OnSpeedSliderTextChange() => UpdateSliderUI(speedSlider, speedSliderInput, ref curNormalSpeed);

    public void OnObjDistSliderChange() => UpdateSliderInputField(minObjDistSlider, minObjDistSliderInput, ref curMinObjDist);

    public void OnObjDistSliderTextChange() => UpdateSliderUI(minObjDistSlider, minObjDistSliderInput, ref curMinObjDist);

    #region Calibration Panel

    public void OnMaxSpeedSliderChange() => UpdateSliderInputField(maxSpeedSlider, maxSpeedSliderInput, ref curMaxSpeed);

    public void OnMaxSpeedSliderTextChange() => UpdateSliderUI(maxSpeedSlider, maxSpeedSliderInput, ref curMaxSpeed);

    public void OnMaxRotSpeedSliderChange() => UpdateSliderInputField(maxRotSpeedSlider, maxRotSpeedSliderInput, ref curMaxRotSpeed);

    public void OnMaxRotSpeedSliderTextChange() => UpdateSliderUI(maxRotSpeedSlider, maxRotSpeedSliderInput, ref curMaxRotSpeed);

    public void OnTimeToMaxSliderChange() => UpdateSliderInputField(timeToMaxSlider, timeToMaxSliderInput, ref curTimeToMax);

    public void OnTimeToMaxSliderTextChange() => UpdateSliderUI(timeToMaxSlider, timeToMaxSliderInput, ref curTimeToMax);

    #endregion

    public void OnApplySettingsClick()
    {
        if (curNormalSpeed != ArduinoController.NormalSpeed) ArduinoController.SetNormalSpeed(curNormalSpeed);
        if (curMinObjDist != (byte)(ArduinoController.MinObjectDist * 10f)) ArduinoController.SetMinObjectDist(curMinObjDist);
        carScript.MaxSpeed = curMaxSpeed;
        carScript.MaxRotationSpeed = curMaxRotSpeed;
        carScript.WheelTimeToMax = curTimeToMax;
    }

    private void UpdateSliderInputField(Slider slider, TMP_InputField sliderInput, ref byte field)
    {
        field = (byte)slider.value;
        sliderInput.text = field.ToString();
    }

    private void UpdateSliderInputField(Slider slider, TMP_InputField sliderInput, ref float field)
    {
        field = slider.value;
        sliderInput.text = field.ToString();
    }

    private void UpdateSliderUI(Slider slider, TMP_InputField sliderInput, ref byte field)
    {
        if (!byte.TryParse(sliderInput.text, out byte newVal))
        {
            return;
        }

        byte clampedVal = (byte)Mathf.Clamp(newVal, byte.MinValue, byte.MaxValue);
        if (newVal != clampedVal)
        {
            sliderInput.text = clampedVal.ToString();
            newVal = clampedVal;
        }

        field = newVal;
        slider.value = newVal;
    }

    private void UpdateSliderUI(Slider slider, TMP_InputField sliderInput, ref float field)
    {
        if (!float.TryParse(sliderInput.text, out float newVal))
        {
            return;
        }

        float clampedVal = Mathf.Clamp(newVal, slider.minValue, slider.maxValue);
        if (newVal != clampedVal)
        {
            sliderInput.text = clampedVal.ToString();
            newVal = clampedVal;
        }

        field = newVal;
        slider.value = newVal;
    }
}
