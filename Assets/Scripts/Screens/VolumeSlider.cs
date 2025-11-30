using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.GameUI
{
    public class VolumeSlider : MonoBehaviour
    {
        [SerializeField] Slider _volumeSlider;
        [SerializeField] Image _volumeIcon;

        [SerializeField] List<Sprite> _volumeIcons = new List<Sprite>();

        void Awake()
        {
            _volumeSlider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void OnSliderValueChanged(float value)
        {
            //if (value == 0)
            //    _volumeIcon.sprite = _volumeIcons[0];
            //else if(value <= 0.1f)
            //    _volumeIcon.sprite = _volumeIcons[1];
            //else if (value <= 0.5f)
            //    _volumeIcon.sprite = _volumeIcons[2];
            //else if (value <= 0.9f)
            //    _volumeIcon.sprite = _volumeIcons[3];
            //else
            //    _volumeIcon.sprite = _volumeIcons[4];
        }
    }
}
