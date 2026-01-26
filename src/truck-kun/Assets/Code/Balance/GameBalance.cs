using System;
using System.Collections.Generic;
using Code.Gameplay.Features.Pedestrian;
using Code.Meta.Difficulty;
using Code.Meta.Upgrades;
using UnityEngine;

namespace Code.Balance
{
  #region Balance Sections

  [Serializable]
  public class MovementBalance
  {
    [Header("Hero Movement")]
    public float ForwardSpeed = 5f;
    public float LateralSpeed = 5f;

    [Header("Road")]
    public float RoadWidth = 6f;

    [Header("Collision")]
    public float HitRadius = 1.2f;
  }

  [Serializable]
  public class PedestrianBalance
  {
    [Header("Spawning")]
    public float SpawnInterval = 1.5f;
    public float SpawnDistanceAhead = 18f;
    public float DespawnDistanceBehind = 12f;
    public int MaxActive = 12;
    public float LateralMargin = 0.5f;

    [Header("Crossing")]
    [Range(0f, 1f)] public float CrossingChance = 0.3f;
    public float CrossingSpeedMultiplier = 1f;
    public float SidewalkOffset = 1.5f;

    [Header("Type Weights (must sum to 1.0)")]
    public List<PedestrianTypeBalance> TypeBalances = new()
    {
      new PedestrianTypeBalance { Kind = PedestrianKind.StudentNerd, SpawnWeight = 0.25f, Speed = 2f, Scale = 0.85f, Color = new Color(0.95f, 0.95f, 1f) },      // Light blue
      new PedestrianTypeBalance { Kind = PedestrianKind.Salaryman, SpawnWeight = 0.30f, Speed = 1.8f, Scale = 1f, Color = new Color(0.4f, 0.4f, 0.45f) },        // Gray suit
      new PedestrianTypeBalance { Kind = PedestrianKind.Grandma, SpawnWeight = 0.15f, Speed = 0.8f, Scale = 0.8f, Color = new Color(1f, 0.7f, 0.8f) },           // Pink
      new PedestrianTypeBalance { Kind = PedestrianKind.OldMan, SpawnWeight = 0.10f, Speed = 0.9f, Scale = 0.9f, Color = new Color(0.6f, 0.45f, 0.3f) },         // Brown
      new PedestrianTypeBalance { Kind = PedestrianKind.Teenager, SpawnWeight = 0.20f, Speed = 2.2f, Scale = 0.95f, Color = new Color(0.2f, 0.8f, 0.4f) }        // Bright green
    };
  }

  [Serializable]
  public class PedestrianTypeBalance
  {
    public PedestrianKind Kind;
    [Range(0f, 1f)] public float SpawnWeight = 0.2f;
    public float Speed = 1.5f;
    public float Scale = 1f;
    public Color Color = Color.white;
    public float ForwardTilt = 0f;
  }

  [Serializable]
  public class EconomyBalance
  {
    [Header("Starting")]
    public int StartingMoney = 1000;

    [Header("Daily Costs")]
    public int DailyFoodCost = 100;
    public int MinimumRequiredMoney = 100;

    [Header("Penalties")]
    public int ViolationPenalty = 100;

    [Header("Rewards")]
    public int BaseQuestReward = 50;
    public int RewardPerTarget = 10;
  }

  [Serializable]
  public class DayBalance
  {
    [Header("Session")]
    public float DurationSeconds = 60f;

    [Header("Quests")]
    public int MinQuestsPerDay = 1;
    public int MaxQuestsPerDay = 1;

    [Header("Quest Targets")]
    public List<QuestTargetBalance> QuestTargets = new()
    {
      new QuestTargetBalance { Kind = PedestrianKind.StudentNerd, MinCount = 3, MaxCount = 5, BaseReward = 100 },
      new QuestTargetBalance { Kind = PedestrianKind.Salaryman, MinCount = 2, MaxCount = 4, BaseReward = 120 },
      new QuestTargetBalance { Kind = PedestrianKind.Teenager, MinCount = 3, MaxCount = 6, BaseReward = 80 }
    };
  }

  [Serializable]
  public class QuestTargetBalance
  {
    public PedestrianKind Kind;
    public int MinCount = 3;
    public int MaxCount = 5;
    public int BaseReward = 100;
  }

  [Serializable]
  public class UpgradeBalance
  {
    public List<UpgradeDefinitionBalance> Upgrades = new()
    {
      new UpgradeDefinitionBalance
      {
        Type = UpgradeType.SpeedBoost,
        Name = "Speed Boost",
        NameRu = "Форсаж",
        MaxLevel = 3,
        Costs = new[] { 500, 1000, 2000 },
        Bonuses = new[] { 0.1f, 0.2f, 0.3f }
      },
      new UpgradeDefinitionBalance
      {
        Type = UpgradeType.Maneuverability,
        Name = "Maneuverability",
        NameRu = "Маневренность",
        MaxLevel = 3,
        Costs = new[] { 500, 1000, 2000 },
        Bonuses = new[] { 0.15f, 0.30f, 0.45f }
      },
      new UpgradeDefinitionBalance
      {
        Type = UpgradeType.MoneyMultiplier,
        Name = "Greed",
        NameRu = "Жадность",
        MaxLevel = 3,
        Costs = new[] { 500, 1000, 2000 },
        Bonuses = new[] { 0.2f, 0.4f, 0.6f }
      }
    };
  }

