
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorManager : MonoBehaviour
{
    static public CursorManager Instance { get; private set; }
    [SerializeField] private InputActionAsset inputActionAsset;
    private InputAction unlockAction;
    private bool isCursorLocked = false;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            Debug.Log("Instance created");
        }
        var uiMap = inputActionAsset.FindActionMap("UI");
        Debug.Assert(uiMap != null, "UI Action Map not found in InputActionAsset.");
        unlockAction = uiMap.FindAction("UnlockMouse");
        Debug.Assert(unlockAction != null, "UnlockMouse Action not found in UI Action Map.");
        Debug.Log("CursorManager initialized.");
    }

    void OnEnable()
    {
        unlockAction.Enable();
        unlockAction.performed += OnCursorUnlockPerformed;

        CursorUnlock();
    }

    void OnDisable()
    {
        unlockAction.performed -= OnCursorUnlockPerformed;
        unlockAction.Disable();
    }

    void Update()
    {
        // Enforce the lock state if it should be locked but somehow got unlocked (e.g. alt-tab or click)
        if (isCursorLocked && (Cursor.lockState != CursorLockMode.Locked || Cursor.visible))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnCursorUnlockPerformed(InputAction.CallbackContext context)
    {
        ToggleCursorState();
    }

    void ToggleCursorState()
    {
        if (isCursorLocked)
        {
            CursorUnlock();
        }
        else
        {
            CursorLock();
        }
        Debug.Log("Cursor state toggled. Locked: " + isCursorLocked);
    }


    public void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isCursorLocked = true;
    }

    public void CursorUnlock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        isCursorLocked = false;
    }
}
