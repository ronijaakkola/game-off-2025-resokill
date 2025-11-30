using UnityEngine;
using Game.Encounter;
using deVoid.Utils;

public class FloorHider : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("Duration of the fade animation in seconds")]
    [SerializeField] private float fadeDuration = 1.0f;

    private Renderer _renderer;
    private Material _materialInstance;
    private Coroutine _currentAnimation;

    void Awake()
    {
        _renderer = GetComponent<Renderer>();

        if (_renderer == null)
        {
            Debug.LogError("Renderer component not found on FloorHider!", this);
            enabled = false;
            return;
        }

        _materialInstance = _renderer.material;

        Signals.Get<Encounter_Start>().AddListener(ReactToEncounterStart);
    }

    void OnDestroy()
    {
        Signals.Get<Encounter_Start>().RemoveListener(ReactToEncounterStart);

        // Clean up material instance
        if (_materialInstance != null)
        {
            Destroy(_materialInstance);
        }
    }

    private void ReactToEncounterStart(int encounterId)
    {
        // Stop any existing animation
        if (_currentAnimation != null)
        {
            StopCoroutine(_currentAnimation);
        }

        _currentAnimation = StartCoroutine(FadeAndDisable());
    }

    private System.Collections.IEnumerator FadeAndDisable()
    {
        float elapsed = 0f;
        float startAlpha = _materialInstance.GetFloat("_Alpha");

        // Animate Alpha property from current value to 0
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            float alpha = Mathf.Lerp(startAlpha, 0f, t);
            _materialInstance.SetFloat("_Alpha", alpha);

            yield return null;
        }

        // Ensure we hit exactly 0
        _materialInstance.SetFloat("Alpha", 0f);

        // Unsubscribe from signal before disabling to prevent further triggers
        Signals.Get<Encounter_Start>().RemoveListener(ReactToEncounterStart);

        // Disable the GameObject
        gameObject.SetActive(false);

        _currentAnimation = null;
    }
}
