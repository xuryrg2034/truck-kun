using Code.Common;
using Code.Gameplay;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest;
using Code.Gameplay.Input;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
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

    private DiContainer _container;
    private BattleFeature _battleFeature;
    private IInputService _inputService;
    private IDaySessionService _daySessionService;
    private bool _dayFinishedHandled;
    private GameObject _dayFinishedOverlay;

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

      ISystemFactory systems = _container.Resolve<ISystemFactory>();
      _battleFeature = systems.Create<BattleFeature>();
      _battleFeature.Initialize();

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

    private void HandleDayFinished()
    {
      if (_dayFinishedHandled)
        return;

      _dayFinishedHandled = true;

      EnsureDayFinishedOverlay();
    }

    private void EnsureDayFinishedOverlay()
    {
      if (_dayFinishedOverlay != null)
        return;

      GameObject overlayRoot = new GameObject("DayFinishedOverlay");
      overlayRoot.transform.SetParent(transform, false);

      Canvas canvas = overlayRoot.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = short.MaxValue;

      CanvasScaler scaler = overlayRoot.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      overlayRoot.AddComponent<GraphicRaycaster>();

      GameObject panel = new GameObject("Panel");
      panel.transform.SetParent(overlayRoot.transform, false);
      Image panelImage = panel.AddComponent<Image>();
      panelImage.color = new Color(0f, 0f, 0f, 0.6f);

      RectTransform panelRect = panel.GetComponent<RectTransform>();
      panelRect.anchorMin = Vector2.zero;
      panelRect.anchorMax = Vector2.one;
      panelRect.offsetMin = Vector2.zero;
      panelRect.offsetMax = Vector2.zero;

      GameObject label = new GameObject("Label");
      label.transform.SetParent(panel.transform, false);
      Text text = label.AddComponent<Text>();
      text.text = "DAY FINISHED";
      text.alignment = TextAnchor.MiddleCenter;
      text.fontSize = 64;
      text.color = Color.white;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.raycastTarget = false;

      RectTransform labelRect = label.GetComponent<RectTransform>();
      labelRect.anchorMin = new Vector2(0.5f, 0.5f);
      labelRect.anchorMax = new Vector2(0.5f, 0.5f);
      labelRect.sizeDelta = new Vector2(800f, 200f);
      labelRect.anchoredPosition = Vector2.zero;

      _dayFinishedOverlay = overlayRoot;
    }
  }
}
