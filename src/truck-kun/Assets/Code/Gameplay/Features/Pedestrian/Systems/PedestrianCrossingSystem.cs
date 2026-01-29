using System;
using System.Collections.Generic;
using Code.Common.Services;
using Code.Configs.Spawning;
using Code.Infrastructure.View;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian.Systems
{
  /// <summary>
  /// Moves crossing pedestrians across the road.
  /// NOTE: This system is currently not used - crossing movement is handled by
  /// PedestrianForceMovementSystem (force-based physics).
  /// Kept for potential fallback to kinematic movement.
  /// </summary>
  public class PedestrianCrossingSystem : IExecuteSystem
  {
    private readonly ITimeService _time;
    private readonly PedestrianSpawnConfig _config;
    private readonly IGroup<GameEntity> _crossingPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    public PedestrianCrossingSystem(
      GameContext game,
      ITimeService time,
      PedestrianSpawnConfig config)
    {
      _time = time;
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "PedestrianSpawnConfig is required! Assign it in LevelConfig.");
      _crossingPedestrians = game.GetGroup(
        GameMatcher.AllOf(GameMatcher.Pedestrian, GameMatcher.CrossingPedestrian, GameMatcher.WorldPosition)
      );
    }

    public void Execute()
    {
      float dt = _time.DeltaTime;

      foreach (GameEntity pedestrian in _crossingPedestrians.GetEntities(_buffer))
      {
        float startX = pedestrian.crossingPedestrian.StartX;
        float targetX = pedestrian.crossingPedestrian.TargetX;
        float speed = pedestrian.crossingPedestrian.Speed;
        bool movingRight = pedestrian.crossingPedestrian.MovingRight;

        Vector3 pos = pedestrian.worldPosition.Value;

        // Move towards target
        float direction = movingRight ? 1f : -1f;
        float movement = speed * dt * direction;
        pos.x += movement;

        // Check if reached target
        bool reachedTarget = movingRight
          ? pos.x >= targetX
          : pos.x <= targetX;

        if (reachedTarget)
        {
          // Snap to target and remove crossing component
          pos.x = targetX;
          pedestrian.RemoveCrossingPedestrian();

          // Optionally rotate back to face forward
          if (_config.RotateToCrossingDirection && pedestrian.hasView)
          {
            IEntityView view = pedestrian.view.Value;
            if (view is Component comp)
            {
              // Get pedestrian visual data for forward tilt
              float tilt = 0f;
              if (pedestrian.hasPedestrianType)
              {
                PedestrianVisualData data = PedestrianVisualData.Default(pedestrian.pedestrianType.Value);
                tilt = data.ForwardTilt;
              }
              comp.transform.rotation = Quaternion.Euler(tilt, 0f, 0f);
            }
          }
        }

        pedestrian.ReplaceWorldPosition(pos);

        // Update view position
        if (pedestrian.hasView)
        {
          IEntityView view = pedestrian.view.Value;
          if (view is Component comp)
            comp.transform.position = pos;
        }
      }
    }
  }
}
