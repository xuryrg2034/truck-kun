using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Movement;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest;
using Code.Gameplay.Input;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;

namespace Code.Gameplay
{
  public sealed class BattleFeature : Feature
  {
    public BattleFeature(ISystemFactory systems)
    {
      Add(systems.Create<InputFeature>());
      Add(systems.Create<HeroFeature>());
      Add(systems.Create<PedestrianFeature>());
      Add(systems.Create<CollisionFeature>());
      Add(systems.Create<QuestFeature>());
      Add(systems.Create<EconomyFeature>());
      Add(systems.Create<BindViewFeature>());
      Add(systems.Create<MovementFeature>());
    }
  }
}
