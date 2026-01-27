using System.Collections.Generic;
using Code.Common;
using Code.Infrastructure.Systems;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Movement
{
  [Game] public class MoveDirection : IComponent { public Vector3 Value; }
  [Game] public class MoveSpeed : IComponent { public float Value; }

  public sealed class MovementFeature : Feature
  {
    public MovementFeature(ISystemFactory systems)
    {
      Add(systems.Create<DirectionalDeltaMoveSystem>());
      Add(systems.Create<RotateAlongDirectionSystem>());
      Add(systems.Create<UpdateTransformPositionSystem>());
    }
  }

  /// <summary>
  /// Kinematic movement system for non-physics entities (NPCs, projectiles, etc.)
  /// Physics-controlled entities (Hero with Rigidbody) are excluded.
  /// </summary>
  public class DirectionalDeltaMoveSystem : IExecuteSystem
  {
    private readonly ITimeService _time;
    private readonly IGroup<GameEntity> _movers;
    private readonly List<GameEntity> _buffer = new(32);

    public DirectionalDeltaMoveSystem(GameContext game, ITimeService time)
    {
      _time = time;
      // Exclude Hero (has its own movement system) and Rigidbody (physics-controlled)
      // This system only moves NPCs (pedestrians, etc.)
      _movers = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.MoveDirection,
        GameMatcher.MoveSpeed,
        GameMatcher.WorldPosition)
        .NoneOf(GameMatcher.Hero, GameMatcher.Rigidbody));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _movers.GetEntities(_buffer))
      {
        Vector3 direction = entity.moveDirection.Value;
        if (direction.sqrMagnitude < 0.0001f)
          continue;

        Vector3 delta = direction * entity.moveSpeed.Value * _time.DeltaTime;
        entity.ReplaceWorldPosition(entity.worldPosition.Value + delta);
      }
    }
  }

  public class RotateAlongDirectionSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _rotators;
    private readonly List<GameEntity> _buffer = new(32);

    public RotateAlongDirectionSystem(GameContext game)
    {
      _rotators = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.MoveDirection,
        GameMatcher.Transform));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _rotators.GetEntities(_buffer))
      {
        Vector3 direction = entity.moveDirection.Value;
        if (direction.sqrMagnitude < 0.0001f)
          continue;

        entity.transform.Value.rotation = Quaternion.LookRotation(direction, Vector3.up);
      }
    }
  }

  /// <summary>
  /// Syncs Transform.position from WorldPosition for kinematic entities.
  /// Physics-controlled entities (with Rigidbody) are excluded - their transform
  /// is controlled by Unity physics and synced via SyncPhysicsPositionSystem.
  /// </summary>
  public class UpdateTransformPositionSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(32);

    public UpdateTransformPositionSystem(GameContext game)
    {
      // Exclude Rigidbody entities - they are controlled by physics
      // Setting transform.position directly interferes with Rigidbody physics
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.WorldPosition,
        GameMatcher.Transform)
        .NoneOf(GameMatcher.Rigidbody));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
        entity.transform.Value.position = entity.worldPosition.Value;
    }
  }
}
