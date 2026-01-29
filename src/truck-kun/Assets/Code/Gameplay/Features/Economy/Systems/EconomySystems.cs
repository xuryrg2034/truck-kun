using System;
using System.Collections.Generic;
using Code.Configs.Global;
using Code.Gameplay.Features.Economy.Services;
using Entitas;

namespace Code.Gameplay.Features.Economy.Systems
{
  public class EconomyInitializationSystem : IInitializeSystem
  {
    private readonly IMoneyService _moneyService;

    public EconomyInitializationSystem(IMoneyService moneyService)
    {
      _moneyService = moneyService;
    }

    public void Initialize()
    {
      _moneyService.Initialize();
    }
  }

  public class ProcessViolationPenaltiesSystem : ReactiveSystem<GameEntity>
  {
    private readonly IMoneyService _moneyService;
    private readonly EconomyConfig _config;

    public ProcessViolationPenaltiesSystem(
      GameContext game,
      IMoneyService moneyService,
      EconomyConfig config) : base(game)
    {
      _moneyService = moneyService;
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "EconomyConfig is required! Assign it in LevelConfig.");
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
      return context.CreateCollector(GameMatcher.ViolationEvent.Added());
    }

    protected override bool Filter(GameEntity entity)
    {
      return entity.hasViolationEvent;
    }

    protected override void Execute(List<GameEntity> entities)
    {
      foreach (GameEntity violation in entities)
      {
        _moneyService.ApplyPenalty(_config.ViolationPenalty);
      }
    }
  }

  public class ProcessQuestRewardsSystem : ReactiveSystem<MetaEntity>
  {
    private readonly IMoneyService _moneyService;

    public ProcessQuestRewardsSystem(MetaContext meta, IMoneyService moneyService) : base(meta)
    {
      _moneyService = moneyService;
    }

    protected override ICollector<MetaEntity> GetTrigger(IContext<MetaEntity> context)
    {
      return context.CreateCollector(MetaMatcher.QuestCompleted.Added());
    }

    protected override bool Filter(MetaEntity entity)
    {
      return entity.isQuestCompleted && entity.hasDailyQuest;
    }

    protected override void Execute(List<MetaEntity> entities)
    {
      foreach (MetaEntity quest in entities)
      {
        int reward = quest.dailyQuest.Reward;
        _moneyService.AddMoney(reward);
      }
    }
  }
}
