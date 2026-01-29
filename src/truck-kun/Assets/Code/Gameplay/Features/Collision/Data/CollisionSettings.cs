using System;
using UnityEngine;

namespace Code.Gameplay.Features.Collision
{
  [Serializable]
  public class CollisionSettings
  {
    [Header("Legacy Distance Check (fallback)")]
    [Tooltip("Radius for distance-based collision detection (fallback if physics fails)")]
    public float HitRadius = 1.2f;

    [Header("Physics Collision")]
    [Tooltip("Use physics-based collision detection (recommended)")]
    public bool UsePhysicsCollision = true;

    [Header("Impact Thresholds")]
    [Tooltip("Minimum impact force to register a hit")]
    public float MinImpactForce = 0.5f;

    [Tooltip("Force considered a 'strong' hit (for VFX scaling)")]
    public float StrongImpactForce = 5f;

    [Header("Knockback Force")]
    [Tooltip("Multiplier for knockback force (impactSpeed * mass * multiplier)")]
    public float ForceMultiplier = 15f;

    [Tooltip("Minimum impact speed required for vertical lift")]
    public float MinSpeedForLift = 5f;

    [Tooltip("Maximum speed for lift calculation (caps the lift factor)")]
    public float MaxLiftSpeed = 20f;

    [Tooltip("Multiplier for vertical lift force (as fraction of horizontal)")]
    public float LiftMultiplier = 0.3f;
  }

  public static class CollisionExtensions
  {
    public static bool IsStrongImpact(this GameEntity hitEvent, CollisionSettings settings)
    {
      if (!hitEvent.hasCollisionImpact)
        return false;

      return hitEvent.collisionImpact.Force >= settings.StrongImpactForce;
    }

    public static float GetImpactStrength(this GameEntity hitEvent, CollisionSettings settings)
    {
      if (!hitEvent.hasCollisionImpact)
        return 0.5f;

      float force = hitEvent.collisionImpact.Force;
      return Mathf.Clamp01(force / settings.StrongImpactForce);
    }
  }
}
