using System.Collections.Generic;
using Code.Common;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Hero
{
  [Game] public class Hero : IComponent { }

  [System.Serializable]
  public class RunnerMovementSettings
  {
    public float ForwardSpeed = 5f;
    public float LateralSpeed = 5f;
    public float RoadWidth = 6f;
  }

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

  public interface IHeroFactory
  {
    GameEntity CreateHero();
  }

  public class HeroFactory : IHeroFactory
  {
    private readonly IHeroSpawnPoint _spawn;
    private readonly IIdentifierService _identifiers;

    public HeroFactory(IHeroSpawnPoint spawn, IIdentifierService identifiers)
    {
      _spawn = spawn;
      _identifiers = identifiers;
    }

    public GameEntity CreateHero()
    {
      GameEntity entity = Contexts.sharedInstance.game.CreateEntity();
      entity.isHero = true;
      entity.AddId(_identifiers.Next());
      entity.AddWorldPosition(_spawn.Position);

      if (_spawn.ViewPrefab != null)
        entity.AddViewPrefab(_spawn.ViewPrefab);

      return entity;
    }
  }

  public sealed class HeroFeature : Feature
  {
    public HeroFeature(ISystemFactory systems)
    {
      Add(systems.Create<InitializeHeroSystem>());
      Add(systems.Create<RunnerHeroMoveSystem>());
    }
  }

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
        GameMatcher.WorldPosition));
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
}
