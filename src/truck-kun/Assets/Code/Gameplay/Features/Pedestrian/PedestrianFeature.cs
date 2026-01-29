using Code.Art.Animation;
using Code.Gameplay.Features.Pedestrian.Systems;
using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Pedestrian
{
  /// <summary>
  /// Pedestrian Feature - handles NPC spawning, despawning, and animation.
  ///
  /// Note: Crossing movement is handled by PedestrianForceMovementSystem
  /// in PhysicsFeature (force-based physics in FixedUpdate).
  /// </summary>
  public sealed class PedestrianFeature : Feature
  {
    public PedestrianFeature(ISystemFactory systems)
    {
      // Spawning
      Add(systems.Create<PedestrianSpawnSystem>());

      // Despawning (cleanup behind hero)
      Add(systems.Create<PedestrianDespawnSystem>());

      // Animation systems
      Add(systems.Create<NPCAnimationSystem>());
      Add(systems.Create<DisableAnimationOnHitSystem>());
    }
  }
}
