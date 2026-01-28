using System.Collections.Generic;
using Code.Infrastructure.Systems;
using Entitas;
using UnityEngine;
using Zenject;

namespace Code.Infrastructure.View
{
  public interface IEntityViewFactory
  {
    EntityBehaviour CreateViewForEntityFromPrefab(GameEntity entity);
  }

  public class EntityViewFactory : IEntityViewFactory
  {
    private readonly IInstantiator _instantiator;
    private readonly Vector3 _farAway = new(-999f, 999f, -999f);

    public EntityViewFactory(IInstantiator instantiator)
    {
      _instantiator = instantiator;
    }

    public EntityBehaviour CreateViewForEntityFromPrefab(GameEntity entity)
    {
      EntityBehaviour view = _instantiator.InstantiatePrefabForComponent<EntityBehaviour>(
        entity.ViewPrefab,
        _farAway,
        Quaternion.identity,
        null);

      view.SetEntity(entity);
      return view;
    }
  }

  public sealed class BindViewFeature : Feature
  {
    public BindViewFeature(ISystemFactory systems)
    {
      Add(systems.Create<BindEntityViewFromPrefabSystem>());
    }
  }

  public class BindEntityViewFromPrefabSystem : IExecuteSystem
  {
    private readonly IEntityViewFactory _entityViewFactory;
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(32);

    public BindEntityViewFromPrefabSystem(GameContext game, IEntityViewFactory entityViewFactory)
    {
      _entityViewFactory = entityViewFactory;
      _entities = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.ViewPrefab)
        .NoneOf(GameMatcher.View));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        _entityViewFactory.CreateViewForEntityFromPrefab(entity);
      }
    }
  }
}
