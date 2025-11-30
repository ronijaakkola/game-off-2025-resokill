using UnityEngine;
using deVoid.UIFramework;
using deVoid.Utils;
using Game.GameInput;

namespace Game.GameScreen
{
    public class Screen_OpenRequest : ASignal<string> { }
    public class Screen_Closed : ASignal<string> { }

    public class ScreensManager : MonoBehaviour
    {
        [SerializeField] UISettings uiSettings;

        UIFrame UI;
        int sceensCurrentlyOpen = 0;

        void Awake()
        {
            UI = uiSettings.CreateUIInstance();

            Signals.Get<Screen_OpenRequest>().AddListener(OpenScreen);
            Signals.Get<Screen_Closed>().AddListener(CloseScreen);
        }

        void OpenScreen(string screenName)
        {
            if (UI == null)
            {
                //Log.Error("Error: Frame is not yet initialized!");
            }
            else if (!IsScreenOpen(screenName))
            {
                ++sceensCurrentlyOpen;
                InputManager.Instance.ChangeToUIActionMap();

                UI.OpenWindow(screenName);
            }
        }

        void CloseScreen(string screenName)
        {
            --sceensCurrentlyOpen;

            if (sceensCurrentlyOpen == 0)
            {
                InputManager.Instance.ChangeToGameActionMap();
            }
        }

        public bool IsScreenOpen(string screenName)
        {
            return UI.IsScreenOpen(screenName);
        }

        void OnDestroy()
        {
            Signals.Get<Screen_OpenRequest>().RemoveListener(OpenScreen);
            Signals.Get<Screen_Closed>().RemoveListener(CloseScreen);
        }
    }
}
