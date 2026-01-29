using Code.Gameplay.Features.Surface.Systems;
using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Surface
{
  /// <summary>
  /// Surface Feature - handles surface hazards on the road.
  ///
  /// When EnableSpawning = true: Systems auto-spawn surfaces ahead of hero.
  /// When EnableSpawning = false: Surfaces are placed manually in scene using SurfaceTrigger.
  ///
  /// The actual surface effects are handled by:
  /// - SurfaceTrigger (MonoBehaviour) - detects hero entering/exiting
  /// - ApplySurfaceModifiersSystem (PhysicsFeature) - applies friction/drag effects
  /// </summary>
  public sealed class SurfaceFeature : Feature
  {
    public SurfaceFeature(ISystemFactory systems, SurfaceSpawnSettings settings)
    {
      if (settings.EnableSpawning)
      {
        Add(systems.Create<SurfaceSpawnSystem>());
      }
    }
  }
}
