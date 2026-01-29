using UnityEngine;

namespace Code.Gameplay.Features.Physics
{
  /// <summary>
  /// Handles surface zone triggers to apply surface effects.
  /// Call this when entering/exiting surface trigger zones.
  /// </summary>
  public static class SurfaceZoneHandler
  {
    /// <summary>
    /// Apply surface zone effects to entity
    /// </summary>
    public static void EnterSurfaceZone(GameEntity entity, SurfaceType type, float friction, float drag)
    {
      if (entity == null || !entity.hasSurfaceModifier)
        return;

      entity.ReplaceSurfaceModifier(friction, drag, type);
    }

    /// <summary>
    /// Reset to normal surface
    /// </summary>
    public static void ExitSurfaceZone(GameEntity entity)
    {
      if (entity == null || !entity.hasSurfaceModifier)
        return;

      entity.ReplaceSurfaceModifier(1f, 1f, SurfaceType.Normal);
    }

    /// <summary>
    /// Get default modifiers for surface type
    /// </summary>
    public static (float friction, float drag) GetDefaultModifiers(SurfaceType type)
    {
      return type switch
      {
        SurfaceType.Normal => (1.0f, 1.0f),
        SurfaceType.Oil => (0.3f, 0.5f),     // Very slippery, less drag
        SurfaceType.Grass => (0.8f, 1.5f),   // Less slippery, more drag
        SurfaceType.Ice => (0.15f, 0.3f),    // Extremely slippery, very low drag
        SurfaceType.Puddle => (0.9f, 1.2f),  // Slightly slippery, slight drag
        _ => (1.0f, 1.0f)
      };
    }
  }
}
