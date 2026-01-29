using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian
{
    /// <summary>
    /// Settings component for Pedestrian prefabs.
    /// Each pedestrian type has its own prefab with these values configured.
    /// </summary>
    public class PedestrianSettings : MonoBehaviour
    {
        [Header("Identity")]
        public PedestrianKind Kind;
        public PedestrianCategory Category;

        [Header("Rewards")]
        [Tooltip("Base reward for hitting this pedestrian")]
        public int BaseReward = 100;

        [Tooltip("Penalty if this is a protected pedestrian")]
        public int PenaltyIfProtected = 150;

        [Header("Physics")]
        [Tooltip("Mass in kg for ragdoll physics")]
        public float Mass = 70f;

        [Tooltip("Linear drag for physics movement")]
        public float Drag = 2f;

        [Tooltip("Angular drag to reduce tumbling")]
        public float AngularDrag = 1f;

        [Header("Movement")]
        [Tooltip("Walking speed in m/s")]
        public float WalkSpeed = 2f;

        [Tooltip("Force applied for physics-based movement")]
        public float MovementForce = 500f;

        [Header("Visual")]
        [Tooltip("Tilt angle when walking (degrees)")]
        public float TiltAngle = 15f;

        /// <summary>
        /// Get reward or penalty based on category
        /// </summary>
        public int GetRewardOrPenalty()
        {
            return Category == PedestrianCategory.Protected
                ? -PenaltyIfProtected
                : BaseReward;
        }

        /// <summary>
        /// Check if hitting this pedestrian is a violation
        /// </summary>
        public bool IsViolation => Category == PedestrianCategory.Protected;

#if UNITY_EDITOR
        [Header("Debug")]
        public bool ShowGizmos = true;

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            // Color by category
            Gizmos.color = Category == PedestrianCategory.Protected
                ? new Color(1f, 0.5f, 0.5f, 0.5f)  // Red for protected
                : new Color(0.5f, 1f, 0.5f, 0.5f); // Green for normal

            Gizmos.DrawWireSphere(transform.position + Vector3.up * 0.5f, 0.5f);

            // Draw walk direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * WalkSpeed);
        }
#endif
    }
}
