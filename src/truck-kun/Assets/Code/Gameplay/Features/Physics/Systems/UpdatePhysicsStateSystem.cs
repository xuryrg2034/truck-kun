using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics.Systems
{
  /// <summary>
  /// Updates PhysicsState component for debugging and effects.
  /// Tracks current speed, sliding state, etc.
  /// </summary>
  public class UpdatePhysicsStateSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public UpdatePhysicsStateSystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.PhysicsState,
        GameMatcher.PhysicsVelocity,
        GameMatcher.PhysicsConstraints,
        GameMatcher.SurfaceModifier));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Vector3 velocity = entity.physicsVelocity.Value;
        PhysicsConstraints constraints = entity.physicsConstraints;
        SurfaceModifier surface = entity.surfaceModifier;

        float currentSpeed = velocity.magnitude;
        bool isSliding = surface.FrictionMultiplier < 0.5f && Mathf.Abs(velocity.x) > 0.5f;
        bool isAtMaxSpeed = velocity.z >= constraints.MaxForwardSpeed - 0.1f;

        PhysicsState currentState = entity.physicsState;

        // Only update if something changed
        if (Mathf.Abs(currentState.CurrentSpeed - currentSpeed) > 0.1f ||
            currentState.IsSliding != isSliding ||
            currentState.IsAtMaxSpeed != isAtMaxSpeed)
        {
          entity.ReplacePhysicsState(
            currentSpeed,
            isSliding,
            isAtMaxSpeed,
            currentState.GroundNormal // Preserve ground normal
          );
        }
      }
    }
  }
}
