using Code.Art.VFX;
using Code.Balance;
using Code.Common.Extensions;
using Code.Common.Services;
using Code.Gameplay;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Feedback;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Input;
using Code.Gameplay.Features.Pedestrian;
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

    [Header("Balance")]
    [SerializeField] private GameBalance _gameBalance;

    public override void InstallBindings()
    {
      // Validate and load GameBalance
      if (_gameBalance == null)
        _gameBalance = Resources.Load<GameBalance>("Configs/GameBalance");

      if (_gameBalance == null)
      {
        Debug.LogError("[GameplaySceneInstaller] GameBalance not found!");
        return;
      }

      // Validate required references
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

      // Balance Provider
      IBalanceProvider balanceProvider = new BalanceProvider(_gameBalance);
      Container.BindInstance(balanceProvider).AsSingle();
      Container.BindInstance(_gameBalance).AsSingle();

      // Difficulty Service
      DifficultyScalingSettings difficultySettings = _gameBalance.Difficulty.ToSettings();
      IDifficultyService difficultyService = new DifficultyScalingService(difficultySettings, balanceProvider);
      Container.BindInstance(difficultyService).AsSingle();

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

      // Create settings from balance (will be modified by EcsBootstrap for difficulty/upgrades)
      BindSettingsFromBalance(balanceProvider.Balance);

      // Factories
      Container.Bind<IPedestrianFactory>().To<PedestrianFactory>().AsSingle();
      Container.Bind<IHeroFactory>().To<HeroFactory>().AsSingle();
      Container.Bind<IEntityViewFactory>().To<EntityViewFactory>().AsSingle();

      // Hero spawn
      Vector3 spawnPos = _heroSpawn != null ? _heroSpawn.position : Vector3.zero;
      Container.Bind<IHeroSpawnPoint>().To<HeroSpawnPoint>().AsSingle()
        .WithArguments(spawnPos, _heroViewPrefab);

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

      Debug.Log("[GameplaySceneInstaller] Gameplay services bound");
    }

    private void BindSettingsFromBalance(GameBalance balance)
    {
      // Movement
      var movement = new RunnerMovementSettings
      {
        ForwardSpeed = balance.Movement.ForwardSpeed,
        LateralSpeed = balance.Movement.LateralSpeed,
        RoadWidth = balance.Movement.RoadWidth,
        MinForwardSpeed = balance.Movement.ForwardSpeed * 0.6f,
        MaxForwardSpeed = balance.Movement.ForwardSpeed * 1.6f,
        MaxLateralSpeed = balance.Movement.LateralSpeed,
        ForwardAcceleration = 10f,
        LateralAcceleration = 15f,
        Deceleration = 8f,
        BaseDrag = 0.5f,
        Mass = 1000f,
        AngularDrag = 0.05f,
        UseContinuousCollision = true
      };
      Container.BindInstance(movement).AsSingle();

      // Collision
      var collision = new CollisionSettings
      {
        HitRadius = balance.Movement.HitRadius,
        UsePhysicsCollision = true,
        MinImpactForce = 0.5f,
        StrongImpactForce = 5f
      };
      Container.BindInstance(collision).AsSingle();

      // Pedestrians
      var pedestrians = new PedestrianSpawnSettings
      {
        SpawnInterval = balance.Pedestrians.SpawnInterval,
        SpawnDistanceAhead = balance.Pedestrians.SpawnDistanceAhead,
        DespawnDistanceBehind = balance.Pedestrians.DespawnDistanceBehind,
        MaxActive = balance.Pedestrians.MaxActive,
        LateralMargin = balance.Pedestrians.LateralMargin,
        CrossingChance = balance.Pedestrians.CrossingChance,
        CrossingSpeedMultiplier = balance.Pedestrians.CrossingSpeedMultiplier,
        SidewalkOffset = balance.Pedestrians.SidewalkOffset
      };
      Container.BindInstance(pedestrians).AsSingle();

      // Day Session
      var daySession = new DaySessionSettings
      {
        DurationSeconds = balance.Day.DurationSeconds
      };
      Container.BindInstance(daySession).AsSingle();

      // Quest
      var quest = new QuestSettings
      {
        MinQuestsPerDay = balance.Day.MinQuestsPerDay,
        MaxQuestsPerDay = balance.Day.MaxQuestsPerDay
      };
      Container.BindInstance(quest).AsSingle();

      // Economy
      var economy = new EconomySettings
      {
        StartingMoney = balance.Economy.StartingMoney,
        ViolationPenalty = balance.Economy.ViolationPenalty,
        BaseQuestReward = balance.Economy.BaseQuestReward
      };
      Container.BindInstance(economy).AsSingle();

      // Feedback
      var feedback = new FeedbackSettings
      {
        ParticleBurstCount = balance.Feedback.ParticleBurstCount,
        ParticleLifetime = balance.Feedback.ParticleLifetime,
        ParticleSpeed = balance.Feedback.ParticleSpeed,
        ParticleGravity = balance.Feedback.ParticleGravity,
        ParticleSize = balance.Feedback.ParticleSize,
        FloatSpeed = balance.Feedback.FloatSpeed,
        FloatDuration = balance.Feedback.FloatDuration,
        FontSize = balance.Feedback.FontSize,
        RewardColor = balance.Feedback.RewardColor,
        PenaltyColor = balance.Feedback.PenaltyColor
      };
      Container.BindInstance(feedback).AsSingle();

      // Surface
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

      // Ragdoll
      var ragdoll = new RagdollSettings
      {
        HitForce = 800f,
        UpwardForce = 300f,
        TorqueForce = 200f,
        DespawnDelay = 2.5f,
        FadeStartDelay = 1.5f,
        MaxActiveRagdolls = 5,
        RagdollDrag = 0.5f,
        RagdollAngularDrag = 0.5f,
        EnableFadeOut = true
      };
      Container.BindInstance(ragdoll).AsSingle();
    }
  }
}
