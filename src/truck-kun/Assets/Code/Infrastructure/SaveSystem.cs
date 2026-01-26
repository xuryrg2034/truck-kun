using System;
using System.Collections.Generic;
using Code.Meta.Upgrades;
using UnityEngine;

namespace Code.Infrastructure
{
  #region Save Data

  [Serializable]
  public class SaveData
  {
    public int PlayerMoney;
    public int CurrentDay;
    public int TotalDaysPlayed;
    public int HighScore;
    public List<UpgradeSaveEntry> Upgrades = new();

    // Timestamp for save validation
    public long SaveTimestamp;

    public SaveData()
    {
      PlayerMoney = 0;
      CurrentDay = 1;
      TotalDaysPlayed = 0;
      HighScore = 0;
      Upgrades = new List<UpgradeSaveEntry>();
      SaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public SaveData(GameStateService gameState)
    {
      PlayerMoney = gameState.PlayerMoney;
      CurrentDay = gameState.DayNumber;
      TotalDaysPlayed = gameState.TotalDaysPlayed;

      // HighScore tracks highest day reached
      HighScore = Mathf.Max(gameState.HighScore, gameState.DayNumber);

      Upgrades = new List<UpgradeSaveEntry>();

      foreach (KeyValuePair<UpgradeType, int> upgrade in gameState.GetAllUpgrades())
      {
        Upgrades.Add(new UpgradeSaveEntry
        {
          Type = (int)upgrade.Key,
          Level = upgrade.Value
        });
      }

      SaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public Dictionary<UpgradeType, int> GetUpgradesDictionary()
    {
      Dictionary<UpgradeType, int> result = new();

      foreach (UpgradeSaveEntry entry in Upgrades)
      {
        if (Enum.IsDefined(typeof(UpgradeType), entry.Type))
        {
          result[(UpgradeType)entry.Type] = entry.Level;
        }
      }

      return result;
    }
  }

  [Serializable]
  public class UpgradeSaveEntry
  {
    public int Type;
    public int Level;
  }

  #endregion

  #region Save Service Interface

  public interface ISaveService
  {
    void SaveGame(SaveData data);
    SaveData LoadGame();
    bool HasSaveData();
    void DeleteSave();
  }

  #endregion

  #region PlayerPrefs Implementation

  public class PlayerPrefsSaveService : ISaveService
  {
    private const string SaveKey = "TruckKun_SaveData";
    private const string BackupKey = "TruckKun_SaveData_Backup";

    public void SaveGame(SaveData data)
    {
      if (data == null)
      {
        Debug.LogWarning("[SaveService] Attempted to save null data");
        return;
      }

      try
      {
        // Backup current save before overwriting
        if (PlayerPrefs.HasKey(SaveKey))
        {
          string currentSave = PlayerPrefs.GetString(SaveKey);
          PlayerPrefs.SetString(BackupKey, currentSave);
        }

        string json = JsonUtility.ToJson(data, false);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"[SaveService] Game saved. Day {data.CurrentDay}, Money: {data.PlayerMoney}¥");
      }
      catch (Exception e)
      {
        Debug.LogError($"[SaveService] Failed to save game: {e.Message}");
      }
    }

    public SaveData LoadGame()
    {
      if (!HasSaveData())
      {
        Debug.Log("[SaveService] No save data found, returning null");
        return null;
      }

      try
      {
        string json = PlayerPrefs.GetString(SaveKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);

        if (data == null)
        {
          Debug.LogWarning("[SaveService] Failed to parse save data, trying backup");
          return TryLoadBackup();
        }

        Debug.Log($"[SaveService] Game loaded. Day {data.CurrentDay}, Money: {data.PlayerMoney}¥");
        return data;
      }
      catch (Exception e)
      {
        Debug.LogError($"[SaveService] Failed to load game: {e.Message}");
        return TryLoadBackup();
      }
    }

    public bool HasSaveData()
    {
      return PlayerPrefs.HasKey(SaveKey);
    }

    public void DeleteSave()
    {
      PlayerPrefs.DeleteKey(SaveKey);
      PlayerPrefs.DeleteKey(BackupKey);
      PlayerPrefs.Save();

      Debug.Log("[SaveService] Save data deleted");
    }

    private SaveData TryLoadBackup()
    {
      if (!PlayerPrefs.HasKey(BackupKey))
        return null;

      try
      {
        string json = PlayerPrefs.GetString(BackupKey);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        Debug.Log("[SaveService] Loaded from backup");
        return data;
      }
      catch (Exception e)
      {
        Debug.LogError($"[SaveService] Failed to load backup: {e.Message}");
        return null;
      }
    }
  }

  #endregion

  #region Save Manager (Singleton for easy access)

  public class SaveManager
  {
    private static SaveManager _instance;
    public static SaveManager Instance => _instance ??= new SaveManager();

    private readonly ISaveService _saveService;

    public ISaveService SaveService => _saveService;

    private SaveManager()
    {
      _saveService = new PlayerPrefsSaveService();
    }

    public void SaveCurrentState()
    {
      SaveData data = new SaveData(GameStateService.Instance);
      _saveService.SaveGame(data);
    }

    public bool LoadAndApply()
    {
      SaveData data = _saveService.LoadGame();

      if (data == null)
        return false;

      GameStateService.Instance.ApplySaveData(data);
      return true;
    }

    public void StartNewGame()
    {
      _saveService.DeleteSave();
      GameStateService.Instance.ResetProgress();
    }

    public bool HasSave()
    {
      return _saveService.HasSaveData();
    }
  }

  #endregion

  #region Auto-Save Component

  public class AutoSaveHandler : MonoBehaviour
  {
    private static AutoSaveHandler _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
      if (_instance != null)
        return;

      GameObject go = new GameObject("[AutoSaveHandler]");
      _instance = go.AddComponent<AutoSaveHandler>();
      DontDestroyOnLoad(go);
    }

    private void OnApplicationQuit()
    {
      // Save on quit using GameStateService
      GameStateService.Instance.Save();
      Debug.Log("[AutoSave] Saved on application quit");
    }

    private void OnApplicationPause(bool pauseStatus)
    {
      // Save when app is paused (mobile)
      if (pauseStatus)
      {
        GameStateService.Instance.Save();
        Debug.Log("[AutoSave] Saved on application pause");
      }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
      // Save when losing focus (alt-tab, etc)
      if (!hasFocus)
      {
        GameStateService.Instance.Save();
        Debug.Log("[AutoSave] Saved on focus lost");
      }
    }
  }

  #endregion
}
