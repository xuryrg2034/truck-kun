using Code.Art.VFX;
using Code.Common.Extensions;
using Code.Common.Services;
using Code.Configs;
using Code.Configs.Global;
using Code.Configs.Spawning;
using Code.Gameplay;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Feedback;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Input;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Obstacle;
using Code.Gameplay.Features.Quest;
using Code.Gameplay.Features.Ragdoll;
using Code.Gameplay.Features.Surface;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Meta.Difficulty;
using Code.Meta.Upgrades;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Code.Infrastructure.Installers
{
  /// <summary>
  /// Installer for Gameplay scene specific services.
  /// Creates all bindings needed for ECS gameplay.
  /// </summary>
  public class GameplaySceneInstaller : MonoInstaller
  {
    [Header("Core")]
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _heroSpawn;
    [SerializeField] private EntityBehaviour _heroViewPrefab;

    [Header("Level Configuration (Required)")]
    [SerializeField] private LevelConfig _levelConfig;

    [Header("Pedestrians")]
    [SerializeField] private PedestrianConfig _pedestrianConfig;

    public override void InstallBindings()
    {
      // Validate required references
      if (_levelConfig == null)
      {
        Debug.LogError("[GameplaySceneInstaller] LevelConfig is required! " +
          "Create via: Create > Truck-kun > Level Config");
        return;
      }

      if (_inputActions == null)
      {
        Debug.LogError("[GameplaySceneInstaller] InputActionAsset is missing!");
        return;
      }

      if (_heroViewPrefab == null)
      {
        Debug.LogError("[GameplaySceneInstaller] Hero View Prefab is missing!");
        return;
      }

      // Load PedestrianConfig from Resources if not assigned
      if (_pedestrianConfig == null)
        _pedestrianConfig = Resources.Load<PedestrianConfig>("Configs/PedestrianConfig");

      if (_pedestrianConfig == null)
      {
        Debug.LogError("[GameplaySceneInstaller] PedestrianConfig not found! " +
          "Create via: Create > Truck-kun > Pedestrian Config and place in Resources/Configs/");
        return;
      }

      // Validate LevelConfig has required sub-configs
      ValidateLevelConfig();

      // Bind all configs from LevelConfig
      BindLevelConfig();

      // Bind runtime settings
      BindRuntimeSettings();

      // Contexts
      Contexts contexts = Contexts.sharedInstance;
      Container.BindInstance(contexts).AsSingle();
      Container.BindInstance(contexts.game).AsSingle();
      Container.BindInstance(contexts.input).AsSingle();
      Container.BindInstance(contexts.meta).AsSingle();

      // Input
      Container.BindInstance(_inputActions).AsSingle();
      Container.Bind<IInputService>().To<InputSystemService>().AsSingle();

      // Core Services
      Container.Bind<IIdentifierService>().To<IdentifierService>().AsSingle();
      Container.Bind<ITimeService>().To<UnityTimeService>().AsSingle();

      // Pedestrian Config
      Container.BindInstance(_pedestrianConfig).AsSingle();

      // Factories
      Container.Bind<IPedestrianFactory>().To<PedestrianFactory>().AsSingle();
      Container.Bind<IHeroFactory>().To<HeroFactory>().AsSingle();
      Container.Bind<IEntityViewFactory>().To<EntityViewFactory>().AsSingle();

      // Hero spawn
      Vector3 spawnPos = _heroSpawn != null ? _heroSpawn.position : Vector3.zero;
      Container.Bind<IHeroSpawnPoint>().To<HeroSpawnPoint>().AsSingle()
        .WithArguments(spawnPos, _heroViewPrefab);

      // Difficulty Service
      var difficultySettings = new DifficultyScalingSettings();
      IDifficultyService difficultyService = new DifficultyScalingService(difficultySettings);
      Container.BindInstance(difficultyService).AsSingle();

      // Gameplay Services
      Container.Bind<IDaySessionService>().To<DaySessionService>().AsSingle();
      Container.Bind<IQuestService>().To<QuestService>().AsSingle();
      Container.Bind<IMoneyService>().To<MoneyService>().AsSingle();
      Container.Bind<IUpgradeService>().To<UpgradeService>().AsSingle();

      // Feedback Services
      Container.Bind<IHitEffectService>().To<HitEffectService>().AsSingle();
      Container.Bind<IFloatingTextService>().To<FloatingTextService>().AsSingle();

      // VFX Controllers
      Container.Bind<HitEffectController>()
        .FromNewComponentOnNewGameObject()
        .WithGameObjectName("[HitEffects]")
        .AsSingle()
        .NonLazy();

      Container.Bind<CameraShakeController>()
        .FromNewComponentOnNewGameObject()
        .WithGameObjectName("[CameraShake]")
        .AsSingle()
        .NonLazy();

      // System Factory
      Container.Bind<ISystemFactory>().To<SystemFactory>().AsSingle();

      Debug.Log($"[GameplaySceneInstaller] Level '{_levelConfig.LevelId}' services bound");
    }

    private void ValidateLevelConfig()
    {
      if (_levelConfig.Economy == null)
        Debug.LogWarning("[GameplaySceneInstaller] LevelConfig.Economy is null!");

      if (_levelConfig.Feedback == null)
        Debug.LogWarning("[GameplaySceneInstaller] LevelConfig.Feedback is null!");

      if (_levelConfig.Day == null)
        Debug.LogWarning("[GameplaySceneInstaller] LevelConfig.Day is null!");

      if (_levelConfig.PedestrianSpawn == null)
        Debug.LogWarning("[GameplaySceneInstaller] LevelConfig.PedestrianSpawn is null!");

      if (_levelConfig.QuestPool == null)
        Debug.LogWarning("[GameplaySceneInstaller] LevelConfig.QuestPool is null!");
    }

    /// <summary>
    /// Bind all configs from LevelConfig
    /// </summary>
    private void BindLevelConfig()
    {
      // Bind the main LevelConfig
      Container.BindInstance(_levelConfig).AsSingle();

      // Bind required global configs
      if (_levelConfig.Economy != null)
        Container.BindInstance(_levelConfig.Economy).AsSingle();

      if (_levelConfig.Feedback != null)
        Container.BindInstance(_levelConfig.Feedback).AsSingle();

      // Bind required level-specific configs
      if (_levelConfig.Day != null)
        Container.BindInstance(_levelConfig.Day).AsSingle();

      if (_levelConfig.PedestrianSpawn != null)
        Container.BindInstance(_levelConfig.PedestrianSpawn).AsSingle();

      if (_levelConfig.QuestPool != null)
        Container.BindInstance(_levelConfig.QuestPool).AsSingle();

      // Bind optional spawner configs
      if (_levelConfig.SurfaceSpawn != null)
        Container.BindInstance(_levelConfig.SurfaceSpawn).AsSingle();

      if (_levelConfig.ObstacleSpawn != null)
        Container.BindInstance(_levelConfig.ObstacleSpawn).AsSingle();
    }

    /// <summary>
    /// Bind runtime settings that haven't been migrated to configs yet.
    /// These are hardcoded defaults for now.
    /// </summary>
    private void BindRuntimeSettings()
    {
      // Movement settings (TODO: move to HeroSettings on prefab)
      var movement = new RunnerMovementSettings
      {
        ForwardSpeed = 15f,
        LateralSpeed = 8f,
        RoadWidth = 8f,
        MinForwardSpeed = 9f,
        MaxForwardSpeed = 24f,
        MaxLateralSpeed = 8f,
        ForwardAcceleration = 10f,
        LateralAcceleration = 15f,
        Deceleration = 8f,
        BaseDrag = 0.5f,
        Mass = 1000f,
        AngularDrag = 0.05f,
        UseContinuousCollision = true
      };
      Container.BindInstance(movement).AsSingle();

      // Collision settings
      var collision = new CollisionSettings
      {
        HitRadius = 1.5f,
        UsePhysicsCollision = true,
        MinImpactForce = 0.5f,
        StrongImpactForce = 5f,
        ForceMultiplier = 15f,
        MinSpeedForLift = 5f,
        MaxLiftSpeed = 20f,
        LiftMultiplier = 0.3f
      };
      Container.BindInstance(collision).AsSingle();

      // Pedestrian Physics settings
      var pedestrianPhysics = new PedestrianPhysicsSettings
      {
        MoveForce = 50f,
        MaxSpeed = 3f,
        Drag = 2f,
        AngularDrag = 0.5f,
        StudentMass = 60f,
        SalarymanMass = 75f,
        GrandmaMass = 50f,
        OldManMass = 65f,
        TeenagerMass = 55f
      };
      Container.BindInstance(pedestrianPhysics).AsSingle();

      // Day Session settings (from DayConfig)
      var daySession = new DaySessionSettings
      {
        DurationSeconds = _levelConfig.Day?.DurationSeconds ?? 120f
      };
      Container.BindInstance(daySession).AsSingle();

      // Surface spawn settings
      var surface = new SurfaceSpawnSettings
      {
        EnableSpawning = false,
        SpawnChance = 0.2f,
        MinSpawnInterval = 25f,
        MaxSpawnInterval = 50f,
        MinLength = 4f,
        MaxLength = 10f,
        Width = 2.5f,
        SpawnDistanceAhead = 60f,
        DespawnDistanceBehind = 25f,
        LateralMargin = 0.5f,
        OilWeight = 1f,
        GrassWeight = 0.6f,
        PuddleWeight = 0.8f,
        IceWeight = 0.1f,
        HeightOffset = 0.02f
      };
      Container.BindInstance(surface).AsSingle();

      // Ragdoll settings
      var ragdoll = new RagdollSettings
      {
        HitForce = 800f,
        UpwardForce = 300f,
        TorqueForce = 200f,
        DespawnAfterHitDelay = 3f,
        FadeDuration = 0.5f,
        MaxActiveRagdolls = 5,
        RagdollDrag = 0.5f,
        RagdollAngularDrag = 0.5f,
        EnableFadeOut = true
      };
      Container.BindInstance(ragdoll).AsSingle();

      // Obstacle settings
      var obstacle = new ObstacleSettings
      {
        RampAngle = 15f,
        BarrierMass = 200f,
        BarrierSpeedPenalty = 0.3f,
        SpeedBumpImpulse = 200f,
        SpeedBumpPenalty = 0.1f,
        HoleDownForce = 500f,
        HoleSpeedPenalty = 0.5f,
        HolePenaltyDuration = 1f
      };
      Container.BindInstance(obstacle).AsSingle();
    }
  }
}
