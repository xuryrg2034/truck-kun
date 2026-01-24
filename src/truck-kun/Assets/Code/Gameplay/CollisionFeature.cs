using System;
using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Pedestrian;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Collision
{
  [Game] public class Hit : IComponent { }

  [Game] public class HitEvent : IComponent
  {
    public PedestrianKind PedestrianType;
    public int PedestrianId;
  }

  [Serializable]
  public class CollisionSettings
  {
    public float HitRadius = 1.2f;
  }

  public sealed class CollisionFeature : Feature
  {
    public CollisionFeature(ISystemFactory systems)
    {
      Add(systems.Create<HeroCollisionDetectionSystem>());
      Add(systems.Create<DestroyHitPedestriansSystem>());
      Add(systems.Create<CleanupHitEventsSystem>());
    }
  }

  public class HeroCollisionDetectionSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly CollisionSettings _settings;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameEntity> _pedestrianBuffer = new(32);

    public HeroCollisionDetectionSystem(
      GameContext game,
      CollisionSettings settings)
    {
      _game = game;
      _settings = settings;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
      _pedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.WorldPosition, GameMatcher.PedestrianType)
        .NoneOf(GameMatcher.Hit));
    }

    public void Execute()
    {
      GameEntity hero = null;
      foreach (GameEntity h in _heroes.GetEntities(_heroBuffer))
      {
        hero = h;
        break;
      }

      if (hero == null)
        return;

      Vector3 heroPos = hero.worldPosition.Value;
      float radiusSq = _settings.HitRadius * _settings.HitRadius;

      foreach (GameEntity pedestrian in _pedestrians.GetEntities(_pedestrianBuffer))
      {
        Vector3 pedPos = pedestrian.worldPosition.Value;
        float distSq = (heroPos - pedPos).sqrMagnitude;

        if (distSq > radiusSq)
          continue;

        pedestrian.isHit = true;

        GameEntity hitEvent = _game.CreateEntity();
        hitEvent.AddHitEvent(pedestrian.pedestrianType.Value, pedestrian.id.Value);
      }
    }
  }

  public class DestroyHitPedestriansSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _hitPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    public DestroyHitPedestriansSystem(GameContext game)
    {
      _hitPedestrians = game.GetGroup(GameMatcher.AllOf(GameMatcher.Pedestrian, GameMatcher.Hit));
    }

    public void Execute()
    {
      foreach (GameEntity pedestrian in _hitPedestrians.GetEntities(_buffer))
      {
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

  public class CleanupHitEventsSystem : ICleanupSystem
  {
    private readonly IGroup<GameEntity> _hitEvents;
    private readonly List<GameEntity> _buffer = new(16);

    public CleanupHitEventsSystem(GameContext game)
    {
      _hitEvents = game.GetGroup(GameMatcher.HitEvent);
    }

    public void Cleanup()
    {
      foreach (GameEntity entity in _hitEvents.GetEntities(_buffer))
        entity.Destroy();
    }
  }
}
