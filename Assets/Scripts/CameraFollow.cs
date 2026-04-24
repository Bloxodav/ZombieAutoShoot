using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform player;

    [Header("Follow")]
    public float smoothSpeed = 8f;
    public Vector3 offset = new Vector3(0f, 15f, -5f);

    [Header("RMB Pan (как в Project Zomboid)")]
    public float maxPanDistance = 6f;
    public float panSmoothSpeed = 10f;

    private Camera _cam;
    private Vector3 _panOffset;
    private bool _isPanning;

    private void Start()
    {
        _cam = GetComponent<Camera>();
        if (!_cam) _cam = Camera.main;
    }

    private void LateUpdate()
    {
        HandlePan();

        Vector3 target = player.position + offset + _panOffset;
        transform.position = Vector3.Lerp(transform.position, target, smoothSpeed * Time.deltaTime);
    }

    private void HandlePan()
    {
        _isPanning = Input.GetMouseButton(1); // RMB зажат

        if (_isPanning)
        {
            Ray ray = _cam.ScreenPointToRay(Input.mousePosition);
            Plane groundPlane = new Plane(Vector3.up, player.position);

            if (groundPlane.Raycast(ray, out float dist))
            {
                Vector3 cursorWorld = ray.GetPoint(dist);
                Vector3 dir = cursorWorld - player.position;
                dir.y = 0f;

                // Ограничиваем дистанцию панорамы
                if (dir.magnitude > maxPanDistance)
                    dir = dir.normalized * maxPanDistance;

                Vector3 targetPan = new Vector3(dir.x, 0f, dir.z);
                _panOffset = Vector3.Lerp(_panOffset, targetPan, panSmoothSpeed * Time.deltaTime);
            }
        }
        else
        {
            // Плавно возвращаемся к игроку
            _panOffset = Vector3.Lerp(_panOffset, Vector3.zero, panSmoothSpeed * Time.deltaTime);
        }
    }
}