using Code.Common.Services;
using Code.Configs;
using Code.Gameplay.Features.Physics;
using Code.Infrastructure.Systems;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Hero
{
    /// <summary>
    /// Hero component - marks entity as player-controlled vehicle
    /// </summary>
    [Game]
    public class Hero : IComponent { }

    #region Factory

    public interface IHeroFactory
    {
        /// <summary>
        /// Create hero entity with given stats at spawn position.
        /// </summary>
        GameEntity CreateHero(VehicleStats stats, Vector3 spawnPosition);
    }

    public class HeroFactory : IHeroFactory
    {
        private readonly IIdentifierService _identifiers;
        private readonly VehicleConfig _vehicleConfig;

        public HeroFactory(
            IIdentifierService identifiers,
            VehicleConfig vehicleConfig)
        {
            _identifiers = identifiers;
            _vehicleConfig = vehicleConfig;
        }

        public GameEntity CreateHero(VehicleStats stats, Vector3 spawnPosition)
        {
            GameEntity entity = Contexts.sharedInstance.game.CreateEntity();

            // Core components
            entity.isHero = true;
            entity.AddId(_identifiers.Next());
            entity.AddWorldPosition(spawnPosition);

            // View prefab (will be instantiated by BindViewFeature)
            if (_vehicleConfig.Prefab != null)
                entity.AddViewPrefab(_vehicleConfig.Prefab);

            // Physics components from VehicleStats
            AddPhysicsComponents(entity, stats);

            // Movement direction (used by physics systems)
            entity.AddMoveDirection(Vector3.forward);
            entity.AddMoveSpeed(stats.ForwardSpeed);

            Debug.Log($"[HeroFactory] Created hero at {spawnPosition} with {stats}");

            return entity;
        }

        private void AddPhysicsComponents(GameEntity entity, VehicleStats stats)
        {
            // Mark as physics body
            entity.isPhysicsBody = true;

            // Initial velocity
            entity.AddPhysicsVelocity(new Vector3(0f, 0f, stats.MinForwardSpeed));

            // Acceleration parameters
            entity.AddAcceleration(
                stats.ForwardAcceleration,
                stats.LateralAcceleration,
                stats.Deceleration
            );

            // Drag
            entity.AddPhysicsDrag(
                stats.BaseDrag,
                stats.BaseDrag
            );

            // Speed constraints (no road boundaries - physics handles it)
            entity.AddPhysicsConstraints(
                stats.MinForwardSpeed,
                stats.MaxForwardSpeed,
                stats.MaxLateralSpeed,
                float.MinValue,  // minX - no limit, road physics handles boundaries
                float.MaxValue   // maxX
            );

            // Default surface (normal road)
            entity.AddSurfaceModifier(
                1.0f,
                1.0f,
                SurfaceType.Normal
            );

            // Physics state for debugging
            entity.AddPhysicsState(
                stats.MinForwardSpeed,
                false,
                false,
                Vector3.up
            );
        }
    }

    #endregion

    #region Feature

    public sealed class HeroFeature : Feature
    {
        public HeroFeature(ISystemFactory systems)
        {
            // Hero initialization moved to EcsBootstrap
            // Physics movement handled by PhysicsFeature
        }
    }

    #endregion
}
