using Cysharp.Threading.Tasks;
using deVoid.UIFramework;
using Game.Audio;
using Game.CharacterPlayer;
using Game.Common;
using Game.Encounter;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.Core;

namespace Game.GameScreen
{
    public class DeathScreenController : Screen<WindowProperties>
    {
        TimeLeftClock missClickProtection;

        override protected void Awake()
        {
            if (missClickProtection == null)
                missClickProtection = new TimeLeftClock(1f);

            base.Awake();
        }

        protected override void OnOpenScreen()
        {
            missClickProtection.ResetTimer();

            base.OnOpenScreen();
        }

        protected override void OnCloseScreen()
        {
            AudioManager.Instance.MainMenuOpened();

            base.OnCloseScreen();
        }

        public void Button_Restart()
        {
            if (missClickProtection.IsTimeOver())
            {
                // Reset combo to 0
                BeatDetector.ResetCombo();

                // Reset camera to player view
                if (CameraController.Instance != null)
                {
                    CameraController.Instance.ResetToPlayerCamera();
                }

                // Reset player health to full
                var playerHealth = FindAnyObjectByType<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.ResetHealth();
                }

                // Respawn player at starting position
                var playerMovement = FindAnyObjectByType<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerMovement.Respawn();
                }

                // Clear all projectiles
                if (ObjectPooler.Instance != null)
                {
                    ObjectPooler.Instance.ReturnAllActiveToPool("Projectile");
                }

                // Restart current encounter (restarts song from beginning)
                var encounterManager = FindAnyObjectByType<EncounterManager>();
                if (encounterManager != null)
                {
                    encounterManager.RestartCurrentEncounter();
                }

                // Close death screen
                UI_Close();
            }
        }

        public void Button_BackToMainMenu()
        {
            if (missClickProtection.IsTimeOver())
            {
                string sceneName = "MainMenu";
                LoadSceneAsync(sceneName).Forget();

                GameData.Instance.ResetProgress();

                UI_Close();
            }
        }

        public async UniTask LoadSceneAsync(string sceneName, LoadSceneMode mode = LoadSceneMode.Single)
        {
            await UniTask.Yield();

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);

            while (!operation.isDone)
            {
                await UniTask.Yield();
            }
        }
    }
}
