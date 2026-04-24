using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Camera")]
    [SerializeField] private Camera mainCamera;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationDampTime = 0.15f;

    [Header("Cursor")]
    public Texture2D cursorTexture;
    public Vector2 cursorHotspot = Vector2.zero;

    public PlayerProgressSO progress;
    public CharacterDataSO characterData;

    private Rigidbody _rb;
    private PlayerCombat _combat;
    private float _baseSpeed;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _combat = GetComponent<PlayerCombat>();

        if (!mainCamera) mainCamera = Camera.main;
        if (!animator) animator = GetComponentInChildren<Animator>();

        _baseSpeed = characterData.moveSpeed;
        moveSpeed = characterData.moveSpeed + progress.speedLevel * 0.5f;

        // Курсор не вылазит за рамки
        Cursor.lockState = CursorLockMode.Confined;
        if (cursorTexture != null)
            Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }

    private void Update()
    {
        RotateTowardsCursor();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void RotateTowardsCursor()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float dist)) return;

        Vector3 worldPoint = ray.GetPoint(dist);
        Vector3 dir = worldPoint - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            _rb.MoveRotation(Quaternion.Slerp(_rb.rotation, targetRot, 20f * Time.deltaTime));
        }
    }

    private void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, 0f, v);
        if (move.sqrMagnitude > 1f) move.Normalize();

        bool isMoving = move.sqrMagnitude >= 0.01f;
        _rb.velocity = new Vector3(move.x * moveSpeed, _rb.velocity.y, move.z * moveSpeed);

        animator.SetBool("isMoving", isMoving);

        float speedRatio = _baseSpeed > 0f ? moveSpeed / _baseSpeed : 1f;
        animator.SetFloat("LocomotionSpeed", speedRatio);

        float dt = Time.fixedDeltaTime;
        Vector3 localMove = transform.InverseTransformDirection(move);
        animator.SetFloat("MoveX", localMove.x, animationDampTime, dt);
        animator.SetFloat("MoveY", localMove.z, animationDampTime, dt);
    }
}