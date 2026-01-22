using Code.Common;
using Code.Gameplay;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Input;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace Code.Infrastructure
{
  public class EcsBootstrap : MonoBehaviour
  {
    [SerializeField] private InputActionAsset _inputActions;
    [SerializeField] private Transform _heroSpawn;
    [SerializeField] private EntityBehaviour _heroViewPrefab;

    private DiContainer _container;
    private BattleFeature _battleFeature;
    private IInputService _inputService;

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

      Vector3 spawnPosition = _heroSpawn != null ? _heroSpawn.position : Vector3.zero;
      _container.Bind<IHeroSpawnPoint>().To<HeroSpawnPoint>().AsSingle()
        .WithArguments(spawnPosition, _heroViewPrefab);

      _container.Bind<IHeroFactory>().To<HeroFactory>().AsSingle();

      _container.Bind<IEntityViewFactory>().To<EntityViewFactory>().AsSingle();

      _container.Bind<ISystemFactory>().To<SystemFactory>().AsSingle();
    }

    private void Update()
    {
      if (_battleFeature == null)
        return;

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
  }
}
