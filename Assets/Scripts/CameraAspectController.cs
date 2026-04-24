using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraAspectController : MonoBehaviour
{
    [Header("Reference Aspect Ratio (Portrait)")]
    public Vector2 referenceAspectRatio = new Vector2(9, 16);

    private Camera cam;
    private float initialOrthoSize;
    private float initialFov;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        initialOrthoSize = cam.orthographicSize;
        initialFov = cam.fieldOfView;

        AdjustCamera();
    }

    private void OnEnable()
    {
        AdjustCamera();
    }

    private void Update()
    {
        AdjustCamera();
    }

    public void AdjustCamera()
    {
        float targetRatio = referenceAspectRatio.x / referenceAspectRatio.y;
        float currentRatio = (float)Screen.width / Screen.height;

        if (currentRatio < targetRatio)
        {
            float scale = targetRatio / currentRatio;

            if (cam.orthographic)
            {
                cam.orthographicSize = initialOrthoSize * scale;
            }
            else
            {
                float vFovRad = initialFov * Mathf.Deg2Rad;
                float hFovRad = 2f * Mathf.Atan(Mathf.Tan(vFovRad / 2f) * targetRatio);
                cam.fieldOfView =
                    2f * Mathf.Atan(Mathf.Tan(hFovRad / 2f) / currentRatio) * Mathf.Rad2Deg;
            }
        }
        else
        {
            cam.orthographicSize = initialOrthoSize;
            cam.fieldOfView = initialFov;
        }
    }
}
