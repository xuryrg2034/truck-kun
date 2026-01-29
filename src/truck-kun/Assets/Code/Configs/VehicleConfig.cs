using Code.Infrastructure.View;
using UnityEngine;

namespace Code.Configs
{
    /// <summary>
    /// Configuration for player vehicle.
    /// Single source of truth for all vehicle parameters.
    /// </summary>
    [CreateAssetMenu(fileName = "VehicleConfig", menuName = "Truck-kun/Vehicle Config")]
    public class VehicleConfig : ScriptableObject
    {
        [Header("Identity")]
        [Tooltip("Unique identifier for this vehicle type")]
        public string VehicleId = "truck_default";

        [Header("Physics Body")]
        [Tooltip("Mass of the vehicle in kg")]
        public float Mass = 1000f;

        [Tooltip("Angular drag to prevent spinning")]
        public float AngularDrag = 0.05f;

        [Tooltip("Enable gravity for ramps and jumps")]
        public bool UseGravity = true;

        [Tooltip("Use continuous collision detection for fast movement")]
        public bool UseContinuousCollision = true;

        [Header("Speed Limits")]
        [Tooltip("Base forward speed in m/s")]
        public float BaseForwardSpeed = 15f;

        [Tooltip("Minimum forward speed - truck never stops completely")]
        public float MinForwardSpeed = 9f;

        [Tooltip("Maximum forward speed with boosts")]
        public float MaxForwardSpeed = 24f;

        [Tooltip("Maximum lateral (sideways) speed")]
        public float MaxLateralSpeed = 8f;

        [Header("Acceleration")]
        [Tooltip("Forward acceleration in m/s²")]
        public float ForwardAcceleration = 10f;

        [Tooltip("Lateral acceleration in m/s²")]
        public float LateralAcceleration = 15f;

        [Tooltip("Deceleration when no input in m/s²")]
        public float Deceleration = 8f;

        [Header("Resistance")]
        [Tooltip("Base air resistance drag")]
        public float BaseDrag = 0.5f;

        [Header("Visuals")]
        [Tooltip("Prefab to instantiate for this vehicle")]
        public EntityBehaviour Prefab;
    }
}
