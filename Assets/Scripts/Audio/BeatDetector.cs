using UnityEngine;
using deVoid.Utils;
using Game.Encounter;

namespace Game.Audio
{
    /// <summary>
    /// Represents the quality of a shot based on timing accuracy.
    /// </summary>
    public enum ShotQuality
    {
        Missed,  // Shot was outside the beat window
        Good,    // Shot was within beat window but not perfect
        Perfect  // Shot was very close to the beat
    }

    public class BeatDetector : MonoBehaviour
    {
        public static BeatDetector Instance { get; private set; }

        [Header("Beat Timing Settings")]
        [Tooltip("Percentage of beat interval that counts as 'on beat' (e.g., 0.3 = 30% of beat interval before/after)")]
        [SerializeField] private float beatTolerancePercentage = 0.3f; // ~30% gives 150ms at 110 BPM
        
        [Tooltip("Offset in milliseconds to shift the timing window. Positive values shift window EARLIER (allows shooting earlier). Negative values shift window LATER (requires shooting later).")]
        [SerializeField] private float timingOffsetMs = 20f;


        [Header("Combo Settings")]
        [Tooltip("Maximum combo value")]
        [SerializeField] private int maxCombo = 20;

        [Tooltip("Time in seconds before combo starts decaying")]
        [SerializeField] private float comboDecayTime = 1f;

        [Tooltip("Time in seconds between fast decay ticks (used when resetting combo between encounters)")]
        [SerializeField] private float fastDecayTime = 0.1f;

        [Header("Timing Statistics")]
        [SerializeField] private int totalShotsTracked = 0;
        [SerializeField] private float averageTimingOffsetMs = 0f; // Average of all shots
        [SerializeField] private float recentAverageTimingOffsetMs = 0f; // Average of last 32 shots

        private const int RECENT_SHOT_HISTORY_SIZE = 32;
        private System.Collections.Generic.Queue<float> recentShotTimings = new System.Collections.Generic.Queue<float>(RECENT_SHOT_HISTORY_SIZE);
        private float totalTimingOffsetMs = 0f; // Sum of all shots

        private bool comboEnabled = true; // Controls whether combo can be gained or decay normally
        private bool isFastDecaying = false; // When true, combo decays faster to reach 0
        private bool statsEnabled = false; // Controls whether stats are recorded (enabled during encounters)

        private float lastBeatTime;
        private float currentTempo;
        private float beatIntervalSeconds;
        private int currentCombo = 0;
        private float lastComboIncreaseTime;

        // Shot accuracy statistics
        private int totalShots = 0;
        private int successfulShots = 0;
        private int perfectShots = 0;

        public float BeatToleranceMs => beatIntervalSeconds * beatTolerancePercentage * 1000f;
        public int CurrentCombo => currentCombo;
        public int MaxCombo => maxCombo;
        public float LastBeatTime => lastBeatTime;
        public float BeatIntervalSeconds => beatIntervalSeconds;
        public int TotalShotsTracked => totalShotsTracked;
        public float AverageTimingOffsetMs => averageTimingOffsetMs;
        public float RecentAverageTimingOffsetMs => recentAverageTimingOffsetMs;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Signals.Get<FMODBeatEvent>().AddListener(OnBeat);
            Signals.Get<Encounter_Start>().AddListener(OnEncounterStart);
            Signals.Get<Encounter_End>().AddListener(OnEncounterEnd);
        }

        void Update()
        {
            // Handle combo decay
            if (currentCombo > 0)
            {
                // Fast decay between encounters
                if (isFastDecaying)
                {
                    if (Time.time - lastComboIncreaseTime >= fastDecayTime)
                    {
                        DecrementCombo();
                        lastComboIncreaseTime = Time.time; // Reset timer after decay

                        // Stop fast decay when combo reaches 0
                        if (currentCombo == 0)
                        {
                            isFastDecaying = false;
                        }
                    }
                }
                // Normal decay during encounters
                else if (comboEnabled && Time.time - lastComboIncreaseTime >= comboDecayTime)
                {
                    DecrementCombo();
                    lastComboIncreaseTime = Time.time; // Reset timer after decay
                }
            }

            PlayMetronome();
        }

        TimeLeftClockRealTime preTick;
        TimeLeftClockRealTime tick;
        TimeLeftClockRealTime postTick;

        // Flags to track clicks per beat
        private bool preFired;
        private bool postFired;

