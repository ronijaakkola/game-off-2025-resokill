using Cysharp.Threading.Tasks;
using deVoid.UIFramework;
using deVoid.UIFramework.Examples;
using deVoid.Utils;
using Game.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameScreen
{
    public class Game_Pause : ASignal<bool> { }

    public class PauseController : Screen<WindowProperties>
    {
        TimeLeftClockRealTime missClickProtection;

        override protected void Awake()
        {
            if (missClickProtection == null)
                missClickProtection = new TimeLeftClockRealTime(0.75f);

            base.Awake();
        }

        protected override void OnOpenScreen()
        {
            Signals.Get<Game_Pause>().Dispatch(true);
            Time.timeScale = 0f;
            missClickProtection.ResetTimer();

            base.OnOpenScreen();
        }

        protected override void OnCloseScreen()
        {
            Signals.Get<Game_Pause>().Dispatch(false);
            Time.timeScale = 1f;

            base.OnCloseScreen();
        }

        public void Button_CloseScreen()
        {
            UI_Close();
        }

        public void Button_OpenSettings()
        {
            Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.SettingsScreen);
        }

        public void Button_BackToMainMenu()
        {
            if (missClickProtection.IsTimeOver())
            {
                GameData.Instance.ResetProgress();

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
