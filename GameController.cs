using UnityEngine;

public class GameController : MonoBehaviour
{
    [Header("Platform Durumu")]
    public bool isMobile;

    [Header("Gece Gunduz Dongusu")]
    public Light directionalLight;
    public float dayNightDuration = 360.0f;
    private float dayNightTimer;

    [Header("Karakter Kontrolleri")]
    public float walkSpeed = 5.0f;
    public float runSpeed = 9.0f;
    private const float doubleTapThreshold = 0.3f;
    private float lastForwardTapTime = -1.0f;
    private bool doubleTapRunning;

    [Header("Mobil UI Girdileri")]
    public GameObject mobileUIControls;
    public float touchJoystickRadius = 140.0f;
    private int movementFingerId = -1;
    private Vector2 movementStartPosition;
    private bool isMobileRunning;

    [Header("El Feneri ve Etkilesim")]
    public GameObject flashlight;
    public Transform holdPosition;
    public float interactionDistance = 3.0f;
    public float throwForce = 2.0f;
    private bool isFlashlightOn;
    private GameObject heldObject;

    [Header("Takipci Ayarlari")]
    public Transform chaser;
    private float chaserSpeed;

    [Header("Zorluk ve Dil Ayarlari")]
    public enum Difficulty { Easy, Medium, Hard }
    public Difficulty currentDifficulty = Difficulty.Medium;
    public enum Language { Turkish, English }
    public Language currentLanguage = Language.Turkish;

    private void Start()
    {
        DetectPlatform();
        SetDifficultySettings();
        SetFlashlightState(false);
    }

    private void DetectPlatform()
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

    private void Update()
    {
        HandleDayNightCycle();
        HandleMovement();
        HandlePCInputActions();
        HandleChaser();
    }

    private void HandleDayNightCycle()
    {
        if (dayNightDuration <= 0.0f || directionalLight == null)
        {
            return;
        }

        dayNightTimer = Mathf.Repeat(dayNightTimer + Time.deltaTime, dayNightDuration);
        float angle = dayNightTimer / dayNightDuration * 360.0f;
        directionalLight.transform.rotation = Quaternion.Euler(angle - 90.0f, 170.0f, 0.0f);
    }

    private void HandleMovement()
    {
        Vector2 input = isMobile ? ReadTouchMovement() : ReadKeyboardMovement();
        float speed = isMobile && isMobileRunning || !isMobile && doubleTapRunning ? runSpeed : walkSpeed;
        Vector3 movement = new Vector3(input.x, 0.0f, input.y);
        transform.Translate(movement * speed * Time.deltaTime, Space.Self);
    }

    private Vector2 ReadKeyboardMovement()
    {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (input.sqrMagnitude > 1.0f)
        {
            input.Normalize();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            doubleTapRunning = Time.time - lastForwardTapTime <= doubleTapThreshold;
            lastForwardTapTime = Time.time;
        }

        if (!Input.GetKey(KeyCode.W) || input.y <= 0.0f)
        {
            doubleTapRunning = false;
        }

        return input;
    }

    private Vector2 ReadTouchMovement()
    {
        Vector2 input = Vector2.zero;

        for (int index = 0; index < Input.touchCount; index++)
        {
            Touch touch = Input.GetTouch(index);
            if (movementFingerId < 0 && touch.phase == TouchPhase.Began && touch.position.x < Screen.width * 0.5f)
            {
                movementFingerId = touch.fingerId;
                movementStartPosition = touch.position;
            }

            if (touch.fingerId != movementFingerId)
            {
                continue;
            }

            if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
            {
                movementFingerId = -1;
                continue;
            }

            Vector2 delta = touch.position - movementStartPosition;
            input = Vector2.ClampMagnitude(delta / Mathf.Max(1.0f, touchJoystickRadius), 1.0f);
        }

        return input;
    }

    private void HandlePCInputActions()
    {
        if (isMobile)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.L))
        {
            ToggleFlashlight();
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            InteractWithObject();
        }
    }

    private void HandleChaser()
    {
        if (chaser == null || chaserSpeed <= 0.0f)
        {
            return;
        }

        Vector3 targetPosition = transform.position;
        targetPosition.y = chaser.position.y;
        chaser.position = Vector3.MoveTowards(chaser.position, targetPosition, chaserSpeed * Time.deltaTime);
    }

    public void ToggleFlashlight()
    {
        SetFlashlightState(!isFlashlightOn);
    }

    private void SetFlashlightState(bool enabledState)
    {
        isFlashlightOn = enabledState;
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

    private void SetDifficultySettings()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                chaserSpeed = 3.0f;
                break;
            case Difficulty.Hard:
                chaserSpeed = 7.0f;
                break;
            default:
                chaserSpeed = 5.0f;
                break;
        }
    }

    private void TryPickUpObject()
    {
        if (holdPosition == null || interactionDistance <= 0.0f)
        {
            return;
        }

        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.forward, out hit, interactionDistance) || !hit.collider.CompareTag("Pickable"))
        {
            return;
        }

        heldObject = hit.collider.gameObject;
        Rigidbody body = heldObject.GetComponent<Rigidbody>();
        if (body != null)
        {
            body.isKinematic = true;
            body.velocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
        }

        heldObject.transform.SetParent(holdPosition);
        heldObject.transform.localPosition = Vector3.zero;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    private void DropObject()
    {
        if (heldObject == null)
        {
            return;
        }

        Rigidbody body = heldObject.GetComponent<Rigidbody>();
        heldObject.transform.SetParent(null);
        if (body != null)
        {
            body.isKinematic = false;
            body.AddForce(transform.forward * throwForce, ForceMode.Impulse);
        }

        heldObject = null;
    }
}
