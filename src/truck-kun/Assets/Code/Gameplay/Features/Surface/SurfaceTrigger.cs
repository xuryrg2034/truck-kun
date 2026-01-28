using Code.Gameplay.Features.Physics;
using Code.Infrastructure.View;
using UnityEngine;

namespace Code.Gameplay.Features.Surface
{
  /// <summary>
  /// Trigger zone that applies surface effects to entities entering it.
  /// Attach to a GameObject with a Collider (isTrigger = true).
  /// </summary>
  public class SurfaceTrigger : MonoBehaviour
  {
    [Header("Surface Type")]
    [SerializeField] private SurfaceType _surfaceType = SurfaceType.Normal;

    [Header("Physics Modifiers")]
    [Tooltip("Friction multiplier: 1.0 = normal, <1 = slippery, >1 = sticky")]
    [SerializeField] private float _frictionMultiplier = 1f;

    [Tooltip("Drag multiplier: affects deceleration and max speed")]
    [SerializeField] private float _dragMultiplier = 1f;

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem _enterEffect;
    [SerializeField] private ParticleSystem _continuousEffect;

    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = false;

    public SurfaceType SurfaceType => _surfaceType;
    public float FrictionMultiplier => _frictionMultiplier;
    public float DragMultiplier => _dragMultiplier;

    private void OnTriggerEnter(Collider other)
    {
      // Always log trigger events for debugging
      Debug.Log($"[SurfaceTrigger] OnTriggerEnter: {other.name}");

      GameEntity entity = GetEntityFromCollider(other);
      if (entity == null)
      {
        Debug.Log($"[SurfaceTrigger] No entity found on {other.name}");
        return;
      }

      // Only affect physics bodies (hero)
      if (!entity.isPhysicsBody)
      {
        Debug.Log($"[SurfaceTrigger] Entity {entity.id.Value} is not physics body");
        return;
      }

      if (!entity.hasSurfaceModifier)
      {
        Debug.Log($"[SurfaceTrigger] Entity {entity.id.Value} has no SurfaceModifier");
        return;
      }

      ApplySurfaceEffect(entity);

      // Play enter effect
      if (_enterEffect != null)
        _enterEffect.Play();

      // Start continuous effect
      if (_continuousEffect != null)
        _continuousEffect.Play();

      // ALWAYS log successful surface entry
      Debug.Log($"<color=yellow>[SURFACE]</color> Hero ENTERED {_surfaceType} - friction:{_frictionMultiplier}, drag:{_dragMultiplier}");
    }

    private void OnTriggerExit(Collider other)
    {
      GameEntity entity = GetEntityFromCollider(other);
      if (entity == null)
        return;

      if (!entity.isPhysicsBody || !entity.hasSurfaceModifier)
        return;

      // Reset to normal surface
      ResetSurfaceEffect(entity);

      // Stop continuous effect
      if (_continuousEffect != null)
        _continuousEffect.Stop();

      // ALWAYS log surface exit
      Debug.Log($"<color=green>[SURFACE]</color> Hero EXITED {_surfaceType} - back to Normal");
    }

    private void ApplySurfaceEffect(GameEntity entity)
    {
      entity.ReplaceSurfaceModifier(
        _frictionMultiplier,
        _dragMultiplier,
        _surfaceType
      );

      // Track which surface we're on (for nested surfaces)
      if (!entity.hasOnSurface)
        entity.AddOnSurface(gameObject);
      else
        entity.ReplaceOnSurface(gameObject);
    }

    private void ResetSurfaceEffect(GameEntity entity)
    {
      // Only reset if this is the surface we're currently on
      if (entity.hasOnSurface && entity.onSurface.SurfaceObject == gameObject)
      {
        entity.ReplaceSurfaceModifier(1f, 1f, SurfaceType.Normal);
        entity.RemoveOnSurface();
      }
    }

    private GameEntity GetEntityFromCollider(Collider other)
    {
      EntityBehaviour entityBehaviour = other.GetComponent<EntityBehaviour>();
      if (entityBehaviour == null)
        entityBehaviour = other.GetComponentInParent<EntityBehaviour>();

      return entityBehaviour?.Entity;
    }

    /// <summary>
    /// Setup surface with predefined type
    /// </summary>
    public void Setup(SurfaceType type)
    {
      _surfaceType = type;
      var (friction, drag) = GetDefaultModifiers(type);
      _frictionMultiplier = friction;
      _dragMultiplier = drag;
    }

    /// <summary>
    /// Setup surface with custom modifiers
    /// </summary>
    public void Setup(SurfaceType type, float friction, float drag)
    {
      _surfaceType = type;
      _frictionMultiplier = friction;
      _dragMultiplier = drag;
    }

    /// <summary>
    /// Get default modifiers for surface type
    /// </summary>
    public static (float friction, float drag) GetDefaultModifiers(SurfaceType type)
    {
      return type switch
      {
        SurfaceType.Normal => (1.0f, 1.0f),
        SurfaceType.Oil => (0.3f, 0.5f),     // Very slippery, low drag
        SurfaceType.Grass => (0.8f, 1.8f),   // Slightly slippery, high drag (slower)
        SurfaceType.Ice => (0.15f, 0.3f),    // Extremely slippery
        SurfaceType.Puddle => (0.85f, 1.3f), // Slightly slippery, moderate drag
        _ => (1.0f, 1.0f)
      };
    }

    #region Editor Helpers

    private void OnValidate()
    {
      // Update modifiers when type changes in editor
      if (Application.isPlaying)
        return;

      // Auto-fill modifiers based on type (can be overridden)
      if (_surfaceType != SurfaceType.Normal)
      {
        var (friction, drag) = GetDefaultModifiers(_surfaceType);
        // Only auto-fill if values are at default
        if (Mathf.Approximately(_frictionMultiplier, 1f) && Mathf.Approximately(_dragMultiplier, 1f))
        {
          _frictionMultiplier = friction;
          _dragMultiplier = drag;
        }
      }
    }

    private void OnDrawGizmos()
    {
      // Draw colored gizmo based on surface type
      Color color = _surfaceType switch
      {
        SurfaceType.Oil => new Color(0.1f, 0.1f, 0.1f, 0.5f),    // Dark
        SurfaceType.Grass => new Color(0.2f, 0.8f, 0.2f, 0.5f),  // Green
        SurfaceType.Ice => new Color(0.7f, 0.9f, 1f, 0.5f),      // Light blue
        SurfaceType.Puddle => new Color(0.3f, 0.5f, 0.8f, 0.5f), // Blue
        _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
      };

      Gizmos.color = color;

      Collider col = GetComponent<Collider>();
      if (col is BoxCollider box)
      {
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawCube(box.center, box.size);
      }
    }

    #endregion
  }
}
