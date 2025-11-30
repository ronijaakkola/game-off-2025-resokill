using UnityEngine;
using System.Collections.Generic;

namespace Game.Core
{
    public class GameManager : MonoBehaviour
    {
        static public GameManager Instance;

        [SerializeField] List<GameObject> gameCore = new List<GameObject>();

        void Awake()
        {
            if (Instance == null)
            {
                foreach(var gObject in gameCore)
                {
                    var instance = Instantiate(gObject, transform);
                    instance.name = gObject.name;
                }

                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
