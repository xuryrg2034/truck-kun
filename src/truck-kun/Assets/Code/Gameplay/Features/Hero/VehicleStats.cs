using Code.Configs;

namespace Code.Gameplay.Features.Hero
{
    /// <summary>
    /// Runtime vehicle stats with applied multipliers from upgrades and difficulty.
    /// Immutable struct - created once per game session.
    /// </summary>
    public readonly struct VehicleStats
    {
        // Physics
        public readonly float Mass;
        public readonly float AngularDrag;
        public readonly bool UseGravity;
        public readonly bool UseContinuousCollision;

        // Speed (with multipliers applied)
        public readonly float ForwardSpeed;
        public readonly float MinForwardSpeed;
        public readonly float MaxForwardSpeed;
        public readonly float MaxLateralSpeed;

        // Acceleration (with multipliers applied)
        public readonly float ForwardAcceleration;
        public readonly float LateralAcceleration;
        public readonly float Deceleration;

        // Drag
        public readonly float BaseDrag;

        private VehicleStats(
            float mass,
            float angularDrag,
            bool useGravity,
            bool useContinuousCollision,
            float forwardSpeed,
            float minForwardSpeed,
            float maxForwardSpeed,
            float maxLateralSpeed,
            float forwardAcceleration,
            float lateralAcceleration,
            float deceleration,
            float baseDrag)
        {
            Mass = mass;
            AngularDrag = angularDrag;
            UseGravity = useGravity;
            UseContinuousCollision = useContinuousCollision;
            ForwardSpeed = forwardSpeed;
            MinForwardSpeed = minForwardSpeed;
            MaxForwardSpeed = maxForwardSpeed;
            MaxLateralSpeed = maxLateralSpeed;
            ForwardAcceleration = forwardAcceleration;
            LateralAcceleration = lateralAcceleration;
            Deceleration = deceleration;
            BaseDrag = baseDrag;
        }

        /// <summary>
        /// Create VehicleStats from config with applied multipliers.
        /// </summary>
        /// <param name="config">Base vehicle configuration</param>
        /// <param name="speedMultiplier">Multiplier from SpeedBoost upgrade (1.0 = no bonus)</param>
        /// <param name="lateralMultiplier">Multiplier from Maneuverability upgrade (1.0 = no bonus)</param>
        /// <param name="difficultyMultiplier">Multiplier from difficulty scaling (1.0 = day 1)</param>
        public static VehicleStats Create(
            VehicleConfig config,
            float speedMultiplier = 1f,
            float lateralMultiplier = 1f,
            float difficultyMultiplier = 1f)
        {
            return new VehicleStats(
                mass: config.Mass,
                angularDrag: config.AngularDrag,
                useGravity: config.UseGravity,
                useContinuousCollision: config.UseContinuousCollision,
                forwardSpeed: config.BaseForwardSpeed * speedMultiplier * difficultyMultiplier,
                minForwardSpeed: config.MinForwardSpeed * speedMultiplier * difficultyMultiplier,
                maxForwardSpeed: config.MaxForwardSpeed * speedMultiplier * difficultyMultiplier,
                maxLateralSpeed: config.MaxLateralSpeed * lateralMultiplier,
                forwardAcceleration: config.ForwardAcceleration * speedMultiplier,
                lateralAcceleration: config.LateralAcceleration * lateralMultiplier,
                deceleration: config.Deceleration * lateralMultiplier,
                baseDrag: config.BaseDrag
            );
        }

        public override string ToString()
        {
            return $"VehicleStats[Speed={ForwardSpeed:F1}, Lateral={MaxLateralSpeed:F1}, Mass={Mass:F0}]";
        }
    }
}
