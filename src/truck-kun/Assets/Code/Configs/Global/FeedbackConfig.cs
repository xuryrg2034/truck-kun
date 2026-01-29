using UnityEngine;

namespace Code.Configs.Global
{
    [CreateAssetMenu(fileName = "FeedbackConfig", menuName = "Truck-kun/Configs/Feedback")]
    public class FeedbackConfig : ScriptableObject
    {
        [Header("Floating Text")]
        public float TextRiseSpeed = 2f;
        public float TextDuration = 1.5f;
        public int FontSize = 32;

        [Header("Colors")]
        public Color RewardColor = new Color(0.2f, 0.8f, 0.2f);
        public Color PenaltyColor = new Color(0.9f, 0.2f, 0.2f);
        public Color ComboColor = new Color(1f, 0.8f, 0f);

        [Header("Hit Particles")]
        public int ParticleBurstCount = 15;
        public float ParticleLifetime = 1f;
        public float ParticleSpeed = 5f;

        [Header("Camera Shake")]
        public float ShakeIntensity = 0.3f;
        public float ShakeDuration = 0.15f;

        [Tooltip("Shake intensity falloff curve (0 to 1)")]
        public AnimationCurve ShakeFalloff = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f);
    }
}
