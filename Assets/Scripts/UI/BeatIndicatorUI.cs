using deVoid.Utils;
using Game.Audio;
using Game.Encounter;
using Game.Player;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BeatIndicatorUI : MonoBehaviour
{
    [Header("Indicator Reference")]
    [SerializeField] private RectTransform indicatorPrefab;

    [Header("Queue Settings")]
    [SerializeField] private int queueLength = 3;
    [SerializeField] private float indicatorSpacing = 200f; // Distance between each indicator pair

    [Header("Animation Settings")]
    [SerializeField] private float meetingPointOffset = 10f; // Distance from center where lines meet

    [Header("Fade Settings")]
    [SerializeField] private float fadeInStartDistance = 50f; // Distance from back where fade-in starts (closer to back)
    [SerializeField] private float fadeInEndDistance = 150f; // Distance from back where fade-in completes (closer to center)
    [SerializeField] private float fadeOutStartDistance = 50f; // Distance from center where fade-out starts
    [SerializeField] private float fadeOutEndDistance = 10f; // Distance from center where fade-out completes

    private List<IndicatorPair> indicatorPairs = new List<IndicatorPair>();

    private class IndicatorPair
    {
        public RectTransform leftIndicator;
        public RectTransform rightIndicator;
        public CanvasGroup leftCanvasGroup;
        public CanvasGroup rightCanvasGroup;
        public int queueIndex; // 0 = front (hitting center), n-1 = back
    }

    void Awake()
    {
        InitializeIndicatorQueue();

        gameObject.SetActive(false);

        Signals.Get<Encounter_Start>().AddListener(ReactToEncounterStart);
        Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);
        Signals.Get<PlayerDiedEvent>().AddListener(ReactToPlayerDeath);
    }

    void InitializeIndicatorQueue()
    {
        // Create all pairs from the prefab
        for (int i = 0; i < queueLength; i++)
        {
            // Instantiate left indicator (mirrored)
            RectTransform newLeft = Instantiate(indicatorPrefab, indicatorPrefab.parent);
            newLeft.name = $"LeftIndicator_{i}";
            newLeft.localScale = new Vector3(-1, 1, 1); // Mirror on X-axis

            // Instantiate right indicator
            RectTransform newRight = Instantiate(indicatorPrefab, indicatorPrefab.parent);
            newRight.name = $"RightIndicator_{i}";

            IndicatorPair newPair = new IndicatorPair
            {
                leftIndicator = newLeft,
                rightIndicator = newRight,
                leftCanvasGroup = GetOrAddCanvasGroup(newLeft),
                rightCanvasGroup = GetOrAddCanvasGroup(newRight),
                queueIndex = i
            };

            indicatorPairs.Add(newPair);
        }

        // Hide the prefab (it's just a template)
        indicatorPrefab.gameObject.SetActive(false);
    }

    CanvasGroup GetOrAddCanvasGroup(RectTransform rectTransform)
    {
        CanvasGroup canvasGroup = rectTransform.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = rectTransform.gameObject.AddComponent<CanvasGroup>();
        }
        return canvasGroup;
    }

    void Update()
    {
        if (BeatDetector.Instance == null)
            return;

        // Calculate beat progress
        float timeSinceLastBeat = Time.time - BeatDetector.Instance.LastBeatTime;
        float beatInterval = BeatDetector.Instance.BeatIntervalSeconds;

        if (beatInterval <= 0)
            return;

        float progress = (timeSinceLastBeat / beatInterval) % 1.0f;

        UpdateQueuePositions(progress);
    }

    void UpdateQueuePositions(float progress)
    {
        foreach (IndicatorPair pair in indicatorPairs)
        {
            // Calculate this indicator's position in the queue
            // Queue index 0 = front (hitting center on beat)
            // Queue index n-1 = back (furthest from center)

            // Calculate start and end distances for this pair
            float startDistance = indicatorSpacing * (pair.queueIndex + 1);
            float endDistance = indicatorSpacing * pair.queueIndex;

            // If this is the front pair (index 0), it should hit the meeting point
            if (pair.queueIndex == 0)
            {
                endDistance = meetingPointOffset;
            }

            // Animate positions
            float leftX = Mathf.Lerp(-startDistance, -endDistance, progress);
            float rightX = Mathf.Lerp(startDistance, endDistance, progress);

            pair.leftIndicator.anchoredPosition = new Vector2(leftX, pair.leftIndicator.anchoredPosition.y);
            pair.rightIndicator.anchoredPosition = new Vector2(rightX, pair.rightIndicator.anchoredPosition.y);

            // Calculate alpha based on position in queue
            float currentDistance = Mathf.Abs(leftX);
            float alpha = CalculateAlpha(currentDistance, pair.queueIndex);
            pair.leftCanvasGroup.alpha = alpha;
            pair.rightCanvasGroup.alpha = alpha;

            // When front indicator reaches center, move it to back of queue
            if (pair.queueIndex == 0 && progress >= 0.95f)
            {
                RepositionToBack(pair);
            }
        }
    }

    float CalculateAlpha(float currentDistance, int queueIndex)
    {
        // Calculate the expected start position for the back indicator
        float backStartDistance = indicatorSpacing * queueLength;

        // Fade-in from back of queue (invisible -> visible)
        // Check if indicator is within the fade-in range from the back
        float distanceFromMaxPosition = backStartDistance - currentDistance;

        if (distanceFromMaxPosition <= fadeInEndDistance)
        {
            // We're in the fade-in zone
            if (distanceFromMaxPosition <= fadeInStartDistance)
            {
                return 0f; // Invisible at the very back
            }
            else if (distanceFromMaxPosition >= fadeInEndDistance)
            {
                // Fully visible, now check for fade-out
                return CalculateFadeOutAlpha(currentDistance);
            }
            else
            {
                // Fading in
                float fadeInRange = fadeInEndDistance - fadeInStartDistance;
                float fadeInProgress = (distanceFromMaxPosition - fadeInStartDistance) / fadeInRange;
                return fadeInProgress;
            }
        }

        // After fade-in, check for fade-out near center
        return CalculateFadeOutAlpha(currentDistance);
    }

    float CalculateFadeOutAlpha(float distance)
    {
        // Fade-out near center (visible -> invisible)
        if (distance >= fadeOutStartDistance)
        {
            return 1f; // Fully visible
        }
        else if (distance <= fadeOutEndDistance)
        {
            return 0f; // Invisible at center
        }
        else
        {
            // Fading out
            return (distance - fadeOutEndDistance) / (fadeOutStartDistance - fadeOutEndDistance);
        }
    }

    void RepositionToBack(IndicatorPair pair)
    {
        // Move this pair to the back of the queue
        pair.queueIndex = queueLength - 1;

        // Shift all other pairs forward in the queue
        foreach (IndicatorPair otherPair in indicatorPairs)
        {
            if (otherPair != pair && otherPair.queueIndex > 0)
            {
                otherPair.queueIndex--;
            }
        }

        // Reset alpha for repositioned pair
        pair.leftCanvasGroup.alpha = 1f;
        pair.rightCanvasGroup.alpha = 1f;
    }

    void ReactToEncounterStart(int id)
    {
        gameObject.SetActive(true);
    }

    void ReactToEncounterEnd(int id)
    {
        gameObject.SetActive(false);
    }

    void ReactToPlayerDeath()
    {
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        Signals.Get<Encounter_Start>().RemoveListener(ReactToEncounterStart);
        Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);
        Signals.Get<PlayerDiedEvent>().RemoveListener(ReactToPlayerDeath);
    }
}
