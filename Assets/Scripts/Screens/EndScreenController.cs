using Cysharp.Threading.Tasks;
using deVoid.UIFramework;
using Game.Audio;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameScreen
{
    public class EndScreenController : Screen<WindowProperties>
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
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.GameOver, transform.position);

            base.OnOpenScreen();
        }

        protected override void OnCloseScreen()
        {
            AudioManager.Instance.MainMenuOpened();
            GameData.Instance.ResetProgress();

            base.OnCloseScreen();
        }

        public void Button_Restart()
        {
            if (missClickProtection.IsTimeOver())
            {
                string sceneName = "Game";
                LoadSceneAsync(sceneName).Forget();

                UI_Close();
            }
        }

        public void Button_BackToMainMenu()
        {
            if (missClickProtection.IsTimeOver())
            {
                string sceneName = "MainMenu";
                LoadSceneAsync(sceneName).Forget();

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
