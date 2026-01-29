using System.Collections.Generic;
using UnityEngine;

namespace Code.Configs.Spawning
{
    [CreateAssetMenu(fileName = "SurfaceSpawnConfig", menuName = "Truck-kun/Configs/SurfaceSpawn")]
    public class SurfaceSpawnConfig : ScriptableObject
    {
        [Header("Enable")]
        [Tooltip("If false, no surfaces will spawn")]
        public bool Enabled = true;

        [Header("Spawn Timing")]
        public float MinInterval = 5f;
        public float MaxInterval = 15f;

        [Header("Placement")]
        [Tooltip("Distance ahead of player to spawn surfaces")]
        public float SpawnDistanceAhead = 30f;

        [Tooltip("Margin from road edge")]
        public float LateralMargin = 1f;

        [Header("Spawn Pool")]
        public List<SurfaceSpawnEntry> SpawnPool = new();

        /// <summary>
        /// Get random surface prefab from pool
        /// </summary>
        public SurfaceSpawnEntry GetRandomEntry()
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
    public class SurfaceSpawnEntry
    {
        [Tooltip("Surface prefab with SurfaceSettings component")]
        public GameObject Prefab;

        [Range(0f, 10f)]
        public float Weight = 1f;

        [Tooltip("Random length range")]
        public Vector2 LengthRange = new Vector2(3f, 6f);

        [Tooltip("Random width range")]
        public Vector2 WidthRange = new Vector2(2f, 4f);

        public Vector2 GetRandomSize()
        {
            return new Vector2(
                Random.Range(LengthRange.x, LengthRange.y),
                Random.Range(WidthRange.x, WidthRange.y)
            );
        }
    }
}
