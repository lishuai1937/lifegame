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

    [Header("Camera")]
    public Transform CameraTarget;

    private CharacterController _controller;
    private Vector3 _velocity;

    void Start()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameState.OpenWorld)
            return;
        HandleMovement();
        HandleInteraction();
    }

    void HandleMovement()
    {
        if (_controller.isGrounded && _velocity.y < 0) _velocity.y = -2f;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 dir = new Vector3(h, 0, v).normalized;

        if (dir.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            if (Camera.main != null) targetAngle += Camera.main.transform.eulerAngles.y;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, RotationSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            Vector3 moveDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            float speed = Input.GetKey(KeyCode.LeftShift) ? SprintSpeed : MoveSpeed;
            _controller.Move(moveDir * speed * Time.deltaTime);
        }

        _velocity.y += Gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Exit world - settle karma from actions taken
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
}