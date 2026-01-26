using Code.Balance;
using Code.Common;
using Code.Gameplay;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Feedback;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest;
using Code.Gameplay.Input;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Meta.Difficulty;
using Code.Meta.Upgrades;
using Code.UI.EndDayScreen;
using Code.UI.QuestUI;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Code.Infrastructure
{
  public class EcsBootstrap : MonoBehaviour
  {
    [Header("Core")]
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _heroSpawn;
    [SerializeField] private EntityBehaviour _heroViewPrefab;

    [Header("Balance (Required)")]
    [SerializeField] private GameBalance _gameBalance;

    // Runtime settings (created from GameBalance)
    private RunnerMovementSettings _runnerMovement;
    private DaySessionSettings _daySessionSettings;
    private PedestrianSpawnSettings _pedestrianSpawnSettings;
    private CollisionSettings _collisionSettings;
    private QuestSettings _questSettings;
    private EconomySettings _economySettings;
    private FeedbackSettings _feedbackSettings;

    private IBalanceProvider _balanceProvider;
    private IDifficultyService _difficultyService;
    private DayCounterUI _dayCounterUI;

    private DiContainer _container;
    private BattleFeature _battleFeature;
    private IInputService _inputService;
    private IDaySessionService _daySessionService;
    private IMoneyService _moneyService;
    private IQuestService _questService;
    private IUpgradeService _upgradeService;
    private QuestUIController _questUI;
    private EndDayController _endDayController;
    private bool _dayFinishedHandled;

    private void Awake()
    {
      // Validate required references
      if (!ValidateRequirements())
        return;

      // Initialize from GameBalance
      InitializeBalance();
      CreateSettingsFromBalance();

      // Load persistent state
      GameStateService gameState = GameStateService.Instance;
      int currentDay = gameState.DayNumber;

      // Initialize difficulty and apply scaling
      InitializeDifficulty(currentDay);

      // Apply upgrades from persistent state
      ApplyUpgradesToSettings();

      // Setup DI container
      Contexts contexts = Contexts.sharedInstance;
      _container = new DiContainer();

      BindContexts(contexts);
      BindServices();

      _inputService = _container.Resolve<IInputService>();

      // Initialize upgrades service
      _upgradeService = _container.Resolve<IUpgradeService>();
      _upgradeService.Initialize();

      // Create game systems
      ISystemFactory systems = _container.Resolve<ISystemFactory>();
      _battleFeature = systems.Create<BattleFeature>();
      _battleFeature.Initialize();

      _moneyService = _container.Resolve<IMoneyService>();
      _questService = _container.Resolve<IQuestService>();

      // Initialize UI
      InitializeQuestUI(contexts);
      InitializeEndDayScreen(contexts);
      InitializeDayCounterUI(currentDay);

      // Handle milestone bonus
      HandleMilestoneBonus(currentDay);

      // Start day
      _daySessionService = _container.Resolve<IDaySessionService>();
      _daySessionService.StartDay();

      if (_daySessionService.State == DayState.Finished)
        HandleDayFinished();

      DifficultyConfig difficulty = _difficultyService.CurrentDifficulty;
      Debug.Log($"[EcsBootstrap] Day {currentDay} started. Balance: {gameState.PlayerMoney}¥. " +
                $"Difficulty: Quests={difficulty.QuestCount}, SpawnInterval={difficulty.PedestrianSpawnInterval:F2}");
    }

    private bool ValidateRequirements()
    {
      bool valid = true;

      if (_inputActions == null)
      {
        Debug.LogError("[EcsBootstrap] InputActionAsset is missing!");
        valid = false;
      }

      if (_heroViewPrefab == null)
      {
        Debug.LogError("[EcsBootstrap] Hero View Prefab is missing!");
        valid = false;
      }

      // Try to load GameBalance from Resources if not assigned
      if (_gameBalance == null)
        _gameBalance = Resources.Load<GameBalance>("Configs/GameBalance");

      if (_gameBalance == null)
      {
        Debug.LogError("[EcsBootstrap] GameBalance is required! Create one at Assets/Resources/Configs/GameBalance.asset");
        valid = false;
      }

      if (!valid)
        enabled = false;

      return valid;
    }

    private void InitializeBalance()
    {
      _balanceProvider = new BalanceProvider(_gameBalance);
      Debug.Log("[EcsBootstrap] Using GameBalance configuration");
    }

    private void CreateSettingsFromBalance()
    {
      GameBalance balance = _balanceProvider.Balance;

      // Movement settings
      _runnerMovement = new RunnerMovementSettings
      {
        ForwardSpeed = balance.Movement.ForwardSpeed,
        LateralSpeed = balance.Movement.LateralSpeed,
        RoadWidth = balance.Movement.RoadWidth
      };

      // Collision settings
      _collisionSettings = new CollisionSettings
      {
        HitRadius = balance.Movement.HitRadius
      };

      // Pedestrian settings
      _pedestrianSpawnSettings = new PedestrianSpawnSettings
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

      // Day settings
      _daySessionSettings = new DaySessionSettings
      {
        DurationSeconds = balance.Day.DurationSeconds
      };

      // Quest settings
      _questSettings = new QuestSettings
      {
        MinQuestsPerDay = balance.Day.MinQuestsPerDay,
        MaxQuestsPerDay = balance.Day.MaxQuestsPerDay
      };

      // Economy settings
      _economySettings = new EconomySettings
      {
        StartingMoney = balance.Economy.StartingMoney,
        ViolationPenalty = balance.Economy.ViolationPenalty,
        BaseQuestReward = balance.Economy.BaseQuestReward
      };

      // Feedback settings
      _feedbackSettings = new FeedbackSettings
      {
        ParticleBurstCount = balance.Feedback.ParticleBurstCount,
        ParticleLifetime = balance.Feedback.ParticleLifetime,
        ParticleSpeed = balance.Feedback.ParticleSpeed,
        ParticleGravity = balance.Feedback.ParticleGravity,
        ParticleSize = balance.Feedback.ParticleSize,
        FloatSpeed = balance.Feedback.FloatSpeed,
        FloatDuration = balance.Feedback.FloatDuration,
        FontSize = balance.Feedback.FontSize,
        SFXVolume = balance.Feedback.SFXVolume,
        RewardColor = balance.Feedback.RewardColor,
        PenaltyColor = balance.Feedback.PenaltyColor
      };
    }

    private void ApplyUpgradesToSettings()
    {
      GameStateService state = GameStateService.Instance;

      // Apply speed upgrade
      float speedBonus = GetUpgradeBonus(UpgradeType.SpeedBoost, state.GetUpgradeLevel(UpgradeType.SpeedBoost));
      if (speedBonus > 0)
        _runnerMovement.ForwardSpeed *= (1f + speedBonus);

      // Apply maneuverability upgrade
      float lateralBonus = GetUpgradeBonus(UpgradeType.Maneuverability, state.GetUpgradeLevel(UpgradeType.Maneuverability));
      if (lateralBonus > 0)
        _runnerMovement.LateralSpeed *= (1f + lateralBonus);

      // Set starting money from state
      _economySettings.StartingMoney = state.PlayerMoney;
    }

    private float GetUpgradeBonus(UpgradeType type, int level)
    {
      if (level <= 0)
        return 0f;

      UpgradeDefinitionBalance upgrade = _balanceProvider.Balance.GetUpgrade(type);
      return upgrade?.GetBonus(level) ?? 0f;
    }

    private void BindContexts(Contexts contexts)
    {
      _container.BindInstance(contexts).AsSingle();
      _container.BindInstance(contexts.game).AsSingle();
      _container.BindInstance(contexts.input).AsSingle();
      _container.BindInstance(contexts.meta).AsSingle();
    }

    private void BindServices()
    {
      // Core services
      _container.BindInstance(_inputActions).AsSingle();
      _container.Bind<IInputService>().To<InputSystemService>().AsSingle();
      _container.Bind<IIdentifierService>().To<IdentifierService>().AsSingle();
      _container.Bind<ITimeService>().To<UnityTimeService>().AsSingle();

      // Balance
      _container.BindInstance(_balanceProvider).AsSingle();
      _container.BindInstance(_difficultyService).AsSingle();

      // Settings
      _container.BindInstance(_runnerMovement).AsSingle();
      _container.BindInstance(_daySessionSettings).AsSingle();
      _container.BindInstance(_pedestrianSpawnSettings).AsSingle();
      _container.BindInstance(_collisionSettings).AsSingle();
      _container.BindInstance(_questSettings).AsSingle();
      _container.BindInstance(_economySettings).AsSingle();
      _container.BindInstance(_feedbackSettings).AsSingle();

      // Day service
      _container.Bind<IDaySessionService>().To<DaySessionService>().AsSingle();

      // Pedestrian factory
      PedestrianFactory pedestrianFactory = new PedestrianFactory(null, _balanceProvider);
      _container.BindInstance<IPedestrianFactory>(pedestrianFactory).AsSingle();

      // Hero factory
      Vector3 spawnPosition = _heroSpawn != null ? _heroSpawn.position : Vector3.zero;
      _container.Bind<IHeroSpawnPoint>().To<HeroSpawnPoint>().AsSingle()
        .WithArguments(spawnPosition, _heroViewPrefab);
      _container.Bind<IHeroFactory>().To<HeroFactory>().AsSingle();
      _container.Bind<IEntityViewFactory>().To<EntityViewFactory>().AsSingle();

      // Quest service
      _container.Bind<IQuestService>().To<QuestService>().AsSingle();

      // Money service
      _container.Bind<IMoneyService>().To<MoneyService>().AsSingle();

      // Feedback services
      _container.Bind<IAudioService>().To<AudioService>().AsSingle();
      _container.Bind<IHitEffectService>().To<HitEffectService>().AsSingle();
      _container.Bind<IFloatingTextService>().To<FloatingTextService>().AsSingle();

      // Upgrade service
      _container.Bind<IUpgradeService>().To<UpgradeService>().AsSingle();

      // System factory
      _container.Bind<ISystemFactory>().To<SystemFactory>().AsSingle();
    }

    private void InitializeDifficulty(int dayNumber)
    {
      DifficultyScalingSettings difficultySettings = _gameBalance.Difficulty.ToSettings();
      _difficultyService = new DifficultyScalingService(difficultySettings, _balanceProvider);
      _difficultyService.ApplyDifficulty(dayNumber);

      // Apply difficulty scaling to settings
      DifficultyConfig difficulty = _difficultyService.CurrentDifficulty;

      _pedestrianSpawnSettings.SpawnInterval = difficulty.PedestrianSpawnInterval;
      _pedestrianSpawnSettings.MaxActive = difficulty.MaxPedestrians;
      _pedestrianSpawnSettings.CrossingChance = difficulty.CrossingChance;

      _runnerMovement.ForwardSpeed *= difficulty.ForwardSpeedMultiplier;

      _questSettings.MinQuestsPerDay = difficulty.QuestCount;
      _questSettings.MaxQuestsPerDay = difficulty.QuestCount;
    }

    private void Update()
    {
      if (_battleFeature == null || _daySessionService == null)
        return;

      if (_daySessionService.State == DayState.Finished)
      {
        HandleDayFinished();
        return;
      }

      if (_daySessionService.Tick())
      {
        HandleDayFinished();
        return;
      }

      _battleFeature.Execute();
      _battleFeature.Cleanup();
    }

    private void OnDestroy()
    {
      if (_battleFeature != null)
      {
        _battleFeature.TearDown();
        _battleFeature = null;
      }

      (_inputService as System.IDisposable)?.Dispose();
      _inputService = null;
    }

    private void InitializeQuestUI(Contexts contexts)
    {
      GameObject questUIObj = new GameObject("QuestUI");
      questUIObj.transform.SetParent(transform, false);

      _questUI = questUIObj.AddComponent<QuestUIController>();
      _questUI.Initialize(contexts.meta);
    }

    private void InitializeEndDayScreen(Contexts contexts)
    {
      GameObject endDayObj = new GameObject("EndDayScreen");
      endDayObj.transform.SetParent(transform, false);

      _endDayController = endDayObj.AddComponent<EndDayController>();
      _endDayController.Initialize(contexts.meta, _moneyService, _questService);
    }

    private void HandleDayFinished()
    {
      if (_dayFinishedHandled)
        return;

      _dayFinishedHandled = true;
      _endDayController.Show();
    }

    private void InitializeDayCounterUI(int dayNumber)
    {
      GameObject dayCounterObj = new GameObject("DayCounterUI");
      dayCounterObj.transform.SetParent(transform, false);

      _dayCounterUI = dayCounterObj.AddComponent<DayCounterUI>();

      DifficultyConfig difficulty = _difficultyService.CurrentDifficulty;
      _dayCounterUI.Initialize(
        dayNumber,
        difficulty.IsMilestoneDay,
        difficulty.MilestoneBonus
      );
    }

    private void HandleMilestoneBonus(int dayNumber)
    {
      if (!_difficultyService.IsMilestoneDay(dayNumber))
        return;

      int bonus = _difficultyService.GetMilestoneBonus(dayNumber);
      if (bonus > 0)
      {
        GameStateService.Instance.AddMoney(bonus);
        Debug.Log($"[EcsBootstrap] Milestone Day {dayNumber}! Bonus: +{bonus}");
      }
    }
  }
}
