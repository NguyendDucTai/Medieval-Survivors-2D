using System.Collections.Generic;
using UnityEngine;

namespace VampireSurvivors.Core
{
    [System.Serializable]
    public class Pool
    {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance;

        public List<Pool> pools;
        public Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            Instance = this;
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.size; i++)
                {
                    GameObject obj = Instantiate(pool.prefab);
                    obj.SetActive(false);
                    obj.transform.SetParent(transform);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.tag, objectPool);
            }
        }

        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
                return null;
            }

            Queue<GameObject> queue = poolDictionary[tag];

            if (queue.Count == 0)
            {
                // Optionally expand pool dynamically if depleted
                foreach (Pool p in pools)
                {
                    if (p.tag == tag)
                    {
                        GameObject obj = Instantiate(p.prefab);
                        obj.SetActive(false);
                        obj.transform.SetParent(transform);
                        queue.Enqueue(obj);
                        break;
                    }
                }
            }

            GameObject objectToSpawn = queue.Dequeue();
            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            // Re-enqueue for recycling later
            queue.Enqueue(objectToSpawn);

            return objectToSpawn;
        }
    }
}
