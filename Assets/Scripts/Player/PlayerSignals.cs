using deVoid.Utils;

namespace Game.Player
{
    public struct PlayerHealthData
    {
        public int CurrentHealth;
        public int MaxHealth;
    }

    public class PlayerHealthChangedEvent : ASignal<PlayerHealthData> { }
    public class PlayerDiedEvent : ASignal { }
}
