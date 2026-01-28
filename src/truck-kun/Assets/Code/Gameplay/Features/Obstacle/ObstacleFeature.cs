using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Obstacle
{
  /// <summary>
  /// Feature for road obstacles (ramps, barriers, speed bumps, holes).
  /// Obstacles are placed manually on the level - no spawn system needed.
  /// Physics interactions handled by ObstacleBehaviour MonoBehaviour.
  /// </summary>
  public sealed class ObstacleFeature : Feature
  {
    public ObstacleFeature(ISystemFactory systems)
    {
      // Currently obstacles are fully physics-based via MonoBehaviour.
      // Add systems here if ECS processing is needed in the future.
    }
  }
}
