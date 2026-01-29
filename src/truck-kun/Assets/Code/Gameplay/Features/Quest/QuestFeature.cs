using System;
using System.Collections.Generic;
using Code.Common.Services;
using Code.Configs.Spawning;
using Code.Gameplay.Features.Pedestrian;
using Code.Infrastructure.Systems;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Quest
{
  #region Components

  [Meta]
  public class DailyQuest : IComponent
  {
    public PedestrianKind TargetType;
    public int RequiredCount;
    public int Reward;
  }

  [Meta]
  public class QuestProgress : IComponent
  {
    public int CurrentCount;
  }

  [Meta]
  public class QuestCompleted : IComponent { }

  [Meta]
  public class ActiveQuest : IComponent { }

  [Game]
  public class ViolationEvent : IComponent
  {
    public PedestrianKind ViolatedType;
    public int PedestrianId;
  }

  #endregion

  #region Data Structures

  public readonly struct QuestProgressInfo
  {
    public readonly int QuestId;
    public readonly PedestrianKind TargetType;
    public readonly int CurrentCount;
    public readonly int RequiredCount;
    public readonly bool IsCompleted;

    public QuestProgressInfo(int questId, PedestrianKind targetType, int currentCount, int requiredCount, bool isCompleted)
    {
      QuestId = questId;
      TargetType = targetType;
      CurrentCount = currentCount;
      RequiredCount = requiredCount;
      IsCompleted = isCompleted;
    }
  }

  #endregion

  #region Service

  public interface IQuestService
  {
    void GenerateDailyQuests(int count);
    bool ValidateHit(PedestrianKind type);
    bool IsViolation(PedestrianKind type);
    void RegisterHit(PedestrianKind type);
    IReadOnlyList<QuestProgressInfo> GetQuestProgress();
    bool HasActiveQuests { get; }
    bool AllQuestsCompleted { get; }
  }

  public class QuestService : IQuestService
  {
    private readonly MetaContext _meta;
    private readonly QuestPoolConfig _config;
    private readonly IIdentifierService _identifiers;
    private readonly IGroup<MetaEntity> _activeQuests;
    private readonly List<MetaEntity> _questBuffer = new(8);

    public QuestService(
      MetaContext meta,
      IIdentifierService identifiers,
      QuestPoolConfig config)
    {
      _meta = meta;
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "QuestPoolConfig is required! Assign it in LevelConfig.");
      _identifiers = identifiers;
      _activeQuests = meta.GetGroup(MetaMatcher.AllOf(MetaMatcher.DailyQuest, MetaMatcher.ActiveQuest));
    }

    public bool HasActiveQuests => _activeQuests.count > 0;

    public bool AllQuestsCompleted
    {
      get
      {
        if (_activeQuests.count == 0)
          return false;

        foreach (MetaEntity quest in _activeQuests.GetEntities(_questBuffer))
        {
          if (!quest.isQuestCompleted)
            return false;
        }

        return true;
      }
    }

    public void GenerateDailyQuests(int count)
    {
      ClearExistingQuests();

      List<QuestDefinition> generatedQuests = _config.GenerateQuestsForDay();
      foreach (var def in generatedQuests)
      {
        int targetCount = def.GetRandomTarget();
        int reward = def.CalculateReward(targetCount, targetCount);

        MetaEntity quest = _meta.CreateEntity();
        quest.AddId(_identifiers.Next());
        quest.AddDailyQuest(def.TargetKind, targetCount, reward);
        quest.AddQuestProgress(0);
        quest.isActiveQuest = true;
      }
    }

    public bool ValidateHit(PedestrianKind type)
    {
      foreach (MetaEntity quest in _activeQuests.GetEntities(_questBuffer))
      {
        if (quest.isQuestCompleted)
          continue;

        if (quest.dailyQuest.TargetType == type)
          return true;
      }

      return false;
    }

    public bool IsViolation(PedestrianKind type)
    {
      // Protected types (elderly) cause violation
      return type.IsProtectedType();
    }

    public void RegisterHit(PedestrianKind type)
    {
      foreach (MetaEntity quest in _activeQuests.GetEntities(_questBuffer))
      {
        if (quest.isQuestCompleted)
          continue;

        if (quest.dailyQuest.TargetType != type)
          continue;

        int newCount = quest.questProgress.CurrentCount + 1;
        quest.ReplaceQuestProgress(newCount);

        if (newCount >= quest.dailyQuest.RequiredCount)
          quest.isQuestCompleted = true;
      }
    }

    public IReadOnlyList<QuestProgressInfo> GetQuestProgress()
    {
      List<QuestProgressInfo> result = new(_activeQuests.count);

      foreach (MetaEntity quest in _activeQuests.GetEntities(_questBuffer))
      {
        result.Add(new QuestProgressInfo(
          quest.id.Value,
          quest.dailyQuest.TargetType,
          quest.questProgress.CurrentCount,
          quest.dailyQuest.RequiredCount,
          quest.isQuestCompleted));
      }

      return result;
    }

    private void ClearExistingQuests()
    {
      foreach (MetaEntity quest in _activeQuests.GetEntities(_questBuffer))
        quest.Destroy();
    }
  }

  #endregion

  #region Feature

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

  #endregion

  #region Systems

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

  #endregion
}
