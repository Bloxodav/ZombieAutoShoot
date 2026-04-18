using UnityEngine;
using UnityEngine.Animations.Rigging;

public class PlayerAimController : MonoBehaviour
{
    public Transform aimTarget;
    public MultiAimConstraint aimConstraint;

    public float smoothSpeed = 8f;
    public float weightSpeed = 6f;
    public float heightOffset = 1.8f;

    private Transform target;
    private float weight;

    private void Update()
    {
        if (target)
        {
            Vector3 pos = target.position + Vector3.up * heightOffset;
            aimTarget.position = Vector3.Lerp(
                aimTarget.position,
                pos,
                Time.deltaTime * smoothSpeed
            );

            weight = Mathf.MoveTowards(weight, 1f, Time.deltaTime * weightSpeed);
        }
        else
        {
            weight = Mathf.MoveTowards(weight, 0f, Time.deltaTime * weightSpeed);
        }

        aimConstraint.weight = weight;
    }

    public void SetTarget(Transform t) => target = t;
    public void ClearTarget() => target = null;
}
