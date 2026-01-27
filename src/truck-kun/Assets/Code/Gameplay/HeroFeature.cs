using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Physics;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Hero
{
  [Game] public class Hero : IComponent { }

  #region Settings

  [System.Serializable]
  public class RunnerMovementSettings
  {
    [Header("Legacy Speed Settings (for kinematic fallback)")]
    public float ForwardSpeed = 5f;
    public float LateralSpeed = 5f;

    [Header("Road Boundaries")]
    public float RoadWidth = 6f;

    [Header("Physics - Speed Limits")]
    [Tooltip("Minimum forward speed (always moving)")]
    public float MinForwardSpeed = 3f;

    [Tooltip("Maximum forward speed (with boost)")]
    public float MaxForwardSpeed = 8f;

    [Tooltip("Maximum lateral (sideways) speed")]
    public float MaxLateralSpeed = 5f;

    [Header("Physics - Acceleration")]
    [Tooltip("Forward acceleration rate (m/s²)")]
    public float ForwardAcceleration = 10f;

    [Tooltip("Lateral acceleration for steering (m/s²)")]
    public float LateralAcceleration = 15f;

    [Tooltip("Deceleration rate when releasing input (m/s²)")]
    public float Deceleration = 8f;

    [Header("Physics - Drag")]
    [Tooltip("Base air resistance")]
    public float BaseDrag = 0.5f;

    [Header("Rigidbody Settings")]
    [Tooltip("Mass of the truck (affects collisions)")]
    public float Mass = 1000f;

    [Tooltip("Angular drag for rotation damping")]
    public float AngularDrag = 0.05f;

    [Header("Collision")]
    [Tooltip("Use continuous collision detection")]
    public bool UseContinuousCollision = true;
  }

  #endregion

  #region Spawn Point

  public interface IHeroSpawnPoint
  {
    Vector3 Position { get; }
    EntityBehaviour ViewPrefab { get; }
  }

  public class HeroSpawnPoint : IHeroSpawnPoint
  {
    public Vector3 Position { get; }
    public EntityBehaviour ViewPrefab { get; }

    public HeroSpawnPoint(Vector3 position, EntityBehaviour viewPrefab)
    {
      Position = position;
      ViewPrefab = viewPrefab;
    }
  }

  #endregion

  #region Factory

  public interface IHeroFactory
  {
    GameEntity CreateHero();
  }

  public class HeroFactory : IHeroFactory
  {
    private readonly IHeroSpawnPoint _spawn;
    private readonly IIdentifierService _identifiers;
    private readonly RunnerMovementSettings _settings;

    public HeroFactory(
      IHeroSpawnPoint spawn,
      IIdentifierService identifiers,
      RunnerMovementSettings settings)
    {
      _spawn = spawn;
      _identifiers = identifiers;
      _settings = settings;
    }

    public GameEntity CreateHero()
    {
      GameEntity entity = Contexts.sharedInstance.game.CreateEntity();

      // Core components
      entity.isHero = true;
      entity.AddId(_identifiers.Next());
      entity.AddWorldPosition(_spawn.Position);

      // View prefab (will be instantiated by BindViewFeature)
      if (_spawn.ViewPrefab != null)
        entity.AddViewPrefab(_spawn.ViewPrefab);

      // Physics components
      AddPhysicsComponents(entity);

      // Legacy movement components (for compatibility during transition)
      entity.AddMoveDirection(Vector3.forward);
      entity.AddMoveSpeed(_settings.ForwardSpeed);

      return entity;
    }

    private void AddPhysicsComponents(GameEntity entity)
    {
      // Mark as physics body
      entity.isPhysicsBody = true;

      // Initial velocity (will be set by physics system)
      entity.AddPhysicsVelocity(new Vector3(0f, 0f, _settings.MinForwardSpeed));

      // Acceleration parameters
      entity.AddAcceleration(
        _settings.ForwardAcceleration,
        _settings.LateralAcceleration,
        _settings.Deceleration
      );

      // Drag
      entity.AddPhysicsDrag(
        _settings.BaseDrag,
        _settings.BaseDrag  // currentDrag starts at baseDrag
      );

      // Speed constraints and road boundaries
      float centerX = _spawn != null ? _spawn.Position.x : 0f;
      float halfWidth = _settings.RoadWidth * 0.5f;

      entity.AddPhysicsConstraints(
        _settings.MinForwardSpeed,
        _settings.MaxForwardSpeed,
        _settings.MaxLateralSpeed,
        centerX - halfWidth,  // minX
        centerX + halfWidth   // maxX
      );

      // Default surface (normal road)
      entity.AddSurfaceModifier(
        1.0f,                    // frictionMultiplier
        1.0f,                    // dragMultiplier
        SurfaceType.Normal
      );

      // Physics state for debugging
      entity.AddPhysicsState(
        _settings.MinForwardSpeed,  // currentSpeed
        false,                       // isSliding
        false,                       // isAtMaxSpeed
        Vector3.up                   // groundNormal
      );
    }
  }

  #endregion

  #region Rigidbody Setup Helper

  /// <summary>
  /// Helper class to configure Rigidbody on hero prefab
  /// Call this from EntityBehaviour.SetEntity() or a setup system
  /// </summary>
  public static class HeroRigidbodySetup
  {
    /// <summary>
    /// Configure Rigidbody for hero physics
    /// </summary>
    public static void ConfigureRigidbody(Rigidbody rb, RunnerMovementSettings settings)
    {
      if (rb == null)
        return;

      // Mass and drag
      rb.mass = settings.Mass;
      rb.drag = 0f;  // We handle drag in our physics system
      rb.angularDrag = settings.AngularDrag;

      // Gravity (disabled for 2.5D runner)
      rb.useGravity = false;

      // Not kinematic - we want physics simulation
      rb.isKinematic = false;

      // Interpolation for smooth rendering
      rb.interpolation = RigidbodyInterpolation.Interpolate;

      // Collision detection mode
      rb.collisionDetectionMode = settings.UseContinuousCollision
        ? CollisionDetectionMode.Continuous
        : CollisionDetectionMode.Discrete;

      // Constraints: freeze Y position (no falling), freeze X and Z rotation (no tipping)
      rb.constraints =
        RigidbodyConstraints.FreezePositionY |
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationZ;
    }

    /// <summary>
    /// Ensure hero has proper collider
    /// </summary>
    public static void EnsureCollider(GameObject heroObject)
    {
      Collider existingCollider = heroObject.GetComponent<Collider>();

      if (existingCollider != null)
        return;

      // Add default box collider if none exists
      BoxCollider box = heroObject.AddComponent<BoxCollider>();
      box.size = new Vector3(2f, 1.5f, 4f);  // Truck-sized
      box.center = new Vector3(0f, 0.75f, 0f);

      Debug.Log("[HeroRigidbodySetup] Added default BoxCollider to hero");
    }

    /// <summary>
    /// Full setup: Rigidbody + Collider
    /// </summary>
    public static Rigidbody SetupPhysicsBody(GameObject heroObject, RunnerMovementSettings settings)
    {
      // Ensure collider exists
      EnsureCollider(heroObject);

      // Get or add Rigidbody
      Rigidbody rb = heroObject.GetComponent<Rigidbody>();
      if (rb == null)
      {
        rb = heroObject.AddComponent<Rigidbody>();
        Debug.Log("[HeroRigidbodySetup] Added Rigidbody to hero");
      }

      // Configure
      ConfigureRigidbody(rb, settings);

      return rb;
    }
  }

  #endregion

  #region Feature

  public sealed class HeroFeature : Feature
  {
    public HeroFeature(ISystemFactory systems)
    {
      Add(systems.Create<InitializeHeroSystem>());
      Add(systems.Create<RunnerHeroMoveSystem>());
      // TODO: Add PhysicsHeroMoveSystem when ready
      // Add(systems.Create<PhysicsHeroMoveSystem>());
    }
  }

  #endregion

  #region Systems

  public class InitializeHeroSystem : IInitializeSystem
  {
    private readonly IHeroFactory _heroFactory;
    private readonly IGroup<GameEntity> _heroes;

    public InitializeHeroSystem(GameContext game, IHeroFactory heroFactory)
    {
      _heroFactory = heroFactory;
      _heroes = game.GetGroup(GameMatcher.Hero);
    }

    public void Initialize()
    {
      if (_heroes.count > 0)
        return;

      _heroFactory.CreateHero();
    }
  }

  /// <summary>
  /// Legacy kinematic movement system (will be replaced by PhysicsHeroMoveSystem)
  /// Kept for backward compatibility during transition
  /// </summary>
  public class RunnerHeroMoveSystem : IExecuteSystem
  {
    private readonly ITimeService _time;
    private readonly RunnerMovementSettings _settings;
    private readonly IHeroSpawnPoint _spawnPoint;
    private readonly IGroup<InputEntity> _inputs;
    private readonly IGroup<GameEntity> _heroes;
    private readonly List<InputEntity> _inputBuffer = new(1);
    private readonly List<GameEntity> _heroBuffer = new(16);

    public RunnerHeroMoveSystem(
      GameContext game,
      InputContext input,
      ITimeService time,
      RunnerMovementSettings settings,
      IHeroSpawnPoint spawnPoint)
    {
      _time = time;
      _settings = settings;
      _spawnPoint = spawnPoint;
      _inputs = input.GetGroup(InputMatcher.AllOf(InputMatcher.MoveInput));
      _heroes = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.WorldPosition)
        .NoneOf(GameMatcher.RigidbodyComponent));  // Skip physics-enabled heroes
    }

    public void Execute()
    {
      float lateralInput = 0f;
      foreach (InputEntity inputEntity in _inputs.GetEntities(_inputBuffer))
        lateralInput = inputEntity.moveInput.Value.x;

      lateralInput = Mathf.Clamp(lateralInput, -1f, 1f);

      float forwardSpeed = Mathf.Max(0f, _settings.ForwardSpeed);
      float lateralSpeed = _settings.LateralSpeed;
      float halfWidth = Mathf.Max(0f, _settings.RoadWidth * 0.5f);
      float centerX = _spawnPoint != null ? _spawnPoint.Position.x : 0f;
      float minX = centerX - halfWidth;
      float maxX = centerX + halfWidth;

      Vector3 forwardDelta = Vector3.forward * forwardSpeed * _time.DeltaTime;
      Vector3 lateralDelta = Vector3.right * lateralInput * lateralSpeed * _time.DeltaTime;

      foreach (GameEntity hero in _heroes.GetEntities(_heroBuffer))
      {
        Vector3 next = hero.worldPosition.Value + forwardDelta + lateralDelta;
        next.x = Mathf.Clamp(next.x, minX, maxX);
        hero.ReplaceWorldPosition(next);
      }
    }
  }

  #endregion
}
