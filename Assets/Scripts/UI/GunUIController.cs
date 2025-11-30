using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Audio;
using Game.Player;
using deVoid.Utils;

public class GunUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI comboText;

    [Header("Combo Animation")]
    [SerializeField] private float comboPulseScale = 1.5f;
    [SerializeField] private float comboPulseDuration = 0.25f;
    [SerializeField] private float comboShrinkScale = 0.7f;
    [SerializeField] private float comboShrinkDuration = 0.25f;

    [Header("High Combo Color Flash")]
    [SerializeField] private Color comboFlashColor = Color.yellow;
    [SerializeField] private float comboFlashSpeed = 2f;

    [Header("Health Display")]
    [SerializeField] private Image[] heartImages;
    [SerializeField] private Sprite heartFullSprite;
    [SerializeField] private Sprite heartEmptySprite;

    private int lastCombo = 0;
    private Coroutine pulseCoroutine;
    private Coroutine shrinkCoroutine;
    private Coroutine colorFlashCoroutine;
    private Color originalComboColor;

    void Start()
    {
        Signals.Get<PlayerHealthChangedEvent>().AddListener(OnPlayerHealthChanged);

        // Store original combo text color
        if (comboText != null)
        {
            originalComboColor = comboText.color;
        }

        // Initialize health display in case PlayerHealth already fired its initial signal
        PlayerHealth playerHealth = FindAnyObjectByType<PlayerHealth>();
        if (playerHealth != null)
        {
            UpdateHealthDisplay(playerHealth.GetCurrentHealth());
        }
    }

    void OnDestroy()
    {
        Signals.Get<PlayerHealthChangedEvent>().RemoveListener(OnPlayerHealthChanged);
    }

    private void OnPlayerHealthChanged(PlayerHealthData data)
    {
        UpdateHealthDisplay(data.CurrentHealth);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateComboDisplay();
        UpdateTimerDisplay();
    }

    private void UpdateComboDisplay()
    {
        if (comboText != null && BeatDetector.Instance != null)
        {
            int currentCombo = BeatDetector.Instance.CurrentCombo;
            comboText.text = $"{currentCombo}x";

            // Trigger pulse animation when combo increases
            if (currentCombo > lastCombo)
            {
                // Stop any ongoing shrink animation
                if (shrinkCoroutine != null)
                {
                    StopCoroutine(shrinkCoroutine);
                    shrinkCoroutine = null;
                    comboText.transform.localScale = Vector3.one; // Reset scale
                }

                // Start pulse animation if not already running
                if (pulseCoroutine == null)
                {
                    pulseCoroutine = StartCoroutine(AnimateComboPulse());
                }
            }
            // Trigger shrink animation when combo decreases
            else if (currentCombo < lastCombo)
            {
                // Stop any ongoing pulse animation
                if (pulseCoroutine != null)
                {
                    StopCoroutine(pulseCoroutine);
                    pulseCoroutine = null;
                    comboText.transform.localScale = Vector3.one; // Reset scale
                }

                // Start shrink animation if not already running
                if (shrinkCoroutine == null)
                {
                    shrinkCoroutine = StartCoroutine(AnimateComboShrink());
                }
            }

            // Trigger color flash for high combo (10-20)
            if (currentCombo >= 10 && currentCombo <= 20)
            {
                if (colorFlashCoroutine == null)
                {
                    colorFlashCoroutine = StartCoroutine(AnimateColorFlash());
                }
            }
            else
            {
                // Stop color flash when combo drops below 11
                if (colorFlashCoroutine != null)
                {
                    StopCoroutine(colorFlashCoroutine);
                    colorFlashCoroutine = null;
                    comboText.color = originalComboColor; // Reset color
                }
            }

            lastCombo = currentCombo;
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null && AudioManager.Instance != null)
        {
            int currentTimeMs = AudioManager.Instance.GetMusicTimelinePositionMs();
            int totalLengthMs = AudioManager.Instance.GetMusicLengthMs();

            int remainingMs = totalLengthMs - currentTimeMs;
            int totalSeconds = remainingMs / 1000;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            timerText.text = $"{minutes}:{seconds:D2}";
        }
    }

    private void UpdateHealthDisplay(int currentHealth)
    {
        if (heartImages == null || heartImages.Length == 0)
            return;

        for (int i = 0; i < heartImages.Length; i++)
        {
            if (heartImages[i] != null)
            {
                heartImages[i].sprite = i < currentHealth ? heartFullSprite : heartEmptySprite;
            }
        }
    }

    private System.Collections.IEnumerator AnimateComboPulse()
    {
        if (comboText == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * comboPulseScale;
        float elapsed = 0f;
        float halfDuration = comboPulseDuration / 2f;

        // Scale up
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            comboText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            comboText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        // Ensure we end at exactly the original scale
        comboText.transform.localScale = originalScale;

        pulseCoroutine = null;
    }

    private System.Collections.IEnumerator AnimateComboShrink()
    {
        if (comboText == null) yield break;

        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * comboShrinkScale;
        float elapsed = 0f;
        float halfDuration = comboShrinkDuration / 2f;

        // Scale down
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            comboText.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Scale back up
        while (elapsed < halfDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / halfDuration;
            comboText.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        // Ensure we end at exactly the original scale
        comboText.transform.localScale = originalScale;

        shrinkCoroutine = null;
    }

    private System.Collections.IEnumerator AnimateColorFlash()
    {
        if (comboText == null) yield break;

        float time = 0f;

        while (BeatDetector.Instance != null &&
               BeatDetector.Instance.CurrentCombo >= 11 &&
               BeatDetector.Instance.CurrentCombo <= 20)
        {
            time += Time.deltaTime * comboFlashSpeed;

            // Use PingPong to oscillate between 0 and 1
            float t = Mathf.PingPong(time, 1f);

            // Lerp between original color and flash color
            comboText.color = Color.Lerp(originalComboColor, comboFlashColor, t);

            yield return null;
        }

        // Reset to original color when combo drops below 11
        comboText.color = originalComboColor;
        colorFlashCoroutine = null;
    }
}
