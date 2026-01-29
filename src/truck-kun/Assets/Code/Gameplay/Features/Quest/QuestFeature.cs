using Code.Gameplay.Features.Quest.Systems;
using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Quest
{
  public sealed class QuestFeature : Feature
  {
    public QuestFeature(ISystemFactory systems)
    {
      Add(systems.Create<QuestInitializationSystem>());
      Add(systems.Create<QuestProgressTrackingSystem>());
      Add(systems.Create<QuestViolationSystem>());
      Add(systems.Create<CleanupViolationEventsSystem>());
    }
  }
}
