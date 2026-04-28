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

    private readonly Dictionary<Renderer, RenderState> _active = new Dictionary<Renderer, RenderState>();
    private readonly HashSet<Renderer> _currentHitRenderers = new HashSet<Renderer>();
    private readonly List<Renderer> _toRemove = new List<Renderer>(16);
    private RaycastHit[] _hitBuffer = new RaycastHit[16];

    private void LateUpdate()
    {
        if (player == null || transparentNoCullTemplate == null) return;

        Vector3 dir = player.position - transform.position;
        float dist = dir.magnitude;

        int hitCount = Physics.RaycastNonAlloc(
            new Ray(transform.position, dir / dist),
            _hitBuffer, dist, obstacleMask);

        _currentHitRenderers.Clear();

        for (int h = 0; h < hitCount; h++)
        {
            if (_hitBuffer[h].collider == null) continue;

            Renderer r = _hitBuffer[h].collider.GetComponent<Renderer>()
                      ?? _hitBuffer[h].collider.GetComponentInParent<Renderer>();
            if (r == null) continue;

            _currentHitRenderers.Add(r);

            if (_active.ContainsKey(r)) continue;

            Material[] orig = r.sharedMaterials;
            Material[] work = new Material[orig.Length];
            Color[] origColors = new Color[orig.Length];

            for (int i = 0; i < orig.Length; i++)
            {
                Material m = orig[i];
                Material inst = new Material(transparentNoCullTemplate);

                if (m != null)
                {
                    CopyProperty(m, inst, "_MainTex");
                    CopyProperty(m, inst, "_Color");
                    CopyProperty(m, inst, "_BumpMap");
                    CopyFloatProperty(m, inst, "_Metallic");
                    CopyFloatProperty(m, inst, "_Glossiness");

                    if (m.IsKeywordEnabled("_EMISSION") && inst.HasProperty("_EmissionColor"))
                    {
                        inst.SetColor("_EmissionColor", m.GetColor("_EmissionColor"));
                        inst.EnableKeyword("_EMISSION");
                    }
                }

                Color c = inst.HasProperty("_Color") ? inst.GetColor("_Color") : Color.white;
                c.a = (m != null && m.HasProperty("_Color")) ? m.GetColor("_Color").a : 1f;
                if (inst.HasProperty("_Color")) inst.SetColor("_Color", c);
                inst.renderQueue = 3000;

                work[i] = inst;
                origColors[i] = (m != null && m.HasProperty("_Color")) ? m.GetColor("_Color") : Color.white;
            }

            r.materials = work;
            _active[r] = new RenderState
            {
                renderer = r,
                originalMaterials = orig,
                workingMaterials = work,
                originalColors = origColors
            };
        }

        _toRemove.Clear();

        foreach (var kvp in _active)
        {
            Renderer r = kvp.Key;
            RenderState state = kvp.Value;

            if (_currentHitRenderers.Contains(r))
            {
                FadeTo(state.workingMaterials, state.originalColors, targetAlpha);
            }
            else
            {
                if (FadeToOriginal(state))
                    _toRemove.Add(r);
            }
        }

        for (int i = 0; i < _toRemove.Count; i++)
        {
            RenderState state = _active[_toRemove[i]];
            if (state.renderer != null)
                state.renderer.sharedMaterials = state.originalMaterials;
            for (int j = 0; j < state.workingMaterials.Length; j++)
                if (state.workingMaterials[j] != null)
                    Destroy(state.workingMaterials[j]);
            _active.Remove(_toRemove[i]);
        }
    }

    private void CopyProperty(Material src, Material dst, string prop)
    {
        if (!src.HasProperty(prop) || !dst.HasProperty(prop)) return;
        if (prop == "_MainTex")
        {
            dst.SetTexture(prop, src.GetTexture(prop));
            dst.mainTextureScale = src.mainTextureScale;
            dst.mainTextureOffset = src.mainTextureOffset;
        }
        else if (prop == "_BumpMap")
        {
            Texture bump = src.GetTexture(prop);
            if (bump != null) { dst.SetTexture(prop, bump); dst.EnableKeyword("_NORMALMAP"); }
        }
        else
        {
            dst.SetColor(prop, src.GetColor(prop));
        }
    }

    private void CopyFloatProperty(Material src, Material dst, string prop)
    {
        if (src.HasProperty(prop) && dst.HasProperty(prop))
            dst.SetFloat(prop, src.GetFloat(prop));
    }

    private void FadeTo(Material[] mats, Color[] origColors, float target)
    {
        for (int i = 0; i < mats.Length; i++)
        {
            if (mats[i] == null || !mats[i].HasProperty("_Color")) continue;
            Color c = mats[i].GetColor("_Color");
            float origA = i < origColors.Length ? origColors[i].a : 1f;
            c.a = Mathf.MoveTowards(c.a, Mathf.Clamp(target, 0f, origA), fadeSpeed * Time.deltaTime);
            mats[i].SetColor("_Color", c);
        }
    }

    private bool FadeToOriginal(RenderState state)
    {
        bool allReached = true;
        for (int i = 0; i < state.workingMaterials.Length; i++)
        {
            Material mat = state.workingMaterials[i];
            if (mat == null || !mat.HasProperty("_Color")) continue;
            float origA = i < state.originalColors.Length ? state.originalColors[i].a : 1f;
            Color c = mat.GetColor("_Color");
            c.a = Mathf.MoveTowards(c.a, origA, fadeSpeed * Time.deltaTime);
            mat.SetColor("_Color", c);
            if (Mathf.Abs(c.a - origA) > 0.01f) allReached = false;
        }
        return allReached;
    }
}