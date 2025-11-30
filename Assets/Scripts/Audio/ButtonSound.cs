using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Audio
{
    public class ButtonSound : MonoBehaviour, IPointerDownHandler
    {
        public void OnPointerDown(PointerEventData eventData)
        {
            AudioManager.Instance.PlayOneShot(AudioManager.Instance.AudioDataInstance.Button, transform.position);
        }
    }
}
