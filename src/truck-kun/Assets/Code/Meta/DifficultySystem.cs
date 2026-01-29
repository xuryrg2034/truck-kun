using System;
using Code.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Meta.Difficulty
{
  #region Config

  [Serializable]
  public class DifficultyConfig
  {
    public int DayNumber;
    public int QuestCount;
    public float PedestrianSpawnInterval;
    public float ForwardSpeedMultiplier;
    public float CrossingChance;
    public int MaxPedestrians;
    public bool IsMilestoneDay;
    public string MilestoneBonus;

    public override string ToString()
    {
      return $"Day {DayNumber}: Quests={QuestCount}, SpawnInterval={PedestrianSpawnInterval:F2}, " +
             $"SpeedMult={ForwardSpeedMultiplier:F2}, Crossing={CrossingChance:F2}";
    }
  }

  [Serializable]
  public class DifficultyScalingSettings
  {
    [Header("Quest Scaling")]
    public int BaseQuestCount = 1;
    public int QuestIncreaseEveryNDays = 5;
    public int MaxQuestCount = 5;

    [Header("Spawn Rate Scaling")]
    public float BaseSpawnInterval = 1.5f;
    public float MinSpawnInterval = 0.5f;
    public float SpawnIntervalDecreasePerDay = 0.05f;

    [Header("Speed Scaling")]
    public float BaseSpeedMultiplier = 1f;
    public float MaxSpeedMultiplier = 2f;
    public float SpeedIncreasePerDay = 0.02f;

    [Header("Crossing Difficulty")]
    public float BaseCrossingChance = 0.3f;
    public float MaxCrossingChance = 0.7f;
    public float CrossingIncreasePerDay = 0.02f;

    [Header("Pedestrian Count")]
    public int BaseMaxPedestrians = 12;
    public int MaxMaxPedestrians = 25;
    public int PedestrianIncreaseEveryNDays = 3;

    [Header("Milestones")]
    public int MilestoneEveryNDays = 5;
    public int MilestoneBonusMoney = 500;
  }

  #endregion

  #region Service

  public interface IDifficultyService
  {
    DifficultyConfig GetDifficultyForDay(int dayNumber);
    DifficultyConfig CurrentDifficulty { get; }
    void ApplyDifficulty(int dayNumber);
    bool IsMilestoneDay(int dayNumber);
    int GetMilestoneBonus(int dayNumber);
  }

  public class DifficultyScalingService : IDifficultyService
  {
    private readonly DifficultyScalingSettings _settings;
    private DifficultyConfig _currentDifficulty;

    public DifficultyConfig CurrentDifficulty => _currentDifficulty;

    public DifficultyScalingService(DifficultyScalingSettings settings = null)
    {
      _settings = settings ?? new DifficultyScalingSettings();
    }

    public DifficultyConfig GetDifficultyForDay(int dayNumber)
    {
      dayNumber = Mathf.Max(1, dayNumber);

      DifficultyConfig config = new DifficultyConfig
      {
        DayNumber = dayNumber
      };

      // Quest count: increases every N days
      // Day 1-4: 1 quest, Day 5-9: 2 quests, Day 10-14: 3 quests, etc.
      int questIncrements = (dayNumber - 1) / _settings.QuestIncreaseEveryNDays;
      config.QuestCount = Mathf.Clamp(
        _settings.BaseQuestCount + questIncrements,
        1,
        _settings.MaxQuestCount
      );

      // Spawn interval: decreases linearly
      float intervalDecrease = (dayNumber - 1) * _settings.SpawnIntervalDecreasePerDay;
      config.PedestrianSpawnInterval = Mathf.Max(
        _settings.MinSpawnInterval,
        _settings.BaseSpawnInterval - intervalDecrease
      );

      // Speed multiplier: increases linearly
      float speedIncrease = (dayNumber - 1) * _settings.SpeedIncreasePerDay;
      config.ForwardSpeedMultiplier = Mathf.Min(
        _settings.MaxSpeedMultiplier,
        _settings.BaseSpeedMultiplier + speedIncrease
      );

      // Crossing chance: increases linearly
      float crossingIncrease = (dayNumber - 1) * _settings.CrossingIncreasePerDay;
      config.CrossingChance = Mathf.Min(
        _settings.MaxCrossingChance,
        _settings.BaseCrossingChance + crossingIncrease
      );

      // Max pedestrians: increases every N days
      int pedestrianIncrements = (dayNumber - 1) / _settings.PedestrianIncreaseEveryNDays;
      config.MaxPedestrians = Mathf.Min(
        _settings.MaxMaxPedestrians,
        _settings.BaseMaxPedestrians + pedestrianIncrements * 2
      );

      // Milestone check
      config.IsMilestoneDay = IsMilestoneDay(dayNumber);
      if (config.IsMilestoneDay)
      {
        config.MilestoneBonus = GetMilestoneDescription(dayNumber);
      }

      return config;
    }

    public void ApplyDifficulty(int dayNumber)
    {
      _currentDifficulty = GetDifficultyForDay(dayNumber);
      Debug.Log($"[Difficulty] Applied: {_currentDifficulty}");
    }

    public bool IsMilestoneDay(int dayNumber)
    {
      return dayNumber > 1 && dayNumber % _settings.MilestoneEveryNDays == 0;
    }

    public int GetMilestoneBonus(int dayNumber)
    {
      if (!IsMilestoneDay(dayNumber))
        return 0;

      // Bonus increases with milestone number
      int milestoneNumber = dayNumber / _settings.MilestoneEveryNDays;
      return _settings.MilestoneBonusMoney * milestoneNumber;
    }

    private string GetMilestoneDescription(int dayNumber)
    {
      int milestoneNumber = dayNumber / _settings.MilestoneEveryNDays;
      int bonus = GetMilestoneBonus(dayNumber);

      return milestoneNumber switch
      {
        1 => $"Week Survivor! +{bonus}",
        2 => $"Veteran Driver! +{bonus}",
        3 => $"Road Master! +{bonus}",
        4 => $"Legend! +{bonus}",
        _ => $"Milestone {milestoneNumber}! +{bonus}"
      };
    }
  }

  #endregion

  #region Day Counter UI

  public class DayCounterUI : MonoBehaviour
  {
    private Canvas _canvas;
    private Text _dayText;
    private Text _milestoneText;
    private int _currentDay;
    private bool _showingMilestone;
    private float _milestoneTimer;

    private static readonly Color DayTextColor = new Color(1f, 1f, 1f, 0.9f);
    private static readonly Color MilestoneColor = new Color(1f, 0.85f, 0.2f);

    public void Initialize(int dayNumber, bool isMilestone = false, string milestoneText = null)
    {
      _currentDay = dayNumber;

      if (_canvas == null)
        CreateUI();

      UpdateDayText();

      if (isMilestone && !string.IsNullOrEmpty(milestoneText))
      {
        ShowMilestone(milestoneText);
      }
    }

    private void CreateUI()
    {
      // Create canvas
      GameObject canvasObj = new GameObject("DayCounterCanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 90;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Day counter (top-right corner)
      GameObject dayPanel = new GameObject("DayPanel");
      dayPanel.transform.SetParent(canvasObj.transform, false);

      Image panelBg = dayPanel.AddComponent<Image>();
      panelBg.color = new Color(0f, 0f, 0f, 0.6f);

      RectTransform panelRect = dayPanel.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(1f, 1f);
      panelRect.anchorMax = new Vector2(1f, 1f);
      panelRect.pivot = new Vector2(1f, 1f);
      panelRect.anchoredPosition = new Vector2(-20f, -20f);
      panelRect.sizeDelta = new Vector2(120f, 50f);

      // Day text
      GameObject dayTextObj = new GameObject("DayText");
      dayTextObj.transform.SetParent(dayPanel.transform, false);

      _dayText = dayTextObj.AddComponent<Text>();
      _dayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _dayText.fontSize = 28;
      _dayText.fontStyle = FontStyle.Bold;
      _dayText.color = DayTextColor;
      _dayText.alignment = TextAnchor.MiddleCenter;

      RectTransform dayTextRect = dayTextObj.GetComponent<RectTransform>();
      dayTextRect.anchorMin = Vector2.zero;
      dayTextRect.anchorMax = Vector2.one;
      dayTextRect.offsetMin = Vector2.zero;
      dayTextRect.offsetMax = Vector2.zero;

      // Milestone text (center screen, hidden by default)
      GameObject milestoneObj = new GameObject("MilestoneText");
      milestoneObj.transform.SetParent(canvasObj.transform, false);

      _milestoneText = milestoneObj.AddComponent<Text>();
      _milestoneText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _milestoneText.fontSize = 42;
      _milestoneText.fontStyle = FontStyle.Bold;
      _milestoneText.color = MilestoneColor;
      _milestoneText.alignment = TextAnchor.MiddleCenter;

      Outline outline = milestoneObj.AddComponent<Outline>();
      outline.effectColor = Color.black;
      outline.effectDistance = new Vector2(3, -3);

      RectTransform milestoneRect = milestoneObj.GetComponent<RectTransform>();
      milestoneRect.anchorMin = new Vector2(0.5f, 0.7f);
      milestoneRect.anchorMax = new Vector2(0.5f, 0.7f);
      milestoneRect.sizeDelta = new Vector2(600f, 100f);

      milestoneObj.SetActive(false);
    }

    private void UpdateDayText()
    {
      if (_dayText != null)
        _dayText.text = $"DAY {_currentDay}";
    }

    private void ShowMilestone(string text)
    {
      if (_milestoneText == null)
        return;

      _milestoneText.text = text;
      _milestoneText.gameObject.SetActive(true);
      _showingMilestone = true;
      _milestoneTimer = 3f; // Show for 3 seconds
    }

    private void Update()
    {
      if (_showingMilestone)
      {
        _milestoneTimer -= Time.deltaTime;

        // Fade out
        if (_milestoneTimer < 1f)
        {
          Color c = _milestoneText.color;
          c.a = _milestoneTimer;
          _milestoneText.color = c;
        }

        if (_milestoneTimer <= 0f)
        {
          _milestoneText.gameObject.SetActive(false);
          _showingMilestone = false;

          // Reset alpha for next time
          Color c = _milestoneText.color;
          c.a = 1f;
          _milestoneText.color = c;
        }
      }
    }

    public void UpdateDay(int newDay)
    {
      _currentDay = newDay;
      UpdateDayText();
    }
  }

  #endregion

  #region Difficulty Settings (for Balance integration)

  [Serializable]
  public class DifficultyBalance
  {
    [Header("Quest Progression")]
    public int BaseQuestCount = 1;
    public int QuestIncreaseEveryNDays = 5;
    public int MaxQuestCount = 5;

    [Header("Spawn Rate Progression")]
    public float BaseSpawnInterval = 1.5f;
    public float MinSpawnInterval = 0.5f;
    public float SpawnIntervalDecreasePerDay = 0.05f;

    [Header("Speed Progression")]
    public float BaseSpeedMultiplier = 1f;
    public float MaxSpeedMultiplier = 2f;
    public float SpeedIncreasePerDay = 0.02f;

    [Header("Crossing Progression")]
    public float BaseCrossingChance = 0.3f;
    public float MaxCrossingChance = 0.7f;
    public float CrossingIncreasePerDay = 0.02f;

    [Header("Pedestrian Count")]
    public int BaseMaxPedestrians = 12;
    public int MaxMaxPedestrians = 25;
    public int PedestrianIncreaseEveryNDays = 3;

    [Header("Milestones")]
    public int MilestoneEveryNDays = 5;
    public int MilestoneBonusMoney = 500;

    public DifficultyScalingSettings ToSettings()
    {
      return new DifficultyScalingSettings
      {
        BaseQuestCount = BaseQuestCount,
        QuestIncreaseEveryNDays = QuestIncreaseEveryNDays,
        MaxQuestCount = MaxQuestCount,
        BaseSpawnInterval = BaseSpawnInterval,
        MinSpawnInterval = MinSpawnInterval,
        SpawnIntervalDecreasePerDay = SpawnIntervalDecreasePerDay,
        BaseSpeedMultiplier = BaseSpeedMultiplier,
        MaxSpeedMultiplier = MaxSpeedMultiplier,
        SpeedIncreasePerDay = SpeedIncreasePerDay,
        BaseCrossingChance = BaseCrossingChance,
        MaxCrossingChance = MaxCrossingChance,
        CrossingIncreasePerDay = CrossingIncreasePerDay,
        BaseMaxPedestrians = BaseMaxPedestrians,
        MaxMaxPedestrians = MaxMaxPedestrians,
        PedestrianIncreaseEveryNDays = PedestrianIncreaseEveryNDays,
        MilestoneEveryNDays = MilestoneEveryNDays,
        MilestoneBonusMoney = MilestoneBonusMoney
      };
    }
  }

  #endregion
}
