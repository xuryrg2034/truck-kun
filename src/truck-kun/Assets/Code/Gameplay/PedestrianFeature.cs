using System;
using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Hero;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian
{
  public enum PedestrianKind
  {
    Target,
    Forbidden
  }

  [Game] public class Pedestrian : IComponent { }

  [Game] public class PedestrianType : IComponent
  {
    public PedestrianKind Value;
  }

  [Serializable]
  public class PedestrianSpawnSettings
  {
    public EntityBehaviour TargetPrefab;
    public EntityBehaviour ForbiddenPrefab;
    public float SpawnInterval = 1.5f;
    public float SpawnDistanceAhead = 18f;
    public float DespawnDistanceBehind = 12f;
    public int MaxActive = 12;
    public float LateralMargin = 0.5f;
    public float CrossingChance = 0.5f;
    public float CrossingSpeed = 1.5f;
    public float RoadCenterOffset = 0f;
  }

  public sealed class PedestrianFeature : Feature
  {
    public PedestrianFeature(ISystemFactory systems)
    {
      Add(systems.Create<PedestrianSpawnSystem>());
      Add(systems.Create<PedestrianDespawnSystem>());
    }
  }

  public class PedestrianSpawnSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly ITimeService _time;
    private readonly RunnerMovementSettings _runnerSettings;
    private readonly PedestrianSpawnSettings _settings;
    private readonly IHeroSpawnPoint _spawnPoint;
    private readonly IIdentifierService _identifiers;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private float _cooldown;
    private bool _spawnTargetNext = true;

    public PedestrianSpawnSystem(
      GameContext game,
      ITimeService time,
      RunnerMovementSettings runnerSettings,
      PedestrianSpawnSettings settings,
      IHeroSpawnPoint spawnPoint,
      IIdentifierService identifiers)
    {
      _game = game;
      _time = time;
      _runnerSettings = runnerSettings;
      _settings = settings;
      _spawnPoint = spawnPoint;
      _identifiers = identifiers;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
      _pedestrians = game.GetGroup(GameMatcher.Pedestrian);
      _cooldown = settings != null ? settings.SpawnInterval : 1f;
    }

    public void Execute()
    {
      if (_settings == null)
        return;

      float interval = Mathf.Max(0.05f, _settings.SpawnInterval);
      _cooldown -= _time.DeltaTime;

      if (_cooldown > 0f)
        return;

      if (_settings.MaxActive <= 0 || _pedestrians.count >= _settings.MaxActive)
      {
        _cooldown = interval;
        return;
      }

      GameEntity hero = null;
      foreach (GameEntity heroEntity in _heroes.GetEntities(_heroBuffer))
      {
        hero = heroEntity;
        break;
      }

      if (hero == null)
      {
        _cooldown = interval;
        return;
      }

      SpawnPedestrian(hero.worldPosition.Value);
      _cooldown = interval;
    }

    private void SpawnPedestrian(Vector3 heroPosition)
    {
      EntityBehaviour prefab = SelectPrefab(out PedestrianKind kind);
      if (prefab == null)
        return;

      GameEntity entity = _game.CreateEntity();
      entity.isPedestrian = true;
      entity.AddPedestrianType(kind);
      entity.AddId(_identifiers.Next());

      Vector3 position = heroPosition;
      position.z += _settings.SpawnDistanceAhead;

      float halfWidth = Mathf.Max(0f, _runnerSettings.RoadWidth * 0.5f - _settings.LateralMargin);
      float centerX = _spawnPoint != null ? _spawnPoint.Position.x : 0f;
      centerX += _settings.RoadCenterOffset;
      position.x = centerX + UnityEngine.Random.Range(-halfWidth, halfWidth);

      entity.AddWorldPosition(position);
      entity.AddViewPrefab(prefab);

      float crossingChance = Mathf.Clamp01(_settings.CrossingChance);
      if (UnityEngine.Random.value < crossingChance)
      {
        float speed = Mathf.Max(0f, _settings.CrossingSpeed);
        if (speed > 0f)
        {
          float sign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
          entity.AddMoveDirection(Vector3.right * sign);
          entity.AddMoveSpeed(speed);
        }
      }
    }

    private EntityBehaviour SelectPrefab(out PedestrianKind kind)
    {
      EntityBehaviour targetPrefab = _settings.TargetPrefab;
      EntityBehaviour forbiddenPrefab = _settings.ForbiddenPrefab;

      if (targetPrefab == null && forbiddenPrefab == null)
      {
        kind = PedestrianKind.Target;
        return null;
      }

      bool spawnTarget = _spawnTargetNext;
      _spawnTargetNext = !_spawnTargetNext;

      if (spawnTarget && targetPrefab != null)
      {
        kind = PedestrianKind.Target;
        return targetPrefab;
      }

      if (!spawnTarget && forbiddenPrefab != null)
      {
        kind = PedestrianKind.Forbidden;
        return forbiddenPrefab;
      }

      if (targetPrefab != null)
      {
        kind = PedestrianKind.Target;
        return targetPrefab;
      }

      kind = PedestrianKind.Forbidden;
      return forbiddenPrefab;
    }
  }

  public class PedestrianDespawnSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly PedestrianSpawnSettings _settings;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameEntity> _pedestrianBuffer = new(32);

    public PedestrianDespawnSystem(GameContext game, PedestrianSpawnSettings settings)
    {
      _game = game;
      _settings = settings;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
      _pedestrians = game.GetGroup(GameMatcher.AllOf(GameMatcher.Pedestrian, GameMatcher.WorldPosition));
    }

    public void Execute()
    {
      if (_settings == null)
        return;

      GameEntity hero = null;
      foreach (GameEntity heroEntity in _heroes.GetEntities(_heroBuffer))
      {
        hero = heroEntity;
        break;
      }

      if (hero == null)
        return;

      float distanceBehind = Mathf.Max(0f, _settings.DespawnDistanceBehind);
      float thresholdZ = hero.worldPosition.Value.z - distanceBehind;

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
