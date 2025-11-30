using System.Collections;
using deVoid.UIFramework.Examples;
using deVoid.Utils;
using Game.Audio;
using Game.CharacterEnemy;
using Game.Core;
using Game.Environment;
using Game.GameScreen;
using UnityEngine;

namespace Game.Encounter
{
    public class Encounter_Start : ASignal<int> { } // int is encounter id
    public class Encounter_End : ASignal<int> { } // int is encounter id

    public class EncounterManager : MonoBehaviour
    {
        [SerializeField] ShadowBubbleController startBubble;
        [SerializeField] GameObject bubbleCollider;

        int encounterId = 0;

        bool encounterActive = false;
        TimeLeftClock encounterTimer;
        TimeLeftClock encounterDelay;

        void Awake()
        {
            encounterId = GameData.Instance.CurrentProgress;
        }

        void Update()
        {
            if (encounterActive)
            {
                if (encounterTimer.IsTimeOver())
                {
                    Debug.Log("Encounter over!");
                    encounterActive = false;

                    Signals.Get<Encounter_End>().Dispatch(encounterId);
                    AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.UnloadCasette, FindAnyObjectByType<PlayerHealth>().transform.position);
                }
            }
        }

        bool startingEncounter = false;
        public void StartEncounter()
        {
            if (startingEncounter)
                return;

            if (encounterId >= 3)
            {
                Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.EndScreen);
                return;
            }
            else
            {
                startingEncounter = true;
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.LoadCasette, FindAnyObjectByType<PlayerHealth>().transform.position);
                StartCoroutine(StartEncounterAfterDuration(2.5f));
            }
        }

        private IEnumerator StartEncounterAfterDuration(float duration)
        {
            if (encounterDelay == null)
            {
                encounterDelay = new TimeLeftClock(duration);
            }
            else
            {
                encounterDelay.ResetTimer();
            }

            while (!encounterDelay.IsTimeOver())
                yield return null;

            ++encounterId;

            AudioManager.Instance.SetMusic((MusicType)encounterId);

            if (startBubble.gameObject.activeSelf)
            {
                startBubble.HideBubble(8.0f);
                bubbleCollider.SetActive(false);
            }

            ThemeSwitcher.Instance.SwitchToTheme(encounterId - 1);

            var enemySpawner = FindAnyObjectByType<EnemySpawner>();
            if (enemySpawner != null)
            {
                enemySpawner.GetComponent<EnemySpawner>().StartSpawningEnemies(encounterId);
            }

            double encounterDuration = AudioManager.Instance.GetMusicLengthMs() / 1000f;

            //if (encounterId == 1 || encounterId == 2)
            //{
            //    encounterDuration = 5f;
            //}

            if (encounterTimer == null)
            {
                encounterTimer = new TimeLeftClock(encounterDuration);
            }
            else
            {
                encounterTimer.ChangeTimeToTrack(encounterDuration, true);
            }

            encounterActive = true;

            Signals.Get<Encounter_Start>().Dispatch(encounterId);

            startingEncounter = false;
        }

        public void RestartCurrentEncounter()
        {
            StartCoroutine(RestartCurrentEncounterCoroutine());
        }

        private IEnumerator RestartCurrentEncounterCoroutine()
        {
            // End the current encounter (kills all enemies)
            encounterActive = false;
            Signals.Get<Encounter_End>().Dispatch(encounterId);

            // Reset wall glows
            var spectrumGenerator = FindAnyObjectByType<SpectrumGenerator>();
            if (spectrumGenerator != null)
            {
                spectrumGenerator.ResetAllWalls();
            }

            // Wait a frame for cleanup
            yield return null;

            // Restart the same encounter without incrementing encounterId
            // SetMusic() already stops the old music before starting the new one
            AudioManager.Instance.SetMusic((MusicType)encounterId);

            ThemeSwitcher.Instance.SwitchToTheme(encounterId - 1);

            var enemySpawner = FindAnyObjectByType<EnemySpawner>();
            if (enemySpawner != null)
            {
                enemySpawner.GetComponent<EnemySpawner>().StartSpawningEnemies(encounterId);
            }

            double encounterDuration = AudioManager.Instance.GetMusicLengthMs() / 1000f;

            if (encounterTimer == null)
            {
                encounterTimer = new TimeLeftClock(encounterDuration);
            }
            else
            {
                encounterTimer.ChangeTimeToTrack(encounterDuration, true);
            }

            encounterActive = true;

            Signals.Get<Encounter_Start>().Dispatch(encounterId);
        }
    }
}
