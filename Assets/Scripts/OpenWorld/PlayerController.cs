using UnityEngine;

/// <summary>
/// Third person character controller for open world
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float MoveSpeed = 5f;
    public float SprintSpeed = 8f;
    public float RotationSpeed = 10f;
    public float Gravity = -9.81f;

    [Header("Third Person Camera")]
    public float CamDistance = 6f;
    public float CamHeight = 3f;
    public float CamSmoothSpeed = 5f;
    public float MouseSensitivity = 3f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private Camera _owCam;
    private float _camYaw;
    private float _camPitch = 15f;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.OpenWorld)
            return;
        HandleMovement();
        HandleCamera();
        HandleInteraction();
    }

    void LateUpdate()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.OpenWorld)
            return;
        UpdateCameraPosition();
    }

    void HandleMovement()
    {
        if (_controller.isGrounded && _velocity.y < 0) _velocity.y = -2f;

        // Fall protection - if fell too far, reset to spawn
        if (transform.localPosition.y < -10f)
        {
            _controller.enabled = false;
            transform.localPosition = new Vector3(0, 1.5f, 0);
            _controller.enabled = true;
            _velocity = Vector3.zero;
            return;
        }

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;

        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg + _camYaw;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, RotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            float speed = Input.GetKey(KeyCode.LeftShift) ? SprintSpeed : MoveSpeed;
            _controller.Move(moveDir * speed * Time.deltaTime);
        }

        _velocity.y += Gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }

    void HandleCamera()
    {
        // Right mouse button to rotate camera
        if (Input.GetMouseButton(1))
        {
            _camYaw += Input.GetAxis("Mouse X") * MouseSensitivity;
            _camPitch -= Input.GetAxis("Mouse Y") * MouseSensitivity;
            _camPitch = Mathf.Clamp(_camPitch, 5f, 60f);
        }

        // Scroll to zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            CamDistance -= scroll * 3f;
            CamDistance = Mathf.Clamp(CamDistance, 2f, 15f);
        }
    }

    void UpdateCameraPosition()
    {
        if (_owCam == null)
        {
            // Find OW_Camera
            var camObj = GameObject.Find("OW_Camera");
            if (camObj != null) _owCam = camObj.GetComponent<Camera>();
            if (_owCam == null) return;
        }

        // Calculate desired position behind player
        Quaternion rotation = Quaternion.Euler(_camPitch, _camYaw, 0);
        Vector3 offset = rotation * new Vector3(0, 0, -CamDistance);
        Vector3 targetPos = transform.position + Vector3.up * CamHeight + offset;

        _owCam.transform.position = Vector3.Lerp(_owCam.transform.position, targetPos, CamSmoothSpeed * Time.deltaTime);
        _owCam.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(transform.position + Vector3.up, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 3f))
            {
                var interactable = hit.collider.GetComponent<IInteractable>();
                interactable?.Interact();
            }
        }
    }

    public void ExitWorld()
    {
        int karmaTotal = 0;
        if (KarmaTracker.Instance != null)
            karmaTotal = KarmaTracker.Instance.SettleAndReset();

        var result = new GridWorldResult
        {
            GoldEarned = 100,
            KarmaChange = karmaTotal,
            IsDead = false
        };
        GameManager.Instance.ExitGridWorld(result);
    }
}