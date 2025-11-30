using deVoid.Utils;
using FMOD.Studio;
using FMODUnity;
using Game.Encounter;
using Game.GameScreen;
using Game.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Audio
{
    public enum MusicType : int
    {
        EncounterOne = 1,
        EncounterTwo,
        EncounterThree,
    }

    public class AudioManager : MonoBehaviour
    {
        static public AudioManager Instance { get; private set; }

        [SerializeField] AudioData audioData;

        List<EventInstance> eventInstances = new List<EventInstance>();
        List<StudioEventEmitter> eventEmitters = new List<StudioEventEmitter>();

        // FMODEvents
        public AudioData AudioDataInstance
        {
            get;
            private set;
        }

        EventInstance menuMusic;
        EventInstance gameMusic;

        float musicVolume = 0.5f;
        float sfxVolume = 0.5f;

        // Buses
        Bus musicBus;
        Bus sfxBus;

        public float MusicVolume => musicVolume;
        public float SfxVolume => sfxVolume;

        void Awake()
        {
            musicBus = RuntimeManager.GetBus("bus:/Music");
            sfxBus = RuntimeManager.GetBus("bus:/SFX");

            AudioDataInstance = GameObject.Instantiate(audioData);

            menuMusic = CreateEventInstance(AudioDataInstance.Menu);
            menuMusic.start();

            Init();

            Instance = this;
        }

        public void Init()
        {
            musicBus.setVolume(MusicVolume);
            sfxBus.setVolume(SfxVolume);
        }

        public void SetBGMusicParameter(string parameterName, float parameterValue, float duration)
        {
            gameMusic.setParameterByName(parameterName, parameterValue);
        }

        public int GetMusicTimelinePositionMs()
        {
            int timelinePositionMs = 0;
            FMOD.RESULT result = gameMusic.getTimelinePosition(out timelinePositionMs);

            if (result == FMOD.RESULT.OK)
            {
                return timelinePositionMs;
            }

            return 0;
        }

        public int GetMusicLengthMs()
        {
            EventDescription eventDescription;
            FMOD.RESULT result = gameMusic.getDescription(out eventDescription);

            if (result == FMOD.RESULT.OK)
            {
                int lengthMs = 0;
                result = eventDescription.getLength(out lengthMs);

                if (result == FMOD.RESULT.OK)
                {
                    return lengthMs;
                }
            }

            return 0;
        }

        // NOTE: Oneshot sounds
        public void PlayOneShot(EventReference sound, Vector3 position)
        {
            RuntimeManager.PlayOneShot(sound, position);
        }

        // NOTE: Continuesly called sound sources for on/off sounds
        public EventInstance CreateEventInstance(EventReference eventReference)
        {
            EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
            eventInstances.Add(eventInstance);
            return eventInstance;
        }

        // NOTE: Continuesly called sound sources for single sounds
        public StudioEventEmitter InitializeEventEmitter(EventReference eventReference, StudioEventEmitter emitter)
        {
            emitter.EventReference = eventReference;
            eventEmitters.Add(emitter);
            return emitter;
        }

        public void ChangeMusicVolume(float value)
        {
            musicVolume = value;
            musicBus.setVolume(MusicVolume);
        }

        public void ChangeSfxVolume(float value)
        {
            sfxVolume = value;
            sfxBus.setVolume(SfxVolume);
        }

        [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private static FMOD.RESULT TimelineCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            if (type == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
            {
                var instance = new EventInstance(instancePtr);
                var parameter = (TIMELINE_MARKER_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));

                string markerName = parameter.name;

                //Debug.Log("FMOD: Hit marker " + parameter.name);
            }
            else if (type == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
            {
                var beatProps = (TIMELINE_BEAT_PROPERTIES)System.Runtime.InteropServices.Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_BEAT_PROPERTIES));

                var beatData = new FMODBeatData
                {
                    Bar = beatProps.bar,
                    Beat = beatProps.beat,
                    Tempo = beatProps.tempo,
                    Position = beatProps.position
                };

                Signals.Get<FMODBeatEvent>().Dispatch(beatData);

                //Debug.Log($"FMOD Beat: Bar {beatProps.bar}, Beat {beatProps.beat}, Tempo {beatProps.tempo} BPM, Position {beatProps.position}ms");
            }

            return FMOD.RESULT.OK;
        }

        public void SetMusic(MusicType type)
        {
            menuMusic.setPaused(true);

            if (gameMusic.isValid())
            {
                gameMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                gameMusic.setCallback(null);
                gameMusic.release();
                gameMusic.clearHandle();
            }

            if (type == MusicType.EncounterOne)
            {
                gameMusic = CreateEventInstance(AudioDataInstance.EncounterOne);
            }
            else if (type == MusicType.EncounterTwo)
            {
                gameMusic = CreateEventInstance(AudioDataInstance.EncounterTwo);
            }
            else if (type == MusicType.EncounterThree)
            {
                gameMusic = CreateEventInstance(AudioDataInstance.EncounterThree);
            }

            gameMusic.start();
            gameMusic.setCallback(TimelineCallback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER | EVENT_CALLBACK_TYPE.TIMELINE_BEAT);
        }

        public void MainMenuOpened()
        {
            if (gameMusic.isValid())
            {
                gameMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                gameMusic.setCallback(null);
                gameMusic.release();
                gameMusic.clearHandle();
            }

            menuMusic.setPaused(false);
        }

        void ReactToGamePause(bool paused)
        {
            if (paused)
            {
                gameMusic.setVolume(0f); // instant mute
            }
            else
            {
                gameMusic.setVolume(1.0f);
            }

            gameMusic.setPaused(paused);

            if (gameMusic.isValid())
            {
                menuMusic.setPaused(!paused);
            }
        }

        void ReactToEncounterEnd(int id)
        {
            gameMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            gameMusic.setCallback(null);
            gameMusic.release();
            gameMusic.clearHandle();
        }

        void ReactToPlayerDeath()
        {
            if (gameMusic.isValid())
            {
                gameMusic.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }

        void CleanUp()
        {
            foreach (EventInstance eventInstance in eventInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }

            foreach (StudioEventEmitter emitter in eventEmitters)
            {
                emitter.Stop();
            }
        }

        void OnDestroy()
        {
            CleanUp();
        }

        void OnEnable()
        {
            Signals.Get<Game_Pause>().AddListener(ReactToGamePause);
            Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);
            Signals.Get<PlayerDiedEvent>().AddListener(ReactToPlayerDeath);
        }

        void OnDisable()
        {
            Signals.Get<Game_Pause>().RemoveListener(ReactToGamePause);
            Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);
            Signals.Get<PlayerDiedEvent>().RemoveListener(ReactToPlayerDeath);
        }
    }
}
