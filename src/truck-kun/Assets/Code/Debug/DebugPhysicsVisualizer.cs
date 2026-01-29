using Code.Gameplay.Features.Physics;
using Code.Infrastructure.View;
using Entitas;
using UnityEngine;

namespace Code.Debug
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  /// <summary>
  /// Visualizes physics data in Scene view using Gizmos.
  /// Attach to any GameObject in the scene.
  /// </summary>
  public class DebugPhysicsVisualizer : MonoBehaviour
  {
    [Header("Visualization")]
    [SerializeField] private bool _showVelocity = true;
    [SerializeField] private bool _showTargetVelocity = true;
    [SerializeField] private bool _showSurface = true;
    [SerializeField] private bool _showCollisionPoints = true;

    [Header("Colors")]
    [SerializeField] private Color _velocityColor = Color.blue;
    [SerializeField] private Color _targetVelocityColor = Color.green;
    [SerializeField] private Color _lateralColor = Color.yellow;

    [Header("Scale")]
    [SerializeField] private float _velocityScale = 0.5f;
    [SerializeField] private float _surfaceRadius = 2f;

    // Cached data for Gizmos (updated in Update)
    private Vector3 _heroPosition;
    private Vector3 _currentVelocity;
    private Vector3 _targetVelocity;
    private float _currentSpeed;
    private float _maxSpeed;
    private float _lateralSpeed;
    private float _maxLateralSpeed;
    private SurfaceType _currentSurface;
    private float _friction;
    private float _drag;
    private bool _hasHero;

    // Collision tracking
    private Vector3 _lastCollisionPoint;
    private float _lastCollisionTime;
    private int _collisionsThisSecond;
    private float _collisionCounterResetTime;

    // References
    private GameContext _game;
    private IGroup<GameEntity> _heroes;

    private void Start()
    {
      if (Contexts.sharedInstance == null)
        return;

      _game = Contexts.sharedInstance.game;
      _heroes = _game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.WorldPosition));
    }

    private void Update()
    {
      if (_game == null || _heroes == null)
        return;

      // Reset collision counter every second
      if (Time.time > _collisionCounterResetTime)
      {
        _collisionsThisSecond = 0;
        _collisionCounterResetTime = Time.time + 1f;
      }

      // Find hero and extract data
      _hasHero = false;
      foreach (GameEntity hero in _heroes)
      {
        _hasHero = true;
        _heroPosition = hero.worldPosition.Value;

        // Current velocity
        if (hero.hasPhysicsVelocity)
          _currentVelocity = hero.physicsVelocity.Value;
        else
          _currentVelocity = Vector3.zero;

        // Target velocity (estimate from current velocity direction)
        _targetVelocity = _currentVelocity;

        // Speed values
        _currentSpeed = new Vector2(_currentVelocity.x, _currentVelocity.z).magnitude;
        _lateralSpeed = Mathf.Abs(_currentVelocity.x);

        // Surface data
        if (hero.hasSurfaceModifier)
        {
          _currentSurface = hero.surfaceModifier.SurfaceType;
          _friction = hero.surfaceModifier.FrictionMultiplier;
          _drag = hero.surfaceModifier.DragMultiplier;
        }
        else
        {
          _currentSurface = SurfaceType.Normal;
          _friction = 1f;
          _drag = 1f;
        }

        break;
      }
    }

    /// <summary>
    /// Called by PhysicsCollisionHandler to register collision
    /// </summary>
    public void RegisterCollision(Vector3 point, float force, string pedestrianType)
    {
      _lastCollisionPoint = point;
      _lastCollisionTime = Time.time;
      _collisionsThisSecond++;

      UnityEngine.Debug.Log($"<color=red>[Collision]</color> Hit {pedestrianType} at velocity {force:F1} m/s | Collisions/sec: {_collisionsThisSecond}");
    }

    private void OnDrawGizmos()
    {
      if (!Application.isPlaying || !_hasHero)
        return;

      // Velocity vector (blue)
      if (_showVelocity && _currentVelocity.sqrMagnitude > 0.01f)
      {
        Gizmos.color = _velocityColor;
        Vector3 velocityEnd = _heroPosition + _currentVelocity * _velocityScale;
        Gizmos.DrawLine(_heroPosition, velocityEnd);
        DrawArrowHead(velocityEnd, _currentVelocity.normalized, 0.3f);

        // Lateral component (yellow)
        if (Mathf.Abs(_currentVelocity.x) > 0.1f)
        {
          Gizmos.color = _lateralColor;
          Vector3 lateralVec = new Vector3(_currentVelocity.x, 0, 0);
          Gizmos.DrawLine(_heroPosition, _heroPosition + lateralVec * _velocityScale);
        }
      }

      // Target velocity (green)
      if (_showTargetVelocity && _targetVelocity.sqrMagnitude > 0.01f)
      {
        Gizmos.color = _targetVelocityColor;
        Vector3 targetEnd = _heroPosition + _targetVelocity * _velocityScale;
        Gizmos.DrawLine(_heroPosition + Vector3.up * 0.1f, targetEnd + Vector3.up * 0.1f);
        DrawArrowHead(targetEnd + Vector3.up * 0.1f, _targetVelocity.normalized, 0.2f);
      }

      // Surface indicator (colored circle under hero)
      if (_showSurface)
      {
        Gizmos.color = GetSurfaceColor(_currentSurface);
        DrawCircle(_heroPosition + Vector3.down * 0.5f, _surfaceRadius, 24);
      }

      // Last collision point
      if (_showCollisionPoints && Time.time - _lastCollisionTime < 2f)
      {
        float alpha = 1f - (Time.time - _lastCollisionTime) / 2f;
        Gizmos.color = new Color(1f, 0f, 0f, alpha);
        Gizmos.DrawSphere(_lastCollisionPoint, 0.3f);
      }
    }

    private void DrawArrowHead(Vector3 position, Vector3 direction, float size)
    {
      if (direction == Vector3.zero)
        return;

      Vector3 right = Vector3.Cross(Vector3.up, direction).normalized;
      Vector3 p1 = position - direction * size + right * size * 0.5f;
      Vector3 p2 = position - direction * size - right * size * 0.5f;

      Gizmos.DrawLine(position, p1);
      Gizmos.DrawLine(position, p2);
    }

    private void DrawCircle(Vector3 center, float radius, int segments)
    {
      float angleStep = 360f / segments;
      Vector3 prevPoint = center + new Vector3(radius, 0, 0);

      for (int i = 1; i <= segments; i++)
      {
        float angle = i * angleStep * Mathf.Deg2Rad;
        Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        Gizmos.DrawLine(prevPoint, nextPoint);
        prevPoint = nextPoint;
      }
    }

    private Color GetSurfaceColor(SurfaceType type)
    {
      return type switch
      {
        SurfaceType.Oil => new Color(0.1f, 0.1f, 0.1f, 0.5f),
        SurfaceType.Grass => new Color(0.2f, 0.8f, 0.2f, 0.5f),
        SurfaceType.Ice => new Color(0.7f, 0.9f, 1f, 0.5f),
        SurfaceType.Puddle => new Color(0.3f, 0.5f, 0.8f, 0.5f),
        _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
      };
    }

  }
#endif
}
