using Code.Gameplay.Features.Feedback.Systems;
using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Feedback
{
  public sealed class FeedbackFeature : Feature
  {
    public FeedbackFeature(ISystemFactory systems)
    {
      Add(systems.Create<HitFeedbackSystem>());
      Add(systems.Create<FloatingTextUpdateSystem>());
    }
  }
}
