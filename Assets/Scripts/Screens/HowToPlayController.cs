using Cysharp.Threading.Tasks;
using deVoid.UIFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.GameScreen
{
    public class HowToPlayController : Screen<WindowProperties>
    {
        public void Button_StartGame()
        {
            string sceneName = "Game";
            LoadSceneAsync(sceneName).Forget();

            UI_Close();
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
