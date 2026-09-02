using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [Header("Platform Durumu")]
    public bool isMobile = false;

    [Header("Gece Gündüz Döngüsü")]
    public Light directionalLight; 
    public float dayNightDuration = 360.0f;
    private float timer;

    [Header("Karakter Kontrolleri")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 9.0f;
    private float currentSpeed;
    private float lastWPressTime;
    private const float doubleTapThreshold = 0.3f;

    [Header("Mobil UI Girdileri")]
    public GameObject mobileUIControls;
    public Joystick mobileJoystick;
    private bool isMobileRunning = false;

    [Header("El Feneri ve Etkileşim")]
    public GameObject flashlight; 
    private bool isFlashlightOn = false;
    public Transform holdPosition;
    private GameObject heldObject = null;

    [Header("Takipçi Ayarları")]
    public Transform chaser;
    private float chaserSpeed;

    [Header("Zorluk ve Dil Ayarları")]
    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty = Difficulty.Medium;
    public enum Language { Turkish, English }
    public Language currentLanguage = Language.Turkish;

    void Start()
    {
        DetectPlatform();
        currentSpeed = walkSpeed;
        SetDifficultySettings();
    }

    void DetectPlatform()
    {
        #if UNITY_ANDROID || UNITY_IOS
            isMobile = true;
        #else
            isMobile = SystemInfo.deviceType == DeviceType.Handheld || Input.touchSupported;
        #endif

        if (mobileUIControls != null)
        {
            mobileUIControls.SetActive(isMobile);
        }
    }

    void Update()
    {
        HandleDayNightCycle();
        HandleMovement();

        if (!isMobile)
        {
            HandlePCInputActions();
        }
    }

    void HandleDayNightCycle()
    {
        timer += Time.deltaTime;
        float angle = (timer / dayNightDuration) * 360.0f;
        if (directionalLight != null)
        {
            directionalLight.transform.rotation = Quaternion.Euler(angle - 90.0f, 170.0f, 0.0f);
        }
        if (timer >= dayNightDuration)
        {
            timer = 0.0f;
        }
    }

    void HandleMovement()
    {
        float moveH = 0f;
        float moveV = 0f;

        if (isMobile)
        {
            if (mobileJoystick != null)
            {
                moveH = mobileJoystick.Horizontal;
                moveV = mobileJoystick.Vertical;
            }
            currentSpeed = isMobileRunning ? runSpeed : walkSpeed;
        }
        else
        {
            moveH = Input.GetAxis("Horizontal");
            moveV = Input.GetAxis("Vertical");

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (Time.time - lastWPressTime < doubleTapThreshold)
                {
                    currentSpeed = runSpeed;
                }
                lastWPressTime = Time.time;
            }
            if (Input.GetKeyUp(KeyCode.W) || moveV <= 0)
            {
                currentSpeed = walkSpeed;
            }
        }

        Vector3 movement = new Vector3(moveH, 0, moveV) * currentSpeed * Time.deltaTime;
        transform.Translate(movement);
    }

    void HandlePCInputActions()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleFlashlight();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            InteractWithObject();
        }
    }

    public void ToggleFlashlight()
    {
        isFlashlightOn = !isFlashlightOn;
        if (flashlight != null)
        {
            flashlight.SetActive(isFlashlightOn);
        }
    }

    public void InteractWithObject()
    {
        if (heldObject == null)
        {
            TryPickUpObject();
        }
        else
        {
            DropObject();
        }
    }

    public void SetMobileRunTrue()
    {
        isMobileRunning = true;
    }

    public void SetMobileRunFalse()
    {
        isMobileRunning = false;
    }

    void SetDifficultySettings()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                chaserSpeed = 3.0f;
                break;
            case Difficulty.Medium:
                chaserSpeed = 5.0f;
                break;
            case Difficulty.Hard:
                chaserSpeed = 7.0f;
                break;
        }
    }

    void TryPickUpObject()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, 3.0f))
        {
            if (hit.collider.CompareTag("Pickable"))
            {
                heldObject = hit.collider.gameObject;
                
                Rigidbody rb = heldObject.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }
                
                heldObject.transform.SetParent(holdPosition);
                heldObject.transform.localPosition = Vector3.zero;
                heldObject.transform.localRotation = Quaternion.identity;
            }
        }
    }

    void DropObject()
    {
        if (heldObject != null)
        {
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            heldObject.transform.SetParent(null);
            
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.AddForce(transform.forward * 2.0f, ForceMode.Impulse); 
            }
            
            heldObject = null;
        }
    }
}
