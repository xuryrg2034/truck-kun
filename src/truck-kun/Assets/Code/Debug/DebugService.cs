#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using Code.Infrastructure;
using UnityEngine;

namespace Code.Debugging
{
  /// <summary>
  /// Debug service with cheat methods for testing
  /// </summary>
  public static class DebugService
  {
    public static bool GodModeEnabled { get; private set; }

    public static event Action OnCheatUsed;
    public static event Action<bool> OnGodModeChanged;

    public static void AddMoney(int amount = 1000)
    {
      GameStateService.Instance.AddMoney(amount);
      Log($"Added {amount} money. New balance: {GameStateService.Instance.PlayerMoney}");
      OnCheatUsed?.Invoke();
    }

    public static void SetMoney(int amount)
    {
      // Direct set via reflection or property
      int current = GameStateService.Instance.PlayerMoney;
      int diff = amount - current;
      if (diff > 0)
        GameStateService.Instance.AddMoney(diff);
      else if (diff < 0)
        GameStateService.Instance.SpendMoney(-diff);

      Log($"Set money to {amount}");
      OnCheatUsed?.Invoke();
    }

    public static void SkipToDay(int day)
    {
      if (day < 1) day = 1;

      GameStateService state = GameStateService.Instance;
      while (state.DayNumber < day)
      {
        state.IncrementDay();
      }
      state.Save();

      Log($"Skipped to day {day}");
      OnCheatUsed?.Invoke();
    }

    public static void CompleteAllQuests()
    {
      // This would need to interact with the quest system
      // For now, just add reward money
      AddMoney(500);
      Log("Completed all quests (simulated with +500 money)");
      OnCheatUsed?.Invoke();
    }

    public static void ResetSave()
    {
      GameStateService.Instance.Reset();
      Log("Save data reset");
      OnCheatUsed?.Invoke();
    }

    public static void ToggleGodMode()
    {
      GodModeEnabled = !GodModeEnabled;
      Log($"God Mode: {(GodModeEnabled ? "ENABLED" : "DISABLED")}");
      OnGodModeChanged?.Invoke(GodModeEnabled);
      OnCheatUsed?.Invoke();
    }

    public static void SetGodMode(bool enabled)
    {
      if (GodModeEnabled != enabled)
      {
        GodModeEnabled = enabled;
        Log($"God Mode: {(GodModeEnabled ? "ENABLED" : "DISABLED")}");
        OnGodModeChanged?.Invoke(GodModeEnabled);
      }
    }

    public static void MaxUpgrades()
    {
      GameStateService state = GameStateService.Instance;
      foreach (Meta.Upgrades.UpgradeType type in Enum.GetValues(typeof(Meta.Upgrades.UpgradeType)))
      {
        state.SetUpgradeLevel(type, 10);
      }
      state.Save();
      Log("All upgrades maxed to level 10");
      OnCheatUsed?.Invoke();
    }

    public static void UnlockAllUpgrades()
    {
      GameStateService state = GameStateService.Instance;
      foreach (Meta.Upgrades.UpgradeType type in Enum.GetValues(typeof(Meta.Upgrades.UpgradeType)))
      {
        if (state.GetUpgradeLevel(type) == 0)
          state.SetUpgradeLevel(type, 1);
      }
      state.Save();
      Log("All upgrades unlocked (level 1)");
      OnCheatUsed?.Invoke();
    }

    public static void AddTime(float seconds)
    {
      // Would interact with day timer if exists
      Log($"Added {seconds} seconds (not implemented)");
    }

    private static void Log(string message)
    {
      UnityEngine.Debug.Log($"<color=#FFD700>[CHEAT]</color> {message}");
    }
  }
}
#endif
