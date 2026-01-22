using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Movement;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Hero
{
  [Game] public class Hero : IComponent { }

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
    private const float DefaultMoveSpeed = 5f;
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
      entity.AddMoveDirection(Vector3.zero);
      entity.AddMoveSpeed(DefaultMoveSpeed);

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
      Add(systems.Create<SetHeroDirectionByInputSystem>());
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

  public class SetHeroDirectionByInputSystem : IExecuteSystem
  {
    private readonly IGroup<InputEntity> _inputs;
    private readonly IGroup<GameEntity> _heroes;
    private readonly List<InputEntity> _inputBuffer = new(1);
    private readonly List<GameEntity> _heroBuffer = new(16);

    public SetHeroDirectionByInputSystem(GameContext game, InputContext input)
    {
      _inputs = input.GetGroup(InputMatcher.AllOf(InputMatcher.MoveInput));
      _heroes = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.MoveDirection));
    }

    public void Execute()
    {
      Vector2 move = Vector2.zero;
      foreach (InputEntity inputEntity in _inputs.GetEntities(_inputBuffer))
        move = inputEntity.moveInput.Value;

      Vector3 direction = new Vector3(move.x, 0f, move.y);
      if (direction.sqrMagnitude > 1f)
        direction.Normalize();

      foreach (GameEntity hero in _heroes.GetEntities(_heroBuffer))
        hero.ReplaceMoveDirection(direction);
    }
  }
}
