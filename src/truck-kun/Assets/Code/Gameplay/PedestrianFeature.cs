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
  #region Enums

  public enum PedestrianKind
  {
    // Concrete types for quests
    StudentNerd,    // Schoolboy with backpack, bent forward
    Salaryman,      // Office worker, gray suit
    Grandma,        // Old lady, slow, pink
    OldMan,         // Old man, slow, brown
    Teenager,       // Young person, colorful

    // Legacy types (mapped to concrete for compatibility)
    Target = StudentNerd,
    Forbidden = Grandma
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

  #endregion

  #region Visual Data

  [Serializable]
  public class PedestrianVisualData
  {
    public PedestrianKind Kind;
    public Color Color = Color.white;
    public float Scale = 1f;
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
    [Header("Prefabs (Optional - will generate if null)")]
    public EntityBehaviour TargetPrefab;
    public EntityBehaviour ForbiddenPrefab;

    [Header("Spawn Settings")]
    public float SpawnInterval = 1.5f;
    public float SpawnDistanceAhead = 18f;
    public float DespawnDistanceBehind = 12f;
    public int MaxActive = 12;
    public float LateralMargin = 0.5f;

    [Header("Movement")]
    public float CrossingChance = 0.5f;
    public float CrossingSpeed = 1.5f;
    public float RoadCenterOffset = 0f;
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
    private readonly Dictionary<PedestrianKind, PedestrianVisualData> _visualCache = new();

    public PedestrianFactory(PedestrianConfig config = null)
    {
      _config = config;
      InitializeCache();
    }

    private void InitializeCache()
    {
      // Cache all visual data
      foreach (PedestrianKind kind in Enum.GetValues(typeof(PedestrianKind)))
      {
        // Skip legacy aliases
        if (kind == PedestrianKind.Target || kind == PedestrianKind.Forbidden)
          continue;

        _visualCache[kind] = _config != null
          ? _config.GetVisualData(kind)
          : PedestrianVisualData.Default(kind);
      }
    }

    public PedestrianVisualData GetVisualData(PedestrianKind kind)
    {
      // Map legacy types to concrete types
      kind = MapLegacyKind(kind);

      if (_visualCache.TryGetValue(kind, out PedestrianVisualData data))
        return data;

      return PedestrianVisualData.Default(kind);
    }

    public PedestrianKind SelectRandomKind()
    {
      if (_config != null)
        return _config.SelectRandomKind();

      // Default random selection
      PedestrianKind[] kinds = {
        PedestrianKind.StudentNerd,
        PedestrianKind.Salaryman,
        PedestrianKind.Grandma,
        PedestrianKind.OldMan,
        PedestrianKind.Teenager
      };

      return kinds[UnityEngine.Random.Range(0, kinds.Length)];
    }

    public bool IsProtected(PedestrianKind kind)
    {
      kind = MapLegacyKind(kind);
      PedestrianVisualData data = GetVisualData(kind);
      return data.Category == PedestrianCategory.Protected;
    }

    public GameObject CreatePedestrianVisual(PedestrianKind kind, Vector3 position)
    {
      kind = MapLegacyKind(kind);
      PedestrianVisualData data = GetVisualData(kind);

      // Create capsule
      GameObject pedestrian = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      pedestrian.name = $"Pedestrian_{data.DisplayName}";
      pedestrian.transform.position = position;

      // Apply scale
      pedestrian.transform.localScale = new Vector3(
        data.Scale * 0.5f,
        data.Scale,
        data.Scale * 0.5f
      );

      // Apply rotation (forward tilt)
      pedestrian.transform.rotation = Quaternion.Euler(data.ForwardTilt, 0f, 0f);

      // Apply color
      Renderer renderer = pedestrian.GetComponent<Renderer>();
      if (renderer != null)
      {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = data.Color;

        // Add slight emission for protected types
        if (data.Category == PedestrianCategory.Protected)
        {
          mat.EnableKeyword("_EMISSION");
          mat.SetColor("_EmissionColor", data.Color * 0.3f);
        }

        renderer.material = mat;
      }

      // Add indicator for type (small sphere on top)
      AddTypeIndicator(pedestrian.transform, data);

      return pedestrian;
    }

    private void AddTypeIndicator(Transform parent, PedestrianVisualData data)
    {
      // Add small sphere on top to help identify type
      GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      indicator.name = "TypeIndicator";
      indicator.transform.SetParent(parent, false);
      indicator.transform.localPosition = new Vector3(0f, 0.7f, 0f);
      indicator.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
      indicator.transform.localRotation = Quaternion.identity;

      // Remove collider from indicator
      Collider indicatorCollider = indicator.GetComponent<Collider>();
      if (indicatorCollider != null)
        UnityEngine.Object.Destroy(indicatorCollider);

      Renderer renderer = indicator.GetComponent<Renderer>();
      if (renderer != null)
      {
        Material mat = new Material(Shader.Find("Standard"));

        // Color based on category
        mat.color = data.Category == PedestrianCategory.Protected
          ? new Color(1f, 0.3f, 0.3f)  // Red for protected
          : new Color(0.3f, 1f, 0.3f); // Green for normal

        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", mat.color * 0.5f);
        renderer.material = mat;
      }
    }

    private static PedestrianKind MapLegacyKind(PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.Target => PedestrianKind.StudentNerd,
        PedestrianKind.Forbidden => PedestrianKind.Grandma,
        _ => kind
      };
    }
  }

  #endregion

  #region Feature

  public sealed class PedestrianFeature : Feature
  {
    public PedestrianFeature(ISystemFactory systems)
    {
      Add(systems.Create<PedestrianSpawnSystem>());
      Add(systems.Create<PedestrianDespawnSystem>());
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
      // Select random kind using factory (respects config weights)
      PedestrianKind kind = _factory.SelectRandomKind();
      PedestrianVisualData visualData = _factory.GetVisualData(kind);

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

      // Try to use prefab if available, otherwise factory will create visual
      EntityBehaviour prefab = SelectPrefabForKind(kind);
      if (prefab != null)
      {
        entity.AddViewPrefab(prefab);
      }
      else
      {
        // Create visual directly using factory
        GameObject visual = _factory.CreatePedestrianVisual(kind, position);
        EntityBehaviour entityBehaviour = visual.AddComponent<EntityBehaviour>();
        entity.AddView(entityBehaviour);
        entityBehaviour.SetEntity(entity);
      }

      // Apply crossing movement with type-specific speed
      float crossingChance = Mathf.Clamp01(_settings.CrossingChance);
      if (UnityEngine.Random.value < crossingChance)
      {
        float speed = visualData.BaseSpeed;
        if (speed > 0f)
        {
          float sign = UnityEngine.Random.value < 0.5f ? -1f : 1f;
          entity.AddMoveDirection(Vector3.right * sign);
          entity.AddMoveSpeed(speed);
        }
      }
    }

    private EntityBehaviour SelectPrefabForKind(PedestrianKind kind)
    {
      // Use legacy prefabs if available
      if (_factory.IsProtected(kind) && _settings.ForbiddenPrefab != null)
        return _settings.ForbiddenPrefab;

      if (!_factory.IsProtected(kind) && _settings.TargetPrefab != null)
        return _settings.TargetPrefab;

      return null;
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
