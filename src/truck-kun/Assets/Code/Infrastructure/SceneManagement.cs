using System;
using System.Collections;
using System.Collections.Generic;
using Code.Meta.Upgrades;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.Infrastructure
{
  #region Game State

  /// <summary>
  /// Persistent game state that survives scene transitions
  /// </summary>
  [Serializable]
  public class GameState
  {
    public int PlayerMoney;
    public int DayNumber;
    public Dictionary<UpgradeType, int> UpgradeLevels;
    public int TotalEarned;
    public int TotalPenalties;

    public GameState()
    {
      PlayerMoney = 1000; // Starting money
      DayNumber = 1;
      UpgradeLevels = new Dictionary<UpgradeType, int>();
      TotalEarned = 0;
      TotalPenalties = 0;
    }

    public GameState Clone()
    {
      return new GameState
      {
        PlayerMoney = PlayerMoney,
        DayNumber = DayNumber,
        UpgradeLevels = new Dictionary<UpgradeType, int>(UpgradeLevels),
        TotalEarned = TotalEarned,
        TotalPenalties = TotalPenalties
      };
    }
  }

  /// <summary>
  /// Singleton service for managing persistent game state
  /// </summary>
  public class GameStateService
  {
    private const string SaveKey = "TruckKunGameState";
    private const int StartingMoney = 1000;

    private static GameStateService _instance;
    public static GameStateService Instance => _instance ??= new GameStateService();

    private GameState _state;

    public int PlayerMoney
    {
      get => _state.PlayerMoney;
      set
      {
        _state.PlayerMoney = value;
        OnMoneyChanged?.Invoke(value);
      }
    }

    public int DayNumber
    {
      get => _state.DayNumber;
      set => _state.DayNumber = value;
    }

    public int TotalEarned
    {
      get => _state.TotalEarned;
      set => _state.TotalEarned = value;
    }

    public int TotalPenalties
    {
      get => _state.TotalPenalties;
      set => _state.TotalPenalties = value;
    }

    public event Action<int> OnMoneyChanged;
    public event Action OnStateLoaded;

    private GameStateService()
    {
      _state = new GameState();
      Load();
    }

    public int GetUpgradeLevel(UpgradeType type)
    {
      return _state.UpgradeLevels.TryGetValue(type, out int level) ? level : 0;
    }

    public void SetUpgradeLevel(UpgradeType type, int level)
    {
      _state.UpgradeLevels[type] = level;
    }

    public Dictionary<UpgradeType, int> GetAllUpgradeLevels()
    {
      return new Dictionary<UpgradeType, int>(_state.UpgradeLevels);
    }

    public bool SpendMoney(int amount)
    {
      if (amount <= 0)
        return true;

      if (_state.PlayerMoney < amount)
        return false;

      PlayerMoney -= amount;
      return true;
    }

    public void AddMoney(int amount)
    {
      if (amount <= 0)
        return;

      PlayerMoney += amount;
    }

    public void AddEarnings(int earned, int penalties)
    {
      TotalEarned += earned;
      TotalPenalties += penalties;
    }

    public void IncrementDay()
    {
      DayNumber++;
    }

    public void Save()
    {
      try
      {
        // Save individual values
        PlayerPrefs.SetInt($"{SaveKey}_Money", _state.PlayerMoney);
        PlayerPrefs.SetInt($"{SaveKey}_Day", _state.DayNumber);
        PlayerPrefs.SetInt($"{SaveKey}_TotalEarned", _state.TotalEarned);
        PlayerPrefs.SetInt($"{SaveKey}_TotalPenalties", _state.TotalPenalties);

        // Save upgrades
        List<string> upgradeParts = new();
        foreach (var kvp in _state.UpgradeLevels)
        {
          upgradeParts.Add($"{kvp.Key}:{kvp.Value}");
        }
        PlayerPrefs.SetString($"{SaveKey}_Upgrades", string.Join(",", upgradeParts));

        PlayerPrefs.SetInt($"{SaveKey}_Exists", 1);
        PlayerPrefs.Save();

        Debug.Log($"[GameState] Saved: Money={_state.PlayerMoney}, Day={_state.DayNumber}");
      }
      catch (Exception e)
      {
        Debug.LogError($"[GameState] Failed to save: {e.Message}");
      }
    }

    public void Load()
    {
      try
      {
        if (PlayerPrefs.GetInt($"{SaveKey}_Exists", 0) == 0)
        {
          _state = new GameState();
          Debug.Log("[GameState] No save found, using defaults");
          return;
        }

        _state.PlayerMoney = PlayerPrefs.GetInt($"{SaveKey}_Money", StartingMoney);
        _state.DayNumber = PlayerPrefs.GetInt($"{SaveKey}_Day", 1);
        _state.TotalEarned = PlayerPrefs.GetInt($"{SaveKey}_TotalEarned", 0);
        _state.TotalPenalties = PlayerPrefs.GetInt($"{SaveKey}_TotalPenalties", 0);

        // Load upgrades
        _state.UpgradeLevels.Clear();
        string upgradesStr = PlayerPrefs.GetString($"{SaveKey}_Upgrades", "");
        if (!string.IsNullOrEmpty(upgradesStr))
        {
          foreach (string part in upgradesStr.Split(','))
          {
            string[] kv = part.Split(':');
            if (kv.Length == 2 &&
                Enum.TryParse(kv[0], out UpgradeType type) &&
                int.TryParse(kv[1], out int level))
            {
              _state.UpgradeLevels[type] = level;
            }
          }
        }

        Debug.Log($"[GameState] Loaded: Money={_state.PlayerMoney}, Day={_state.DayNumber}");
        OnStateLoaded?.Invoke();
      }
      catch (Exception e)
      {
        Debug.LogError($"[GameState] Failed to load: {e.Message}");
        _state = new GameState();
      }
    }

    public void Reset()
    {
      PlayerPrefs.DeleteKey($"{SaveKey}_Exists");
      PlayerPrefs.DeleteKey($"{SaveKey}_Money");
      PlayerPrefs.DeleteKey($"{SaveKey}_Day");
      PlayerPrefs.DeleteKey($"{SaveKey}_TotalEarned");
      PlayerPrefs.DeleteKey($"{SaveKey}_TotalPenalties");
      PlayerPrefs.DeleteKey($"{SaveKey}_Upgrades");
      PlayerPrefs.Save();

      _state = new GameState();
      Debug.Log("[GameState] Reset to defaults");
    }

    public GameState GetStateCopy()
    {
      return _state.Clone();
    }
  }

  #endregion

  #region Scene Transition

  public enum SceneType
  {
    Hub,
    Gameplay
  }

  /// <summary>
  /// Service for managing scene transitions with loading screen
  /// </summary>
  public class SceneTransitionService : MonoBehaviour
  {
    private const string HubSceneName = "HubScene";
    private const string GameplaySceneName = "GameScene";
    private const float MinLoadingTime = 0.5f;

    private static SceneTransitionService _instance;
    public static SceneTransitionService Instance
    {
      get
      {
        if (_instance == null)
        {
          GameObject go = new GameObject("SceneTransitionService");
          _instance = go.AddComponent<SceneTransitionService>();
          DontDestroyOnLoad(go);
        }
        return _instance;
      }
    }

    private Canvas _loadingCanvas;
    private Text _loadingText;
    private Image _progressBar;
    private bool _isTransitioning;

    public bool IsTransitioning => _isTransitioning;

    public event Action<SceneType> OnSceneLoadStarted;
    public event Action<SceneType> OnSceneLoadCompleted;

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;
      DontDestroyOnLoad(gameObject);
      CreateLoadingUI();
    }

    public void LoadHub(Action onComplete = null)
    {
      if (_isTransitioning)
        return;

      // Save current state before transition
      GameStateService.Instance.Save();

      StartCoroutine(LoadSceneAsync(HubSceneName, SceneType.Hub, onComplete));
    }

    public void LoadGameplay(Action onComplete = null)
    {
      if (_isTransitioning)
        return;

      // Save current state before transition
      GameStateService.Instance.Save();

      StartCoroutine(LoadSceneAsync(GameplaySceneName, SceneType.Gameplay, onComplete));
    }

    private IEnumerator LoadSceneAsync(string sceneName, SceneType sceneType, Action onComplete)
    {
      _isTransitioning = true;
      OnSceneLoadStarted?.Invoke(sceneType);

      ShowLoadingScreen();
      UpdateLoadingText("Загрузка...");

      float startTime = Time.realtimeSinceStartup;

      // Start async load
      AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
      asyncLoad.allowSceneActivation = false;

      // Update progress
      while (!asyncLoad.isDone)
      {
        float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
        UpdateProgress(progress);

        // Scene is ready to activate when progress reaches 0.9
        if (asyncLoad.progress >= 0.9f)
        {
          // Ensure minimum loading time for smooth transition
          float elapsed = Time.realtimeSinceStartup - startTime;
          if (elapsed < MinLoadingTime)
          {
            yield return new WaitForSecondsRealtime(MinLoadingTime - elapsed);
          }

          UpdateLoadingText("Готово!");
          UpdateProgress(1f);

          yield return new WaitForSecondsRealtime(0.2f);

          asyncLoad.allowSceneActivation = true;
        }

        yield return null;
      }

      HideLoadingScreen();

      _isTransitioning = false;
      OnSceneLoadCompleted?.Invoke(sceneType);
      onComplete?.Invoke();
    }

    private void CreateLoadingUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("LoadingCanvas");
      canvasObj.transform.SetParent(transform, false);

      _loadingCanvas = canvasObj.AddComponent<Canvas>();
      _loadingCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _loadingCanvas.sortingOrder = 9999;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Background
      GameObject bgObj = new GameObject("Background");
      bgObj.transform.SetParent(canvasObj.transform, false);

      Image bgImage = bgObj.AddComponent<Image>();
      bgImage.color = new Color(0.05f, 0.05f, 0.1f, 1f);

      RectTransform bgRect = bgObj.GetComponent<RectTransform>();
      bgRect.anchorMin = Vector2.zero;
      bgRect.anchorMax = Vector2.one;
      bgRect.offsetMin = Vector2.zero;
      bgRect.offsetMax = Vector2.zero;

      // Loading text
      GameObject textObj = new GameObject("LoadingText");
      textObj.transform.SetParent(canvasObj.transform, false);

      _loadingText = textObj.AddComponent<Text>();
      _loadingText.text = "Загрузка...";
      _loadingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _loadingText.fontSize = 48;
      _loadingText.fontStyle = FontStyle.Bold;
      _loadingText.color = Color.white;
      _loadingText.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = new Vector2(0.5f, 0.5f);
      textRect.anchorMax = new Vector2(0.5f, 0.5f);
      textRect.anchoredPosition = new Vector2(0f, 50f);
      textRect.sizeDelta = new Vector2(600f, 100f);

      // Progress bar background
      GameObject progressBgObj = new GameObject("ProgressBarBg");
      progressBgObj.transform.SetParent(canvasObj.transform, false);

      Image progressBgImage = progressBgObj.AddComponent<Image>();
      progressBgImage.color = new Color(0.2f, 0.2f, 0.25f, 1f);

      RectTransform progressBgRect = progressBgObj.GetComponent<RectTransform>();
      progressBgRect.anchorMin = new Vector2(0.5f, 0.5f);
      progressBgRect.anchorMax = new Vector2(0.5f, 0.5f);
      progressBgRect.anchoredPosition = new Vector2(0f, -30f);
      progressBgRect.sizeDelta = new Vector2(400f, 20f);

      // Progress bar fill
      GameObject progressFillObj = new GameObject("ProgressBarFill");
      progressFillObj.transform.SetParent(progressBgObj.transform, false);

      _progressBar = progressFillObj.AddComponent<Image>();
      _progressBar.color = new Color(0.2f, 0.7f, 0.3f, 1f);

      RectTransform progressFillRect = progressFillObj.GetComponent<RectTransform>();
      progressFillRect.anchorMin = new Vector2(0f, 0f);
      progressFillRect.anchorMax = new Vector2(0f, 1f);
      progressFillRect.pivot = new Vector2(0f, 0.5f);
      progressFillRect.anchoredPosition = Vector2.zero;
      progressFillRect.sizeDelta = new Vector2(0f, 0f);

      // Day info text
      GameObject dayTextObj = new GameObject("DayText");
      dayTextObj.transform.SetParent(canvasObj.transform, false);

      Text dayText = dayTextObj.AddComponent<Text>();
      dayText.text = $"День {GameStateService.Instance.DayNumber}";
      dayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      dayText.fontSize = 28;
      dayText.color = new Color(0.7f, 0.7f, 0.7f);
      dayText.alignment = TextAnchor.MiddleCenter;

      RectTransform dayTextRect = dayTextObj.GetComponent<RectTransform>();
      dayTextRect.anchorMin = new Vector2(0.5f, 0.5f);
      dayTextRect.anchorMax = new Vector2(0.5f, 0.5f);
      dayTextRect.anchoredPosition = new Vector2(0f, -80f);
      dayTextRect.sizeDelta = new Vector2(400f, 40f);

      _loadingCanvas.gameObject.SetActive(false);
    }

    private void ShowLoadingScreen()
    {
      if (_loadingCanvas != null)
      {
        // Update day text
        Transform dayTextTransform = _loadingCanvas.transform.Find("DayText");
        if (dayTextTransform != null)
        {
          Text dayText = dayTextTransform.GetComponent<Text>();
          if (dayText != null)
            dayText.text = $"День {GameStateService.Instance.DayNumber}";
        }

        _loadingCanvas.gameObject.SetActive(true);
      }
    }

    private void HideLoadingScreen()
    {
      if (_loadingCanvas != null)
        _loadingCanvas.gameObject.SetActive(false);
    }

    private void UpdateLoadingText(string text)
    {
      if (_loadingText != null)
        _loadingText.text = text;
    }

    private void UpdateProgress(float progress)
    {
      if (_progressBar != null)
      {
        RectTransform rect = _progressBar.GetComponent<RectTransform>();
        rect.anchorMax = new Vector2(progress, 1f);
      }
    }

    private void OnDestroy()
    {
      if (_instance == this)
        _instance = null;
    }
  }

  #endregion

  #region Scene Bootstrap Helper

  /// <summary>
  /// Helper for initializing scenes with GameState
  /// </summary>
  public static class SceneBootstrapHelper
  {
    public static void InitializeForGameplay(
      Code.Gameplay.Features.Hero.RunnerMovementSettings movementSettings,
      Code.Gameplay.Features.Economy.EconomySettings economySettings)
    {
      GameStateService state = GameStateService.Instance;

      // Apply speed upgrade
      float speedBonus = GetUpgradeBonus(UpgradeType.SpeedBoost, state.GetUpgradeLevel(UpgradeType.SpeedBoost));
      if (speedBonus > 0 && movementSettings != null)
      {
        movementSettings.ForwardSpeed *= (1f + speedBonus);
      }

      // Apply maneuverability upgrade
      float lateralBonus = GetUpgradeBonus(UpgradeType.Maneuverability, state.GetUpgradeLevel(UpgradeType.Maneuverability));
      if (lateralBonus > 0 && movementSettings != null)
      {
        movementSettings.LateralSpeed *= (1f + lateralBonus);
      }

      // Set starting money from state
      if (economySettings != null)
      {
        economySettings.StartingMoney = state.PlayerMoney;
      }

      Debug.Log($"[SceneBootstrap] Gameplay initialized: Speed={movementSettings?.ForwardSpeed}, Lateral={movementSettings?.LateralSpeed}");
    }

    public static float GetMoneyMultiplier()
    {
      int level = GameStateService.Instance.GetUpgradeLevel(UpgradeType.MoneyMultiplier);
      return 1f + GetUpgradeBonus(UpgradeType.MoneyMultiplier, level);
    }

    private static float GetUpgradeBonus(UpgradeType type, int level)
    {
      if (level <= 0)
        return 0f;

      // Default bonuses matching UpgradeConfig defaults
      float[] bonuses = type switch
      {
        UpgradeType.SpeedBoost => new[] { 0.1f, 0.2f, 0.3f },
        UpgradeType.Maneuverability => new[] { 0.15f, 0.30f, 0.45f },
        UpgradeType.MoneyMultiplier => new[] { 0.2f, 0.4f, 0.6f },
        _ => new[] { 0f, 0f, 0f }
      };

      int index = Mathf.Clamp(level - 1, 0, bonuses.Length - 1);
      return bonuses[index];
    }
  }

  #endregion
}
