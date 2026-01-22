using UnityEngine;

namespace Code.Infrastructure.View
{
  public interface IEntityView
  {
    GameEntity Entity { get; }
    void SetEntity(GameEntity entity);
    void ReleaseEntity();
  }

  public class EntityBehaviour : MonoBehaviour, IEntityView
  {
    private GameEntity _entity;
    public GameEntity Entity => _entity;

    public void SetEntity(GameEntity entity)
    {
      _entity = entity;
      _entity.AddView(this);
      _entity.Retain(this);
      _entity.AddTransform(transform);
    }

    public void ReleaseEntity()
    {
      if (_entity == null)
        return;

      if (_entity.hasTransform)
        _entity.RemoveTransform();
      if (_entity.hasView)
        _entity.RemoveView();

      _entity.Release(this);
      _entity = null;
    }

    private void OnDestroy() => ReleaseEntity();
  }
}
