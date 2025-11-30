using UnityEngine;
using TMPro;
using Game.Audio;
using Game.Core;

public class CasetteTimerController : MonoBehaviour
{
    private TextMeshPro timerText;

    void Start()
    {
        timerText = GetComponent<TextMeshPro>();
    }

    void Update()
    {
        UpdateTimerDisplay();
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null)
            return;

        // Check if music is playing and show timer
        if (AudioManager.Instance != null)
        {
            int currentTimeMs = AudioManager.Instance.GetMusicTimelinePositionMs();
            int totalLengthMs = AudioManager.Instance.GetMusicLengthMs();
            int remainingMs = totalLengthMs - currentTimeMs;

            // During encounter: show timer
            if (remainingMs > 0)
            {
                int totalSeconds = remainingMs / 1000;
                int minutes = totalSeconds / 60;
                int seconds = totalSeconds % 60;

                timerText.text = $"{minutes}:{seconds:D2}";
                return;
            }
        }

        // No music playing - check game state for appropriate message
        if (GameData.Instance != null)
        {
            if (GameData.Instance.CurrentProgress == 0)
            {
                timerText.text = "Insert cassette to begin";
            }
            else if (GameData.Instance.CurrentProgress >= 3)
            {
                timerText.text = "Congratulations!";
            }
            else
            {
                timerText.text = "Insert next cassette";
            }
        }
        else
        {
            // Fallback if GameData not available
            timerText.text = "Insert next cassette";
        }
    }
}
