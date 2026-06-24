using UnityEngine;

public enum ControlMode
{
    PC,
    VR
}

public class ControlModeManager : MonoBehaviour
{
    public static ControlModeManager Instance;

    [Header("Current Mode")]
    [SerializeField] private ControlMode startMode = ControlMode.PC;

    [Header("PC Objects")]
    [SerializeField] private GameObject pcPlayer;
    [SerializeField] private MonoBehaviour[] pcScripts;

    [Header("VR Objects")]
    [SerializeField] private GameObject xrOrigin;
    [SerializeField] private GameObject[] vrControllerObjects;
    [SerializeField] private GameObject xrDeviceSimulator;

    public ControlMode CurrentMode { get; private set; }

    public bool IsPC => CurrentMode == ControlMode.PC;
    public bool IsVR => CurrentMode == ControlMode.VR;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SetMode(startMode);
    }

    private void Update()
    {
        // Для быстрого теста: M переключает режим
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMode();
        }
    }

    public void ToggleMode()
    {
        if (CurrentMode == ControlMode.PC)
            SetMode(ControlMode.VR);
        else
            SetMode(ControlMode.PC);
    }

    public void SetPCMode()
    {
        SetMode(ControlMode.PC);
    }

    public void SetVRMode()
    {
        SetMode(ControlMode.VR);
    }

    public void SetMode(ControlMode mode)
    {
        CurrentMode = mode;

        bool pcActive = mode == ControlMode.PC;
        bool vrActive = mode == ControlMode.VR;

        if (pcPlayer != null)
            pcPlayer.SetActive(pcActive);

        foreach (MonoBehaviour script in pcScripts)
        {
            if (script != null)
                script.enabled = pcActive;
        }

        if (xrOrigin != null)
            xrOrigin.SetActive(vrActive);

        foreach (GameObject obj in vrControllerObjects)
        {
            if (obj != null)
                obj.SetActive(vrActive);
        }

        // XR Device Simulator нужен только для теста в Editor, в обычном PC режиме лучше выключать
        if (xrDeviceSimulator != null)
            xrDeviceSimulator.SetActive(false);

        if (pcActive)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        Debug.Log("[ControlModeManager] Mode: " + mode);
    }
}