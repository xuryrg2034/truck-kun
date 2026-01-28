using System;
using System.Collections.Generic;
using Code.Common.Components;
using Code.Gameplay.Features.Physics;
using Code.Infrastructure.Systems;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Surface
{
  #region Settings

  [Serializable]
  public class SurfaceSpawnSettings
  {
    [Header("Spawn Control")]
    [Tooltip("Enable surface hazard spawning")]
    public bool EnableSpawning = true;

    [Tooltip("Chance to spawn a surface per spawn check (0-1)")]
    [Range(0f, 1f)]
    public float SpawnChance = 0.15f;

    [Tooltip("Minimum distance between surfaces")]
    public float MinSpawnInterval = 30f;

    [Tooltip("Maximum distance between surfaces")]
    public float MaxSpawnInterval = 60f;

    [Header("Surface Dimensions")]
    [Tooltip("Minimum length of surface patch")]
    public float MinLength = 3f;

    [Tooltip("Maximum length of surface patch")]
    public float MaxLength = 8f;

    [Tooltip("Width of surface patch")]
    public float Width = 3f;

    [Header("Road Bounds")]
    [Tooltip("Distance ahead of hero to spawn")]
    public float SpawnDistanceAhead = 50f;

    [Tooltip("Distance behind hero to despawn")]
    public float DespawnDistanceBehind = 20f;

    [Tooltip("Lateral margin from road edge")]
    public float LateralMargin = 1f;

    [Header("Surface Type Weights")]
    [Tooltip("Weight for Oil surfaces")]
    public float OilWeight = 1f;

    [Tooltip("Weight for Grass surfaces")]
    public float GrassWeight = 0.5f;

    [Tooltip("Weight for Puddle surfaces")]
    public float PuddleWeight = 0.7f;

    [Tooltip("Weight for Ice surfaces (rare)")]
    public float IceWeight = 0.2f;

    [Header("Visual")]
    [Tooltip("Height offset above road")]
    public float HeightOffset = 0.01f;
  }

  #endregion

  #region Feature

  /// <summary>
  /// Surface Feature - handles surface hazards on the road.
  ///
  /// When EnableSpawning = true: Systems auto-spawn surfaces ahead of hero.
  /// When EnableSpawning = false: Surfaces are placed manually in scene using SurfaceTrigger.
  ///
  /// The actual surface effects are handled by:
  /// - SurfaceTrigger (MonoBehaviour) - detects hero entering/exiting
  /// - ApplySurfaceModifiersSystem (PhysicsFeature) - applies friction/drag effects
  /// </summary>
  public sealed class SurfaceFeature : Feature
  {
    public SurfaceFeature(ISystemFactory systems, SurfaceSpawnSettings settings)
    {
      // Only add spawn systems if auto-spawning is enabled
      if (settings.EnableSpawning)
      {
        Add(systems.Create<SurfaceSpawnSystem>());
        // Note: SurfaceDespawnSystem removed to avoid DI complexity
        // Surfaces are cleaned up when hero moves far enough
      }

      // Surface effects are handled by SurfaceTrigger (MonoBehaviour)
      // and ApplySurfaceModifiersSystem (in PhysicsFeature)
    }
  }

  #endregion

  #region Factory

  /// <summary>
  /// Factory for creating surface hazard GameObjects
  /// </summary>
  public static class SurfaceFactory
  {
    /// <summary>
    /// Create a surface hazard at the specified position
    /// </summary>
    public static GameObject CreateSurface(SurfaceType type, Vector3 position, float length, float width)
    {
      GameObject surface = new GameObject($"Surface_{type}");
      surface.transform.position = position;

      // Create visual mesh
      GameObject visual = CreateVisualMesh(type, length, width);
      visual.transform.SetParent(surface.transform, false);

      // Add trigger collider
      BoxCollider trigger = surface.AddComponent<BoxCollider>();
      trigger.isTrigger = true;
      trigger.size = new Vector3(width, 0.5f, length);
      trigger.center = new Vector3(0f, 0.25f, length * 0.5f);

      // Add surface trigger component
      SurfaceTrigger surfaceTrigger = surface.AddComponent<SurfaceTrigger>();
      surfaceTrigger.Setup(type);

      // Add particle effects
      AddParticleEffects(surface, type);

      return surface;
    }

    private static GameObject CreateVisualMesh(SurfaceType type, float length, float width)
    {
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
      visual.name = "Visual";

      // Remove collider from visual (we use parent's trigger)
      UnityEngine.Object.Destroy(visual.GetComponent<Collider>());

      // Scale and rotate to lay flat
      visual.transform.localScale = new Vector3(width, length, 1f);
      visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      visual.transform.localPosition = new Vector3(0f, 0.01f, length * 0.5f);

      // Set material/color based on type
      Renderer renderer = visual.GetComponent<Renderer>();
      if (renderer != null)
      {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = GetSurfaceColor(type);

        // Make semi-transparent
        SetupTransparentMaterial(mat);

        renderer.material = mat;
      }

      return visual;
    }

    private static Color GetSurfaceColor(SurfaceType type)
    {
      return type switch
      {
        SurfaceType.Oil => new Color(0.1f, 0.1f, 0.15f, 0.8f),     // Dark oil
        SurfaceType.Grass => new Color(0.2f, 0.6f, 0.2f, 0.9f),    // Green grass
        SurfaceType.Ice => new Color(0.8f, 0.9f, 1f, 0.6f),        // Light blue ice
        SurfaceType.Puddle => new Color(0.3f, 0.4f, 0.6f, 0.7f),   // Blue water
        _ => new Color(0.5f, 0.5f, 0.5f, 0.5f)
      };
    }

    private static void SetupTransparentMaterial(Material mat)
    {
      // Setup for URP transparency
      mat.SetFloat("_Surface", 1); // Transparent
      mat.SetFloat("_Blend", 0);   // Alpha
      mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
      mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
      mat.SetInt("_ZWrite", 0);
      mat.DisableKeyword("_ALPHATEST_ON");
      mat.EnableKeyword("_ALPHABLEND_ON");
      mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
      mat.renderQueue = 3000;
    }

    private static void AddParticleEffects(GameObject surface, SurfaceType type)
    {
      // Create particle system for visual feedback
      GameObject particleObj = new GameObject("Particles");
      particleObj.transform.SetParent(surface.transform, false);

      ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
      var main = ps.main;
      main.playOnAwake = false;
      main.loop = false;
      main.duration = 0.5f;
      main.startLifetime = 0.8f;
      main.startSpeed = 2f;
      main.startSize = 0.2f;
      main.maxParticles = 20;

      var emission = ps.emission;
      emission.rateOverTime = 0;
      emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 10) });

      var shape = ps.shape;
      shape.shapeType = ParticleSystemShapeType.Box;
      shape.scale = new Vector3(1f, 0.1f, 1f);

      // Color based on type
      var colorOverLifetime = ps.colorOverLifetime;
      colorOverLifetime.enabled = true;

      Color startColor = type switch
      {
        SurfaceType.Oil => Color.black,
        SurfaceType.Grass => Color.green,
        SurfaceType.Ice => Color.cyan,
        SurfaceType.Puddle => Color.blue,
        _ => Color.gray
      };

      Gradient gradient = new Gradient();
      gradient.SetKeys(
        new[] { new GradientColorKey(startColor, 0f), new GradientColorKey(startColor, 1f) },
        new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
      );
      colorOverLifetime.color = gradient;

      // Store reference in SurfaceTrigger for enter effect
      // (Would need to modify SurfaceTrigger to accept this, or use GetComponentInChildren)
    }
  }

  #endregion

  #region Systems

  /// <summary>
  /// Spawns surface hazards ahead of the hero
  /// </summary>
  public class SurfaceSpawnSystem : IExecuteSystem
  {
    private readonly SurfaceSpawnSettings _settings;
    private readonly IGroup<GameEntity> _heroes;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameObject> _activeSurfaces = new(16);

    private float _nextSpawnZ;
    private float _totalWeight;

    public SurfaceSpawnSystem(GameContext game, SurfaceSpawnSettings settings)
    {
      _settings = settings;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));

      // Calculate total weight for random selection
      _totalWeight = _settings.OilWeight + _settings.GrassWeight +
                     _settings.PuddleWeight + _settings.IceWeight;
    }

    public void Execute()
    {
      if (!_settings.EnableSpawning)
        return;

      GameEntity hero = null;
      foreach (GameEntity h in _heroes.GetEntities(_heroBuffer))
      {
        hero = h;
        break;
      }

      if (hero == null)
        return;

      Vector3 heroPos = hero.worldPosition.Value;

      // Initialize spawn position
      if (_nextSpawnZ < heroPos.z)
        _nextSpawnZ = heroPos.z + _settings.MinSpawnInterval;

      // Check if we should spawn
      float spawnZ = heroPos.z + _settings.SpawnDistanceAhead;
      while (_nextSpawnZ < spawnZ)
      {
        // Random chance to actually spawn
        if (UnityEngine.Random.value < _settings.SpawnChance)
        {
          SpawnSurface(_nextSpawnZ, heroPos.x);
        }

        // Schedule next spawn check
        _nextSpawnZ += UnityEngine.Random.Range(_settings.MinSpawnInterval, _settings.MaxSpawnInterval);
      }

      // Cleanup old surfaces behind hero
      DespawnOldSurfaces(heroPos.z - _settings.DespawnDistanceBehind);
    }

    private void DespawnOldSurfaces(float despawnZ)
    {
      for (int i = _activeSurfaces.Count - 1; i >= 0; i--)
      {
        GameObject surface = _activeSurfaces[i];
        if (surface == null)
        {
          _activeSurfaces.RemoveAt(i);
          continue;
        }

        if (surface.transform.position.z < despawnZ)
        {
          UnityEngine.Object.Destroy(surface);
          _activeSurfaces.RemoveAt(i);
        }
      }
    }

    private void SpawnSurface(float z, float heroX)
    {
      // Select random surface type based on weights
      SurfaceType type = SelectRandomSurfaceType();

      // Random dimensions
      float length = UnityEngine.Random.Range(_settings.MinLength, _settings.MaxLength);
      float width = _settings.Width;

      // Random X position (within road bounds)
      float halfRoad = 3f; // Approximate half road width
      float margin = _settings.LateralMargin + width * 0.5f;
      float x = UnityEngine.Random.Range(-halfRoad + margin, halfRoad - margin);

      Vector3 position = new Vector3(x, _settings.HeightOffset, z);

      // Create surface
      GameObject surface = SurfaceFactory.CreateSurface(type, position, length, width);
      _activeSurfaces.Add(surface);

      Debug.Log($"[SurfaceSpawn] Created {type} surface at {position}, length={length:F1}");
    }

    private SurfaceType SelectRandomSurfaceType()
    {
      float random = UnityEngine.Random.value * _totalWeight;
      float cumulative = 0f;

      cumulative += _settings.OilWeight;
      if (random < cumulative) return SurfaceType.Oil;

      cumulative += _settings.GrassWeight;
      if (random < cumulative) return SurfaceType.Grass;

      cumulative += _settings.PuddleWeight;
      if (random < cumulative) return SurfaceType.Puddle;

      return SurfaceType.Ice;
    }

  }

  #endregion
}
