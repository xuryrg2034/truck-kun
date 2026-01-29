using Code.Gameplay.Features.Pedestrian;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Code.Gameplay.Features.Quest
{
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
}
