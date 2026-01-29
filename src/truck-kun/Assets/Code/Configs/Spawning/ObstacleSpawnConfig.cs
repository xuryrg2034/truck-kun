using System.Collections.Generic;
using UnityEngine;

namespace Code.Configs.Spawning
{
    [CreateAssetMenu(fileName = "ObstacleSpawnConfig", menuName = "Truck-kun/Configs/ObstacleSpawn")]
    public class ObstacleSpawnConfig : ScriptableObject
    {
        [Header("Enable")]
        [Tooltip("If false, no obstacles will spawn")]
        public bool Enabled = true;

        [Header("Spawn Timing")]
        public float MinInterval = 8f;
        public float MaxInterval = 20f;

        [Header("Placement")]
        [Tooltip("Distance ahead of player to spawn obstacles")]
        public float SpawnDistanceAhead = 35f;

        [Header("Spawn Pool")]
        public List<ObstacleSpawnEntry> SpawnPool = new();

        /// <summary>
        /// Get random obstacle entry from pool
        /// </summary>
        public ObstacleSpawnEntry GetRandomEntry()
        {
            if (!Enabled || SpawnPool == null || SpawnPool.Count == 0)
                return null;

            float totalWeight = 0f;
            foreach (var entry in SpawnPool)
            {
                if (entry.Prefab != null)
                    totalWeight += entry.Weight;
            }

            if (totalWeight <= 0f)
                return null;

            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var entry in SpawnPool)
            {
                if (entry.Prefab == null)
                    continue;

                cumulative += entry.Weight;
                if (random <= cumulative)
                    return entry;
            }

            return SpawnPool[0];
        }

        /// <summary>
        /// Get random spawn interval
        /// </summary>
        public float GetRandomInterval()
        {
            return Random.Range(MinInterval, MaxInterval);
        }
    }

    [System.Serializable]
    public class ObstacleSpawnEntry
    {
        [Tooltip("Obstacle prefab with ObstacleSettings component")]
        public GameObject Prefab;

        [Range(0f, 10f)]
        public float Weight = 1f;

        [Tooltip("Where on the road this obstacle can spawn")]
        public ObstaclePlacement Placement;
    }

    public enum ObstaclePlacement
    {
        Road,           // On the road (barriers, holes)
        Roadside,       // At road edge (ramps)
        CrossingPath    // In pedestrian crossing area
    }
}