        void PlayMetronome()
        {
            if (postTick == null || preTick == null)
                return;

            if (!postFired && postTick.IsTimeOver())
            {
                if (AudioManager.Instance.AudioDataInstance.MetronomeOnPostBeat)
                    AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.Metronome, transform.position);

                postFired = true;
            }
            else if (!preFired && preTick.IsTimeOver())
            {
                if (AudioManager.Instance.AudioDataInstance.MetronomeOnPreBeat)
                    AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.Metronome, transform.position);

                preFired = true;
            }
        }

        private void OnBeat(FMODBeatData beatData)
        {
            lastBeatTime = Time.time;
            currentTempo = beatData.Tempo;

            // Calculate the interval between beats in seconds
            // BPM = beats per minute, so interval = 60 / BPM
            if (currentTempo > 0)
            {
                beatIntervalSeconds = 60f / currentTempo;
            }

            // Apply timing offset to all metronome sounds
            float offsetSeconds = timingOffsetMs / 1000f;

            // Play metronome sound on beat (with offset applied)
            if (AudioManager.Instance != null &&
                AudioManager.Instance.AudioDataInstance != null)
            {
                if (AudioManager.Instance.AudioDataInstance.MetronomeOnBeat)
                {
                    if (offsetSeconds != 0f)
                    {
                        StartCoroutine(PlayMetronomeDelayed(offsetSeconds));
                    }
                    else
                    {
                        AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.Metronome, transform.position);
                    }
                }
            }

            if (postTick == null || preTick == null)
            {
                float tolerance = beatIntervalSeconds * beatTolerancePercentage;
                float perfectPercentage = 0.2f;
                float perfectTime = tolerance * perfectPercentage;

                float interval = Instance.beatIntervalSeconds;

                // helper vars to clamp tick times to never go below 0 or above interval
                float postTime = Mathf.Clamp(offsetSeconds + perfectTime, 0f, interval);
                float preTime = Mathf.Clamp(interval + offsetSeconds - perfectTime, 0f, interval); 
                float tickTime = Mathf.Clamp(interval + offsetSeconds, 0f, interval);


                // Post tick: plays at (offset + perfectTime) after the beat
                postTick = new TimeLeftClockRealTime(postTime);

                // Pre tick: plays at (beatInterval + offset - perfectTime) after the beat
                preTick = new TimeLeftClockRealTime(preTime);

                // Main tick: shifted by offset to move the center of the timing window
                tick = new TimeLeftClockRealTime(tickTime);
            }
            else
            {
                postTick.ResetTimer();
                preTick.ResetTimer();
                tick.ResetTimer();

                postFired = false;
                preFired = false;
            }

            //Debug.Log($"BeatDetector: Beat registered at {lastBeatTime:F3}s, Tempo: {currentTempo} BPM, Interval: {beatIntervalSeconds:F3}s");
        }

        private System.Collections.IEnumerator PlayMetronomeDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (AudioManager.Instance != null && AudioManager.Instance.AudioDataInstance != null)
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.Metronome, transform.position);
            }
        }

        /// <summary>
        /// Returns a value from 0 to 1 indicating how accurate the timing is.
        /// 1.0 = perfect timing (exactly on beat)
        /// 0.0 = at the edge of the tolerance window
        /// Returns -1 if outside the beat window
        /// </summary>
        public static float GetBeatAccuracy()
        {
            if (Instance == null) return -1f;

            float currentTime = Time.time;
            float offsetSeconds = Instance.timingOffsetMs / 1000f;
            
            // Apply offset to shift the timing window
            float adjustedCurrentTime = currentTime - offsetSeconds;
            float timeSinceLastBeat = adjustedCurrentTime - Instance.lastBeatTime;
            float toleranceSeconds = Instance.BeatToleranceMs / 1000f;

            // Check window after last beat
            if (timeSinceLastBeat >= 0 && timeSinceLastBeat <= toleranceSeconds)
            {
                return 1f - (timeSinceLastBeat / toleranceSeconds);
            }

            // Check window before next beat
            if (Instance.beatIntervalSeconds > 0)
            {
                float timeUntilNextBeat = Instance.beatIntervalSeconds - timeSinceLastBeat;
                if (timeUntilNextBeat >= 0 && timeUntilNextBeat <= toleranceSeconds)
                {
                    return 1f - (timeUntilNextBeat / toleranceSeconds);
                }
            }

            return -1f; // Outside beat window
        }

        // /// <summary>
        // /// Classifies the shot quality based on timing accuracy.
        // /// Perfect: accuracy >= 0.9 (within top 10% of beat window)
        // /// Good: accuracy >= 0.0 (within beat window)
        // /// Missed: accuracy == -1 (outside beat window)
        // /// </summary>
        // public static ShotQuality GetShotQuality()
        // {
        //     float accuracy = GetBeatAccuracy();

        //     if (accuracy < 0f)
        //         return ShotQuality.Missed;

        //     if (accuracy >= 0.80f)
        //         return ShotQuality.Perfect;

        //     return ShotQuality.Good;
        // }

        /// <summary>
        /// Returns the average shot accuracy as a percentage (0-100).
        /// Good and Perfect shots count as 100% accurate, Missed shots count as 0%.
        /// </summary>
        public static int GetAverageAccuracy()
        {
            if (Instance == null || Instance.totalShots == 0)
                return 0;

            float accuracyPercentage = ((float)Instance.successfulShots / Instance.totalShots) * 100f;
            return Mathf.RoundToInt(accuracyPercentage);
        }

        /// <summary>
        /// Returns the percentage of shots that were Perfect quality (0-100).
        /// </summary>
        public static int GetPerfectShotPercentage()
        {
            if (Instance == null || Instance.totalShots == 0)
                return 0;

            float perfectPercentage = ((float)Instance.perfectShots / Instance.totalShots) * 100f;
            return Mathf.RoundToInt(perfectPercentage);
        }

        public ShotQuality GetShotQuality2()
        {
            if (tick == null)
            {
                if (statsEnabled)
                {
                    totalShots++;
                }
                return ShotQuality.Missed;
            }

            float percentagePassed = tick.PercentagePassed(); // 0..1
            
            // Calculate timing offset from the shifted beat center
            float timingFromBeat;
            if (percentagePassed <= 0.5f)
            {
                // Shot after the beat (late) - positive value
                timingFromBeat = percentagePassed * Instance.beatIntervalSeconds * 1000f;
            }
            else
            {
                // Shot before the next beat (early) - negative value
                timingFromBeat = -(1f - percentagePassed) * Instance.beatIntervalSeconds * 1000f;
            }
            
            // Track timing offset for all-time averaging
            totalShotsTracked++;
            totalTimingOffsetMs += timingFromBeat;
            averageTimingOffsetMs = totalTimingOffsetMs / totalShotsTracked;
            
            // Track timing offset for recent (last 32 shots) averaging
            recentShotTimings.Enqueue(timingFromBeat);
            if (recentShotTimings.Count > RECENT_SHOT_HISTORY_SIZE)
            {
                recentShotTimings.Dequeue(); // Remove oldest shot
            }
            
            // Calculate recent average
            float recentSum = 0f;
            foreach (float timing in recentShotTimings)
            {
                recentSum += timing;
            }
            recentAverageTimingOffsetMs = recentSum / recentShotTimings.Count;
            
            // Calculate deviation from center (tick is already shifted, no additional offset needed)
            float deviation = Mathf.Min(percentagePassed, 1f - percentagePassed);
            float deviationPercentage = deviation; // 0 to 0.5 (0 = on beat, 0.5 = halfway between beats)
            
            float tolerance = beatTolerancePercentage; // e.g., 0.3
            float perfectPercentage = 0.2f; // fraction of tolerance

            ShotQuality quality;
            if (deviationPercentage <= tolerance * perfectPercentage)
                quality = ShotQuality.Perfect;
            else if (deviationPercentage <= tolerance)
                quality = ShotQuality.Good;
            else
                quality = ShotQuality.Missed;

            // Debug log with timing info
            string earlyLate = timingFromBeat >= 0 ? "LATE" : "EARLY";
            string avgEarlyLate = averageTimingOffsetMs >= 0 ? "late" : "early";
            string recentAvgEarlyLate = recentAverageTimingOffsetMs >= 0 ? "late" : "early";
            
            Debug.Log($"Shot #{totalShotsTracked}: {Mathf.Abs(timingFromBeat):F1}ms {earlyLate} | Quality: {quality} | " +
                      $"All-time Avg: {Mathf.Abs(averageTimingOffsetMs):F1}ms {avgEarlyLate} | " +
                      $"Recent Avg (last {recentShotTimings.Count}): {Mathf.Abs(recentAverageTimingOffsetMs):F1}ms {recentAvgEarlyLate}");

            // Record shot statistics
            if (statsEnabled)
            {
                totalShots++;
                if (quality == ShotQuality.Good || quality == ShotQuality.Perfect)
                {
                    successfulShots++;
                }
                if (quality == ShotQuality.Perfect)
                {
                    perfectShots++;
                }
            }

            return quality;
        }

        /// <summary>
        /// Resets timing statistics
        /// </summary>
        public static void ResetTimingStats()
        {
            if (Instance == null) return;
            
            Instance.totalShotsTracked = 0;
            Instance.totalTimingOffsetMs = 0f;
            Instance.averageTimingOffsetMs = 0f;
            Instance.recentAverageTimingOffsetMs = 0f;
            Instance.recentShotTimings.Clear();
            Debug.Log("Timing statistics reset.");
        }

        /// <summary>
        /// Logs the current timing statistics summary
        /// </summary>
        public static void LogTimingStats()
        {
            if (Instance == null) return;
            
            string avgEarlyLate = Instance.averageTimingOffsetMs >= 0 ? "LATE" : "EARLY";
            string recentAvgEarlyLate = Instance.recentAverageTimingOffsetMs >= 0 ? "LATE" : "EARLY";
            
            Debug.Log($"=== TIMING STATISTICS ===\n" +
                      $"Total Shots: {Instance.totalShotsTracked}\n" +
                      $"All-time Average: {Mathf.Abs(Instance.averageTimingOffsetMs):F1}ms {avgEarlyLate}\n" +
                      $"Recent Average (last {Instance.recentShotTimings.Count}): {Mathf.Abs(Instance.recentAverageTimingOffsetMs):F1}ms {recentAvgEarlyLate}\n" +
                      $"Suggested timingOffsetMs (based on recent): {-Instance.recentAverageTimingOffsetMs:F1}ms");
        }

        /// <summary>
        /// Increments the combo counter when player hits a beat successfully.
        /// Combo is clamped to maxCombo.
        /// </summary>
        public static void IncrementCombo()
        {
            if (Instance == null || !Instance.comboEnabled) return;

            Instance.currentCombo = Mathf.Min(Instance.currentCombo + 1, Instance.maxCombo);
            Instance.lastComboIncreaseTime = Time.time; // Reset decay timer
        }

        /// <summary>
            /// Resets the combo decay timer without incrementing the combo.
            /// Used when player hits a Good shot at combo > 10.
        /// </summary>
        public static void ResetComboDecayTimer()
        {
            if (Instance == null || !Instance.comboEnabled) return;

            Instance.lastComboIncreaseTime = Time.time;
        }

        /// <summary>
        /// Decrements the combo counter.
        /// Combo is clamped to minimum of 0.
        /// </summary>
        /// <param name="amount">Amount to decrement by (default: 1)</param>
        public static void DecrementCombo(int amount = 1)
        {
            if (Instance == null) return;

            Instance.currentCombo = Mathf.Max(Instance.currentCombo - amount, 0);
        }

        /// <summary>
        /// Resets the combo counter to 0.
        /// </summary>
        public static void ResetCombo()
        {
            if (Instance == null) return;

            Instance.currentCombo = 0;
        }

        /// <summary>
        /// Called when an encounter starts. Enables combo system, stats recording, and resets timing statistics.
        /// </summary>
        private void OnEncounterStart(int encounterId)
        {
            comboEnabled = true;
            isFastDecaying = false;
            statsEnabled = true;

            tick = null;
            preTick = null;
            postTick = null;
            
            // Reset timing statistics at the start of each encounter
            totalShotsTracked = 0;
            totalTimingOffsetMs = 0f;
            averageTimingOffsetMs = 0f;
            recentAverageTimingOffsetMs = 0f;
            recentShotTimings.Clear();
        }

        /// <summary>
        /// Called when an encounter ends. Disables combo gaining, starts fast decay to reset combo to 0, and disables stats recording.
        /// </summary>
        private void OnEncounterEnd(int encounterId)
        {
            comboEnabled = false;
            isFastDecaying = true;
            statsEnabled = false;
            lastComboIncreaseTime = Time.time; // Start fast decay immediately
            
            // Log timing statistics at encounter end
            LogTimingStats();
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                // Log final timing statistics
                LogTimingStats();
                
                Signals.Get<FMODBeatEvent>().RemoveListener(OnBeat);
                Signals.Get<Encounter_Start>().RemoveListener(OnEncounterStart);
                Signals.Get<Encounter_End>().RemoveListener(OnEncounterEnd);
            }
        }
    }
}
