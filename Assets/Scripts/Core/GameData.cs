using deVoid.Utils;
using Game.Encounter;
using UnityEngine;

namespace Game.Core
{
    public class GameData : MonoBehaviour
    {
        static public GameData Instance;

        public int CurrentProgress {  get; private set; }

        void Awake()
        {
            CurrentProgress = 0;

            Instance = this;
        }

        public void ResetProgress()
        {
            CurrentProgress = 0;
        }

        void ReactToEncounterEnd(int encounter)
        {
            CurrentProgress = encounter;
        }

        void OnEnable()
        {
            Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);
        }

        void OnDestroy()
        {
            Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);

            Instance = null;
        }
    }
}
