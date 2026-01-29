using System.Collections.Generic;
using Code.Common.Services;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics.Systems
{
  /// <summary>
  /// Calculates target physics velocity based on input and acceleration.
  ///
  /// Forward: Constant speed (auto-runner always moves forward)
  /// Lateral: Accelerates based on input, decelerates when no input
  ///
  /// Uses smooth acceleration for natural feel.
  /// Speed values come from entity's ECS components (MoveSpeed, PhysicsConstraints).
  /// </summary>
  public class CalculatePhysicsVelocitySystem : IExecuteSystem
  {
    private readonly ITimeService _time;
    private readonly IGroup<GameEntity> _movers;
    private readonly List<GameEntity> _buffer = new(4);

    public CalculatePhysicsVelocitySystem(GameContext game, ITimeService time)
    {
      _time = time;
      _movers = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.PhysicsBody,
        GameMatcher.PhysicsVelocity,
        GameMatcher.PhysicsConstraints,
        GameMatcher.MoveSpeed,
        GameMatcher.Acceleration,
        GameMatcher.MoveDirection));
    }

    public void Execute()
    {
      float dt = _time.FixedDeltaTime;

      foreach (GameEntity entity in _movers.GetEntities(_buffer))
      {
        Vector3 currentVelocity = entity.physicsVelocity.Value;
        Vector3 moveDir = entity.moveDirection.Value;
        Acceleration accel = entity.acceleration;
        PhysicsConstraints constraints = entity.physicsConstraints;

        // === FORWARD VELOCITY ===
        // Target forward speed from entity's MoveSpeed component (set from VehicleStats)
        float targetForwardSpeed = entity.moveSpeed.Value;

        // Smoothly accelerate/decelerate to target forward speed
        float forwardVelocity;
        if (currentVelocity.z < targetForwardSpeed)
        {
          // Accelerating
          forwardVelocity = currentVelocity.z + accel.ForwardAcceleration * dt;
          forwardVelocity = Mathf.Min(forwardVelocity, targetForwardSpeed);
        }
        else if (currentVelocity.z > targetForwardSpeed)
        {
          // Decelerating (hit something?)
          forwardVelocity = currentVelocity.z - accel.Deceleration * dt;
          forwardVelocity = Mathf.Max(forwardVelocity, targetForwardSpeed);
        }
        else
        {
          forwardVelocity = targetForwardSpeed;
        }

        // === LATERAL VELOCITY ===
        float lateralInput = moveDir.x;
        // MaxLateralSpeed from entity's PhysicsConstraints component
        float targetLateralSpeed = lateralInput * constraints.MaxLateralSpeed;
        float lateralVelocity = currentVelocity.x;

        if (Mathf.Abs(lateralInput) > 0.01f)
        {
          // Player is steering - accelerate towards target
          float direction = Mathf.Sign(targetLateralSpeed - lateralVelocity);
          lateralVelocity += direction * accel.LateralAcceleration * dt;

          // Clamp to not overshoot target
          if (direction > 0)
            lateralVelocity = Mathf.Min(lateralVelocity, targetLateralSpeed);
          else
            lateralVelocity = Mathf.Max(lateralVelocity, targetLateralSpeed);
        }
        else
        {
          // No input - decelerate lateral movement towards zero
          if (Mathf.Abs(lateralVelocity) > 0.01f)
          {
            float decelAmount = accel.Deceleration * dt;
            if (lateralVelocity > 0)
              lateralVelocity = Mathf.Max(0f, lateralVelocity - decelAmount);
            else
              lateralVelocity = Mathf.Min(0f, lateralVelocity + decelAmount);
          }
          else
          {
            lateralVelocity = 0f;
          }
        }

        // Store calculated velocity (Y is preserved from current for physics)
        Vector3 newVelocity = new Vector3(lateralVelocity, 0f, forwardVelocity);

        entity.ReplacePhysicsVelocity(newVelocity);
      }
    }
  }
}