  [Serializable]
  public class UpgradeDefinitionBalance
  {
    public UpgradeType Type;
    public string Name;
    public string NameRu;
    public int MaxLevel = 3;
    public int[] Costs = { 500, 1000, 2000 };
    public float[] Bonuses = { 0.1f, 0.2f, 0.3f };

    public int GetCost(int level)
    {
      if (level < 0 || level >= MaxLevel)
        return -1;
      return level < Costs.Length ? Costs[level] : Costs[^1];
    }

    public float GetBonus(int level)
    {
      if (level <= 0)
        return 0f;
      int index = Mathf.Clamp(level - 1, 0, Bonuses.Length - 1);
      return Bonuses[index];
    }
  }

  [Serializable]
  public class FeedbackBalance
  {
    [Header("Particles")]
    public int ParticleBurstCount = 15;
    public float ParticleLifetime = 1f;
    public float ParticleSpeed = 3f;
    public float ParticleGravity = 2f;
    public float ParticleSize = 0.15f;

    [Header("Floating Text")]
    public float FloatSpeed = 2f;
    public float FloatDuration = 1.2f;
    public int FontSize = 32;

    [Header("Audio")]
    [Range(0f, 1f)] public float SFXVolume = 0.7f;

    [Header("Colors")]
    public Color RewardColor = new Color(0.2f, 1f, 0.3f);
    public Color PenaltyColor = new Color(1f, 0.3f, 0.2f);
  }

  #endregion

  #region Main Balance ScriptableObject

  [CreateAssetMenu(fileName = "GameBalance", menuName = "Truck-kun/Game Balance")]
  public class GameBalance : ScriptableObject
  {
    [Header("Movement & Physics")]
    public MovementBalance Movement = new();

    [Header("Pedestrians")]
    public PedestrianBalance Pedestrians = new();

    [Header("Economy")]
    public EconomyBalance Economy = new();

    [Header("Day & Quests")]
    public DayBalance Day = new();

    [Header("Upgrades")]
    public UpgradeBalance Upgrades = new();

    [Header("Feedback Effects")]
    public FeedbackBalance Feedback = new();

    [Header("Difficulty Progression")]
    public DifficultyBalance Difficulty = new();

    public void ResetToDefaults()
    {
      Movement = new MovementBalance();
      Pedestrians = new PedestrianBalance();
      Economy = new EconomyBalance();
      Day = new DayBalance();
      Upgrades = new UpgradeBalance();
      Feedback = new FeedbackBalance();
      Difficulty = new DifficultyBalance();
    }

    #region Helper Methods

    public PedestrianTypeBalance GetPedestrianTypeBalance(PedestrianKind kind)
    {
      foreach (PedestrianTypeBalance balance in Pedestrians.TypeBalances)
      {
        if (balance.Kind == kind)
          return balance;
      }

      return new PedestrianTypeBalance { Kind = kind };
    }

    public QuestTargetBalance GetQuestTarget(PedestrianKind kind)
    {
      foreach (QuestTargetBalance target in Day.QuestTargets)
      {
        if (target.Kind == kind)
          return target;
      }

      return null;
    }

    public UpgradeDefinitionBalance GetUpgrade(UpgradeType type)
    {
      foreach (UpgradeDefinitionBalance upgrade in Upgrades.Upgrades)
      {
        if (upgrade.Type == type)
          return upgrade;
      }

      return null;
    }

    public PedestrianKind SelectRandomPedestrianKind()
    {
      float totalWeight = 0f;
      foreach (PedestrianTypeBalance tb in Pedestrians.TypeBalances)
        totalWeight += tb.SpawnWeight;

      if (totalWeight <= 0f)
        return PedestrianKind.StudentNerd;

      float random = UnityEngine.Random.value * totalWeight;
      float cumulative = 0f;

      foreach (PedestrianTypeBalance tb in Pedestrians.TypeBalances)
      {
        cumulative += tb.SpawnWeight;
        if (random <= cumulative)
          return tb.Kind;
      }

      return Pedestrians.TypeBalances[^1].Kind;
    }

    #endregion
  }

  #endregion

  #region Balance Provider Service

  public interface IBalanceProvider
  {
    GameBalance Balance { get; }

