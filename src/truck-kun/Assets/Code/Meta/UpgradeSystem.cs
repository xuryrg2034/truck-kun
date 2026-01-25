using System;
using System.Collections.Generic;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Hero;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using Zenject;

namespace Code.Meta.Upgrades
{
  #region Enums

  public enum UpgradeType
  {
    SpeedBoost,
    Maneuverability,
    MoneyMultiplier
  }

  #endregion

  #region Components

  [Meta, Unique]
  public class UpgradesData : IComponent
  {
    public Dictionary<UpgradeType, int> Levels;
  }

  #endregion

  #region Data Structures

  [Serializable]
  public class UpgradeDefinition
  {
    public UpgradeType Type;
    public string Name;
    public string Description;
    public int MaxLevel = 3;
    public int[] Costs = { 500, 1000, 2000 };
    public float[] Bonuses = { 0.1f, 0.2f, 0.3f };

    public int GetCost(int currentLevel)
    {
      if (currentLevel >= MaxLevel || currentLevel < 0)
        return -1;

      return currentLevel < Costs.Length ? Costs[currentLevel] : Costs[^1];
    }

    public float GetBonus(int level)
    {
      if (level <= 0)
        return 0f;

      int index = Mathf.Clamp(level - 1, 0, Bonuses.Length - 1);
      return Bonuses[index];
    }
  }

  public readonly struct UpgradeInfo
  {
    public readonly UpgradeType Type;
    public readonly string Name;
    public readonly string Description;
    public readonly int CurrentLevel;
    public readonly int MaxLevel;
    public readonly int NextCost;
    public readonly float CurrentBonus;
    public readonly float NextBonus;
    public readonly bool IsMaxed;

    public UpgradeInfo(
      UpgradeType type,
      string name,
      string description,
      int currentLevel,
      int maxLevel,
      int nextCost,
      float currentBonus,
      float nextBonus)
    {
      Type = type;
      Name = name;
      Description = description;
      CurrentLevel = currentLevel;
      MaxLevel = maxLevel;
      NextCost = nextCost;
      CurrentBonus = currentBonus;
      NextBonus = nextBonus;
      IsMaxed = currentLevel >= maxLevel;
    }
  }

  #endregion

  #region ScriptableObject

