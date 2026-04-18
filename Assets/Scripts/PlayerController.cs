using UnityEngine;
using PinePie.SimpleJoystick;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private JoystickController joystick;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform cameraTransform;

    [Header("Animation")]
    [SerializeField] private Animator animator;
    [SerializeField] private float animationDampTime = 0.15f;

    [Header("Rotation")]
    [SerializeField] private float freeRoamRotationSpeed = 10f;

    private Rigidbody rb;
    private PlayerCombat combat;
    private FootstepController footsteps;

    public PlayerProgressSO progress;
    public CharacterDataSO characterData;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        combat = GetComponent<PlayerCombat>();
        footsteps = GetComponent<FootstepController>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;

        float bonusSpeed = progress.speedLevel * 0.5f;
        moveSpeed = characterData.moveSpeed + bonusSpeed;

        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    private void FixedUpdate()
    {
        Vector2 input = joystick.InputDirection;

        bool isMoving = input.sqrMagnitude >= 0.01f;
        animator.SetBool("isMoving", isMoving);

        Vector3 camForward = cameraTransform.forward; camForward.y = 0; camForward.Normalize();
        Vector3 camRight = cameraTransform.right; camRight.y = 0; camRight.Normalize();
        Vector3 move = camForward * input.y + camRight * input.x;
        if (move.sqrMagnitude > 0.01f) move.Normalize();

        rb.velocity = new Vector3(move.x * moveSpeed, rb.velocity.y, move.z * moveSpeed);

        if (move != Vector3.zero && !combat.HasTarget)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, freeRoamRotationSpeed * Time.fixedDeltaTime));
        }

        float dt = Time.fixedDeltaTime;

        if (combat.HasTarget)
        {
            Vector3 localMove = transform.InverseTransformDirection(move);
            animator.SetFloat("MoveX", localMove.x, animationDampTime, dt);
            animator.SetFloat("MoveY", localMove.z, animationDampTime, dt);
        }
        else
        {
            animator.SetFloat("MoveX", 0f, animationDampTime, dt);
            animator.SetFloat("MoveY", input.magnitude, animationDampTime, dt);
        }

        if (footsteps != null)
        {
            if (isMoving) footsteps.OnMoving(dt);
            else footsteps.OnStopped();
        }
    }
}