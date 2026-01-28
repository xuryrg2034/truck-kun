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

  #region MonoBehaviour Bridge

  /// <summary>
  /// Attach to obstacle prefabs to handle trigger/collision events.
  /// Bridges Unity physics events to ECS.
  /// </summary>
  public class ObstacleBehaviour : MonoBehaviour
  {
    [Header("Type")]
    [SerializeField] private ObstacleKind _kind;
    [SerializeField] private bool _isPassable;

    [Header("Ramp Settings")]
    [SerializeField] private float _rampAngle = 15f;

    [Header("Barrier Settings")]
    [SerializeField] private float _barrierMass = 200f;
    [SerializeField] private float _barrierSpeedPenalty = 0.3f;

    [Header("SpeedBump Settings")]
    [SerializeField] private float _speedBumpImpulse = 200f;
    [SerializeField] private float _speedBumpPenalty = 0.1f;

    [Header("Hole Settings")]
    [SerializeField] private float _holeDownForce = 500f;
    [SerializeField] private float _holeSpeedPenalty = 0.5f;

    public ObstacleKind Kind => _kind;
    public bool IsPassable => _isPassable;
    public float RampAngle => _rampAngle;
    public float BarrierMass => _barrierMass;
    public float BarrierSpeedPenalty => _barrierSpeedPenalty;
    public float SpeedBumpImpulse => _speedBumpImpulse;
    public float SpeedBumpPenalty => _speedBumpPenalty;
    public float HoleDownForce => _holeDownForce;
    public float HoleSpeedPenalty => _holeSpeedPenalty;

    private void OnTriggerEnter(Collider other)
    {
      if (_kind != ObstacleKind.Hole)
        return;

      HandleHoleEnter(other);
    }

    private void OnTriggerStay(Collider other)
    {
      if (_kind != ObstacleKind.Hole)
        return;

      HandleHoleStay(other);
    }

    private void OnTriggerExit(Collider other)
    {
      if (_kind != ObstacleKind.Hole)
        return;

      HandleHoleExit(other);
    }

    private void OnCollisionEnter(UnityEngine.Collision collision)
    {
      if (_kind == ObstacleKind.SpeedBump)
      {
        HandleSpeedBumpHit(collision);
      }
      else if (_kind == ObstacleKind.Barrier)
      {
        HandleBarrierHit(collision);
      }
    }

    private void HandleHoleEnter(Collider other)
    {
      Rigidbody rb = other.attachedRigidbody;
      if (rb == null)
        return;

      Debug.Log($"[Obstacle] {other.name} entered Hole");
    }

    private void HandleHoleStay(Collider other)
    {
      Rigidbody rb = other.attachedRigidbody;
      if (rb == null)
        return;

      // Apply downward force
      rb.AddForce(Vector3.down * _holeDownForce, ForceMode.Force);

      // Apply speed penalty via drag increase
      Vector3 vel = rb.linearVelocity;
      vel.z *= (1f - _holeSpeedPenalty * Time.fixedDeltaTime);
      rb.linearVelocity = vel;
    }

    private void HandleHoleExit(Collider other)
    {
      Debug.Log($"[Obstacle] {other.name} exited Hole");
    }

    private void HandleSpeedBumpHit(UnityEngine.Collision collision)
    {
      Rigidbody rb = collision.rigidbody;
      if (rb == null)
        return;

      // Apply upward impulse
      rb.AddForce(Vector3.up * _speedBumpImpulse, ForceMode.Impulse);

      // Apply speed penalty
      Vector3 vel = rb.linearVelocity;
      vel.z *= (1f - _speedBumpPenalty);
      rb.linearVelocity = vel;

      Debug.Log($"[Obstacle] {collision.gameObject.name} hit SpeedBump");
    }

    private void HandleBarrierHit(UnityEngine.Collision collision)
    {
      Rigidbody otherRb = collision.rigidbody;
      if (otherRb == null)
        return;

      // Apply speed penalty to hero
      Vector3 vel = otherRb.linearVelocity;
      vel.z *= (1f - _barrierSpeedPenalty);
      otherRb.linearVelocity = vel;

      Debug.Log($"[Obstacle] {collision.gameObject.name} hit Barrier, speed reduced by {_barrierSpeedPenalty * 100}%");
    }
  }

  #endregion
}
