using UnityEngine;

namespace Code.Gameplay.Features.Hero
{
    /// <summary>
    /// Settings component for Hero prefab.
    /// Defines physics and movement parameters for this specific vehicle.
    /// </summary>
    public class HeroSettings : MonoBehaviour
    {
        [Header("Physics Body")]
        [Tooltip("Mass of the vehicle in kg")]
        public float Mass = 1500f;

        [Tooltip("Angular drag to prevent spinning")]
        public float AngularDrag = 5f;

        public bool UseGravity = true;

        [Tooltip("Use continuous collision detection for fast movement")]
        public bool UseContinuousCollision = true;

        [Header("Speed Limits")]
        [Tooltip("Maximum forward speed in m/s")]
        public float MaxForwardSpeed = 15f;

        [Tooltip("Minimum forward speed (truck never stops completely)")]
        public float MinForwardSpeed = 3f;

        [Tooltip("Maximum lateral (sideways) speed in m/s")]
        public float MaxLateralSpeed = 8f;

        [Header("Acceleration")]
        [Tooltip("Forward acceleration in m/s²")]
        public float ForwardAcceleration = 10f;

        [Tooltip("Lateral acceleration in m/s²")]
        public float LateralAcceleration = 15f;

        [Tooltip("Deceleration when no input in m/s²")]
        public float Deceleration = 8f;

        [Header("Drag")]
        [Tooltip("Base air resistance drag")]
        public float BaseDrag = 0.5f;

#if UNITY_EDITOR
        [Header("Debug")]
        [Tooltip("Show gizmos in editor")]
        public bool ShowGizmos = true;

        private void OnDrawGizmosSelected()
        {
            if (!ShowGizmos) return;

            // Draw speed indicator
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * MaxForwardSpeed * 0.5f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, transform.right * MaxLateralSpeed * 0.5f);
            Gizmos.DrawRay(transform.position, -transform.right * MaxLateralSpeed * 0.5f);
        }
#endif
    }
}
