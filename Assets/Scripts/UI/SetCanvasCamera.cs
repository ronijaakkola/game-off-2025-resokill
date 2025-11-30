using UnityEngine;

namespace Game.GameUI
{
    public class SetCanvasCamera : MonoBehaviour
    {
        void Start()
        {
            var canvas = GetComponent<Canvas>();
            canvas.worldCamera = Camera.main;
            canvas.planeDistance = 0.4f;
        }
    }
}
