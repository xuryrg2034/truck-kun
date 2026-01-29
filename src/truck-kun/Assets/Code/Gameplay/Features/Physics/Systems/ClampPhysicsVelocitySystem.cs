using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics.Systems
{
  /// <summary>
  /// Enforces speed limits and road boundaries on velocity.
  /// This is the final velocity processing before applying to Rigidbody.
  /// </summary>
  public class ClampPhysicsVelocitySystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public ClampPhysicsVelocitySystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.PhysicsVelocity,
        GameMatcher.PhysicsConstraints,
        GameMatcher.WorldPosition));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Vector3 velocity = entity.physicsVelocity.Value;
        PhysicsConstraints constraints = entity.physicsConstraints;
        Vector3 position = entity.worldPosition.Value;

        // === FORWARD SPEED LIMITS ===
        velocity.z = Mathf.Clamp(velocity.z, constraints.MinForwardSpeed, constraints.MaxForwardSpeed);

        // === LATERAL SPEED LIMITS ===
        velocity.x = Mathf.Clamp(velocity.x, -constraints.MaxLateralSpeed, constraints.MaxLateralSpeed);

        // === ROAD BOUNDARY ENFORCEMENT ===
        // If approaching boundary, reduce/reverse lateral velocity to stay in bounds
        float boundaryBuffer = 0.5f; // Start slowing before hitting boundary

        if (position.x <= constraints.MinX + boundaryBuffer && velocity.x < 0)
        {
          // Approaching left boundary while moving left
          float distanceToEdge = position.x - constraints.MinX;
          float slowdownFactor = Mathf.Clamp01(distanceToEdge / boundaryBuffer);
          velocity.x *= slowdownFactor;

          // Hard stop at boundary
          if (position.x <= constraints.MinX)
            velocity.x = Mathf.Max(0f, velocity.x);
        }
        else if (position.x >= constraints.MaxX - boundaryBuffer && velocity.x > 0)
        {
          // Approaching right boundary while moving right
          float distanceToEdge = constraints.MaxX - position.x;
          float slowdownFactor = Mathf.Clamp01(distanceToEdge / boundaryBuffer);
          velocity.x *= slowdownFactor;

          // Hard stop at boundary
          if (position.x >= constraints.MaxX)
            velocity.x = Mathf.Min(0f, velocity.x);
        }

        entity.ReplacePhysicsVelocity(velocity);
      }
    }
  }
}
