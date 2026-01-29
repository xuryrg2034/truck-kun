using UnityEngine;

namespace Code.Configs.Spawning
{
    [CreateAssetMenu(fileName = "DayConfig", menuName = "Truck-kun/Configs/Day")]
    public class DayConfig : ScriptableObject
    {
        [Header("Session Duration")]
        [Tooltip("Day duration in seconds")]
        public float DurationSeconds = 60f;

        [Header("Difficulty Scaling")]
        [Tooltip("Spawn rate multiplier over day duration (X: 0-1 normalized time, Y: multiplier)")]
        public AnimationCurve SpawnRateMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);

        [Tooltip("Pedestrian speed multiplier over day duration")]
        public AnimationCurve PedestrianSpeedMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 1.2f);

        [Header("Events")]
        [Tooltip("Second when rush hour starts")]
        public float RushHourStart = 30f;

        [Tooltip("Spawn multiplier during rush hour")]
        public float RushHourSpawnMultiplier = 2f;

        /// <summary>
        /// Get spawn rate multiplier for current time
        /// </summary>
        public float GetSpawnMultiplier(float elapsedTime)
        {
            float normalizedTime = Mathf.Clamp01(elapsedTime / DurationSeconds);
            float curveMultiplier = SpawnRateMultiplier.Evaluate(normalizedTime);

            if (elapsedTime >= RushHourStart)
                curveMultiplier *= RushHourSpawnMultiplier;

            return curveMultiplier;
        }

        /// <summary>
        /// Get pedestrian speed multiplier for current time
        /// </summary>
        public float GetSpeedMultiplier(float elapsedTime)
        {
            float normalizedTime = Mathf.Clamp01(elapsedTime / DurationSeconds);
            return PedestrianSpeedMultiplier.Evaluate(normalizedTime);
        }
    }
}
