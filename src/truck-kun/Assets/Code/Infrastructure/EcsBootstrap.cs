using Code.Common;
using Code.Gameplay;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest;
using Code.Gameplay.Input;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Code.Meta.Upgrades;
using Code.UI.EndDayScreen;
using Code.UI.QuestUI;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

namespace Code.Infrastructure
{
  public class EcsBootstrap : MonoBehaviour
  {
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _heroSpawn;
    [SerializeField] private EntityBehaviour _heroViewPrefab;
    [SerializeField] private RunnerMovementSettings _runnerMovement = new RunnerMovementSettings();
    [SerializeField] private DaySessionSettings _daySessionSettings = new DaySessionSettings();
    [SerializeField] private PedestrianSpawnSettings _pedestrianSpawnSettings = new PedestrianSpawnSettings();
    [SerializeField] private CollisionSettings _collisionSettings = new CollisionSettings();
    [SerializeField] private QuestConfig _questConfig;
    [SerializeField] private QuestSettings _questSettings = new QuestSettings();
    [SerializeField] private EconomySettings _economySettings = new EconomySettings();
    [SerializeField] private UpgradeConfig _upgradeConfig;

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
      if (_inputActions == null)
      {
        Debug.LogError("EcsBootstrap: InputActionAsset is missing.");
        enabled = false;
        return;
      }

      Contexts contexts = Contexts.sharedInstance;
      _container = new DiContainer();

      BindContexts(contexts);
      BindServices();

      _inputService = _container.Resolve<IInputService>();

      // Initialize upgrades and apply to settings before creating features
      _upgradeService = _container.Resolve<IUpgradeService>();
      _upgradeService.Initialize();
      _upgradeService.ApplyUpgradesToSettings(_runnerMovement);

      ISystemFactory systems = _container.Resolve<ISystemFactory>();
      _battleFeature = systems.Create<BattleFeature>();
      _battleFeature.Initialize();

      _moneyService = _container.Resolve<IMoneyService>();
      _questService = _container.Resolve<IQuestService>();

      InitializeQuestUI(contexts);
      InitializeEndDayScreen(contexts);

      _daySessionService = _container.Resolve<IDaySessionService>();
      _daySessionService.StartDay();
      if (_daySessionService.State == DayState.Finished)
        HandleDayFinished();
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
      _container.BindInstance(_inputActions).AsSingle();
      _container.Bind<IInputService>().To<InputSystemService>().AsSingle();

      _container.Bind<IIdentifierService>().To<IdentifierService>().AsSingle();
      _container.Bind<ITimeService>().To<UnityTimeService>().AsSingle();

      if (_runnerMovement == null)
        _runnerMovement = new RunnerMovementSettings();

      _container.BindInstance(_runnerMovement).AsSingle();

      if (_daySessionSettings == null)
        _daySessionSettings = new DaySessionSettings();

      _container.BindInstance(_daySessionSettings).AsSingle();
      _container.Bind<IDaySessionService>().To<DaySessionService>().AsSingle();

      if (_pedestrianSpawnSettings == null)
        _pedestrianSpawnSettings = new PedestrianSpawnSettings();

      _container.BindInstance(_pedestrianSpawnSettings).AsSingle();

      if (_collisionSettings == null)
        _collisionSettings = new CollisionSettings();

      _container.BindInstance(_collisionSettings).AsSingle();

      Vector3 spawnPosition = _heroSpawn != null ? _heroSpawn.position : Vector3.zero;
      _container.Bind<IHeroSpawnPoint>().To<HeroSpawnPoint>().AsSingle()
        .WithArguments(spawnPosition, _heroViewPrefab);

      _container.Bind<IHeroFactory>().To<HeroFactory>().AsSingle();

      _container.Bind<IEntityViewFactory>().To<EntityViewFactory>().AsSingle();

      if (_questConfig != null)
        _container.BindInstance(_questConfig).AsSingle();

      if (_questSettings == null)
        _questSettings = new QuestSettings();

      _container.BindInstance(_questSettings).AsSingle();
      _container.Bind<IQuestService>().To<QuestService>().AsSingle();

      if (_economySettings == null)
        _economySettings = new EconomySettings();

      _container.BindInstance(_economySettings).AsSingle();
      _container.Bind<IMoneyService>().To<MoneyService>().AsSingle();

      if (_upgradeConfig != null)
        _container.BindInstance(_upgradeConfig).AsSingle();

      _container.Bind<IUpgradeService>().To<UpgradeService>().AsSingle();

      _container.Bind<ISystemFactory>().To<SystemFactory>().AsSingle();
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
  }
}
