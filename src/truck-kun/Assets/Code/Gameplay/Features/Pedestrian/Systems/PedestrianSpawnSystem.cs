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

      // Calculate spawn position
      Vector3 position = CalculateSpawnPosition(heroPosition);

      // Determine if this pedestrian will cross the road
      bool isCrossing = UnityEngine.Random.value < _config.CrossingChance;

      // Calculate X position based on crossing behavior
      float centerX = 0f;
      float halfRoadWidth = _config.RoadWidth * 0.5f;
      float sidewalkOffset = halfRoadWidth + _config.SidewalkOffset;

      float startX = 0f;
      float targetX = 0f;
      bool startFromLeft = false;

      if (isCrossing)
      {
        // Crossing pedestrian: spawn at sidewalk, target opposite side
        startFromLeft = UnityEngine.Random.value < 0.5f;
        startX = centerX + (startFromLeft ? -sidewalkOffset : sidewalkOffset);
        targetX = centerX + (startFromLeft ? sidewalkOffset : -sidewalkOffset);
        position.x = startX;
      }
      else
      {
        // Regular pedestrian: spawn randomly on the road
        float spawnWidth = halfRoadWidth - _config.LateralMargin;
        position.x = centerX + UnityEngine.Random.Range(-spawnWidth, spawnWidth);
      }

      // Validate spawn position (check for obstacles)
      if (_config.CheckOverlap && !TryFindValidPosition(ref position, isCrossing, centerX, halfRoadWidth))
      {
        // Failed to find valid position after max attempts
        return;
      }

      // Create entity
      GameEntity entity = _game.CreateEntity();
      entity.isPedestrian = true;
      entity.AddPedestrianType(kind);
      entity.AddId(_identifiers.Next());

      if (isCrossing)
      {
        float crossingSpeed = visualData.BaseSpeed * _config.CrossingSpeedMultiplier;
        entity.AddCrossingPedestrian(startX, targetX, crossingSpeed, !startFromLeft);
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

    /// <summary>
    /// Calculate spawn position with fixed Y and Z variation.
    /// </summary>
    private Vector3 CalculateSpawnPosition(Vector3 heroPosition)
    {
      // Fixed Y (ground level) - FIXES: pedestrians spawning in air when truck jumps
      float y = _config.SpawnY;

      // Z ahead of hero with random variation
      float z = heroPosition.z + _config.MinSpawnDistanceAhead;
      z += UnityEngine.Random.Range(0f, _config.SpawnZVariation);

      // X will be set later based on crossing behavior
      return new Vector3(0f, y, z);
    }

    /// <summary>
    /// Try to find a valid spawn position without obstacles.
    /// </summary>
    private bool TryFindValidPosition(ref Vector3 position, bool isCrossing, float centerX, float halfRoadWidth)
    {
      for (int attempt = 0; attempt < _config.MaxSpawnAttempts; attempt++)
      {
        // Check for obstacles at this position
        if (!UnityEngine.Physics.CheckSphere(position, _config.OverlapRadius, _config.ObstacleLayer))
        {
          return true; // Position is valid
        }

        // Try a different X position
        if (isCrossing)
        {
          // For crossing pedestrians, just shift Z slightly
          position.z += UnityEngine.Random.Range(1f, 3f);
        }
        else
        {
          // For regular pedestrians, try different X
          float spawnWidth = halfRoadWidth - _config.LateralMargin;
          position.x = centerX + UnityEngine.Random.Range(-spawnWidth, spawnWidth);
        }
      }

      return false; // Could not find valid position
    }
  }
}