  [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "Truck-kun/Upgrade Config")]
  public class UpgradeConfig : ScriptableObject
  {
    [SerializeField] private List<UpgradeDefinition> _upgrades = new()
    {
      new UpgradeDefinition
      {
        Type = UpgradeType.SpeedBoost,
        Name = "Форсаж",
        Description = "Увеличивает скорость движения",
        MaxLevel = 3,
        Costs = new[] { 500, 1000, 2000 },
        Bonuses = new[] { 0.1f, 0.2f, 0.3f }
      },
      new UpgradeDefinition
      {
        Type = UpgradeType.Maneuverability,
        Name = "Маневренность",
        Description = "Увеличивает боковую скорость",
        MaxLevel = 3,
        Costs = new[] { 500, 1000, 2000 },
        Bonuses = new[] { 0.15f, 0.30f, 0.45f }
      },
      new UpgradeDefinition
      {
        Type = UpgradeType.MoneyMultiplier,
        Name = "Жадность",
        Description = "Множитель наград",
        MaxLevel = 3,
        Costs = new[] { 500, 1000, 2000 },
        Bonuses = new[] { 0.2f, 0.4f, 0.6f }
      }
    };

    public IReadOnlyList<UpgradeDefinition> Upgrades => _upgrades;

    public UpgradeDefinition GetDefinition(UpgradeType type)
    {
      foreach (UpgradeDefinition upgrade in _upgrades)
      {
        if (upgrade.Type == type)
          return upgrade;
      }

      return null;
    }
  }

  #endregion

  #region Service

  public interface IUpgradeService
  {
    bool PurchaseUpgrade(UpgradeType type);
    int GetUpgradeLevel(UpgradeType type);
    void ApplyUpgradesToSettings(RunnerMovementSettings settings);
    float GetMoneyMultiplier();
    IReadOnlyList<UpgradeInfo> GetAllUpgrades();
    UpgradeInfo GetUpgradeInfo(UpgradeType type);
    void Initialize();
    void SaveToPlayerPrefs();
    void LoadFromPlayerPrefs();
  }

  public class UpgradeService : IUpgradeService
  {
    private const string UpgradesPrefsKey = "PlayerUpgrades";

    private readonly MetaContext _meta;
    private readonly IMoneyService _moneyService;
    private readonly UpgradeConfig _config;

    private Dictionary<UpgradeType, int> _levels;

    public UpgradeService(
      MetaContext meta,
      IMoneyService moneyService,
      [InjectOptional] UpgradeConfig config = null)
    {
      _meta = meta;
      _moneyService = moneyService;
      _config = config;
      _levels = new Dictionary<UpgradeType, int>();
    }

    public void Initialize()
    {
      LoadFromPlayerPrefs();

      if (!_meta.hasUpgradesData)
      {
        _meta.SetUpgradesData(new Dictionary<UpgradeType, int>(_levels));
      }
      else
      {
        _levels = new Dictionary<UpgradeType, int>(_meta.upgradesData.Levels);
      }
    }

    public bool PurchaseUpgrade(UpgradeType type)
    {
      UpgradeDefinition definition = GetDefinition(type);
      if (definition == null)
        return false;

      int currentLevel = GetUpgradeLevel(type);
      if (currentLevel >= definition.MaxLevel)
        return false;

      int cost = definition.GetCost(currentLevel);
      if (cost < 0)
        return false;

      if (!_moneyService.SpendMoney(cost))
        return false;

      _levels[type] = currentLevel + 1;
      SyncToMeta();
      SaveToPlayerPrefs();

      return true;
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
      return _levels.TryGetValue(type, out int level) ? level : 0;
    }

    public void ApplyUpgradesToSettings(RunnerMovementSettings settings)
    {
      if (settings == null)
        return;

      // Speed Boost
      float speedBonus = GetBonus(UpgradeType.SpeedBoost);
      if (speedBonus > 0)
      {
        settings.ForwardSpeed *= (1f + speedBonus);
      }

      // Maneuverability
      float lateralBonus = GetBonus(UpgradeType.Maneuverability);
      if (lateralBonus > 0)
      {
        settings.LateralSpeed *= (1f + lateralBonus);
      }
    }

    public float GetMoneyMultiplier()
    {
      float bonus = GetBonus(UpgradeType.MoneyMultiplier);
      return 1f + bonus;
    }

    public IReadOnlyList<UpgradeInfo> GetAllUpgrades()
    {
      if (_config == null)
        return Array.Empty<UpgradeInfo>();

      List<UpgradeInfo> result = new(_config.Upgrades.Count);

      foreach (UpgradeDefinition def in _config.Upgrades)
      {
        result.Add(CreateUpgradeInfo(def));
      }

      return result;
    }

    public UpgradeInfo GetUpgradeInfo(UpgradeType type)
    {
      UpgradeDefinition def = GetDefinition(type);
      if (def == null)
        return default;

      return CreateUpgradeInfo(def);
    }

    public void SaveToPlayerPrefs()
    {
      string json = SerializeUpgrades();
      PlayerPrefs.SetString(UpgradesPrefsKey, json);
      PlayerPrefs.Save();
    }

    public void LoadFromPlayerPrefs()
    {
      _levels.Clear();

      string json = PlayerPrefs.GetString(UpgradesPrefsKey, "");
      if (!string.IsNullOrEmpty(json))
      {
        DeserializeUpgrades(json);
      }
    }

    private UpgradeDefinition GetDefinition(UpgradeType type)
    {
      return _config?.GetDefinition(type);
    }

    private float GetBonus(UpgradeType type)
    {
      UpgradeDefinition def = GetDefinition(type);
      if (def == null)
        return 0f;

      int level = GetUpgradeLevel(type);
      return def.GetBonus(level);
    }

    private UpgradeInfo CreateUpgradeInfo(UpgradeDefinition def)
    {
      int currentLevel = GetUpgradeLevel(def.Type);
      int nextCost = def.GetCost(currentLevel);
      float currentBonus = def.GetBonus(currentLevel);
      float nextBonus = def.GetBonus(currentLevel + 1);

      return new UpgradeInfo(
        def.Type,
        def.Name,
        def.Description,
        currentLevel,
        def.MaxLevel,
        nextCost,
        currentBonus,
        nextBonus);
    }

    private void SyncToMeta()
    {
      _meta.ReplaceUpgradesData(new Dictionary<UpgradeType, int>(_levels));
    }

    private string SerializeUpgrades()
    {
      // Simple format: "SpeedBoost:1,Maneuverability:2,MoneyMultiplier:0"
      List<string> parts = new();
      foreach (var kvp in _levels)
      {
        parts.Add($"{kvp.Key}:{kvp.Value}");
      }
      return string.Join(",", parts);
    }

    private void DeserializeUpgrades(string json)
    {
      string[] parts = json.Split(',');
      foreach (string part in parts)
      {
        string[] kv = part.Split(':');
        if (kv.Length == 2 &&
            Enum.TryParse(kv[0], out UpgradeType type) &&
            int.TryParse(kv[1], out int level))
        {
          _levels[type] = level;
        }
      }
    }
  }

  #endregion

  #region Hub Service

  /// <summary>
  /// Simplified upgrade service for Hub scene (without Entitas dependency)
  /// </summary>
  public class HubUpgradeService : IUpgradeService
  {
    private const string UpgradesPrefsKey = "PlayerUpgrades";

    private readonly IMoneyService _moneyService;
    private readonly UpgradeConfig _config;
    private Dictionary<UpgradeType, int> _levels;

    public HubUpgradeService(IMoneyService moneyService, UpgradeConfig config = null)
    {
      _moneyService = moneyService;
      _config = config ?? CreateDefaultConfig();
      _levels = new Dictionary<UpgradeType, int>();
    }

    public void Initialize()
    {
      LoadFromPlayerPrefs();
    }

    public bool PurchaseUpgrade(UpgradeType type)
    {
      UpgradeDefinition definition = _config.GetDefinition(type);
      if (definition == null)
        return false;

      int currentLevel = GetUpgradeLevel(type);
      if (currentLevel >= definition.MaxLevel)
        return false;

      int cost = definition.GetCost(currentLevel);
      if (cost < 0)
        return false;

      if (!_moneyService.SpendMoney(cost))
        return false;

      _levels[type] = currentLevel + 1;
      SaveToPlayerPrefs();

      return true;
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
      return _levels.TryGetValue(type, out int level) ? level : 0;
    }

    public void ApplyUpgradesToSettings(RunnerMovementSettings settings)
    {
      if (settings == null)
        return;

      float speedBonus = GetBonus(UpgradeType.SpeedBoost);
      if (speedBonus > 0)
        settings.ForwardSpeed *= (1f + speedBonus);

      float lateralBonus = GetBonus(UpgradeType.Maneuverability);
      if (lateralBonus > 0)
        settings.LateralSpeed *= (1f + lateralBonus);
    }

    public float GetMoneyMultiplier()
    {
      return 1f + GetBonus(UpgradeType.MoneyMultiplier);
    }

    public IReadOnlyList<UpgradeInfo> GetAllUpgrades()
    {
      List<UpgradeInfo> result = new(_config.Upgrades.Count);
      foreach (UpgradeDefinition def in _config.Upgrades)
      {
        result.Add(CreateUpgradeInfo(def));
      }
      return result;
    }

    public UpgradeInfo GetUpgradeInfo(UpgradeType type)
    {
      UpgradeDefinition def = _config.GetDefinition(type);
      return def != null ? CreateUpgradeInfo(def) : default;
    }

    public void SaveToPlayerPrefs()
    {
      List<string> parts = new();
      foreach (var kvp in _levels)
        parts.Add($"{kvp.Key}:{kvp.Value}");

      PlayerPrefs.SetString(UpgradesPrefsKey, string.Join(",", parts));
      PlayerPrefs.Save();
    }

    public void LoadFromPlayerPrefs()
    {
      _levels.Clear();
      string data = PlayerPrefs.GetString(UpgradesPrefsKey, "");

      if (string.IsNullOrEmpty(data))
        return;

      foreach (string part in data.Split(','))
      {
        string[] kv = part.Split(':');
        if (kv.Length == 2 &&
            Enum.TryParse(kv[0], out UpgradeType type) &&
            int.TryParse(kv[1], out int level))
        {
          _levels[type] = level;
        }
      }
    }

    private float GetBonus(UpgradeType type)
    {
      UpgradeDefinition def = _config.GetDefinition(type);
      return def?.GetBonus(GetUpgradeLevel(type)) ?? 0f;
    }

    private UpgradeInfo CreateUpgradeInfo(UpgradeDefinition def)
    {
      int currentLevel = GetUpgradeLevel(def.Type);
      return new UpgradeInfo(
        def.Type,
        def.Name,
        def.Description,
        currentLevel,
        def.MaxLevel,
        def.GetCost(currentLevel),
        def.GetBonus(currentLevel),
        def.GetBonus(currentLevel + 1));
    }

    private static UpgradeConfig CreateDefaultConfig()
    {
      UpgradeConfig config = ScriptableObject.CreateInstance<UpgradeConfig>();
      return config;
    }
  }

  #endregion
}
