using System;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian
{
  /// <summary>
  /// Physics settings for pedestrian movement and collisions.
  /// Used by PedestrianForceMovementSystem for force-based movement.
  /// </summary>
  [Serializable]
  public class PedestrianPhysicsSettings
  {
    [Header("Movement Forces")]
    [Tooltip("Force applied to move pedestrian towards target velocity")]
    public float MoveForce = 50f;

    [Tooltip("Maximum movement speed")]
    public float MaxSpeed = 3f;

    [Tooltip("Linear drag applied to Rigidbody")]
    public float Drag = 2f;

    [Tooltip("Angular drag applied to Rigidbody")]
    public float AngularDrag = 0.5f;

    [Header("Mass by Type (kg)")]
    [Tooltip("Mass for StudentNerd type")]
    public float StudentMass = 60f;

    [Tooltip("Mass for Salaryman type")]
    public float SalarymanMass = 75f;

    [Tooltip("Mass for Grandma type")]
    public float GrandmaMass = 50f;

    [Tooltip("Mass for OldMan type")]
    public float OldManMass = 65f;

    [Tooltip("Mass for Teenager type")]
    public float TeenagerMass = 55f;

    /// <summary>
    /// Get mass for specific pedestrian type.
    /// </summary>
    public float GetMass(PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.StudentNerd => StudentMass,
        PedestrianKind.Salaryman => SalarymanMass,
        PedestrianKind.Grandma => GrandmaMass,
        PedestrianKind.OldMan => OldManMass,
        PedestrianKind.Teenager => TeenagerMass,
        _ => 60f // Default mass
      };
    }
  }
}
