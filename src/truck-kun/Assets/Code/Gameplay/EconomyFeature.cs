using System;
using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Quest;
using Code.Infrastructure.Systems;
using Code.Meta.Upgrades;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using Zenject;

namespace Code.Gameplay.Features.Economy
{
  #region Components

  [Meta, Unique]
  public class PlayerMoney : IComponent
  {
    public int Amount;
  }

  [Meta, Unique]
  public class EarnedThisDay : IComponent
  {
    public int Amount;
  }

  [Meta, Unique]
  public class PenaltiesThisDay : IComponent
  {
    public int Amount;
  }

  #endregion

  #region Settings

  [Serializable]
  public class EconomySettings
  {
    public int StartingMoney = 1000;
    public int ViolationPenalty = 100;
    public int BaseQuestReward = 50;
  }

  [CreateAssetMenu(fileName = "EconomyConfig", menuName = "Truck-kun/Economy Config")]
  public class EconomyConfig : ScriptableObject
  {
    [SerializeField] private int _startingMoney = 1000;
    [SerializeField] private int _violationPenalty = 100;
    [SerializeField] private int _baseQuestReward = 50;

    public int StartingMoney => _startingMoney;
    public int ViolationPenalty => _violationPenalty;
    public int BaseQuestReward => _baseQuestReward;
  }

  #endregion

  #region Service

  public interface IMoneyService
  {
    int Balance { get; }
    int EarnedToday { get; }
    int PenaltiesToday { get; }

    void AddMoney(int amount);
    bool SpendMoney(int amount);
    int GetBalance();
    void ApplyPenalty(int amount);
    void ResetDayEarnings();
    void Initialize();
  }

  public class MoneyService : IMoneyService
  {
    private readonly MetaContext _meta;
    private readonly EconomySettings _settings;
    private readonly IUpgradeService _upgradeService;

    public MoneyService(
      MetaContext meta,
      [InjectOptional] EconomySettings settings = null,
      [InjectOptional] IUpgradeService upgradeService = null)
    {
      _meta = meta;
      _settings = settings ?? new EconomySettings();
      _upgradeService = upgradeService;
    }

    public int Balance => _meta.hasPlayerMoney ? _meta.playerMoney.Amount : 0;
    public int EarnedToday => _meta.hasEarnedThisDay ? _meta.earnedThisDay.Amount : 0;
    public int PenaltiesToday => _meta.hasPenaltiesThisDay ? _meta.penaltiesThisDay.Amount : 0;

    public void Initialize()
    {
      if (!_meta.hasPlayerMoney)
      {
        // Load saved balance from PlayerPrefs, or use starting money
        int savedBalance = PlayerPrefs.GetInt("PlayerBalance", -1);
        int initialMoney = savedBalance >= 0 ? savedBalance : _settings.StartingMoney;
        _meta.SetPlayerMoney(initialMoney);
      }

      if (!_meta.hasEarnedThisDay)
        _meta.SetEarnedThisDay(0);

      if (!_meta.hasPenaltiesThisDay)
        _meta.SetPenaltiesThisDay(0);
    }

    public void AddMoney(int amount)
    {
      if (amount <= 0)
        return;

      // Apply money multiplier from upgrades
      float multiplier = _upgradeService?.GetMoneyMultiplier() ?? 1f;
      int finalAmount = Mathf.RoundToInt(amount * multiplier);

      int newBalance = Balance + finalAmount;
      _meta.ReplacePlayerMoney(newBalance);

      int newEarned = EarnedToday + finalAmount;
      _meta.ReplaceEarnedThisDay(newEarned);
    }

    public bool SpendMoney(int amount)
    {
      if (amount <= 0)
        return true;

      if (Balance < amount)
        return false;

      int newBalance = Balance - amount;
      _meta.ReplacePlayerMoney(newBalance);
      return true;
    }

    public int GetBalance()
    {
      return Balance;
    }

    public void ApplyPenalty(int amount)
    {
      if (amount <= 0)
        return;

      int newBalance = Mathf.Max(0, Balance - amount);
      _meta.ReplacePlayerMoney(newBalance);

      int newPenalties = PenaltiesToday + amount;
      _meta.ReplacePenaltiesThisDay(newPenalties);
    }

    public void ResetDayEarnings()
    {
      _meta.ReplaceEarnedThisDay(0);
      _meta.ReplacePenaltiesThisDay(0);
    }
  }

  #endregion

  #region Feature

  public sealed class EconomyFeature : Feature
  {
    public EconomyFeature(ISystemFactory systems)
    {
      Add(systems.Create<EconomyInitializationSystem>());
      Add(systems.Create<ProcessViolationPenaltiesSystem>());
      Add(systems.Create<ProcessQuestRewardsSystem>());
    }
  }

  #endregion

  #region Systems

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
    private readonly EconomySettings _settings;

    public ProcessViolationPenaltiesSystem(
      GameContext game,
      IMoneyService moneyService,
      [InjectOptional] EconomySettings settings = null) : base(game)
    {
      _moneyService = moneyService;
      _settings = settings ?? new EconomySettings();
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
        _moneyService.ApplyPenalty(_settings.ViolationPenalty);
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

  #endregion
}
