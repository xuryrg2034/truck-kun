using System;
using System.Collections.Generic;
using Code.Art.Animation;
using Code.Balance;
using Code.Common.Services;
using Code.Configs.Spawning;
using Code.Gameplay.Features.Hero;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using Zenject;

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
      return SelectRandomKind(null);
    }

    /// <summary>
    /// Select random pedestrian kind, filtered by allowed types.
    /// Skips types that don't have a prefab assigned.
    /// </summary>
    public PedestrianKind SelectRandomKind(List<PedestrianKind> allowedKinds)
    {
      if (_spawnWeights.Count == 0)
        return PedestrianKind.StudentNerd;

      // Build filtered list of spawn weights
      List<PedestrianSpawnWeight> filtered = new();
      foreach (PedestrianSpawnWeight sw in _spawnWeights)
      {
        // Skip if not in allowed list (when list is specified)
        if (allowedKinds != null && allowedKinds.Count > 0 && !allowedKinds.Contains(sw.Kind))
          continue;

        // Skip if no prefab assigned
        if (!HasPrefab(sw.Kind))
          continue;

        filtered.Add(sw);
      }

      // Fallback: if all filtered out, try to find any type with prefab
      if (filtered.Count == 0)
      {
        foreach (PedestrianSpawnWeight sw in _spawnWeights)
        {
          if (HasPrefab(sw.Kind))
          {
            Debug.LogWarning($"[PedestrianConfig] No allowed types have prefabs, falling back to {sw.Kind}");
            return sw.Kind;
          }
        }
        Debug.LogError("[PedestrianConfig] No pedestrian prefabs assigned! Assign prefabs in PedestrianConfig.");
        return PedestrianKind.StudentNerd;
      }

      // Calculate total weight
      float totalWeight = 0f;
      foreach (PedestrianSpawnWeight sw in filtered)
        totalWeight += sw.Weight;

      if (totalWeight <= 0f)
        return filtered[0].Kind;

      // Weighted random selection
      float random = UnityEngine.Random.value * totalWeight;
      float cumulative = 0f;

      foreach (PedestrianSpawnWeight sw in filtered)
      {
        cumulative += sw.Weight;
        if (random <= cumulative)
          return sw.Kind;
      }

      return filtered[^1].Kind;
    }

    /// <summary>
    /// Check if a pedestrian kind has a prefab assigned
    /// </summary>
    public bool HasPrefab(PedestrianKind kind)
    {
      foreach (PedestrianVisualData data in _visualData)
      {
        if (data.Kind == kind)
          return data.Prefab != null;
      }
      return false;
    }

    /// <summary>
    /// Get list of kinds that have prefabs assigned
    /// </summary>
    public List<PedestrianKind> GetAvailableKinds()
    {
      List<PedestrianKind> available = new();
      foreach (PedestrianVisualData data in _visualData)
      {
        if (data.Prefab != null)
          available.Add(data.Kind);
      }
      return available;
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

    [Header("Allowed Pedestrian Types")]
    [Tooltip("Which pedestrian types can spawn on this level. Empty = all types.")]
    public List<PedestrianKind> AllowedKinds = new();

    [Header("Crossing Behavior")]
    [Range(0f, 1f)] public float CrossingChance = 0.3f;
    public float CrossingSpeedMultiplier = 1f;
    public float RoadCenterOffset = 0f;
    public float SidewalkOffset = 1.5f;  // Extra offset from road edge for spawning

    [Header("Crossing Visual")]
    public bool RotateToCrossingDirection = true;

    /// <summary>
    /// Check if a pedestrian kind is allowed on this level
    /// </summary>
    public bool IsKindAllowed(PedestrianKind kind)
    {
      // Empty list = all types allowed
      if (AllowedKinds == null || AllowedKinds.Count == 0)
        return true;

      return AllowedKinds.Contains(kind);
    }
  }

  #endregion

  #region Factory

  public interface IPedestrianFactory
  {
    GameObject CreatePedestrianVisual(PedestrianKind kind, Vector3 position);
    PedestrianVisualData GetVisualData(PedestrianKind kind);
    PedestrianKind SelectRandomKind();
    PedestrianKind SelectRandomKind(List<PedestrianKind> allowedKinds);
    bool IsProtected(PedestrianKind kind);
    bool HasPrefab(PedestrianKind kind);
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

    public PedestrianKind SelectRandomKind(List<PedestrianKind> allowedKinds)
    {
      return _config.SelectRandomKind(allowedKinds);
    }

    public bool HasPrefab(PedestrianKind kind)
    {
      return _config.HasPrefab(kind);
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
    private readonly PedestrianSpawnSettings _legacySettings;
    private readonly PedestrianSpawnConfig _newConfig;
    private readonly IHeroSpawnPoint _spawnPoint;
    private readonly IIdentifierService _identifiers;
    private readonly IPedestrianFactory _factory;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private float _cooldown;
    private int _maxActive;

    // Properties to get values from new config or fall back to legacy
    private float SpawnDistanceAhead => _newConfig != null ? _newConfig.SpawnDistanceAhead : _legacySettings?.SpawnDistanceAhead ?? 18f;
    private float DespawnDistance => _newConfig != null ? _newConfig.DespawnDistance : _legacySettings?.DespawnDistanceBehind ?? 12f;
    private float CrossingChance => _newConfig != null ? _newConfig.CrossingChance : _legacySettings?.CrossingChance ?? 0.3f;
    private float RoadWidth => _newConfig != null ? _newConfig.RoadWidth : _runnerSettings?.RoadWidth ?? 8f;

    public PedestrianSpawnSystem(
      GameContext game,
      ITimeService time,
      RunnerMovementSettings runnerSettings,
      PedestrianSpawnSettings settings,
      IHeroSpawnPoint spawnPoint,
      IIdentifierService identifiers,
      IPedestrianFactory factory,
      [InjectOptional] PedestrianSpawnConfig newConfig = null)
    {
      _game = game;
      _time = time;
      _runnerSettings = runnerSettings;
      _legacySettings = settings;
      _newConfig = newConfig;
      _spawnPoint = spawnPoint;
      _identifiers = identifiers;
      _factory = factory;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
      _pedestrians = game.GetGroup(GameMatcher.Pedestrian);

      // Initialize from new config or legacy
      if (_newConfig != null)
      {
        _cooldown = _newConfig.GetRandomInterval();
        _maxActive = 20; // Default max for new config
        Debug.Log("[PedestrianSpawnSystem] Using new PedestrianSpawnConfig");
      }
      else if (_legacySettings != null)
      {
        _cooldown = _legacySettings.SpawnInterval;
        _maxActive = _legacySettings.MaxActive;
        Debug.Log("[PedestrianSpawnSystem] Using legacy PedestrianSpawnSettings");
      }
      else
      {
        _cooldown = 1f;
        _maxActive = 10;
      }
    }

    public void Execute()
    {
      if (_legacySettings == null && _newConfig == null)
        return;

      _cooldown -= _time.DeltaTime;

      if (_cooldown > 0f)
        return;

      if (_pedestrians.count >= _maxActive)
      {
        _cooldown = GetNextInterval();
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
        _cooldown = GetNextInterval();
        return;
      }

      SpawnPedestrian(hero.worldPosition.Value);
      _cooldown = GetNextInterval();
    }

    private float GetNextInterval()
    {
      if (_newConfig != null)
        return _newConfig.GetRandomInterval();

      return Mathf.Max(0.05f, _legacySettings?.SpawnInterval ?? 1f);
    }

    private void SpawnPedestrian(Vector3 heroPosition)
    {
      // Use allowed kinds from level settings (empty = all types)
      List<PedestrianKind> allowedKinds = _legacySettings?.AllowedKinds;
      PedestrianKind kind = _factory.SelectRandomKind(allowedKinds);

      // Skip spawn if no valid prefab (SelectRandomKind logs error)
      if (!_factory.HasPrefab(kind))
        return;

      PedestrianVisualData visualData = _factory.GetVisualData(kind);

      GameEntity entity = _game.CreateEntity();
      entity.isPedestrian = true;
      entity.AddPedestrianType(kind);
      entity.AddId(_identifiers.Next());

      float centerX = _spawnPoint != null ? _spawnPoint.Position.x : 0f;
      centerX += _legacySettings?.RoadCenterOffset ?? 0f;
      float halfRoadWidth = RoadWidth * 0.5f;

      Vector3 position = heroPosition;
      position.z += SpawnDistanceAhead;

      // Determine if this pedestrian will cross the road
      bool isCrossing = UnityEngine.Random.value < CrossingChance;

      float sidewalkOffset = halfRoadWidth + (_legacySettings?.SidewalkOffset ?? 1.5f);
      float lateralMargin = _legacySettings?.LateralMargin ?? 0.5f;

      if (isCrossing)
      {
        // Crossing pedestrian: spawn at sidewalk, target opposite side
        bool startFromLeft = UnityEngine.Random.value < 0.5f;

        float startX = centerX + (startFromLeft ? -sidewalkOffset : sidewalkOffset);
        float targetX = centerX + (startFromLeft ? sidewalkOffset : -sidewalkOffset);

        position.x = startX;

        // Calculate crossing speed based on type
        float crossingSpeedMult = _legacySettings?.CrossingSpeedMultiplier ?? 1f;
        float crossingSpeed = visualData.BaseSpeed * crossingSpeedMult;

        entity.AddCrossingPedestrian(startX, targetX, crossingSpeed, !startFromLeft);
      }
      else
      {
        // Regular pedestrian: spawn randomly on the road
        float spawnWidth = halfRoadWidth - lateralMargin;
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
      bool rotateToCrossing = _legacySettings?.RotateToCrossingDirection ?? true;
      if (isCrossing && rotateToCrossing)
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
    private readonly PedestrianSpawnSettings _legacySettings;
    private readonly IGroup<GameEntity> _crossingPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    private bool RotateToCrossingDirection => _legacySettings?.RotateToCrossingDirection ?? true;

    public PedestrianCrossingSystem(
      GameContext game,
      ITimeService time,
      PedestrianSpawnSettings settings)
    {
      _time = time;
      _legacySettings = settings;
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
          if (RotateToCrossingDirection && pedestrian.hasView)
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
    private readonly PedestrianSpawnSettings _legacySettings;
    private readonly PedestrianSpawnConfig _newConfig;
    private readonly IGroup<GameEntity> _heroes;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameEntity> _pedestrianBuffer = new(32);

    private float DespawnDistance => _newConfig != null
      ? _newConfig.DespawnDistance
      : _legacySettings?.DespawnDistanceBehind ?? 12f;

    public PedestrianDespawnSystem(
      GameContext game,
      PedestrianSpawnSettings settings,
      [InjectOptional] PedestrianSpawnConfig newConfig = null)
    {
      _legacySettings = settings;
      _newConfig = newConfig;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));
      _pedestrians = game.GetGroup(GameMatcher.AllOf(GameMatcher.Pedestrian, GameMatcher.WorldPosition));
    }

    public void Execute()
    {
      if (_legacySettings == null && _newConfig == null)
        return;

      GameEntity hero = null;
      foreach (GameEntity heroEntity in _heroes.GetEntities(_heroBuffer))
      {
        hero = heroEntity;
        break;
      }

      if (hero == null)
        return;

      float distanceBehind = Mathf.Max(0f, DespawnDistance);
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
