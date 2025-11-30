using UnityEngine;
using deVoid.Utils;
using Game.Audio;

public class SpectrumAnimation : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] float minScale = 0.5f;
    [SerializeField] float maxScale = 3.0f;

    [Header("Animation Settings")]
    [SerializeField] float growSpeed = 15f;
    [SerializeField] float shrinkSpeed = 8f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private float currentScaleY;
    private bool isAnimating = false;

    void Start()
    {
        originalScale = transform.localScale;
        currentScaleY = originalScale.y;
        targetScale = originalScale;

        // Subscribe to beat events
        Signals.Get<FMODBeatEvent>().AddListener(OnBeat);
    }

    void OnDestroy()
    {
        // Unsubscribe from beat events
        Signals.Get<FMODBeatEvent>().RemoveListener(OnBeat);
    }

    void OnBeat(FMODBeatData beatData)
    {
        // Generate random target scale between min and max
        float randomHeight = Random.Range(minScale, maxScale);
        targetScale = new Vector3(originalScale.x, randomHeight, originalScale.z);
        isAnimating = true;
    }

    void Update()
    {
        if (isAnimating)
        {
            // Animate towards target scale
            if (currentScaleY < targetScale.y)
            {
                // Growing phase - fast
                currentScaleY = Mathf.Lerp(currentScaleY, targetScale.y, Time.deltaTime * growSpeed);

                // Check if we've reached the peak
                if (Mathf.Abs(currentScaleY - targetScale.y) < 0.01f)
                {
                    currentScaleY = targetScale.y;
                    targetScale = originalScale; // Start shrinking back
                }
            }
            else
            {
                // Shrinking phase - slower
                currentScaleY = Mathf.Lerp(currentScaleY, originalScale.y, Time.deltaTime * shrinkSpeed);

                // Check if we've returned to original
                if (Mathf.Abs(currentScaleY - originalScale.y) < 0.01f)
                {
                    currentScaleY = originalScale.y;
                    isAnimating = false;
                }
            }

            // Apply the scale
            transform.localScale = new Vector3(originalScale.x, currentScaleY, originalScale.z);
        }
    }
}
