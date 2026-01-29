using Code.Configs.Global;
using Code.Configs.Spawning;
using UnityEngine;

namespace Code.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Truck-kun/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Info")]
        public string LevelId;
        public string LevelName;

        [Header("Vehicle")]
        public VehicleConfig Vehicle;

        [Header("Global Configs")]
        public EconomyConfig Economy;
        public FeedbackConfig Feedback;

        [Header("Level-Specific Configs")]
        public DayConfig Day;
        public PedestrianSpawnConfig PedestrianSpawn;
        public QuestPoolConfig QuestPool;

        [Header("Optional Spawners")]
        [Tooltip("Null = no surface hazards on this level")]
        public SurfaceSpawnConfig SurfaceSpawn;

        [Tooltip("Null = no obstacles on this level")]
        public ObstacleSpawnConfig ObstacleSpawn;
    }
}
