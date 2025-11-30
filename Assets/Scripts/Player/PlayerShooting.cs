using Game.CharacterEnemy;
using Game.GameInput;
using Game.Audio;
using Game.Encounter;
using UnityEngine;
using MoreMountains.Feedbacks;
using FMODUnity;
using deVoid.Utils;

namespace Game.CharacterPlayer
{
    public class PlayerShooting : MonoBehaviour
    {
        [Header("Animation & Effects")]
        [SerializeField] private ProjectileSpawner projectileSpawner;
        [SerializeField] private Animator animator;
        [SerializeField] private MMF_Player fireFeedback;
        [SerializeField] private MMF_Player fireAltFeedback;

        bool shooting = false;
        bool okToShoot = true;
        bool shootingAlternate = false;
        bool okToShootAlternate = true;
        bool shootingEnabled = false;

        void Awake()
        {
            Signals.Get<Encounter_Start>().AddListener(ReactToEncounterStart);
        }

        void Update()
        {
            if (!shootingEnabled)
                return;

            RegisterInput();
            Shoot();
            ShootAlternate();
        }

        void ReactToEncounterStart(int id)
        {
            shootingEnabled = true;
            animator?.SetBool("Pregame", false);
        }

        void RegisterInput()
        {
            shooting = InputManager.Instance.FireInput;
            shootingAlternate = InputManager.Instance.FireAlternate;

            // Reset gate when fire button is released
            if (!shooting)
            {
                okToShoot = true;
            }

            // Reset gate when alternate fire button is released
            if (!shootingAlternate)
            {
                okToShootAlternate = true;
            }
        }

        void Shoot()
        {
            if (!shooting)
                return;

            // Prevent multiple shots per keypress
            if (!okToShoot)
                return;

            // Close the gate immediately after shooting
            okToShoot = false;

            // Get shot quality based on timing accuracy
            ShotQuality quality = BeatDetector.Instance.GetShotQuality2();

            // Update combo based on shot quality
            if (quality == ShotQuality.Missed)
            {
                BeatDetector.DecrementCombo(2); // Missed shots decrease combo by 2
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.ShootMissed, transform.position);
            }
            else if (quality == ShotQuality.Perfect)
            {
                // Perfect shots always increment combo (up to max 20)
                BeatDetector.IncrementCombo();
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.ShootPerfect, transform.position);
            }
            else if (quality == ShotQuality.Good)
            {
                // Good shots increment combo only up to 10
                if (BeatDetector.Instance.CurrentCombo < 10)
                {
                    BeatDetector.IncrementCombo();
                }
                else
                {
                    // Above combo 10, Good shots only reset decay timer
                    BeatDetector.ResetComboDecayTimer();
                }
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.ShootGood, transform.position);
            }

            animator?.SetTrigger("Fire");
            fireFeedback?.PlayFeedbacks();
            projectileSpawner.Spawn(quality);
        }

        void ShootAlternate()
        {
            if (!shootingAlternate)
                return;

            // Prevent multiple shots per keypress
            if (!okToShootAlternate)
                return;

            // Require at least 10 combo to use alternate fire
            if (BeatDetector.Instance.CurrentCombo < 10)
                return;

            // Close the gate immediately after shooting
            okToShootAlternate = false;

            // Subtract 10 combo when using alternate fire
            BeatDetector.DecrementCombo(10);

            // Get shot quality based on timing accuracy
            ShotQuality quality = BeatDetector.Instance.GetShotQuality2();

            if (quality == ShotQuality.Perfect)
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.ShootSpecialPerfect, transform.position);
            }
            else
            {
                AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.ShootSpecialGood, transform.position);
            }

            animator?.SetTrigger("FireAlternate");
            fireAltFeedback?.PlayFeedbacks();
            projectileSpawner.SpawnAlternate(quality);
        }

        void OnDestroy()
        {
            Signals.Get<Encounter_Start>().RemoveListener(ReactToEncounterStart);
        }
    }
}