    // Convenience accessors
    MovementBalance Movement { get; }
    PedestrianBalance Pedestrians { get; }
    EconomyBalance Economy { get; }
    DayBalance Day { get; }
    UpgradeBalance Upgrades { get; }
    FeedbackBalance Feedback { get; }
  }

  public class BalanceProvider : IBalanceProvider
  {
    private const string BalanceResourcePath = "Configs/GameBalance";

    private GameBalance _balance;

    public GameBalance Balance
    {
      get
      {
        if (_balance == null)
          LoadBalance();
        return _balance;
      }
    }

    public MovementBalance Movement => Balance.Movement;
    public PedestrianBalance Pedestrians => Balance.Pedestrians;
    public EconomyBalance Economy => Balance.Economy;
    public DayBalance Day => Balance.Day;
    public UpgradeBalance Upgrades => Balance.Upgrades;
    public FeedbackBalance Feedback => Balance.Feedback;

    public BalanceProvider()
    {
      LoadBalance();
    }

    public BalanceProvider(GameBalance balance)
    {
      _balance = balance ?? LoadOrCreateDefault();
    }

    private void LoadBalance()
    {
      _balance = LoadOrCreateDefault();
    }

    private GameBalance LoadOrCreateDefault()
    {
      // Try to load from Resources
      GameBalance loaded = Resources.Load<GameBalance>(BalanceResourcePath);

      if (loaded != null)
      {
        Debug.Log("[BalanceProvider] Loaded GameBalance from Resources");
        return loaded;
      }

      // Create runtime default
      Debug.LogWarning($"[BalanceProvider] GameBalance not found at Resources/{BalanceResourcePath}. Using defaults.");
      GameBalance defaultBalance = ScriptableObject.CreateInstance<GameBalance>();
      return defaultBalance;
    }
  }

  #endregion

  #region Adapter Classes (for backward compatibility)

  /// <summary>
  /// Adapter to provide old Settings interfaces from new Balance
  /// </summary>
  public static class BalanceAdapters
  {
    public static Code.Gameplay.Features.Hero.RunnerMovementSettings ToMovementSettings(MovementBalance balance)
    {
      return new Code.Gameplay.Features.Hero.RunnerMovementSettings
      {
        ForwardSpeed = balance.ForwardSpeed,
        LateralSpeed = balance.LateralSpeed,
        RoadWidth = balance.RoadWidth
      };
    }

    public static Code.Gameplay.Features.Pedestrian.PedestrianSpawnSettings ToPedestrianSpawnSettings(
      PedestrianBalance balance)
    {
      return new Code.Gameplay.Features.Pedestrian.PedestrianSpawnSettings
      {
        SpawnInterval = balance.SpawnInterval,
        SpawnDistanceAhead = balance.SpawnDistanceAhead,
        DespawnDistanceBehind = balance.DespawnDistanceBehind,
        MaxActive = balance.MaxActive,
        LateralMargin = balance.LateralMargin,
        CrossingChance = balance.CrossingChance,
        CrossingSpeedMultiplier = balance.CrossingSpeedMultiplier,
        SidewalkOffset = balance.SidewalkOffset
      };
    }

    public static Code.Gameplay.Features.Economy.EconomySettings ToEconomySettings(EconomyBalance balance)
    {
      return new Code.Gameplay.Features.Economy.EconomySettings
      {
        StartingMoney = balance.StartingMoney,
        ViolationPenalty = balance.ViolationPenalty,
        BaseQuestReward = balance.BaseQuestReward
      };
    }

    public static Code.Gameplay.Features.Collision.CollisionSettings ToCollisionSettings(MovementBalance balance)
    {
      return new Code.Gameplay.Features.Collision.CollisionSettings
      {
        HitRadius = balance.HitRadius
      };
    }

    public static Code.Gameplay.Features.Quest.QuestSettings ToQuestSettings(DayBalance balance)
    {
      return new Code.Gameplay.Features.Quest.QuestSettings
      {
        MinQuestsPerDay = balance.MinQuestsPerDay,
        MaxQuestsPerDay = balance.MaxQuestsPerDay
      };
    }

    public static Code.Gameplay.Features.Feedback.FeedbackSettings ToFeedbackSettings(FeedbackBalance balance)
    {
      return new Code.Gameplay.Features.Feedback.FeedbackSettings
      {
        ParticleBurstCount = balance.ParticleBurstCount,
        ParticleLifetime = balance.ParticleLifetime,
        ParticleSpeed = balance.ParticleSpeed,
        ParticleGravity = balance.ParticleGravity,
        ParticleSize = balance.ParticleSize,
        FloatSpeed = balance.FloatSpeed,
        FloatDuration = balance.FloatDuration,
        FontSize = balance.FontSize,
        SFXVolume = balance.SFXVolume,
        RewardColor = balance.RewardColor,
        PenaltyColor = balance.PenaltyColor
      };
    }
  }

  #endregion
}
