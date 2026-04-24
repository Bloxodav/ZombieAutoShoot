using UnityEngine.Animations.Rigging;
using UnityEngine;

public class PlayerAimController : MonoBehaviour
{
    public Transform aimTarget;
    public MultiAimConstraint aimConstraint;

    public float smoothSpeed = 8f;
    public float weightSpeed = 6f;
    public float heightOffset = 1.8f;

    private Vector3 _desiredPosition;
    private float _weight;

    private void Start()
    {
        if (aimTarget != null)
            _desiredPosition = aimTarget.position;
    }

    private void Update()
    {
        // Плавно двигаем aimTarget к желаемой позиции (курсору)
        aimTarget.position = Vector3.Lerp(
            aimTarget.position,
            _desiredPosition,
            Time.deltaTime * smoothSpeed
        );

        // Всегда держим вес = 1 (всегда прицеливаемся)
        _weight = Mathf.MoveTowards(_weight, 1f, Time.deltaTime * weightSpeed);
        aimConstraint.weight = _weight;
    }

    public void SetAimPoint(Vector3 worldPoint)
    {
        if (aimTarget != null)
            _desiredPosition = worldPoint + Vector3.up * heightOffset;
    }

    // Оставь если используются где-то ещё, но они ничего не делают
    public void SetTarget(Transform t) { }
    public void ClearTarget() { }
}