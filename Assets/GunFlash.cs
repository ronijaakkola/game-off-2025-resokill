using UnityEngine;
using Game.Audio;

public class GunFlash : MonoBehaviour
{
    [Header("Material Settings")]
    [Tooltip("The material to pulse (GunUnlitMat)")]
    [SerializeField] private Material gunMaterial;

    [Header("Pulse Settings")]
    [Tooltip("Duration of one complete pulse cycle in seconds")]
    [SerializeField] private float pulseDuration = 1f;

    [Tooltip("Target color to pulse to (default: #B0B0B0)")]
    [SerializeField] private Color targetColor = new Color(0.69f, 0.69f, 0.69f, 1f); // #B0B0B0

    [Tooltip("Minimum combo required to activate pulse")]
    [SerializeField] private int minComboForPulse = 10;

    [Header("Particle System")]
    [Tooltip("Particle system to enable when combo >= 10")]
    [SerializeField] private ParticleSystem comboParticleSystem;

    private Color originalColor;
    private float pulseTimer;
    private bool isPulsing;

    void Awake()
    {
        if (gunMaterial != null)
        {
            // Store the original base map color
            originalColor = gunMaterial.GetColor("_BaseColor");
        }
    }

    void Update()
    {
        if (BeatDetector.Instance == null)
            return;

        bool shouldActivate = BeatDetector.Instance.CurrentCombo >= minComboForPulse;

        // Handle material pulse
        if (gunMaterial != null)
        {
            if (shouldActivate)
            {
                // Pulse animation
                pulseTimer += Time.deltaTime;
                float t = Mathf.PingPong(pulseTimer / pulseDuration, 1f);
                Color currentColor = Color.Lerp(originalColor, targetColor, t);
                gunMaterial.SetColor("_BaseColor", currentColor);
                isPulsing = true;
            }
            else if (isPulsing)
            {
                // Reset to original color when combo drops below threshold
                gunMaterial.SetColor("_BaseColor", originalColor);
                pulseTimer = 0f;
                isPulsing = false;
            }
        }

        // Handle particle system
        if (comboParticleSystem != null)
        {
            if (shouldActivate && !comboParticleSystem.isPlaying)
            {
                comboParticleSystem.Play();
            }
            else if (!shouldActivate && comboParticleSystem.isPlaying)
            {
                comboParticleSystem.Stop();
            }
        }
    }

    void OnDestroy()
    {
        // Reset material to original color when destroyed
        if (gunMaterial != null && isPulsing)
        {
            gunMaterial.SetColor("_BaseColor", originalColor);
        }
    }
}
