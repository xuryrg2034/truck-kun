using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics.Systems
{
  /// <summary>
  /// Applies surface modifiers to velocity.
  /// Different surfaces affect lateral control and drag.
  ///
  /// Effects:
  /// - Friction: Affects lateral control (low = slippery, slides more)
  /// - Drag: Affects forward speed (high = slower)
  ///
  /// Surface types:
  /// - Normal: No modification
  /// - Oil: Low friction (0.3), low drag - slides but maintains speed
  /// - Grass: Normal friction, high drag (1.8) - slows down
  /// - Ice: Very low friction (0.15), low drag - extremely slippery
  /// - Puddle: Slight friction reduction, moderate drag
  /// </summary>
  public class ApplySurfaceModifiersSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    // Store previous lateral velocity to implement sliding
    private readonly Dictionary<int, float> _previousLateralVelocity = new();

    public ApplySurfaceModifiersSystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.PhysicsVelocity,
        GameMatcher.SurfaceModifier,
        GameMatcher.PhysicsDrag,
        GameMatcher.Id));
    }

    public void Execute()
    {
      float dt = Time.fixedDeltaTime;

      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Vector3 velocity = entity.physicsVelocity.Value;
        SurfaceModifier surface = entity.surfaceModifier;
        PhysicsDrag drag = entity.physicsDrag;
        int entityId = entity.id.Value;

        // Get previous lateral velocity for sliding calculation
        if (!_previousLateralVelocity.TryGetValue(entityId, out float prevLateralVel))
          prevLateralVel = velocity.x;

        // === FRICTION EFFECT ON LATERAL CONTROL ===
        // Low friction means the truck "slides" - it keeps moving laterally
        // even when trying to change direction
        if (surface.FrictionMultiplier < 1f)
        {
          float friction = surface.FrictionMultiplier;

          // Blend between desired velocity and previous velocity based on friction
          // Low friction = more influence from previous velocity (sliding)
          // friction 1.0 = 100% control, friction 0.3 = 30% control
          float slideAmount = 1f - friction;

          // Current velocity.x is the "desired" lateral velocity
          // prevLateralVel is where we were sliding
          float slidingVelocity = Mathf.Lerp(velocity.x, prevLateralVel, slideAmount * 0.8f);

          // Add slight random drift on very slippery surfaces
          if (friction < 0.4f)
          {
            float drift = (Mathf.PerlinNoise(Time.time * 2f, entityId) - 0.5f) * 0.5f;
            slidingVelocity += drift * (1f - friction);
          }

          velocity.x = slidingVelocity;
        }

        // === DRAG EFFECT ON FORWARD SPEED ===
        // Update current drag value
        float newCurrentDrag = drag.BaseDrag * surface.DragMultiplier;

        if (Mathf.Abs(newCurrentDrag - drag.CurrentDrag) > 0.001f)
        {
          entity.ReplacePhysicsDrag(drag.BaseDrag, newCurrentDrag);
        }

        // Apply drag to forward velocity
        // High drag = surface slows you down (grass, mud)
        // Low drag = maintains speed (ice, oil)
        if (surface.DragMultiplier != 1f)
        {
          float dragEffect = surface.DragMultiplier;

          if (dragEffect > 1f)
          {
            // High drag - STRONG slowdown effect
            float slowdownRate = (dragEffect - 1f) * 2f;
            float slowdownFactor = 1f - (slowdownRate * dt);
            slowdownFactor = Mathf.Max(slowdownFactor, 0.5f);

            velocity.z *= slowdownFactor;

            // Clamp minimum speed so truck doesn't completely stop
            float minSpeed = 0.5f;
            if (velocity.z < minSpeed && entity.hasPhysicsConstraints)
            {
              velocity.z = Mathf.Max(velocity.z, dragEffect > 5f ? 0.1f : minSpeed);
            }
          }
          else
          {
            // Low drag - slight speed boost on slippery surfaces
            float boostFactor = 1f + ((1f - dragEffect) * 0.1f * dt);
            boostFactor = Mathf.Min(boostFactor, 1.02f);
            velocity.z *= boostFactor;
          }
        }

        // Store for next frame
        _previousLateralVelocity[entityId] = velocity.x;

        // Apply modified velocity
        entity.ReplacePhysicsVelocity(velocity);
      }
    }
  }
}
