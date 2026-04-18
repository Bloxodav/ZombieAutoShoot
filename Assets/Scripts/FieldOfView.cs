using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FieldOfView : MonoBehaviour
{
    [Header("Settings")]
    public float viewRadius = 5f;
    [Range(0, 360)] public float viewAngle = 90f;

    [Header("Layers")]
    public LayerMask obstacleMask;
    public LayerMask groundMask;

    [Header("Quality")]
    public int meshResolution = 30;
    public float heightOffset = 0.2f;

    private Mesh viewMesh;
    private MeshFilter _meshFilter; 

    private readonly List<Vector3> _viewPoints = new List<Vector3>(64);

    private Vector3[] _vertices;
    private int[] _triangles;
    private int _lastStepCount = -1;

    void Start()
    {
        viewMesh = new Mesh();
        viewMesh.name = "View Mesh";
        _meshFilter = GetComponent<MeshFilter>();
        _meshFilter.mesh = viewMesh;
    }

    void LateUpdate()
    {
        DrawFieldOfView();
    }

    void DrawFieldOfView()
    {
        int stepCount = Mathf.RoundToInt(viewAngle * meshResolution / 100);
        float stepAngleSize = viewAngle / stepCount;

        _viewPoints.Clear();

        for (int i = 0; i <= stepCount; i++)
        {
            float angle = transform.eulerAngles.y - viewAngle / 2 + stepAngleSize * i;
            ViewCastInfo newViewCast = ViewCast(angle);
            _viewPoints.Add(SnapToGround(newViewCast.point));
        }

        int vertexCount = _viewPoints.Count + 1;

        if (stepCount != _lastStepCount)
        {
            _vertices = new Vector3[vertexCount];
            _triangles = new int[(vertexCount - 2) * 3];
            _lastStepCount = stepCount;
        }

        _vertices[0] = transform.InverseTransformPoint(SnapToGround(transform.position));

        for (int i = 0; i < vertexCount - 1; i++)
        {
            _vertices[i + 1] = transform.InverseTransformPoint(_viewPoints[i]);

            if (i < vertexCount - 2)
            {
                _triangles[i * 3] = 0;
                _triangles[i * 3 + 1] = i + 1;
                _triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();
        viewMesh.vertices = _vertices;
        viewMesh.triangles = _triangles;
        viewMesh.RecalculateNormals();
    }

    Vector3 SnapToGround(Vector3 point)
    {
        Vector3 rayStart = point + Vector3.up * 5f;
        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 10f, groundMask))
            return hit.point + Vector3.up * heightOffset;
        return point;
    }

    ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, viewRadius, obstacleMask))
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        return new ViewCastInfo(false, transform.position + dir * viewRadius, viewRadius, globalAngle);
    }

    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal) angleInDegrees += transform.eulerAngles.y;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float dst;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _dst, float _angle)
        {
            hit = _hit;
            point = _point;
            dst = _dst;
            angle = _angle;
        }
    }
}