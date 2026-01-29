using UnityEngine;

namespace Code.Configs.Global
{
    [CreateAssetMenu(fileName = "EconomyConfig", menuName = "Truck-kun/Configs/Economy")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Start")]
        [Tooltip("Starting money for new game")]
        public int StartingMoney = 1000;

        [Header("Penalties")]
        [Tooltip("Penalty for hitting protected pedestrians")]
        public int ViolationPenalty = 100;

        [Header("Daily Costs")]
        [Tooltip("Cost for food deducted at end of each day")]
        public int DailyFoodCost = 100;

        [Tooltip("Minimum money required to continue (game over if below)")]
        public int MinimumRequiredMoney = 100;

        [Header("Combo System")]
        [Tooltip("Multiplier increase per consecutive hit")]
        public float ComboMultiplierStep = 0.1f;

        [Tooltip("Maximum combo multiplier")]
        public float MaxComboMultiplier = 2.0f;

        [Tooltip("Time in seconds before combo resets")]
        public float ComboResetTime = 3f;
    }
}
