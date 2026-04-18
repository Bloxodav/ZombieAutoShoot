using System.Collections.Generic;
using UnityEngine;

public class CameraObstructionHandler : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private Material transparentNoCullTemplate;
    [Range(0f, 1f)] public float targetAlpha = 0.35f;
    public float fadeSpeed = 6f;

    private class RenderState
    {
        public Renderer renderer;
        public Material[] originalMaterials;
        public Material[] workingMaterials;
        public Color[] originalColors;
    }

    private readonly Dictionary<Renderer, RenderState> active = new Dictionary<Renderer, RenderState>();

    private readonly HashSet<Renderer> _currentHitRenderers = new HashSet<Renderer>();
    private readonly List<Renderer> _keys = new List<Renderer>(16);
    private RaycastHit[] _hitBuffer = new RaycastHit[16];

    void LateUpdate()
    {
        if (player == null || transparentNoCullTemplate == null) return;

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;

        int hitCount = Physics.RaycastNonAlloc(
            new Ray(transform.position, dir.normalized),
            _hitBuffer, dist, obstacleMask);

        _currentHitRenderers.Clear();

        for (int h = 0; h < hitCount; h++)
        {
            var hit = _hitBuffer[h];
            if (hit.collider == null) continue;

            Renderer r = hit.collider.GetComponent<Renderer>();
            if (r == null) r = hit.collider.GetComponentInParent<Renderer>();
            if (r == null) continue;

            _currentHitRenderers.Add(r);

            if (!active.ContainsKey(r))
            {
                Material[] orig = r.sharedMaterials;
                Material[] work = new Material[orig.Length];
                Color[] origColors = new Color[orig.Length];

                for (int i = 0; i < orig.Length; i++)
                {
                    Material m = orig[i];
                    Material inst = new Material(transparentNoCullTemplate);

                    if (m != null)
                    {
                        if (m.HasProperty("_MainTex") && inst.HasProperty("_MainTex"))
                        {
                            inst.SetTexture("_MainTex", m.GetTexture("_MainTex"));
                            inst.mainTextureScale = m.mainTextureScale;
                            inst.mainTextureOffset = m.mainTextureOffset;
                        }
                        if (m.HasProperty("_Color") && inst.HasProperty("_Color"))
                            inst.SetColor("_Color", m.GetColor("_Color"));
                        if (m.HasProperty("_BumpMap") && inst.HasProperty("_BumpMap"))
                        {
                            Texture bump = m.GetTexture("_BumpMap");
                            if (bump != null) { inst.SetTexture("_BumpMap", bump); inst.EnableKeyword("_NORMALMAP"); }
                        }
                        if (m.HasProperty("_Metallic") && inst.HasProperty("_Metallic"))
                            inst.SetFloat("_Metallic", m.GetFloat("_Metallic"));
                        if (m.HasProperty("_Glossiness") && inst.HasProperty("_Glossiness"))
                            inst.SetFloat("_Glossiness", m.GetFloat("_Glossiness"));
                        if (m.IsKeywordEnabled("_EMISSION") && inst.HasProperty("_EmissionColor"))
                        {
                            inst.SetColor("_EmissionColor", m.GetColor("_EmissionColor"));
                            inst.EnableKeyword("_EMISSION");
                        }
                    }

                    Color instColor = inst.HasProperty("_Color") ? inst.GetColor("_Color") : Color.white;
                    instColor.a = (m != null && m.HasProperty("_Color")) ? m.GetColor("_Color").a : 1f;
                    if (inst.HasProperty("_Color")) inst.SetColor("_Color", instColor);
                    inst.renderQueue = 3000;

                    work[i] = inst;
                    origColors[i] = (m != null && m.HasProperty("_Color")) ? m.GetColor("_Color") : Color.white;
                }

                r.materials = work;
                active.Add(r, new RenderState
                {
                    renderer = r,
                    originalMaterials = orig,
                    workingMaterials = work,
                    originalColors = origColors
                });
            }
        }

        _keys.Clear();
        _keys.AddRange(active.Keys);

        foreach (var r in _keys)
        {
            var state = active[r];
            if (_currentHitRenderers.Contains(r))
            {
                FadeTo(state.workingMaterials, state.originalColors, targetAlpha, fadeSpeed);
            }
            else
            {
                bool finished = FadeToOriginal(state);
                if (finished)
                {
                    if (state.renderer != null)
                        state.renderer.sharedMaterials = state.originalMaterials;
                    for (int i = 0; i < state.workingMaterials.Length; i++)
                        if (state.workingMaterials[i] != null)
                            Destroy(state.workingMaterials[i]);
                    active.Remove(r);
                }
            }
        }
    }

    private bool FadeTo(Material[] workingMats, Color[] originalColors, float target, float speed)
    {
        bool allReached = true;
        for (int i = 0; i < workingMats.Length; i++)
        {
            var mat = workingMats[i];
            if (mat == null || !mat.HasProperty("_Color")) continue;
            Color c = mat.GetColor("_Color");
            float origA = (originalColors != null && i < originalColors.Length) ? originalColors[i].a : 1f;
            float desired = Mathf.Clamp(target, 0f, origA);
            float newA = Mathf.MoveTowards(c.a, desired, speed * Time.deltaTime);
            c.a = newA;
            mat.SetColor("_Color", c);
            if (Mathf.Abs(newA - desired) > 0.01f) allReached = false;
        }
        return allReached;
    }

    private bool FadeToOriginal(RenderState state)
    {
        bool allReached = true;
        for (int i = 0; i < state.workingMaterials.Length; i++)
        {
            var mat = state.workingMaterials[i];
            if (mat == null || !mat.HasProperty("_Color")) continue;
            float origA = (state.originalColors != null && i < state.originalColors.Length) ? state.originalColors[i].a : 1f;
            Color c = mat.GetColor("_Color");
            float newA = Mathf.MoveTowards(c.a, origA, fadeSpeed * Time.deltaTime);
            c.a = newA;
            mat.SetColor("_Color", c);
            if (Mathf.Abs(newA - origA) > 0.01f) allReached = false;
        }
        return allReached;
    }
}