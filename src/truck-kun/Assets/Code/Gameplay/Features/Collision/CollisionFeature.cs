using Code.Gameplay.Features.Collision.Systems;
using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Collision
{
  public sealed class CollisionFeature : Feature
  {
    public CollisionFeature(ISystemFactory systems)
    {
      Add(systems.Create<FallbackCollisionDetectionSystem>());
      Add(systems.Create<DestroyHitPedestriansSystem>());
      Add(systems.Create<CleanupHitEventsSystem>());
    }
  }
}
