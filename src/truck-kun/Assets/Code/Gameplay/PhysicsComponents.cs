using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Physics
{
  #region Enums

  /// <summary>
  /// Type of surface affecting vehicle physics
  /// </summary>
  public enum SurfaceType
  {
    Normal,   // Asphalt road - default physics
    Oil,      // Oil spill - reduced friction, sliding
    Grass,    // Grass/dirt - increased drag, slower
    Ice,      // Ice patch - very low friction
    Puddle    // Water puddle - splash effect, slight drag
  }

  #endregion

  #region Core Physics Components

  /// <summary>
  /// Reference to Unity Rigidbody for physics-based movement
  /// </summary>
  [Game]
  public class RigidbodyComponent : IComponent
  {
    public Rigidbody Value;
  }

  /// <summary>
  /// Target velocity for physics body (applied via Rigidbody.velocity)
  /// </summary>
  [Game]
  public class PhysicsVelocity : IComponent
  {
    public Vector3 Value;
  }

  /// <summary>
  /// Flag marking entity as physics-controlled (uses Rigidbody instead of kinematic)
  /// </summary>
  [Game]
  public class PhysicsBody : IComponent { }

  #endregion

  #region Acceleration Components

  /// <summary>
  /// Acceleration values for gradual speed changes
  /// </summary>
  [Game]
  public class Acceleration : IComponent
  {
    /// <summary>Acceleration rate when speeding up (m/s²)</summary>
    public float ForwardAcceleration;

    /// <summary>Lateral acceleration for steering (m/s²)</summary>
    public float LateralAcceleration;

    /// <summary>Deceleration rate when slowing down (m/s²)</summary>
    public float Deceleration;
  }

  #endregion

  #region Drag Components

  /// <summary>
  /// Drag/resistance values affecting movement
  /// </summary>
  [Game]
  public class PhysicsDrag : IComponent
  {
    /// <summary>Base drag coefficient (air resistance)</summary>
    public float BaseDrag;

    /// <summary>Current effective drag (modified by surface)</summary>
    public float CurrentDrag;
  }

  #endregion

  #region Surface Components

  /// <summary>
  /// Surface modifiers affecting physics behavior
  /// </summary>
  [Game]
  public class SurfaceModifier : IComponent
  {
    /// <summary>Friction multiplier: 1.0 = normal, 0.3 = ice, 1.5 = sticky</summary>
    public float FrictionMultiplier;

    /// <summary>Drag multiplier: affects braking and deceleration</summary>
    public float DragMultiplier;

    /// <summary>Type of surface currently under the vehicle</summary>
    public SurfaceType SurfaceType;
  }

  /// <summary>
  /// Trigger zone that applies surface effects
  /// </summary>
  [Game]
  public class SurfaceZone : IComponent
  {
    public SurfaceType Type;
    public float FrictionMultiplier;
    public float DragMultiplier;
  }

  #endregion

  #region Constraint Components

  /// <summary>
  /// Speed constraints for physics body
  /// </summary>
  [Game]
  public class PhysicsConstraints : IComponent
  {
    /// <summary>Minimum forward speed (auto-runner always moves forward)</summary>
    public float MinForwardSpeed;

    /// <summary>Maximum forward speed (with upgrades)</summary>
    public float MaxForwardSpeed;

    /// <summary>Maximum lateral (sideways) speed</summary>
    public float MaxLateralSpeed;

    /// <summary>Road boundaries - minimum X position</summary>
    public float MinX;

    /// <summary>Road boundaries - maximum X position</summary>
    public float MaxX;
  }

  #endregion

  #region Additional Physics Components

  /// <summary>
  /// Current physics state for debugging and effects
  /// </summary>
  [Game]
  public class PhysicsState : IComponent
  {
    /// <summary>Current actual speed (magnitude of velocity)</summary>
    public float CurrentSpeed;

    /// <summary>Is vehicle currently sliding (low friction)?</summary>
    public bool IsSliding;

    /// <summary>Is vehicle at max speed?</summary>
    public bool IsAtMaxSpeed;

    /// <summary>Ground contact normal for slope handling</summary>
    public Vector3 GroundNormal;
  }

  /// <summary>
  /// Impact data for collision response
  /// </summary>
  [Game]
  public class PhysicsImpact : IComponent
  {
    /// <summary>Direction of impact</summary>
    public Vector3 Direction;

    /// <summary>Force magnitude of impact</summary>
    public float Force;

    /// <summary>Time when impact occurred</summary>
    public float Timestamp;
  }

  #endregion

  #region Settings

  /// <summary>
  /// Default physics settings (can be used in GameBalance)
  /// </summary>
  [System.Serializable]
  public class PhysicsSettings
  {
    [Header("Speed Limits")]
    public float MinForwardSpeed = 3f;
    public float MaxForwardSpeed = 15f;
    public float MaxLateralSpeed = 8f;

    [Header("Acceleration")]
    public float ForwardAcceleration = 5f;
    public float LateralAcceleration = 15f;
    public float Deceleration = 10f;

    [Header("Drag")]
    public float BaseDrag = 0.5f;

    [Header("Surface Defaults")]
    public float NormalFriction = 1f;
    public float OilFriction = 0.3f;
    public float GrassFriction = 0.8f;
    public float IceFriction = 0.15f;

    public float NormalDrag = 1f;
    public float OilDrag = 0.5f;
    public float GrassDrag = 1.5f;
    public float IceDrag = 0.3f;

    /// <summary>
    /// Get surface modifiers for given surface type
    /// </summary>
    public (float friction, float drag) GetSurfaceModifiers(SurfaceType type)
    {
      return type switch
      {
        SurfaceType.Normal => (NormalFriction, NormalDrag),
        SurfaceType.Oil => (OilFriction, OilDrag),
        SurfaceType.Grass => (GrassFriction, GrassDrag),
        SurfaceType.Ice => (IceFriction, IceDrag),
        SurfaceType.Puddle => (0.9f, 1.2f),
        _ => (NormalFriction, NormalDrag)
      };
    }
  }

  #endregion

  #region Extensions

  public static class PhysicsComponentExtensions
  {
    /// <summary>
    /// Check if entity has full physics setup
    /// </summary>
    public static bool HasPhysicsSetup(this GameEntity entity)
    {
      return entity.hasRigidbody && entity.isPhysicsBody;
    }

    /// <summary>
    /// Get current speed from velocity
    /// </summary>
    public static float GetSpeed(this GameEntity entity)
    {
      if (entity.hasPhysicsVelocity)
        return entity.physicsVelocity.Value.magnitude;

      if (entity.hasRigidbody && entity.rigidbody.Value != null)
        return entity.rigidbody.Value.linearVelocity.magnitude;

      return 0f;
    }

    /// <summary>
    /// Get forward speed (Z component)
    /// </summary>
    public static float GetForwardSpeed(this GameEntity entity)
    {
      if (entity.hasPhysicsVelocity)
        return entity.physicsVelocity.Value.z;

      if (entity.hasRigidbody && entity.rigidbody.Value != null)
        return entity.rigidbody.Value.linearVelocity.z;

      return 0f;
    }

    /// <summary>
    /// Get lateral speed (X component)
    /// </summary>
    public static float GetLateralSpeed(this GameEntity entity)
    {
      if (entity.hasPhysicsVelocity)
        return entity.physicsVelocity.Value.x;

      if (entity.hasRigidbody && entity.rigidbody.Value != null)
        return entity.rigidbody.Value.linearVelocity.x;

      return 0f;
    }
  }

  #endregion
}
