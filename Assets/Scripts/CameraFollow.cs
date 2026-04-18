using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform target;

    [Header("Offset (настрой под свой вид)")]
    public Vector3 offset = new Vector3(0, 21.6f, -9.6f);

    [Header("Smoothing")]
    [Tooltip("Время сглаживания. Меньше = жёстче. 0.05-0.12 оптимально для мобилок.")]
    public float smoothTime = 0.08f;

    [Tooltip("Если true — камера жёстко привязана (без сглаживания). Для отладки.")]
    public bool hardFollow = false;

    private Vector3 _velocity = Vector3.zero;
    private Quaternion _fixedRotation;

    private void Start()
    {
        if (target)
            transform.position = target.position + offset;

        _fixedRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (!target) return;

        Vector3 desired = target.position + offset;

        if (hardFollow)
        {
            transform.position = desired;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desired,
                ref _velocity,
                smoothTime
            );
        }

        transform.rotation = _fixedRotation;
    }
}