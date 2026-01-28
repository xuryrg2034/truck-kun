using Code.Infrastructure.View;
using UnityEngine;

namespace Code.Art.VFX
{
  public class CameraFollow : MonoBehaviour
  {
    [SerializeField] private Vector3 _followOffset = new Vector3(0f, 1f, -10f);
    [SerializeField] private bool _useLocalOffset = false;
    [SerializeField] private float _smoothTime = 0.2f;

    private Transform _target;
    private Vector3 _velocity;

    private void LateUpdate()
    {
      if (_target == null)
        TryAcquireTarget();

      if (_target == null)
        return;

      Vector3 desired = _useLocalOffset
        ? _target.TransformPoint(_followOffset)
        : _target.position + _followOffset;
      transform.position = Vector3.SmoothDamp(transform.position, desired, ref _velocity, _smoothTime);
    }

    private bool TryAcquireTarget()
    {
      if (_target != null)
        return true;

      foreach (EntityBehaviour view in FindObjectsOfType<EntityBehaviour>())
      {
        if (view.Entity != null && view.Entity.isHero)
        {
          _target = view.transform;
          return true;
        }
      }

      return false;
    }
  }
}
