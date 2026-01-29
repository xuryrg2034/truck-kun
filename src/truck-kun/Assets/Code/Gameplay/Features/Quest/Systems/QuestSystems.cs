using System;
using System.Collections.Generic;
using Code.Configs.Spawning;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest.Services;
using Entitas;

namespace Code.Gameplay.Features.Quest.Systems
{
  public class QuestInitializationSystem : IInitializeSystem
  {
    private readonly IQuestService _questService;
    private readonly QuestPoolConfig _config;

    public QuestInitializationSystem(IQuestService questService, QuestPoolConfig config)
    {
      _questService = questService;
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "QuestPoolConfig is required! Assign it in LevelConfig.");
    }

    public void Initialize()
    {
      int questCount = UnityEngine.Random.Range(_config.MinQuestsPerDay, _config.MaxQuestsPerDay + 1);
      _questService.GenerateDailyQuests(questCount);
    }
  }

  public class QuestProgressTrackingSystem : ReactiveSystem<GameEntity>
  {
    private readonly IQuestService _questService;

    public QuestProgressTrackingSystem(GameContext game, IQuestService questService) : base(game)
    {
      _questService = questService;
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
      return context.CreateCollector(GameMatcher.HitEvent.Added());
    }

    protected override bool Filter(GameEntity entity)
    {
      return entity.hasHitEvent;
    }

    protected override void Execute(List<GameEntity> entities)
    {
      foreach (GameEntity hitEvent in entities)
      {
        PedestrianKind type = hitEvent.hitEvent.PedestrianType;

        if (_questService.ValidateHit(type))
          _questService.RegisterHit(type);
      }
    }
  }

  public class QuestViolationSystem : ReactiveSystem<GameEntity>
  {
    private readonly GameContext _game;
    private readonly IQuestService _questService;

    public QuestViolationSystem(GameContext game, IQuestService questService) : base(game)
    {
      _game = game;
      _questService = questService;
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
      return context.CreateCollector(GameMatcher.HitEvent.Added());
    }

    protected override bool Filter(GameEntity entity)
    {
      return entity.hasHitEvent;
    }

    protected override void Execute(List<GameEntity> entities)
    {
      foreach (GameEntity hitEvent in entities)
      {
        PedestrianKind type = hitEvent.hitEvent.PedestrianType;
        int pedestrianId = hitEvent.hitEvent.PedestrianId;

        if (_questService.IsViolation(type))
        {
          GameEntity violation = _game.CreateEntity();
          violation.AddViolationEvent(type, pedestrianId);
        }
      }
    }
  }

  public class CleanupViolationEventsSystem : ICleanupSystem
  {
    private readonly IGroup<GameEntity> _violationEvents;
    private readonly List<GameEntity> _buffer = new(8);

    public CleanupViolationEventsSystem(GameContext game)
    {
      _violationEvents = game.GetGroup(GameMatcher.ViolationEvent);
    }

    public void Cleanup()
    {
      foreach (GameEntity entity in _violationEvents.GetEntities(_buffer))
        entity.Destroy();
    }
  }
}
