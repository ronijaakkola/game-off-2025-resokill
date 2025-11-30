using UnityEngine;
using TMPro;
using Game.Audio;
using Game.Encounter;
using Game.GameScreen;
using deVoid.Utils;
using deVoid.UIFramework.Examples;
using MoreMountains.Tools;

public class AccuracyUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI beatAccuracyText;
    [SerializeField] private TextMeshProUGUI perfectAccuracyText;

    [Header("Panel Reference")]
    [SerializeField] private CanvasGroup panelCanvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine _fadeCoroutine;

    void Awake()
    {
        Signals.Get<Encounter_Start>().AddListener(ReactToEncounterStart);
        Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);
        Signals.Get<Game_Pause>().AddListener(ReactToGamePause);
        Signals.Get<Screen_OpenRequest>().AddListener(ReactToScreenOpen);
        Signals.Get<Screen_Closed>().AddListener(ReactToScreenClosed);

        panelCanvasGroup.alpha = 0f;
    }

    void OnDestroy()
    {
        Signals.Get<Encounter_Start>().RemoveListener(ReactToEncounterStart);
        Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);
        Signals.Get<Game_Pause>().RemoveListener(ReactToGamePause);
        Signals.Get<Screen_OpenRequest>().RemoveListener(ReactToScreenOpen);
        Signals.Get<Screen_Closed>().RemoveListener(ReactToScreenClosed);
    }

    void Update()
    {
        if (beatAccuracyText != null)
        {
            int beatAccuracy = BeatDetector.GetAverageAccuracy();
            beatAccuracyText.text = $"<b>Beat Accuracy</b> {beatAccuracy}%";
        }

        if (perfectAccuracyText != null)
        {
            int perfectAccuracy = BeatDetector.GetPerfectShotPercentage();
            perfectAccuracyText.text = $"<b>Perfect Accuracy</b> {perfectAccuracy}%";
        }
    }

    private void ReactToEncounterStart(int encounterId)
    {
        HidePanel();
    }

    private void ReactToEncounterEnd(int encounterId)
    {
        ShowPanel();
    }

    private void ReactToGamePause(bool isPaused)
    {
        if (isPaused)
        {
            ShowPanel();
        }
        else
        {
            HidePanel();
        }
    }

    private void ReactToScreenOpen(string screenId)
    {
        // Show stats when death screen or end screen opens
        if (screenId == ScreenIds.DeathScreen || screenId == ScreenIds.EndScreen)
        {
            ShowPanel();
        }
    }

    private void ReactToScreenClosed(string screenId)
    {
        // Hide stats when death screen or end screen closes
        if (screenId == ScreenIds.DeathScreen || screenId == ScreenIds.EndScreen)
        {
            HidePanel();
        }
    }

    private void HidePanel()
    {
        // Stop any existing fade animation
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(MMFade.FadeCanvasGroup(panelCanvasGroup, fadeDuration, 0f));
    }

    private void ShowPanel()
    {
        // Stop any existing fade animation
        if (_fadeCoroutine != null)
        {
            StopCoroutine(_fadeCoroutine);
        }

        _fadeCoroutine = StartCoroutine(MMFade.FadeCanvasGroup(panelCanvasGroup, fadeDuration, 1f));
    }
}
