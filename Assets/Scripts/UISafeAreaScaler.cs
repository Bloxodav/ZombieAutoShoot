using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UISafeAreaScaler : MonoBehaviour
{
    [Header("Reference Aspect Ratio")]
    public Vector2 referenceAspect = new Vector2(9, 16);

    [Header("Offsets")]
    public float verticalOffset = 0f;
    public float horizontalOffset = 0f;

    private RectTransform rect;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    private void ApplySafeArea()
    {
        float targetRatio = referenceAspect.x / referenceAspect.y;
        float currentRatio = (float)Screen.width / Screen.height;

        Vector2 offset = Vector2.zero;

        if (currentRatio < targetRatio)
        {
            offset.y = verticalOffset;
        }
        else if (currentRatio > targetRatio)
        {
            offset.x = horizontalOffset;
        }

        rect.anchoredPosition += offset;
    }
}
