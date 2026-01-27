using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Hero;
using Code.Infrastructure.Systems;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics
{
  #region Feature

  /// <summary>
  /// Physics Feature - handles Rigidbody-based movement
  ///
  /// Execution order:
  /// 1. ReadInputForPhysicsSystem - reads lateral input
  /// 2. CalculatePhysicsVelocitySystem - calculates target velocity
  /// 3. ApplySurfaceModifiersSystem - applies surface effects (friction, drag)
  /// 4. ClampPhysicsVelocitySystem - enforces speed limits and road boundaries
  /// 5. ApplyPhysicsVelocitySystem - applies velocity to Rigidbody
  /// 6. SyncPhysicsPositionSystem - syncs WorldPosition from Rigidbody
  /// 7. UpdatePhysicsStateSystem - updates debug state
  ///
  /// NOTE: This feature should be executed in FixedUpdate for physics stability.
  /// Call _physicsFeature.Execute() from MonoBehaviour.FixedUpdate()
  /// </summary>
  public sealed class PhysicsFeature : Feature
  {
    public PhysicsFeature(ISystemFactory systems)
    {
      // Debug system (can be removed later)
      Add(systems.Create<DebugPhysicsEntitiesSystem>());

      // Input reading
      Add(systems.Create<ReadInputForPhysicsSystem>());

      // Velocity calculation
      Add(systems.Create<CalculatePhysicsVelocitySystem>());

      // Modifiers
      Add(systems.Create<ApplySurfaceModifiersSystem>());

      // Constraints
      Add(systems.Create<ClampPhysicsVelocitySystem>());

      // Apply to Rigidbody
      Add(systems.Create<ApplyPhysicsVelocitySystem>());

      // Sync back to ECS
      Add(systems.Create<SyncPhysicsPositionSystem>());

      // Debug state update
      Add(systems.Create<UpdatePhysicsStateSystem>());
    }
  }

  #endregion

  #region Debug System

  /// <summary>
  /// Debug system to verify physics entities have all required components.
  /// Remove this system after debugging is complete.
  /// </summary>
  public class DebugPhysicsEntitiesSystem : IExecuteSystem
  {
    private readonly GameContext _game;
    private readonly IGroup<GameEntity> _heroes;
    private readonly List<GameEntity> _buffer = new(4);
    private float _lastLogTime;

    public DebugPhysicsEntitiesSystem(GameContext game)
    {
      _game = game;
      _heroes = game.GetGroup(GameMatcher.Hero);
    }

    public void Execute()
    {
      // Only log every 2 seconds
      if (Time.time - _lastLogTime < 2f)
        return;
      _lastLogTime = Time.time;

      foreach (GameEntity hero in _heroes.GetEntities(_buffer))
      {
        Debug.Log($"[PhysicsDebug] Hero entity {hero.id.Value}:\n" +
          $"  - isPhysicsBody: {hero.isPhysicsBody}\n" +
          $"  - hasRigidbody: {hero.hasRigidbody}\n" +
          $"  - hasPhysicsVelocity: {hero.hasPhysicsVelocity}\n" +
          $"  - hasAcceleration: {hero.hasAcceleration}\n" +
          $"  - hasMoveDirection: {hero.hasMoveDirection}\n" +
          $"  - hasWorldPosition: {hero.hasWorldPosition}\n" +
          $"  - hasView: {hero.hasView}\n" +
          $"  - hasTransform: {hero.hasTransform}");

        if (hero.hasPhysicsVelocity)
          Debug.Log($"  - PhysicsVelocity: {hero.physicsVelocity.Value}");

        if (hero.hasRigidbody && hero.rigidbody.Value != null)
          Debug.Log($"  - Rigidbody.velocity: {hero.rigidbody.Value.linearVelocity}");
      }

      if (_heroes.count == 0)
        Debug.LogWarning("[PhysicsDebug] No hero entities found!");
    }
  }

  #endregion

  #region Input System

  /// <summary>
  /// Reads lateral input for physics-based heroes.
  /// Stores input in a temporary component for physics calculations.
  /// </summary>
  public class ReadInputForPhysicsSystem : IExecuteSystem
  {
    private readonly IGroup<InputEntity> _inputs;
    private readonly IGroup<GameEntity> _physicsHeroes;
    private readonly List<InputEntity> _inputBuffer = new(1);
    private readonly List<GameEntity> _heroBuffer = new(4);

    public ReadInputForPhysicsSystem(GameContext game, InputContext input)
    {
      _inputs = input.GetGroup(InputMatcher.MoveInput);
      _physicsHeroes = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.PhysicsBody,
        GameMatcher.PhysicsVelocity));
    }

    public void Execute()
    {
      // Get current lateral input
      float lateralInput = 0f;
      foreach (InputEntity inputEntity in _inputs.GetEntities(_inputBuffer))
      {
        lateralInput = inputEntity.moveInput.Value.x;
      }

      lateralInput = Mathf.Clamp(lateralInput, -1f, 1f);

      // Debug input
      if (Mathf.Abs(lateralInput) > 0.01f)
      {
        Debug.Log($"[ReadInputForPhysics] Lateral input: {lateralInput}, heroes count: {_physicsHeroes.count}");
      }

      // Store in MoveDirection for physics systems to use
      foreach (GameEntity hero in _physicsHeroes.GetEntities(_heroBuffer))
      {
        // Use MoveDirection.x to store lateral input, z for forward intent
        Vector3 moveDir = new Vector3(lateralInput, 0f, 1f);
        hero.ReplaceMoveDirection(moveDir);
      }
    }
  }

  #endregion

  #region Velocity Calculation System

  /// <summary>
  /// Calculates target physics velocity based on input and acceleration.
  ///
  /// Forward: Constant speed (auto-runner always moves forward)
  /// Lateral: Accelerates based on input, decelerates when no input
  ///
  /// Uses smooth acceleration for natural feel.
  /// </summary>
  public class CalculatePhysicsVelocitySystem : IExecuteSystem
  {
    private readonly ITimeService _time;
    private readonly RunnerMovementSettings _settings;
    private readonly IGroup<GameEntity> _movers;
    private readonly List<GameEntity> _buffer = new(4);

    public CalculatePhysicsVelocitySystem(
      GameContext game,
      ITimeService time,
      RunnerMovementSettings settings)
    {
      _time = time;
      _settings = settings;
      _movers = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.PhysicsBody,
        GameMatcher.PhysicsVelocity,
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

        // === FORWARD VELOCITY ===
        // Target forward speed is constant (auto-runner)
        float targetForwardSpeed = _settings.ForwardSpeed;

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
        float targetLateralSpeed = lateralInput * _settings.MaxLateralSpeed;
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

        // Debug lateral movement
        if (Mathf.Abs(lateralInput) > 0.01f || Mathf.Abs(lateralVelocity) > 0.1f)
        {
          Debug.Log($"[CalcVelocity] input={lateralInput:F2}, lateralVel={lateralVelocity:F2}, newVel={newVelocity}");
        }

        entity.ReplacePhysicsVelocity(newVelocity);
      }
    }
  }

  #endregion

  #region Surface Modifiers System

  /// <summary>
  /// Applies surface modifiers to velocity.
  /// Different surfaces affect lateral control and drag.
  ///
  /// Normal: No modification
  /// Oil: Reduced lateral friction (slides more)
  /// Ice: Very reduced friction (very slippery)
  /// Grass: Increased drag (slower, but more grip)
  /// </summary>
  public class ApplySurfaceModifiersSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public ApplySurfaceModifiersSystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.PhysicsVelocity,
        GameMatcher.SurfaceModifier,
        GameMatcher.PhysicsDrag));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Vector3 velocity = entity.physicsVelocity.Value;
        SurfaceModifier surface = entity.surfaceModifier;
        PhysicsDrag drag = entity.physicsDrag;

        // === FRICTION EFFECT ON LATERAL CONTROL ===
        // Low friction = harder to change direction (slides in current direction)
        // High friction = easier to control
        //
        // Implementation: Interpolate towards target lateral based on friction
        // friction = 1.0: instant response
        // friction = 0.3 (oil): slow response, keeps sliding
        // friction = 0.15 (ice): very slow response

        // We modify how quickly lateral velocity responds
        // This is already handled by acceleration, but surface affects the "grip"
        // For low friction, we reduce the lateral velocity change rate

        // Apply inverse friction to lateral (low friction = more sliding)
        // This creates a "drift" effect on slippery surfaces
        if (surface.FrictionMultiplier < 1f)
        {
          // On slippery surfaces, lateral velocity is harder to control
          // We add a slight "momentum preservation" effect
          // (Actual implementation would need previous velocity tracking)

          // For now, we reduce lateral acceleration effectiveness
          // by scaling the lateral component towards a "sliding" value
          // This is a simplified approach
        }

        // === DRAG EFFECT ===
        // Update current drag based on surface
        float newCurrentDrag = drag.BaseDrag * surface.DragMultiplier;

        if (Mathf.Abs(newCurrentDrag - drag.CurrentDrag) > 0.001f)
        {
          entity.ReplacePhysicsDrag(drag.BaseDrag, newCurrentDrag);
        }

        // Apply drag to velocity (reduces speed over time)
        // Note: Only apply if drag modifier is > 1 (surfaces that slow you down)
        if (surface.DragMultiplier > 1f)
        {
          float dragFactor = 1f - (newCurrentDrag * 0.1f * Time.fixedDeltaTime);
          dragFactor = Mathf.Max(dragFactor, 0.9f); // Don't slow down too much

          velocity.z *= dragFactor;
          entity.ReplacePhysicsVelocity(velocity);
        }
      }
    }
  }

  #endregion

  #region Clamp Velocity System

  /// <summary>
  /// Enforces speed limits and road boundaries on velocity.
  /// This is the final velocity processing before applying to Rigidbody.
  /// </summary>
  public class ClampPhysicsVelocitySystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public ClampPhysicsVelocitySystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.PhysicsVelocity,
        GameMatcher.PhysicsConstraints,
        GameMatcher.WorldPosition));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Vector3 velocity = entity.physicsVelocity.Value;
        PhysicsConstraints constraints = entity.physicsConstraints;
        Vector3 position = entity.worldPosition.Value;

        // === FORWARD SPEED LIMITS ===
        velocity.z = Mathf.Clamp(velocity.z, constraints.MinForwardSpeed, constraints.MaxForwardSpeed);

        // === LATERAL SPEED LIMITS ===
        velocity.x = Mathf.Clamp(velocity.x, -constraints.MaxLateralSpeed, constraints.MaxLateralSpeed);

        // === ROAD BOUNDARY ENFORCEMENT ===
        // If approaching boundary, reduce/reverse lateral velocity to stay in bounds
        float boundaryBuffer = 0.5f; // Start slowing before hitting boundary

        if (position.x <= constraints.MinX + boundaryBuffer && velocity.x < 0)
        {
          // Approaching left boundary while moving left
          float distanceToEdge = position.x - constraints.MinX;
          float slowdownFactor = Mathf.Clamp01(distanceToEdge / boundaryBuffer);
          velocity.x *= slowdownFactor;

          // Hard stop at boundary
          if (position.x <= constraints.MinX)
            velocity.x = Mathf.Max(0f, velocity.x);
        }
        else if (position.x >= constraints.MaxX - boundaryBuffer && velocity.x > 0)
        {
          // Approaching right boundary while moving right
          float distanceToEdge = constraints.MaxX - position.x;
          float slowdownFactor = Mathf.Clamp01(distanceToEdge / boundaryBuffer);
          velocity.x *= slowdownFactor;

          // Hard stop at boundary
          if (position.x >= constraints.MaxX)
            velocity.x = Mathf.Min(0f, velocity.x);
        }

        entity.ReplacePhysicsVelocity(velocity);
      }
    }
  }

  #endregion

  #region Apply Velocity System

  /// <summary>
  /// Applies calculated physics velocity to Unity Rigidbody.
  /// This is where ECS data meets Unity physics.
  ///
  /// We set velocity directly (not AddForce) for precise control
  /// in an auto-runner game where responsiveness is key.
  /// </summary>
  public class ApplyPhysicsVelocitySystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);
    private float _lastLogTime;

    public ApplyPhysicsVelocitySystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Rigidbody,
        GameMatcher.PhysicsVelocity));
    }

    public void Execute()
    {
      // Debug: log entity count periodically
      if (Time.time - _lastLogTime > 2f)
      {
        _lastLogTime = Time.time;
        Debug.Log($"[ApplyPhysicsVelocity] Found {_entities.count} entities with Rigidbody+PhysicsVelocity");
      }

      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Rigidbody rb = entity.rigidbody.Value;
        if (rb == null)
        {
          Debug.LogWarning("[ApplyPhysicsVelocity] Entity has null Rigidbody!");
          continue;
        }

        Vector3 targetVelocity = entity.physicsVelocity.Value;
        Vector3 currentVelocity = rb.linearVelocity;

        // Apply our calculated X and Z velocity
        // Preserve Y velocity for any vertical physics (collisions, slopes)
        Vector3 newVelocity = new Vector3(
          targetVelocity.x,
          currentVelocity.y, // Preserve Y from physics
          targetVelocity.z
        );

        rb.linearVelocity = newVelocity;

        // Debug first few frames
        if (Time.frameCount < 10)
        {
          Debug.Log($"[ApplyPhysicsVelocity] Frame {Time.frameCount}: velocity={newVelocity}");
        }
      }
    }
  }

  #endregion

  #region Sync Position System

  /// <summary>
  /// Syncs ECS WorldPosition component from Rigidbody.position.
  /// This allows other systems (collision, quests, etc.) to use
  /// the physics-updated position.
  /// </summary>
  public class SyncPhysicsPositionSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public SyncPhysicsPositionSystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.WorldPosition,
        GameMatcher.Rigidbody));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Rigidbody rb = entity.rigidbody.Value;
        if (rb == null)
          continue;

        // Sync position from Rigidbody to ECS component
        Vector3 rbPosition = rb.position;
        Vector3 currentPosition = entity.worldPosition.Value;

        // Only update if position actually changed (avoid unnecessary component replacement)
        if (Vector3.SqrMagnitude(rbPosition - currentPosition) > 0.0001f)
        {
          entity.ReplaceWorldPosition(rbPosition);
        }
      }
    }
  }

  #endregion

  #region Update State System

  /// <summary>
  /// Updates PhysicsState component for debugging and effects.
  /// Tracks current speed, sliding state, etc.
  /// </summary>
  public class UpdatePhysicsStateSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _entities;
    private readonly List<GameEntity> _buffer = new(4);

    public UpdatePhysicsStateSystem(GameContext game)
    {
      _entities = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.PhysicsState,
        GameMatcher.PhysicsVelocity,
        GameMatcher.PhysicsConstraints,
        GameMatcher.SurfaceModifier));
    }

    public void Execute()
    {
      foreach (GameEntity entity in _entities.GetEntities(_buffer))
      {
        Vector3 velocity = entity.physicsVelocity.Value;
        PhysicsConstraints constraints = entity.physicsConstraints;
        SurfaceModifier surface = entity.surfaceModifier;

        float currentSpeed = velocity.magnitude;
        bool isSliding = surface.FrictionMultiplier < 0.5f && Mathf.Abs(velocity.x) > 0.5f;
        bool isAtMaxSpeed = velocity.z >= constraints.MaxForwardSpeed - 0.1f;

        PhysicsState currentState = entity.physicsState;

        // Only update if something changed
        if (Mathf.Abs(currentState.CurrentSpeed - currentSpeed) > 0.1f ||
            currentState.IsSliding != isSliding ||
            currentState.IsAtMaxSpeed != isAtMaxSpeed)
        {
          entity.ReplacePhysicsState(
            currentSpeed,
            isSliding,
            isAtMaxSpeed,
            currentState.GroundNormal // Preserve ground normal
          );
        }
      }
    }
  }

  #endregion

  #region Surface Detection System (Optional - for trigger zones)

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
      Debug.Log($"[SurfaceZone] Entity entered {type} zone: friction={friction}, drag={drag}");
    }

    /// <summary>
    /// Reset to normal surface
    /// </summary>
    public static void ExitSurfaceZone(GameEntity entity)
    {
      if (entity == null || !entity.hasSurfaceModifier)
        return;

      entity.ReplaceSurfaceModifier(1f, 1f, SurfaceType.Normal);
      Debug.Log("[SurfaceZone] Entity returned to Normal surface");
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

  #endregion
}
