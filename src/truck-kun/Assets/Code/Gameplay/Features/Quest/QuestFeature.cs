using System;
using System.Collections.Generic;
using Code.Common.Services;
using Code.Configs.Spawning;
using Code.Gameplay.Features.Pedestrian;
using Code.Infrastructure.Systems;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using Zenject;

// Note: QuestPoolConfig in Code.Configs.Spawning is the new config
// QuestConfig here is legacy and kept for backward compatibility

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

  [Serializable]
  public class QuestDefinition
  {
    public PedestrianKind TargetType;
    public int MinCount = 1;
    public int MaxCount = 5;
    public int BaseReward = 100;

    public string GetDescription()
    {
      return TargetType.GetDisplayNameRu();
    }
  }

  [Serializable]
  public class QuestSettings
  {
    public int MinQuestsPerDay = 1;
    public int MaxQuestsPerDay = 1;
  }

  #endregion

  #region ScriptableObject

  [CreateAssetMenu(fileName = "QuestConfig", menuName = "Truck-kun/Quest Config")]
  public class QuestConfig : ScriptableObject
  {
    [SerializeField] private List<QuestDefinition> _availableQuests = new()
    {
      new QuestDefinition { TargetType = PedestrianKind.StudentNerd, MinCount = 3, MaxCount = 5, BaseReward = 100 },
      new QuestDefinition { TargetType = PedestrianKind.Salaryman, MinCount = 2, MaxCount = 4, BaseReward = 120 },
      new QuestDefinition { TargetType = PedestrianKind.Teenager, MinCount = 3, MaxCount = 6, BaseReward = 80 }
    };
    [SerializeField] private int _rewardPerTarget = 10;

    public IReadOnlyList<QuestDefinition> AvailableQuests => _availableQuests;
    public int RewardPerTarget => _rewardPerTarget;

    public QuestDefinition GetRandomQuest()
    {
      if (_availableQuests.Count == 0)
        return null;

      return _availableQuests[UnityEngine.Random.Range(0, _availableQuests.Count)];
    }

    public QuestDefinition GetQuestForType(PedestrianKind kind)
    {
      foreach (QuestDefinition quest in _availableQuests)
      {
        if (quest.TargetType == kind)
          return quest;
      }
      return null;
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
    private readonly QuestConfig _legacyConfig;
    private readonly QuestPoolConfig _newConfig;
    private readonly IIdentifierService _identifiers;
    private readonly IGroup<MetaEntity> _activeQuests;
    private readonly List<MetaEntity> _questBuffer = new(8);

    public QuestService(
      MetaContext meta,
      IIdentifierService identifiers,
      [InjectOptional] QuestConfig legacyConfig = null,
      [InjectOptional] QuestPoolConfig newConfig = null)
    {
      _meta = meta;
      _legacyConfig = legacyConfig;
      _newConfig = newConfig;
      _identifiers = identifiers;
      _activeQuests = meta.GetGroup(MetaMatcher.AllOf(MetaMatcher.DailyQuest, MetaMatcher.ActiveQuest));

      if (_newConfig != null)
        Debug.Log("[QuestService] Using new QuestPoolConfig");
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

      // Available non-protected types for quests (default fallback)
      PedestrianKind[] availableTypes = {
        PedestrianKind.StudentNerd,
        PedestrianKind.Salaryman,
        PedestrianKind.Teenager
      };

      // Use new config if available
      if (_newConfig != null && _newConfig.AvailableQuests.Count > 0)
      {
        List<Configs.Spawning.QuestDefinition> generatedQuests = _newConfig.GenerateQuestsForDay();
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
        return;
      }

      // Legacy config or default behavior
      for (int i = 0; i < count; i++)
      {
        PedestrianKind targetType;
        int targetCount;
        int reward;

        if (_legacyConfig != null && _legacyConfig.AvailableQuests.Count > 0)
        {
          QuestDefinition definition = _legacyConfig.GetRandomQuest();
          targetType = definition.TargetType;
          targetCount = UnityEngine.Random.Range(definition.MinCount, definition.MaxCount + 1);
          reward = definition.BaseReward + targetCount * _legacyConfig.RewardPerTarget;
        }
        else
        {
          // Default: pick random non-protected type
          targetType = availableTypes[UnityEngine.Random.Range(0, availableTypes.Length)];
          targetCount = UnityEngine.Random.Range(3, 6);
          reward = 100 + targetCount * 10;
        }

        MetaEntity quest = _meta.CreateEntity();
        quest.AddId(_identifiers.Next());
        quest.AddDailyQuest(targetType, targetCount, reward);
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
    private readonly QuestSettings _settings;

    public QuestInitializationSystem(IQuestService questService, [InjectOptional] QuestSettings settings = null)
    {
      _questService = questService;
      _settings = settings ?? new QuestSettings();
    }

    public void Initialize()
    {
      int questCount = UnityEngine.Random.Range(_settings.MinQuestsPerDay, _settings.MaxQuestsPerDay + 1);
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
