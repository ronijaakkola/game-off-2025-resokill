using UnityEngine;
using UnityEngine.Rendering;
using Game.Encounter;
using deVoid.Utils;
using VolFx;

[RequireComponent(typeof(Volume))]
public class DitherScaleShaker : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of the scale animation in seconds")]
    [SerializeField] private float animationDuration = 1.0f;

    [Tooltip("How long to hold at zero value before animating back")]
    [SerializeField] private float holdDuration = 0.5f;

    [Tooltip("Animation curve for scale to zero")]
    [SerializeField] private AnimationCurve scaleDownCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    [Tooltip("Animation curve for scale back to original")]
    [SerializeField] private AnimationCurve scaleUpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Volume _volume;
    private DitherVol _ditherVol;
    private float _originalScale;
    private Coroutine _currentAnimation;

    void Awake()
    {
        // Get Volume component
        _volume = GetComponent<Volume>();

        // Get DitherVol from the volume profile
        if (_volume.profile.TryGet(out _ditherVol))
        {
            _originalScale = _ditherVol.m_Scale.value;
        }
        else
        {
            Debug.LogError("DitherVol not found in Volume profile!", this);
            enabled = false;
            return;
        }

        // Subscribe to Encounter_Start signal
        Signals.Get<Encounter_Start>().AddListener(ReactToEncounterStart);
    }

    void OnDestroy()
    {
        // Unsubscribe from Encounter_Start signal
        Signals.Get<Encounter_Start>().RemoveListener(ReactToEncounterStart);
    }

    private void ReactToEncounterStart(int encounterId)
    {
        if (encounterId == 1) return;

        // Stop any existing animation
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        // Start the scale shake animation
        _currentAnimation = StartCoroutine(AnimateScaleShake());
    }

    private System.Collections.IEnumerator AnimateScaleShake()
    {
        float halfDuration = animationDuration / 2f;
        float elapsed = 0f;

        // Phase 1: Animate scale from original to 0
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float curveValue = scaleDownCurve.Evaluate(t);
            _ditherVol.m_Scale.value = _originalScale * curveValue;
            yield return null;
        }

        // Ensure we hit exactly 0
        _ditherVol.m_Scale.value = 0f;

        // Hold at zero
        yield return new WaitForSeconds(holdDuration);

        elapsed = 0f;

        // Phase 2: Animate scale from 0 back to original
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            float curveValue = scaleUpCurve.Evaluate(t);
            _ditherVol.m_Scale.value = _originalScale * curveValue;
            yield return null;
        }

        // Ensure we hit exactly the original value
        _ditherVol.m_Scale.value = _originalScale;

        _currentAnimation = null;
    }
}
