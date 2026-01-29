using System.Collections.Generic;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Configs.Spawning
{
    [CreateAssetMenu(fileName = "PedestrianSpawnConfig", menuName = "Truck-kun/Configs/PedestrianSpawn")]
    public class PedestrianSpawnConfig : ScriptableObject
    {
        [Header("Spawn Timing")]
        public float MinSpawnInterval = 1f;
        public float MaxSpawnInterval = 3f;

        [Header("Spawn Limits")]
        [Tooltip("Maximum active pedestrians at once")]
        public int MaxActive = 12;

        [Header("Allowed Types")]
        [Tooltip("Which pedestrian types can spawn. Empty = all types.")]
        public List<PedestrianKind> AllowedKinds = new();

        [Header("Behavior")]
        [Range(0f, 1f)]
        [Tooltip("Chance that pedestrian will cross the road")]
        public float CrossingChance = 0.7f;

        [Tooltip("Speed multiplier for crossing pedestrians")]
        public float CrossingSpeedMultiplier = 1f;

        [Tooltip("Rotate pedestrian to face crossing direction")]
        public bool RotateToCrossingDirection = true;

        [Tooltip("Distance behind player when pedestrian despawns")]
        public float DespawnDistance = 25f;

        [Header("Spawn Position")]
        [Tooltip("Fixed Y position for spawning (ground level)")]
        public float SpawnY = 0f;

        [Tooltip("Minimum distance ahead of player to spawn")]
        public float MinSpawnDistanceAhead = 25f;

        [Tooltip("Random Z variation added to min distance")]
        public float SpawnZVariation = 10f;

        [Header("Road Bounds")]
        [Tooltip("Total road width for spawning")]
        public float RoadWidth = 8f;

        [Tooltip("Extra offset from road edge for spawning on sidewalk")]
        public float SidewalkOffset = 1.5f;

        [Tooltip("Margin from road edges for regular spawning")]
        public float LateralMargin = 0.5f;

        [Header("Spawn Validation")]
        [Tooltip("Check for obstacles before spawning")]
        public bool CheckOverlap = true;

        [Tooltip("Radius for overlap check")]
        public float OverlapRadius = 0.5f;

        [Tooltip("Layer mask for obstacle detection")]
        public LayerMask ObstacleLayer;

        [Tooltip("Layer mask for ground detection (raycast down to find surface)")]
        public LayerMask GroundLayer;

        [Tooltip("Maximum attempts to find valid spawn position")]
        public int MaxSpawnAttempts = 3;

        [Header("Spawn Pool")]
        public List<PedestrianSpawnEntry> SpawnPool = new();

        /// <summary>
        /// Get a random prefab from the pool based on weights
        /// </summary>
        public GameObject GetRandomPrefab(int currentDay = 1)
        {
            if (SpawnPool == null || SpawnPool.Count == 0)
                return null;

            // Filter by day availability
            List<PedestrianSpawnEntry> available = new();
            float totalWeight = 0f;

            foreach (var entry in SpawnPool)
            {
                if (entry.Prefab == null)
                    continue;

                if (currentDay < entry.MinDay)
                    continue;

                if (entry.MaxDay > 0 && currentDay > entry.MaxDay)
                    continue;

                available.Add(entry);
                totalWeight += entry.Weight;
            }

            if (available.Count == 0)
                return null;

            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var entry in available)
            {
                cumulative += entry.Weight;
                if (random <= cumulative)
                    return entry.Prefab;
            }

            return available[0].Prefab;
        }

        /// <summary>
        /// Get random spawn interval
        /// </summary>
        public float GetRandomInterval()
        {
            return Random.Range(MinSpawnInterval, MaxSpawnInterval);
        }
    }

    [System.Serializable]
    public class PedestrianSpawnEntry
    {
        [Tooltip("Pedestrian prefab with PedestrianSettings component")]
        public GameObject Prefab;

        [Range(0f, 10f)]
        [Tooltip("Spawn weight (higher = more likely)")]
        public float Weight = 1f;

        [Tooltip("First day this pedestrian can appear")]
        public int MinDay = 1;

        [Tooltip("Last day this pedestrian can appear (0 = no limit)")]
        public int MaxDay = 0;
    }
}
