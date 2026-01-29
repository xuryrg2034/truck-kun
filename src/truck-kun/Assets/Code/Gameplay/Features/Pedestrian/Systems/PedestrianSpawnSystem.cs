using System;
using System.Collections.Generic;
using Code.Common.Services;
using Code.Configs.Spawning;
using Code.Gameplay.Features.Pedestrian.Factory;
using Code.Infrastructure.View;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian.Systems
{
  public class PedestrianSpawnSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly ITimeService _time;
    private readonly PedestrianSpawnConfig _config;
    private readonly IIdentifierService _identifiers;
    private readonly IPedestrianFactory _factory;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private float _cooldown;

    public PedestrianSpawnSystem(
      GameContext game,
      ITimeService time,
      PedestrianSpawnConfig config,
      IIdentifierService identifiers,
      IPedestrianFactory factory)
    {
      _game = game;
      _time = time;
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "PedestrianSpawnConfig is required! Assign it in LevelConfig.");
      _identifiers = identifiers;
      _factory = factory;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
      _pedestrians = game.GetGroup(GameMatcher.Pedestrian);
      _cooldown = _config.GetRandomInterval();
    }

    public void Execute()
    {
      _cooldown -= _time.DeltaTime;

      if (_cooldown > 0f)
        return;

      if (_pedestrians.count >= _config.MaxActive)
      {
        _cooldown = _config.GetRandomInterval();
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
        _cooldown = _config.GetRandomInterval();
        return;
      }

      SpawnPedestrian(hero.worldPosition.Value);
      _cooldown = _config.GetRandomInterval();
    }

    private void SpawnPedestrian(Vector3 heroPosition)
    {
      // Use allowed kinds from config (empty = all types)
      PedestrianKind kind = _factory.SelectRandomKind(_config.AllowedKinds);

      // Skip spawn if no valid prefab
      if (!_factory.HasPrefab(kind))
        return;

      PedestrianVisualData visualData = _factory.GetVisualData(kind);

      GameEntity entity = _game.CreateEntity();
      entity.isPedestrian = true;
      entity.AddPedestrianType(kind);
      entity.AddId(_identifiers.Next());

      // Road center is at X=0 (physical road boundaries handle limits)
      float centerX = 0f;
      float halfRoadWidth = _config.RoadWidth * 0.5f;

      Vector3 position = heroPosition;
      position.z += _config.SpawnDistanceAhead;

      // Determine if this pedestrian will cross the road
      bool isCrossing = UnityEngine.Random.value < _config.CrossingChance;

      float sidewalkOffset = halfRoadWidth + _config.SidewalkOffset;

      if (isCrossing)
      {
        // Crossing pedestrian: spawn at sidewalk, target opposite side
        bool startFromLeft = UnityEngine.Random.value < 0.5f;

        float startX = centerX + (startFromLeft ? -sidewalkOffset : sidewalkOffset);
        float targetX = centerX + (startFromLeft ? sidewalkOffset : -sidewalkOffset);

        position.x = startX;

        // Calculate crossing speed based on type
        float crossingSpeed = visualData.BaseSpeed * _config.CrossingSpeedMultiplier;

        entity.AddCrossingPedestrian(startX, targetX, crossingSpeed, !startFromLeft);
      }
      else
      {
        // Regular pedestrian: spawn randomly on the road
        float spawnWidth = halfRoadWidth - _config.LateralMargin;
        position.x = centerX + UnityEngine.Random.Range(-spawnWidth, spawnWidth);
      }

      entity.AddWorldPosition(position);

      // Create visual
      GameObject visual = _factory.CreatePedestrianVisual(kind, position);

      // If visual creation failed (no prefab), destroy entity and skip
      if (visual == null)
      {
        entity.Destroy();
        return;
      }

      // Rotate crossing pedestrians to face their direction
      if (isCrossing && _config.RotateToCrossingDirection)
      {
        bool movingRight = entity.crossingPedestrian.MovingRight;
        visual.transform.rotation = Quaternion.Euler(
          visualData.ForwardTilt,
          movingRight ? 90f : -90f,
          0f
        );
      }

      EntityBehaviour entityBehaviour = visual.AddComponent<EntityBehaviour>();
      entityBehaviour.SetEntity(entity);
    }
  }
}
