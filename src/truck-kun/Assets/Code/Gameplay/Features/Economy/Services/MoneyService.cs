using System;
using Code.Configs.Global;
using Code.Meta.Upgrades;
using Entitas;
using UnityEngine;
using Zenject;

namespace Code.Gameplay.Features.Economy.Services
{
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
    private readonly EconomyConfig _config;
    private readonly IUpgradeService _upgradeService;

    public MoneyService(
      MetaContext meta,
      EconomyConfig config,
      [InjectOptional] IUpgradeService upgradeService = null)
    {
      _meta = meta;
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "EconomyConfig is required! Assign it in LevelConfig.");
      _upgradeService = upgradeService;
    }

    public int Balance => _meta.hasPlayerMoney ? _meta.playerMoney.Amount : 0;
    public int EarnedToday => _meta.hasEarnedThisDay ? _meta.earnedThisDay.Amount : 0;
    public int PenaltiesToday => _meta.hasPenaltiesThisDay ? _meta.penaltiesThisDay.Amount : 0;

    public void Initialize()
    {
      if (!_meta.hasPlayerMoney)
      {
        int savedBalance = PlayerPrefs.GetInt("PlayerBalance", -1);
        int initialMoney = savedBalance >= 0 ? savedBalance : _config.StartingMoney;
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
}
