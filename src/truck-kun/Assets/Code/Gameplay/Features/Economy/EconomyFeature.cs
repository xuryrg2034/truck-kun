using Code.Gameplay.Features.Economy.Systems;
using Code.Infrastructure.Systems;

namespace Code.Gameplay.Features.Economy
{
  public sealed class EconomyFeature : Feature
  {
    public EconomyFeature(ISystemFactory systems)
    {
      Add(systems.Create<EconomyInitializationSystem>());
      Add(systems.Create<ProcessViolationPenaltiesSystem>());
      Add(systems.Create<ProcessQuestRewardsSystem>());
    }
  }
}
