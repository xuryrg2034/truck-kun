using System.Collections.Generic;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Configs.Spawning
{
    [CreateAssetMenu(fileName = "QuestPoolConfig", menuName = "Truck-kun/Configs/QuestPool")]
    public class QuestPoolConfig : ScriptableObject
    {
        [Header("Generation")]
        [Tooltip("Minimum quests generated per day")]
        public int MinQuestsPerDay = 2;

        [Tooltip("Maximum quests generated per day")]
        public int MaxQuestsPerDay = 4;

        [Header("Quest Pool")]
        public List<QuestDefinition> AvailableQuests = new();

        /// <summary>
        /// Generate random quests for today
        /// </summary>
        public List<QuestDefinition> GenerateQuestsForDay()
        {
            int questCount = Random.Range(MinQuestsPerDay, MaxQuestsPerDay + 1);
            List<QuestDefinition> result = new();

            if (AvailableQuests.Count == 0)
                return result;

            // Shuffle and pick
            List<QuestDefinition> shuffled = new(AvailableQuests);
            for (int i = shuffled.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
            }

            for (int i = 0; i < Mathf.Min(questCount, shuffled.Count); i++)
            {
                result.Add(shuffled[i]);
            }

            return result;
        }
    }

    [System.Serializable]
    public class QuestDefinition
    {
        [Tooltip("Unique quest identifier")]
        public string QuestId;

        [Tooltip("Quest name in Russian")]
        public string NameRu;

        [Header("Target")]
        public QuestType Type;

        [Tooltip("For HitSpecificType quests")]
        public PedestrianKind TargetKind;

        [Header("Requirements")]
        public int MinTarget = 3;
        public int MaxTarget = 10;

        [Header("Reward")]
        public int BaseReward = 200;

        [Tooltip("Bonus per target above minimum")]
        public int BonusPerExtra = 20;

        /// <summary>
        /// Get randomized target for this quest
        /// </summary>
        public int GetRandomTarget()
        {
            return Random.Range(MinTarget, MaxTarget + 1);
        }

        /// <summary>
        /// Calculate reward for given completion count
        /// </summary>
        public int CalculateReward(int completed, int target)
        {
            int reward = BaseReward;
            if (completed > target)
            {
                reward += (completed - target) * BonusPerExtra;
            }
            return reward;
        }
    }

    public enum QuestType
    {
        HitCount,           // Hit N pedestrians total
        HitSpecificType,    // Hit N of specific type
        ComboChain,         // Achieve combo of N
        SpeedHit,           // Hit at speed > X
        NoPenalty           // Complete day without penalties
    }
}
