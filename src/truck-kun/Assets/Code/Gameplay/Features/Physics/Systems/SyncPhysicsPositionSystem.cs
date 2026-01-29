using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics.Systems
{
  /// <summary>
  /// Syncs ECS WorldPosition component from Rigidbody.position.
  /// This allows other systems (collision, quests, etc.) to use
  /// the physics-updated position.
  /// </summary>
  public class SyncPhysicsPositionSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public SyncPhysicsPositionSystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.WorldPosition,
        GameMatcher.Rigidbody));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Rigidbody rb = entity.rigidbody.Value;
        if (rb == null)
          continue;

        // Sync position from Rigidbody to ECS component
        Vector3 rbPosition = rb.position;
        Vector3 currentPosition = entity.worldPosition.Value;

        // Only update if position actually changed (avoid unnecessary component replacement)
        if (Vector3.SqrMagnitude(rbPosition - currentPosition) > 0.0001f)
        {
          entity.ReplaceWorldPosition(rbPosition);
        }
      }
    }
  }
}
