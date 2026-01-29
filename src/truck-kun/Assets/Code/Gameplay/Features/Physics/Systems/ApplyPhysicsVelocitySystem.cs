using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics.Systems
{
  /// <summary>
  /// Applies calculated physics velocity to Unity Rigidbody.
  /// This is where ECS data meets Unity physics.
  ///
  /// We set velocity directly (not AddForce) for precise control
  /// in an auto-runner game where responsiveness is key.
  ///
  /// When LaunchBoost is active, preserves physics velocity for jumps/ramps.
  /// </summary>
  public class ApplyPhysicsVelocitySystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public ApplyPhysicsVelocitySystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Rigidbody,
        GameMatcher.PhysicsVelocity));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Rigidbody rb = entity.rigidbody.Value;
        if (rb == null)
          continue;

        Vector3 targetVelocity = entity.physicsVelocity.Value;
        Vector3 currentVelocity = rb.linearVelocity;

        // Check if entity has active launch boost
        if (entity.hasLaunchBoost)
        {
          LaunchBoost boost = entity.launchBoost;
          float elapsed = Time.time - boost.StartTime;

          if (elapsed < boost.Duration)
          {
            // During launch: preserve physics velocity, don't override
            // Only gently guide X towards target for steering control
            float blendFactor = elapsed / boost.Duration; // 0 at start, 1 at end
            float xVelocity = Mathf.Lerp(currentVelocity.x, targetVelocity.x, blendFactor * 0.5f);

            rb.linearVelocity = new Vector3(xVelocity, currentVelocity.y, currentVelocity.z);
            continue;
          }
          else
          {
            // Launch finished, remove component
            entity.RemoveLaunchBoost();
          }
        }

        // Normal velocity application
        // Apply our calculated X and Z velocity
        // Preserve Y velocity for gravity and vertical physics
        Vector3 newVelocity = new Vector3(
          targetVelocity.x,
          currentVelocity.y, // Preserve Y from physics (gravity)
          targetVelocity.z
        );

        rb.linearVelocity = newVelocity;
      }
    }
  }
}
