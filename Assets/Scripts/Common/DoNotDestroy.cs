using UnityEngine;

namespace Game.Helper
{
    public class DoNotDestroy : MonoBehaviour
    {
        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}

