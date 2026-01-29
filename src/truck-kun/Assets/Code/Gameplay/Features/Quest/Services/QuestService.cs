using System;
using System.Collections.Generic;
using Code.Common.Services;
using Code.Configs.Spawning;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Pedestrian.Extensions;
using Entitas;

namespace Code.Gameplay.Features.Quest.Services
{
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
}
