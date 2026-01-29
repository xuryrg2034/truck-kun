using Code.Gameplay.Features.Pedestrian.Systems;
using Code.Gameplay.Features.Physics.Systems;
using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Physics
{
  /// <summary>
  /// Physics Feature - handles Rigidbody-based movement
  ///
  /// Execution order:
  /// 1. PedestrianForceMovementSystem - force-based pedestrian movement
  /// 2. ReadInputForPhysicsSystem - reads lateral input
  /// 3. CalculatePhysicsVelocitySystem - calculates target velocity
  /// 4. ApplySurfaceModifiersSystem - applies surface effects (friction, drag)
  /// 5. ClampPhysicsVelocitySystem - enforces speed limits and road boundaries
  /// 6. ApplyPhysicsVelocitySystem - applies velocity to Rigidbody
  /// 7. SyncPhysicsPositionSystem - syncs WorldPosition from Rigidbody
  /// 8. UpdatePhysicsStateSystem - updates debug state
  ///
  /// NOTE: This feature should be executed in FixedUpdate for physics stability.
  /// Call _physicsFeature.Execute() from MonoBehaviour.FixedUpdate()
  /// </summary>
  public sealed class PhysicsFeature : Feature
  {
    public PhysicsFeature(ISystemFactory systems)
    {
      // Pedestrian force-based movement (crossing pedestrians)
      Add(systems.Create<PedestrianForceMovementSystem>());

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

      // State update
      Add(systems.Create<UpdatePhysicsStateSystem>());
    }
  }
}
