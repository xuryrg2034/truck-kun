using UnityEngine;

namespace Code.Gameplay.Features.Obstacle
{
  /// <summary>
  /// Attach to obstacle prefabs to handle trigger/collision events.
  /// Bridges Unity physics events to ECS.
  /// </summary>
  public class ObstacleBehaviour : MonoBehaviour
  {
    [Header("Type")]
    [SerializeField] private ObstacleKind _kind;
    [SerializeField] private bool _isPassable;

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
    public float BarrierMass => _barrierMass;
    public float BarrierSpeedPenalty => _barrierSpeedPenalty;
    public float SpeedBumpImpulse => _speedBumpImpulse;
    public float SpeedBumpPenalty => _speedBumpPenalty;
    public float HoleDownForce => _holeDownForce;
    public float HoleSpeedPenalty => _holeSpeedPenalty;

    private void OnTriggerEnter(Collider other)
    {
      if (_kind == ObstacleKind.Hole)
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
      switch (_kind)
      {
        case ObstacleKind.SpeedBump:
          HandleSpeedBumpHit(collision);
          break;
        case ObstacleKind.Barrier:
          HandleBarrierHit(collision);
          break;
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
}
