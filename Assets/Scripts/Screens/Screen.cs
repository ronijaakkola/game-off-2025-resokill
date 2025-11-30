using deVoid.UIFramework;
using deVoid.Utils;

namespace Game.GameScreen
{
    public abstract class Screen<TProps> : AWindowController<TProps> where TProps : IWindowProperties
    {
        public bool isClosing = false;

        protected override void OnOpenScreen()
        {
            isClosing = false;
        }

        protected override void OnCloseScreen()
        {
            isClosing = true;
        }

        protected override void WhileHiding()
        {
            Signals.Get<Screen_Closed>().Dispatch(ScreenId);
        }

        protected override void OnOpenAnimationFinished()
        {
            // TODO: Mieti onko tämä liian myöhään. Pitääkö kutsu tehdä heti openissa
            //Debug.Log("Signal: Open screen " + ScreenId);

            // TODO
            //Signals.Get<Screen_Opened>().Dispatch(ScreenId);
        }

        protected override void OnCloseAnimationFinished()
        {

        }
    }
}
