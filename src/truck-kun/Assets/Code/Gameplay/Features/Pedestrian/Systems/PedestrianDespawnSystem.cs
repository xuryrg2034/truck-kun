using System;
using System.Collections.Generic;
using Code.Configs.Spawning;
using Code.Infrastructure.View;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian.Systems
{
  public class PedestrianDespawnSystem : IExecuteSystem
  {
    private readonly PedestrianSpawnConfig _config;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameEntity> _pedestrianBuffer = new(32);

    public PedestrianDespawnSystem(
      GameContext game,
      PedestrianSpawnConfig config)
    {
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "PedestrianSpawnConfig is required! Assign it in LevelConfig.");
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
      _pedestrians = game.GetGroup(GameMatcher.AllOf(GameMatcher.Pedestrian, GameMatcher.WorldPosition));
    }

    public void Execute()
    {
      GameEntity hero = null;
      foreach (GameEntity heroEntity in _heroes.GetEntities(_heroBuffer))
      {
        hero = heroEntity;
        break;
      }

      if (hero == null)
        return;

      float thresholdZ = hero.worldPosition.Value.z - _config.DespawnDistance;

      foreach (GameEntity pedestrian in _pedestrians.GetEntities(_pedestrianBuffer))
      {
        if (pedestrian.worldPosition.Value.z > thresholdZ)
          continue;

        if (pedestrian.hasView)
        {
          IEntityView view = pedestrian.view.Value;
          view.ReleaseEntity();
          if (view is Component component)
            UnityEngine.Object.Destroy(component.gameObject);
        }

        pedestrian.Destroy();
      }
    }
  }
}
