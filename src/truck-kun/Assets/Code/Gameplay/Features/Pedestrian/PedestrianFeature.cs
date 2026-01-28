using System;
using System.Collections.Generic;
using Code.Art.Animation;
using Code.Balance;
using Code.Common.Services;
using Code.Gameplay.Features.Hero;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian
{
  #region Enums

  public enum PedestrianKind
  {
    StudentNerd,    // Schoolboy with backpack, bent forward
    Salaryman,      // Office worker, gray suit
    Grandma,        // Old lady, slow, pink
    OldMan,         // Old man, slow, brown
    Teenager        // Young person, colorful
  }

  public enum PedestrianCategory
  {
    Normal,     // Can be hit for quests
    Protected   // Hitting causes penalty (elderly)
  }

  #endregion

  #region Components

  [Game] public class Pedestrian : IComponent { }

  [Game] public class PedestrianType : IComponent
  {
    public PedestrianKind Value;
  }

  [Game] public class CrossingPedestrian : IComponent
  {
    public float StartX;
    public float TargetX;
    public float Speed;
    public bool MovingRight;
  }

  #endregion

  #region Visual Data

  [Serializable]
  public class PedestrianVisualData
  {
    [Header("Prefab (drag and drop)")]
    [Tooltip("Optional prefab to use. If not set, procedural model will be generated.")]
    public GameObject Prefab;

    [Header("Settings")]
    public PedestrianKind Kind;
    public Color Color = Color.white;
    public float Scale = 1f;
    public float Speed = 1f;
    public float ForwardTilt = 0f;        // X rotation (bent forward)
    public float BaseSpeed = 1.5f;        // Movement speed
    public PedestrianCategory Category = PedestrianCategory.Normal;
    public string DisplayName = "Pedestrian";

    public static PedestrianVisualData Default(PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.StudentNerd => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.95f, 0.95f, 1f),  // White/light blue
          Scale = 0.85f,
          ForwardTilt = 15f,                     // Bent forward (backpack)
          BaseSpeed = 2f,
          Category = PedestrianCategory.Normal,
          DisplayName = "Student"
        },
        PedestrianKind.Salaryman => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.4f, 0.4f, 0.45f), // Gray suit
          Scale = 1f,
          ForwardTilt = 0f,
          BaseSpeed = 1.8f,
          Category = PedestrianCategory.Normal,
          DisplayName = "Salaryman"
        },
        PedestrianKind.Grandma => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(1f, 0.7f, 0.8f),    // Pink
          Scale = 0.8f,
          ForwardTilt = 10f,                     // Slightly hunched
          BaseSpeed = 0.8f,                      // Slow
          Category = PedestrianCategory.Protected,
          DisplayName = "Grandma"
        },
        PedestrianKind.OldMan => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.6f, 0.45f, 0.3f), // Brown
          Scale = 0.9f,
          ForwardTilt = 8f,
          BaseSpeed = 0.9f,
          Category = PedestrianCategory.Protected,
          DisplayName = "Old Man"
        },
        PedestrianKind.Teenager => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.2f, 0.8f, 0.4f),  // Bright green
          Scale = 0.95f,
          ForwardTilt = -5f,                     // Leaning back (cool pose)
          BaseSpeed = 2.2f,                      // Fast
          Category = PedestrianCategory.Normal,
          DisplayName = "Teenager"
        },
        _ => new PedestrianVisualData
        {
          Kind = kind,
          Color = Color.white,
          Scale = 1f,
          ForwardTilt = 0f,
          BaseSpeed = 1.5f,
          Category = PedestrianCategory.Normal,
          DisplayName = "Pedestrian"
        }
      };
    }
  }

  [Serializable]
  public class PedestrianSpawnWeight
  {
    public PedestrianKind Kind;
    [Range(0f, 1f)] public float Weight = 1f;

    public PedestrianSpawnWeight() { }

    public PedestrianSpawnWeight(PedestrianKind kind, float weight)
    {
      Kind = kind;
      Weight = weight;
    }
  }

  #endregion

  #region ScriptableObject Config

  [CreateAssetMenu(fileName = "PedestrianConfig", menuName = "Truck-kun/Pedestrian Config")]
  public class PedestrianConfig : ScriptableObject
  {
    [Header("Visual Settings")]
    [SerializeField] private List<PedestrianVisualData> _visualData = new()
    {
      PedestrianVisualData.Default(PedestrianKind.StudentNerd),
      PedestrianVisualData.Default(PedestrianKind.Salaryman),
      PedestrianVisualData.Default(PedestrianKind.Grandma),
      PedestrianVisualData.Default(PedestrianKind.OldMan),
      PedestrianVisualData.Default(PedestrianKind.Teenager)
    };

    [Header("Spawn Weights")]
    [SerializeField] private List<PedestrianSpawnWeight> _spawnWeights = new()
    {
      new PedestrianSpawnWeight(PedestrianKind.StudentNerd, 0.25f),
      new PedestrianSpawnWeight(PedestrianKind.Salaryman, 0.30f),
      new PedestrianSpawnWeight(PedestrianKind.Grandma, 0.15f),
      new PedestrianSpawnWeight(PedestrianKind.OldMan, 0.10f),
      new PedestrianSpawnWeight(PedestrianKind.Teenager, 0.20f)
    };

    public IReadOnlyList<PedestrianVisualData> VisualData => _visualData;
    public IReadOnlyList<PedestrianSpawnWeight> SpawnWeights => _spawnWeights;

    public PedestrianVisualData GetVisualData(PedestrianKind kind)
    {
      foreach (PedestrianVisualData data in _visualData)
      {
        if (data.Kind == kind)
          return data;
      }
      return PedestrianVisualData.Default(kind);
    }

    public PedestrianKind SelectRandomKind()
    {
      if (_spawnWeights.Count == 0)
        return PedestrianKind.StudentNerd;

      float totalWeight = 0f;
      foreach (PedestrianSpawnWeight sw in _spawnWeights)
        totalWeight += sw.Weight;

      if (totalWeight <= 0f)
        return _spawnWeights[0].Kind;

      float random = UnityEngine.Random.value * totalWeight;
      float cumulative = 0f;

      foreach (PedestrianSpawnWeight sw in _spawnWeights)
      {
        cumulative += sw.Weight;
        if (random <= cumulative)
          return sw.Kind;
      }

      return _spawnWeights[^1].Kind;
    }
  }

  #endregion

  #region Spawn Settings

  [Serializable]
  public class PedestrianSpawnSettings
  {
    [Header("Spawn Settings")]
    public float SpawnInterval = 1.5f;
    public float SpawnDistanceAhead = 18f;
    public float DespawnDistanceBehind = 12f;
    public int MaxActive = 12;
    public float LateralMargin = 0.5f;

    [Header("Crossing Behavior")]
    [Range(0f, 1f)] public float CrossingChance = 0.3f;
    public float CrossingSpeedMultiplier = 1f;
    public float RoadCenterOffset = 0f;
    public float SidewalkOffset = 1.5f;  // Extra offset from road edge for spawning

    [Header("Crossing Visual")]
    public bool RotateToCrossingDirection = true;
  }

  #endregion

  #region Factory

  public interface IPedestrianFactory
  {
    GameObject CreatePedestrianVisual(PedestrianKind kind, Vector3 position);
    PedestrianVisualData GetVisualData(PedestrianKind kind);
    PedestrianKind SelectRandomKind();
    bool IsProtected(PedestrianKind kind);
  }

  public class PedestrianFactory : IPedestrianFactory
  {
    private readonly PedestrianConfig _config;
    private readonly IBalanceProvider _balanceProvider;
    private readonly PedestrianPhysicsSettings _physicsSettings;
    private readonly Dictionary<PedestrianKind, PedestrianVisualData> _visualCache = new();

    public PedestrianFactory(
      PedestrianConfig config,
      PedestrianPhysicsSettings physicsSettings = null,
      IBalanceProvider balanceProvider = null)
    {
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "PedestrianConfig is required! Assign it in GameplaySceneInstaller.");
      _physicsSettings = physicsSettings;
      _balanceProvider = balanceProvider;
      InitializeCache();
    }

    private void InitializeCache()
    {
      // Cache all visual data from config
      foreach (PedestrianKind kind in Enum.GetValues(typeof(PedestrianKind)))
      {
        PedestrianVisualData data = _config.GetVisualData(kind);

        // Override with GameBalance values if available
        if (_balanceProvider?.Balance != null)
        {
          PedestrianTypeBalance balanceData = _balanceProvider.Balance.GetPedestrianTypeBalance(kind);
          if (balanceData != null)
          {
            // Only override color if it's not white (white = default, not set)
            if (balanceData.Color != Color.white)
              data.Color = balanceData.Color;

            data.Scale = balanceData.Scale;
            data.Speed = balanceData.Speed;
            data.ForwardTilt = balanceData.ForwardTilt;
          }
        }

        _visualCache[kind] = data;
      }
    }

    public PedestrianVisualData GetVisualData(PedestrianKind kind)
    {
      if (_visualCache.TryGetValue(kind, out PedestrianVisualData data))
        return data;

      Debug.LogError($"[PedestrianFactory] No visual data for {kind}! Check PedestrianConfig.");
      return _config.GetVisualData(PedestrianKind.StudentNerd); // Fallback to first type
    }

    public PedestrianKind SelectRandomKind()
    {
      return _config.SelectRandomKind();
    }

    public bool IsProtected(PedestrianKind kind)
    {
      PedestrianVisualData data = GetVisualData(kind);
      return data.Category == PedestrianCategory.Protected;
    }

    public GameObject CreatePedestrianVisual(PedestrianKind kind, Vector3 position)
    {
      PedestrianVisualData data = GetVisualData(kind);

      if (data.Prefab == null)
      {
        Debug.LogError($"[PedestrianFactory] No prefab assigned for {kind}! " +
          "Assign prefabs in PedestrianConfig asset.");
        return null;
      }

      return CreateFromPrefab(data, position);
    }

    private GameObject CreateFromPrefab(PedestrianVisualData data, Vector3 position)
    {
      GameObject pedestrian = UnityEngine.Object.Instantiate(data.Prefab, position, Quaternion.identity);
      pedestrian.name = $"Pedestrian_{data.DisplayName}";

      // Apply scale
      pedestrian.transform.localScale = Vector3.one * data.Scale;

      // Apply rotation (forward tilt)
      pedestrian.transform.rotation = Quaternion.Euler(data.ForwardTilt, 0f, 0f);

      // Set layer for collision filtering
      pedestrian.layer = LayerMask.NameToLayer("Pedestrian");

      // Ensure collider exists for physics collision detection
      CapsuleCollider collider = pedestrian.GetComponent<CapsuleCollider>();
      if (collider == null)
      {
        collider = pedestrian.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.3f;
        collider.center = new Vector3(0f, 1f, 0f);
      }
      // Explicitly not a trigger - we want OnCollisionEnter, not OnTriggerEnter
      collider.isTrigger = false;

      // Dynamic Rigidbody for full physics simulation
      Rigidbody rb = pedestrian.GetComponent<Rigidbody>();
      if (rb == null)
      {
        rb = pedestrian.AddComponent<Rigidbody>();
      }

      // Configure for force-based movement
      rb.isKinematic = false;
      rb.useGravity = true;
      rb.mass = _physicsSettings?.GetMass(data.Kind) ?? 60f;
      rb.linearDamping = _physicsSettings?.Drag ?? 2f;
      rb.angularDamping = _physicsSettings?.AngularDrag ?? 0.5f;
      rb.interpolation = RigidbodyInterpolation.Interpolate;

      // Freeze rotation to keep pedestrian upright (except Y for turning)
      rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

      return pedestrian;
    }
  }

  #endregion

  #region Feature

  public sealed class PedestrianFeature : Feature
  {
    public PedestrianFeature(ISystemFactory systems)
    {
      Add(systems.Create<PedestrianSpawnSystem>());
      // Note: PedestrianCrossingSystem removed - crossing movement now handled by
      // PedestrianForceMovementSystem in PhysicsFeature (force-based physics)
      Add(systems.Create<PedestrianDespawnSystem>());

      // Animation systems
      Add(systems.Create<NPCAnimationSystem>());
      Add(systems.Create<DisableAnimationOnHitSystem>());
    }
  }

  #endregion

  #region Systems

  public class PedestrianSpawnSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly ITimeService _time;
    private readonly RunnerMovementSettings _runnerSettings;
    private readonly PedestrianSpawnSettings _settings;
    private readonly IHeroSpawnPoint _spawnPoint;
    private readonly IIdentifierService _identifiers;
    private readonly IPedestrianFactory _factory;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private float _cooldown;

    public PedestrianSpawnSystem(
      GameContext game,
      ITimeService time,
      RunnerMovementSettings runnerSettings,
      PedestrianSpawnSettings settings,
      IHeroSpawnPoint spawnPoint,
      IIdentifierService identifiers,
      IPedestrianFactory factory)
    {
      _game = game;
      _time = time;
      _runnerSettings = runnerSettings;
      _settings = settings;
      _spawnPoint = spawnPoint;
      _identifiers = identifiers;
      _factory = factory;
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
      PedestrianKind kind = _factory.SelectRandomKind();
      PedestrianVisualData visualData = _factory.GetVisualData(kind);

      GameEntity entity = _game.CreateEntity();
      entity.isPedestrian = true;
      entity.AddPedestrianType(kind);
      entity.AddId(_identifiers.Next());

      float centerX = _spawnPoint != null ? _spawnPoint.Position.x : 0f;
      centerX += _settings.RoadCenterOffset;
      float halfRoadWidth = _runnerSettings.RoadWidth * 0.5f;

      Vector3 position = heroPosition;
      position.z += _settings.SpawnDistanceAhead;

      // Determine if this pedestrian will cross the road
      bool isCrossing = UnityEngine.Random.value < _settings.CrossingChance;

      if (isCrossing)
      {
        // Crossing pedestrian: spawn at sidewalk, target opposite side
        bool startFromLeft = UnityEngine.Random.value < 0.5f;

        float sidewalkOffset = halfRoadWidth + _settings.SidewalkOffset;
        float startX = centerX + (startFromLeft ? -sidewalkOffset : sidewalkOffset);
        float targetX = centerX + (startFromLeft ? sidewalkOffset : -sidewalkOffset);

        position.x = startX;

        // Calculate crossing speed based on type
        float crossingSpeed = visualData.BaseSpeed * _settings.CrossingSpeedMultiplier;

        entity.AddCrossingPedestrian(startX, targetX, crossingSpeed, !startFromLeft);
      }
      else
      {
        // Regular pedestrian: spawn randomly on the road
        float spawnWidth = halfRoadWidth - _settings.LateralMargin;
        position.x = centerX + UnityEngine.Random.Range(-spawnWidth, spawnWidth);
      }

      entity.AddWorldPosition(position);

      // Create visual
      GameObject visual = _factory.CreatePedestrianVisual(kind, position);

      // Rotate crossing pedestrians to face their direction
      if (isCrossing && _settings.RotateToCrossingDirection)
      {
        bool movingRight = entity.crossingPedestrian.MovingRight;
        visual.transform.rotation = Quaternion.Euler(
          visualData.ForwardTilt,
          movingRight ? 90f : -90f,
          0f
          );
      }

      EntityBehaviour entityBehaviour = visual.AddComponent<EntityBehaviour>();
      entityBehaviour.SetEntity(entity);  // SetEntity adds View internally
    }
  }

  public class PedestrianCrossingSystem : IExecuteSystem
  {
    private readonly ITimeService _time;
    private readonly PedestrianSpawnSettings _settings;
    private readonly IGroup<GameEntity> _crossingPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    public PedestrianCrossingSystem(
      GameContext game,
      ITimeService time,
      PedestrianSpawnSettings settings)
    {
      _time = time;
      _settings = settings;
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
          if (_settings.RotateToCrossingDirection && pedestrian.hasView)
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

  public class PedestrianDespawnSystem : IExecuteSystem
  {
    private readonly PedestrianSpawnSettings _settings;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameEntity> _pedestrianBuffer = new(32);

    public PedestrianDespawnSystem(GameContext game, PedestrianSpawnSettings settings)
    {
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

  #endregion

  #region Helper Extensions

  public static class PedestrianKindExtensions
  {
    public static string GetDisplayName(this PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.StudentNerd => "Student",
        PedestrianKind.Salaryman => "Salaryman",
        PedestrianKind.Grandma => "Grandma",
        PedestrianKind.OldMan => "Old Man",
        PedestrianKind.Teenager => "Teenager",
        _ => kind.ToString()
      };
    }

    public static string GetDisplayNameRu(this PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.StudentNerd => "Школьник",
        PedestrianKind.Salaryman => "Офисник",
        PedestrianKind.Grandma => "Бабушка",
        PedestrianKind.OldMan => "Дед",
        PedestrianKind.Teenager => "Подросток",
        _ => kind.ToString()
      };
    }

    public static bool IsProtectedType(this PedestrianKind kind)
    {
      return kind == PedestrianKind.Grandma || kind == PedestrianKind.OldMan;
    }
  }

  #endregion
}
