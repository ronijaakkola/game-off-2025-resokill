using deVoid.UIFramework.Examples;
using deVoid.Utils;
using Game.GameScreen;
using UnityEngine;

namespace Game.Core
{
    public class GameLauncher : MonoBehaviour
    {
        void Start()
        {
            Signals.Get<Screen_OpenRequest>().Dispatch(ScreenIds.MainMenuScreen);
        }
    }
}
