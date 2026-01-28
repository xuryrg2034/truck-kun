using Code.Audio;
using Code.UI.Settings;
using UnityEngine;
using Zenject;

namespace Code.Infrastructure.Installers
{
  /// <summary>
  /// Global installer for services that persist across all scenes.
  /// Attached to ProjectContext prefab in Resources.
  /// </summary>
  public class ProjectInstaller : MonoInstaller
  {
    [SerializeField] private AudioLibrary _audioLibrary;

    public override void InstallBindings()
    {
      // Game State - bind existing singleton
      Container.Bind<GameStateService>()
        .FromInstance(GameStateService.Instance)
        .AsSingle();

      // Settings Service - bind existing singleton
      Container.Bind<ISettingsService>()
        .FromInstance(SettingsService.Instance)
        .AsSingle();

      // Audio Service - create as MonoBehaviour
      Container.Bind<IAudioService>()
        .To<AudioService>()
        .FromNewComponentOnNewGameObject()
        .WithGameObjectName("[AudioService]")
        .AsSingle()
        .NonLazy();

      // Bind AudioLibrary if assigned
      if (_audioLibrary != null)
      {
        Container.BindInstance(_audioLibrary).AsSingle();
      }

      Debug.Log("[ProjectInstaller] Global services bound");
    }
  }
}
