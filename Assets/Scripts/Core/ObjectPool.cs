using System.Collections.Generic;
using UnityEngine;

namespace Game.Common
{
    public class ObjectPooler : MonoBehaviour
    {
        static public ObjectPooler Instance;

        // A dictionary that holds the pools for each prefab
        Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        Dictionary<GameObject, GameObject> _prefabReference = new Dictionary<GameObject, GameObject>();
        Dictionary<GameObject, Transform> _poolParents = new Dictionary<GameObject, Transform>();

        void Awake()
        {
            Instance = this;
        }

        // Returns an object from the pool or instantiates a new one if necessary
        public GameObject GetPooledObject(GameObject prefab)
        {
            if (!_poolDictionary.ContainsKey(prefab))
            {
                CreatePool(prefab, 1);
            }

            Queue<GameObject> pool = _poolDictionary[prefab];

            if (pool.Count == 0)
                ExpandPool(prefab, 1);

            GameObject objectToSpawn = pool.Dequeue();
            objectToSpawn.SetActive(true);

            return objectToSpawn;
        }

        public void ReturnToPool(GameObject obj)
        {
            obj.SetActive(false);

            if (_prefabReference.TryGetValue(obj, out GameObject prefab))
            {
                obj.transform.SetParent(_poolParents[prefab]);
                _poolDictionary[prefab].Enqueue(obj);
            }
            else
            {
                Debug.Log("The object returned does not belong to any pool.");
            }
        }

        public void ReturnAllActiveToPool(string tag)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject obj in objectsWithTag)
            {
                if (obj.activeInHierarchy)
                {
                    ReturnToPool(obj);
                }
            }
        }

        public void CreatePool(GameObject prefab, int initialSize)
        {
            if (!_poolDictionary.ContainsKey(prefab))
            {
                GameObject parent = new GameObject(prefab.name + "_Pool");
                parent.transform.SetParent(transform);
                _poolParents[prefab] = parent.transform;

                _poolDictionary[prefab] = new Queue<GameObject>();

                for (int i = 0; i < initialSize; i++)
                {
                    ExpandPool(prefab, 1);
                }
            }
        }

        private void ExpandPool(GameObject prefab, int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                GameObject obj = Instantiate(prefab, _poolParents[prefab]);
                obj.SetActive(false);
                _poolDictionary[prefab].Enqueue(obj);

                _prefabReference[obj] = prefab;
            }
        }

        void OnDestroy()
        {
            Instance = null;
        }
    }
}