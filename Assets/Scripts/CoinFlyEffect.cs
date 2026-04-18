using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class CoinFlyEffect : MonoBehaviour
{
    private Coroutine _flyCoroutine;

    public void Play(Vector3 startWorldPos, RectTransform targetUI, Canvas canvas,
                     float flyDuration, float scatterRadius, float delay,
                     System.Action onComplete)
    {
        if (_flyCoroutine != null) StopCoroutine(_flyCoroutine);
        _flyCoroutine = StartCoroutine(FlyRoutine(startWorldPos, targetUI, canvas,
                                                   flyDuration, scatterRadius, delay, onComplete));
    }

    private IEnumerator FlyRoutine(Vector3 startWorldPos, RectTransform targetUI,
                                   Canvas canvas, float flyDuration, float scatterRadius,
                                   float delay, System.Action onComplete)
    {
        var rt = GetComponent<RectTransform>();
        var canvasRect = canvas.GetComponent<RectTransform>();

        Vector2 startPos = WorldToCanvasLocal(startWorldPos, canvas, canvasRect);
        rt.anchoredPosition = startPos;
        rt.localScale = Vector3.one;

        if (delay > 0f) yield return new WaitForSeconds(delay);

        Vector2 scatter = Random.insideUnitCircle.normalized * scatterRadius
                                + Random.insideUnitCircle * (scatterRadius * 0.3f);
        Vector2 scatterTarget = startPos + scatter;
        float scatterDur = flyDuration * 0.3f;

        yield return TweenPos(rt, startPos, scatterTarget, scatterDur, EaseOut);

        Vector2 flyStart = rt.anchoredPosition;
        float mainDur = flyDuration * 0.7f;
        float elapsed = 0f;

        while (elapsed < mainDur)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / mainDur);

            Vector2 targetPos = TargetAnchoredPos(targetUI, canvasRect);
            rt.anchoredPosition = Vector2.Lerp(flyStart, targetPos, EaseIn(t));
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.25f, EaseIn(t));

            yield return null;
        }

        _flyCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator TweenPos(RectTransform rt, Vector2 from, Vector2 to,
                                 float duration, System.Func<float, float> ease)
    {
        float e = 0f;
        while (e < duration)
        {
            e += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(from, to, ease(Mathf.Clamp01(e / duration)));
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    private Vector2 WorldToCanvasLocal(Vector3 worldPos, Canvas canvas, RectTransform canvasRect)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? Camera.main : canvas.worldCamera;

        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, worldPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect, screenPoint, cam, out Vector2 local);
        return local;
    }

    private Vector2 TargetAnchoredPos(RectTransform target, RectTransform canvasRect)
    {
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);
        return canvasRect.InverseTransformPoint((corners[0] + corners[2]) * 0.5f);
    }

    private float EaseOut(float t) => 1f - (1f - t) * (1f - t);
    private float EaseIn(float t) => t * t * t;
}