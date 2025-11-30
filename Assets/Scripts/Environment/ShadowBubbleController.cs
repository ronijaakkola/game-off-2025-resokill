using UnityEngine;
using System.Collections;

namespace Game.Environment
{
    public class ShadowBubbleController : MonoBehaviour
    {
        [Header("Bubble Settings")]
        [SerializeField] private float maxMultiplier = 2f; // Max scale when hiding
        [SerializeField, Range(0f, 1f)] private float fadeDuringScalePercent = 0.3f; // How much alpha fades during scale
        [SerializeField] private float fadeAfterScaleDuration = 1f; // Time to finish fade after scaling

        private Material mat;
        private Vector3 initialScale;

        void Awake()
        {
            mat = GetComponent<MeshRenderer>().material;
            initialScale = transform.localScale;

            ShowBubble();
        }

        public void ShowBubble()
        {
            gameObject.SetActive(true);
            SetAlpha(1f);
            transform.localScale = initialScale;
        }

        public void HideBubble(float duration)
        {
            StartCoroutine(HideRoutine(duration));
        }

        private IEnumerator HideRoutine(float scaleDuration)
        {
            float elapsed = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 targetScale = initialScale * maxMultiplier;

            Color startColor = mat.color;
            Color targetColor = startColor;
            targetColor.a = startColor.a * (1f - fadeDuringScalePercent); // partial fade during scaling

            // --- Stage 1: Scale + partial fade ---
            while (elapsed < scaleDuration)
            {
                elapsed += Time.deltaTime;
                float tScale = Mathf.Clamp01(elapsed / scaleDuration);

                // Scale
                transform.localScale = Vector3.Lerp(startScale, targetScale, tScale);

                // Partial fade
                mat.color = Color.Lerp(startColor, targetColor, tScale);

                yield return null;
            }

            // --- Stage 2: Finish fade over fadeAfterScaleDuration ---
            elapsed = 0f;
            Color stage2StartColor = mat.color;
            Color stage2TargetColor = startColor;
            stage2TargetColor.a = 0f;

            while (elapsed < fadeAfterScaleDuration)
            {
                elapsed += Time.deltaTime;
                float tFade = Mathf.Clamp01(elapsed / fadeAfterScaleDuration);
                mat.color = Color.Lerp(stage2StartColor, stage2TargetColor, tFade);
                yield return null;
            }

            // Reset scale and alpha, disable
            transform.localScale = initialScale;
            SetAlpha(1f);
            gameObject.SetActive(false);
        }

        private void SetAlpha(float a)
        {
            Color c = mat.color;
            c.a = a;
            mat.color = c;
        }
    }
}
