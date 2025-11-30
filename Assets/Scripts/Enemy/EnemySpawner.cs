using deVoid.Utils;
using Game.Common;
using Game.Encounter;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Game.CharacterEnemy
{
    public class Enemy_ChangeSpawnSide : ASignal<List<Direction>> { }
    public class Enemy_Died : ASignal<GameObject> { }

    [Serializable]
    public class EnemyWaveData2
    {
        [Header("Wave Timing")]
        public float duration;
        public float waveBreak;

        [Header("Spawn Sides")]
        public List<Direction> sides = new List<Direction>();

        [Header("Enemy Level Lists (1, 2, 3)")]
        public int normalEnemies;
        public int toughEnemies;
        public int fastEnemies;
    }

    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] GameObject bossProp;

        [SerializeField] GameObject enemyPrefabNormal;
        [SerializeField] GameObject enemyPrefabTough;
        [SerializeField] GameObject enemyPrefabFast;

        [SerializeField] private List<EnemyWaveData2> enemyWaves1 = new();
        [SerializeField] private List<EnemyWaveData2> enemyWaves2 = new();
        [SerializeField] private List<EnemyWaveData2> enemyWaves3 = new();

        bool spawnEnemies = false;

        List<Transform> aliveEnemyList = new List<Transform>();
        int EnemiesAlive =>  aliveEnemyList.Count;

        public List<Transform> Enemies => aliveEnemyList;

        int enemySpawnedCount = 0;
        int enemyMaxCount = 0;

        public bool AllEnemiesSpawned => enemySpawnedCount >= enemyMaxCount;
        public bool WaveDone => AllEnemiesSpawned && EnemiesAlive == 0;


        TimeLeftClock breakTimer;
        TimeLeftClock waveTimer;

        TimeLeftClock initialWaitTimer;

        int currentEncounter = 0;
        private int currentWaveIndex = 0;

        private bool spawningWave = false;
        private bool inBreak = false;

        private List<int> spawnQueue = new();
        private int spawnIndex = 0;

        Transform player;

        void Awake()
        {
            spawnEnemies = false;

            bossProp?.SetActive(false);
        }

        void Start()
        {
            player = FindAnyObjectByType<PlayerHealth>().transform;
        }

        void Update()
        {
            if (spawnEnemies)
            {
                if (currentWaveIndex >= GetCurrentWaveListCount())
                    return;

                if (!spawningWave && !inBreak)
                {
                    StartWave();
                }

                if (spawningWave)
                {
                    UpdateWaveSpawning();
                }
                else if (inBreak)
                {
                    UpdateBreak();
                }
            }
        }

        int GetCurrentWaveListCount()
        {
            return currentEncounter switch
            {
                2 => enemyWaves2.Count,
                3 => enemyWaves3.Count,
                _ => enemyWaves1.Count
            };
        }

        private void StartWave()
        {
            if (initialWaitTimer.IsTimeOver())
            {
                EnemyWaveData2 wave = GetWaveData();

                spawnQueue.Clear();

                // Build spawn list
                for (int i = 0; i < wave.normalEnemies; i++)
                    spawnQueue.Add(1);

                for (int i = 0; i < wave.toughEnemies; i++)
                    spawnQueue.Add(2);

                for (int i = 0; i < wave.fastEnemies; i++)
                    spawnQueue.Add(3);

                for (int i = 0; i < spawnQueue.Count; i++)
                {
                    int rand = UnityEngine.Random.Range(i, spawnQueue.Count);
                    (spawnQueue[i], spawnQueue[rand]) = (spawnQueue[rand], spawnQueue[i]);
                }

                if (waveTimer == null)
                {
                    float spawnTime = wave.duration / spawnQueue.Count;
                    waveTimer = new TimeLeftClock(spawnTime);
                }
                else
                {
                    float spawnTime = wave.duration / spawnQueue.Count;
                    waveTimer.ChangeTimeToTrack(spawnTime, true);
                }

                spawnIndex = 0;
                spawningWave = true;

                Signals.Get<Enemy_ChangeSpawnSide>().Dispatch(wave.sides);

                string sidesLine = "";
                foreach (var side in wave.sides)
                {
                    sidesLine += side + " ";
                }

                Debug.Log("Changing enemy spawn sides: " + sidesLine.Trim());

                //if (currentEncounter >= 1 && currentWaveIndex == 1)
                if (currentEncounter >= 3 && currentWaveIndex == (enemyWaves3.Count - 1))
                {
                    bossProp?.SetActive(true);
                }
            }
        }

        private void UpdateWaveSpawning()
        {
            EnemyWaveData2 wave = GetWaveData();

            // spawn enemies while timer is active
            if (waveTimer.IsTimeOver())
            {
                int enemyType = spawnQueue[spawnIndex];
                spawnIndex++;

                Direction side = wave.sides[UnityEngine.Random.Range(0, wave.sides.Count)];
                SpawnEnemy(enemyType, side);

                waveTimer.ResetTimer();
            }

            if (spawnIndex >= spawnQueue.Count)
            {
                // stop spawning
                spawningWave = false;
                inBreak = true;

                if (breakTimer == null)
                {
                    breakTimer = new TimeLeftClock(wave.waveBreak);
                }
                else
                {
                    breakTimer.ChangeTimeToTrack(wave.waveBreak, true);
                }
            }
        }

        private void UpdateBreak()
        {
            if (breakTimer.IsTimeOver())
            {
                inBreak = false;
                currentWaveIndex++;
            }
        }

        public void StartSpawningEnemies(int wave)
        {
            if (wave > 3)
            {
                Debug.Log("No wave for that index: " + wave);
                return;
            }

            if (initialWaitTimer == null)
            {
                initialWaitTimer = new TimeLeftClock(15);
            }

            if (wave < 2)
            {
                initialWaitTimer.ChangeTimeToTrack(15, true);
            }
            else
            {
                initialWaitTimer.ChangeTimeToTrack(10, true);
            }

            spawnEnemies = true;
            spawningWave = false;
            currentWaveIndex = 0;
            currentEncounter = wave;
            inBreak = false;
        }

        EnemyWaveData2 GetWaveData()
        {
            if (currentEncounter == 2)
            {
                return enemyWaves2[currentWaveIndex];
            }
            else if (currentEncounter == 3)
            {
                return enemyWaves3[currentWaveIndex];
            }
            else
            {
                return enemyWaves1[currentWaveIndex];
            }
        }

        GameObject SpawnEnemy(int enemyLevel, Direction side)
        {
            GameObject enemyOrNull = null;
            if (spawnEnemies)
            {
                if (enemyLevel == 1)
                {
                    enemyOrNull = SpawnEnemy(enemyLevel, enemyPrefabNormal, side);
                }
                else if (enemyLevel == 2)
                {
                    enemyOrNull = SpawnEnemy(enemyLevel, enemyPrefabTough, side);
                }
                else if (enemyLevel == 3)
                {
                    enemyOrNull = SpawnEnemy(enemyLevel, enemyPrefabFast, side);
                }
            }

            return enemyOrNull;
        }

        GameObject SpawnEnemy(int enemyLevel, GameObject enemy, Direction side)
        {
            GameObject enemyOrNull = null;

            Vector3 v3Pos = GetRandomPositionFromSide(side, player, 5f);

            GameObject obj = ObjectPooler.Instance.GetPooledObject(enemy);
            if (obj != null)
            {
                obj.gameObject.SetActive(true);

                Enemy enemyAI = obj.GetComponent<Enemy>();
                enemyAI.ReviveEnemy(enemyLevel, v3Pos);
                aliveEnemyList.Add(obj.transform);

                enemyOrNull = obj;
            }

            return enemyOrNull;
        }

        public void ReactToEnemyDeath(GameObject enemy)
        {
            aliveEnemyList.Remove(enemy.transform);
        }

        private bool spawnAreaInit = false;
        private Vector3 minBounds, maxBounds;
        [SerializeField] float spawnStripWidth = 1f;
        Vector3 GetRandomPositionFromSide(Direction side, Transform player, float minDistanceFromPlayer)
        {
            if (!spawnAreaInit)
            {
                var navMeshData = NavMesh.CalculateTriangulation();
                if (navMeshData.vertices.Length == 0)
                {
                    Debug.LogError("No NavMesh found!");
                    return Vector3.zero;
                }

                minBounds = navMeshData.vertices[0];
                maxBounds = navMeshData.vertices[0];

                foreach (var v in navMeshData.vertices)
                {
                    minBounds = Vector3.Min(minBounds, v);
                    maxBounds = Vector3.Max(maxBounds, v);
                }

                spawnAreaInit = true;
            }

            Vector3 spawnPos = Vector3.zero;
            Vector3 lastValidPos = Vector3.zero;
            bool foundValid = false;

            const int maxAttempts = 20;
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                attempts++;

                switch (side)
                {
                    case Direction.North:
                        spawnPos.z = maxBounds.z - UnityEngine.Random.Range(0f, spawnStripWidth);
                        spawnPos.x = UnityEngine.Random.Range(minBounds.x, maxBounds.x);
                        break;

                    case Direction.East:
                        spawnPos.x = maxBounds.x - UnityEngine.Random.Range(0f, spawnStripWidth);
                        spawnPos.z = UnityEngine.Random.Range(minBounds.z, maxBounds.z);
                        break;

                    case Direction.South:
                        spawnPos.z = minBounds.z + UnityEngine.Random.Range(0f, spawnStripWidth);
                        spawnPos.x = UnityEngine.Random.Range(minBounds.x, maxBounds.x);
                        break;

                    case Direction.West:
                        spawnPos.x = minBounds.x + UnityEngine.Random.Range(0f, spawnStripWidth);
                        spawnPos.z = UnityEngine.Random.Range(minBounds.z, maxBounds.z);
                        break;
                }

                // Try NavMesh projection
                if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    spawnPos = hit.position;
                }

                // Always remember last sampled position
                lastValidPos = spawnPos;

                // Check distance
                if (Vector3.Distance(spawnPos, player.position) >= minDistanceFromPlayer)
                {
                    foundValid = true;
                    break;
                }
            }

            // Fallback: return last sampled position even if too close
            return foundValid ? spawnPos : lastValidPos;
        }

        void ReactToEncounterEnd(int id)
        {
            spawnEnemies = false;
        }

        void OnEnable()
        {
            Signals.Get<Enemy_Died>().AddListener(ReactToEnemyDeath);
            Signals.Get<Encounter_End>().AddListener(ReactToEncounterEnd);
        }

        void OnDisable()
        {
            Signals.Get<Enemy_Died>().RemoveListener(ReactToEnemyDeath);
            Signals.Get<Encounter_End>().RemoveListener(ReactToEncounterEnd);
        }
    }
}
