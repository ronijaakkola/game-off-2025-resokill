using deVoid.Utils;

namespace Game.Audio
{
    public struct FMODBeatData
    {
        public int Bar;
        public int Beat;
        public float Tempo;
        public long Position;
    }

    public class FMODBeatEvent : ASignal<FMODBeatData> { }
}
