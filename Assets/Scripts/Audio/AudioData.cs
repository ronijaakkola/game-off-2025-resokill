using FMODUnity;
using UnityEngine;

namespace Game.Audio
{
    [CreateAssetMenu(fileName = "Audio", menuName = "ScriptableObjects/Audio")]
    public class AudioData : ScriptableObject
    {
        [field: Header("Music")]
        [field: SerializeField] public EventReference EncounterOne { get; private set; }
        [field: SerializeField] public EventReference EncounterTwo { get; private set; }
        [field: SerializeField] public EventReference EncounterThree { get; private set; }
        [field: SerializeField] public EventReference Menu { get; private set; }

        [field: Header("Player SFX")]
        [field: SerializeField] public EventReference PlayerFootsteps { get; private set; }
        [field: SerializeField] public EventReference PlayerJump { get; private set; }
        [field: SerializeField] public EventReference PlayerDamaged { get; private set; }
        [field: SerializeField] public EventReference ShootPerfect { get; private set; }
        [field: SerializeField] public EventReference ShootGood { get; private set; }
        [field: SerializeField] public EventReference ShootMissed { get; private set; }
        [field: SerializeField] public EventReference ShootSpecialGood { get; private set; }
        [field: SerializeField] public EventReference ShootSpecialPerfect { get; private set; }
        [field: SerializeField] public EventReference PlayerHeal { get; private set; }
        [field: SerializeField] public EventReference PlayerDanger { get; private set; }

        [field: Header("Game SFX")]
        [field: SerializeField] public EventReference LoadCasette { get; private set; }
        [field: SerializeField] public EventReference UnloadCasette { get; private set; }
        [field: SerializeField] public EventReference GameOver { get; private set; }
        [field: SerializeField] public EventReference Metronome { get; private set; }
        [field: SerializeField] public EventReference Button { get; private set; }

        [field: SerializeField] public bool MetronomeOnBeat { get; set; } = false;
        [field: SerializeField] public bool MetronomeOnPreBeat { get; set; } = false;
        [field: SerializeField] public bool MetronomeOnPostBeat { get; set; } = false;
    }
}
