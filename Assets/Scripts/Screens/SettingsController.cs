using UnityEngine;
using UnityEngine.UI;
using deVoid.UIFramework;
using Game.Audio;
using Game.GameInput;

namespace Game.GameScreen
{
    public class SettingsController : Screen<WindowProperties>
    {
        [Header("Volume Sliders")]
        [SerializeField] Slider musicVolumeSlider;
        [SerializeField] Slider sfxVolumeSlider;
        [SerializeField] Slider mouseSensitivitySlider;

        override protected void Awake()
        {
            base.Awake();

            musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            mouseSensitivitySlider.onValueChanged.AddListener(OnMouseSenseChanged);
        }

        protected override void OnOpenScreen()
        {
            OnDataUpdated();

            base.OnOpenScreen();
        }

        protected override void OnCloseScreen()
        {
            base.OnCloseScreen();
        }

        void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance.ChangeMusicVolume(value);
        }

        void OnSfxVolumeChanged(float value)
        {
            AudioManager.Instance.ChangeSfxVolume(value);
        }

        void OnMouseSenseChanged(float value)
        {
            InputManager.Instance.MouseSensitivity = value;
        }

        void OnDataUpdated()
        {
            musicVolumeSlider.value = AudioManager.Instance.MusicVolume;
            sfxVolumeSlider.value = AudioManager.Instance.SfxVolume;
            mouseSensitivitySlider.value = InputManager.Instance.MouseSensitivity;
        }

        public void DragSfxDragStopped()
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.ShootPerfect, transform.position);
        }

        protected override void OnDestroy()
        {
            musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            sfxVolumeSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            mouseSensitivitySlider.onValueChanged.RemoveListener(OnMouseSenseChanged);

            base.OnDestroy();
        }
    }
}
