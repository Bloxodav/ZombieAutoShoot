using UnityEngine;

[ExecuteInEditMode]
public class SimpleAim : MonoBehaviour
{
    public float innerRadius = 2f;
    public float outerRadius = 4f;
    [Range(20, 100)] public int segments = 50;
    public LayerMask obstacleMask;
    public Color indicatorColor = new Color(1, 0.9f, 0, 0.5f);

    [Tooltip("Как часто обновлять raycast (секунды). 0.05 = 20 раз/сек — достаточно")]
    public float raycastInterval = 0.05f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;
    private Mesh _mesh;
    private Material _material;

    private float[] _cachedOuterRadii;
    private float _raycastTimer;

    void OnEnable()
    {
        _meshFilter = GetComponent<MeshFilter>();
        if (_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();

        _meshRenderer = GetComponent<MeshRenderer>();
        if (_meshRenderer == null) _meshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (_material == null)
        {
            _material = new Material(Shader.Find("Sprites/Default"));
            _meshRenderer.material = _material;
        }

        _mesh = new Mesh();
        _meshFilter.mesh = _mesh;
        _cachedOuterRadii = new float[segments + 1];

        for (int i = 0; i <= segments; i++)
            _cachedOuterRadii[i] = outerRadius;
    }

    void OnDisable()
    {
        if (_material != null)
        {
            DestroyImmediate(_material);
            _material = null;
        }
    }

    void LateUpdate()
    {
        _raycastTimer -= Time.deltaTime;
        if (_raycastTimer <= 0f)
        {
            _raycastTimer = raycastInterval;
            UpdateRaycasts();
        }

        UpdateMesh();

        if (_material != null && _material.color != indicatorColor)
            _material.color = indicatorColor;
    }

    private void UpdateRaycasts()
    {
        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

            float currentOuter = outerRadius;
            if (obstacleMask != 0)
            {
                if (Physics.Raycast(transform.position + Vector3.up * 0.5f, dir, out RaycastHit hit, outerRadius, obstacleMask))
                    currentOuter = hit.distance;
            }
            _cachedOuterRadii[i] = currentOuter;
        }
    }

    void UpdateMesh()
    {
        Vector3[] vertices = new Vector3[(segments + 1) * 2];
        int[] triangles = new int[segments * 6];

        for (int i = 0; i <= segments; i++)
        {
            float angle = (float)i / segments * 360f * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

            vertices[i * 2] = dir * innerRadius;
            vertices[i * 2 + 1] = dir * Mathf.Max(innerRadius, _cachedOuterRadii[i]);

            if (i < segments)
            {
                int baseIdx = i * 2;
                int triIdx = i * 6;
                triangles[triIdx] = baseIdx;
                triangles[triIdx + 1] = baseIdx + 1;
                triangles[triIdx + 2] = baseIdx + 2;
                triangles[triIdx + 3] = baseIdx + 1;
                triangles[triIdx + 4] = baseIdx + 3;
                triangles[triIdx + 5] = baseIdx + 2;
            }
        }

        _mesh.Clear();
        _mesh.vertices = vertices;
        _mesh.triangles = triangles;
        _mesh.RecalculateBounds();
    }
}