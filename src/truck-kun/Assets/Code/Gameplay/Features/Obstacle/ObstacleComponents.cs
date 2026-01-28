using System;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Obstacle
{
  #region Enums

  /// <summary>
  /// Types of road obstacles.
  /// </summary>
  public enum ObstacleKind
  {
    /// <summary>Inclined surface - launches hero upward via physics</summary>
    Ramp,

    /// <summary>Heavy movable object - slows hero on impact</summary>
    Barrier,

    /// <summary>Small bump - minor vertical impulse and speed loss</summary>
    SpeedBump,

    /// <summary>Trigger zone - pulls down, significant speed penalty</summary>
    Hole
  }

  #endregion

  #region Components

  /// <summary>
  /// Flag marking an entity as an obstacle.
  /// </summary>
  [Game]
  public class Obstacle : IComponent { }

  /// <summary>
  /// Type of obstacle for behavior differentiation.
  /// </summary>
  [Game]
  public class ObstacleType : IComponent
  {
    public ObstacleKind Value;
  }

  /// <summary>
  /// Speed penalty applied when hero interacts with obstacle.
  /// Value is percentage reduction (0.2 = 20% speed loss).
  /// </summary>
  [Game]
  public class SpeedPenalty : IComponent
  {
    public float Value;
  }

  /// <summary>
  /// Marks obstacle as passable (pedestrians can walk through/over).
  /// </summary>
  [Game]
  public class Passable : IComponent { }

  #endregion

  #region Settings

  /// <summary>
  /// Settings for obstacle physics and effects.
  /// </summary>
  [Serializable]
  public class ObstacleSettings
  {
    [Header("Ramp")]
    [Tooltip("Angle of ramp incline in degrees")]
    public float RampAngle = 15f;

    [Header("Barrier")]
    [Tooltip("Mass of barrier in kg")]
    public float BarrierMass = 200f;

    [Tooltip("Speed penalty when hitting barrier (0-1)")]
    public float BarrierSpeedPenalty = 0.3f;

    [Header("Speed Bump")]
    [Tooltip("Vertical impulse force for speed bump")]
    public float SpeedBumpImpulse = 200f;

    [Tooltip("Speed penalty for speed bump (0-1)")]
    public float SpeedBumpPenalty = 0.1f;

    [Header("Hole")]
    [Tooltip("Downward force in hole zone")]
    public float HoleDownForce = 500f;

    [Tooltip("Speed penalty while in hole (0-1)")]
    public float HoleSpeedPenalty = 0.5f;

    [Tooltip("Duration of speed penalty after exiting hole")]
    public float HolePenaltyDuration = 1f;
  }

  #endregion

  // ObstacleBehaviour moved to separate file: ObstacleBehaviour.cs
}
