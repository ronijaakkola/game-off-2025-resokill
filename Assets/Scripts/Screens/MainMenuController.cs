using Cysharp.Threading.Tasks;
using deVoid.UIFramework;
using deVoid.UIFramework.Examples;
using deVoid.Utils;
using Game.Audio;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameScreen
{
    public class MainMenuController : Screen<WindowProperties>
    {
        override protected void Awake()
        {
            base.Awake();
        }

        protected override void OnOpenScreen()
        {
            AudioManager.Instance.MainMenuOpened();

            base.OnOpenScreen();
        }

        public void Button_StartGame()
        {
            UI_Close();
            Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.HowToPlayScreen);
        }

        public void Button_OpenSettings()
        {
            Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.SettingsScreen);
        }

        public void Button_Quit()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
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
