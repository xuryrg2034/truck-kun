#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Text;
using Code.Infrastructure;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.DevTools
{
  /// <summary>
  /// Debug panel UI for testing and balancing
  /// Toggle with F1, various hotkeys for quick cheats
  /// </summary>
  public class DebugPanel : MonoBehaviour
  {
    private static DebugPanel _instance;

    private Canvas _canvas;
    private GameObject _panel;
    private Text _fpsText;
    private Text _entityCountText;
    private Text _systemsText;
    private Text _stateText;
    private Toggle _godModeToggle;

    private bool _isVisible;
    private float _fpsUpdateTimer;
    private float _fps;
    private int _frameCount;
    private float _fpsAccumulator;

    private static readonly Color PanelColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);
    private static readonly Color ButtonColor = new Color(0.3f, 0.3f, 0.35f);
    private static readonly Color CheatColor = new Color(0.8f, 0.6f, 0.1f);
    private static readonly Color DangerColor = new Color(0.7f, 0.2f, 0.2f);

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
      if (_instance != null)
        return;

      GameObject go = new GameObject("[DebugPanel]");
      _instance = go.AddComponent<DebugPanel>();
      DontDestroyOnLoad(go);
    }

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;
      CreateUI();
      _panel.SetActive(false);
    }

    private void Update()
    {
      // Hotkeys
      if (Keyboard.current != null)
      {
        // F1 - Toggle panel
        if (Keyboard.current.f1Key.wasPressedThisFrame)
        {
          TogglePanel();
        }

        // F5 - Quick add money
        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
          DebugService.AddMoney(1000);
        }

        // F6 - Complete quests
        if (Keyboard.current.f6Key.wasPressedThisFrame)
        {
          DebugService.CompleteAllQuests();
        }

        // F7 - Toggle god mode
        if (Keyboard.current.f7Key.wasPressedThisFrame)
        {
          DebugService.ToggleGodMode();
          if (_godModeToggle != null)
            _godModeToggle.isOn = DebugService.GodModeEnabled;
        }

        // F8 - Skip day
        if (Keyboard.current.f8Key.wasPressedThisFrame)
        {
          DebugService.SkipToDay(GameStateService.Instance.DayNumber + 1);
        }
      }

      // Update FPS counter
      UpdateFPS();

      // Update info if visible
      if (_isVisible)
      {
        UpdateInfo();
      }
    }

    private void UpdateFPS()
    {
      _frameCount++;
      _fpsAccumulator += Time.unscaledDeltaTime;
      _fpsUpdateTimer += Time.unscaledDeltaTime;

      if (_fpsUpdateTimer >= 0.5f)
      {
        _fps = _frameCount / _fpsAccumulator;
        _frameCount = 0;
        _fpsAccumulator = 0f;
        _fpsUpdateTimer = 0f;

        if (_fpsText != null && _isVisible)
        {
          _fpsText.text = $"FPS: {_fps:F1}";
          _fpsText.color = _fps >= 60 ? Color.green : (_fps >= 30 ? Color.yellow : Color.red);
        }
      }
    }

    private void UpdateInfo()
    {
      // Update game state info
      if (_stateText != null)
      {
        GameStateService state = GameStateService.Instance;
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"<b>Game State:</b>");
        sb.AppendLine($"  Day: {state.DayNumber}");
        sb.AppendLine($"  Money: {state.PlayerMoney} ¥");
        sb.AppendLine($"  Total Days: {state.TotalDaysPlayed}");
        sb.AppendLine($"  High Score: {state.HighScore}");
        sb.AppendLine($"  God Mode: {(DebugService.GodModeEnabled ? "<color=yellow>ON</color>" : "OFF")}");
        _stateText.text = sb.ToString();
      }

      // Update entity count (if Entitas contexts are available)
      if (_entityCountText != null)
      {
        int entityCount = GetEntityCount();
        _entityCountText.text = $"Entities: {entityCount}";
      }
    }

    private int GetEntityCount()
    {
      int count = 0;

      // Try to get entity counts from Entitas contexts
      try
      {
        // Use reflection to avoid hard dependency
        System.Type contextsType = System.Type.GetType("Contexts, Assembly-CSharp");
        if (contextsType != null)
        {
          var sharedInstanceProp = contextsType.GetProperty("sharedInstance");
          if (sharedInstanceProp != null)
          {
            var contexts = sharedInstanceProp.GetValue(null);
            if (contexts != null)
            {
              // Get game context
              var gameProp = contextsType.GetProperty("game");
              if (gameProp != null)
              {
                var gameContext = gameProp.GetValue(contexts);
                if (gameContext != null)
                {
                  var countProp = gameContext.GetType().GetProperty("count");
                  if (countProp != null)
                  {
                    count += (int)countProp.GetValue(gameContext);
                  }
                }
              }

              // Get meta context
              var metaProp = contextsType.GetProperty("meta");
              if (metaProp != null)
              {
                var metaContext = metaProp.GetValue(contexts);
                if (metaContext != null)
                {
                  var countProp = metaContext.GetType().GetProperty("count");
                  if (countProp != null)
                  {
                    count += (int)countProp.GetValue(metaContext);
                  }
                }
              }
            }
          }
        }
      }
      catch
      {
        // Ignore reflection errors
      }

      return count;
    }

    private void TogglePanel()
    {
      _isVisible = !_isVisible;
      _panel.SetActive(_isVisible);

      if (_isVisible)
      {
        UpdateInfo();
      }
    }

    private void CreateUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("DebugCanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 9999;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Panel
      _panel = new GameObject("DebugPanel");
      _panel.transform.SetParent(canvasObj.transform, false);

      Image panelBg = _panel.AddComponent<Image>();
      panelBg.color = PanelColor;

      RectTransform panelRect = _panel.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0f, 0f);
      panelRect.anchorMax = new Vector2(0f, 1f);
      panelRect.pivot = new Vector2(0f, 0.5f);
      panelRect.anchoredPosition = new Vector2(10f, 0f);
      panelRect.sizeDelta = new Vector2(320f, -20f);

      // Content
      GameObject contentObj = new GameObject("Content");
      contentObj.transform.SetParent(_panel.transform, false);

      RectTransform contentRect = contentObj.AddComponent<RectTransform>();
      contentRect.anchorMin = Vector2.zero;
      contentRect.anchorMax = Vector2.one;
      contentRect.offsetMin = new Vector2(10f, 10f);
      contentRect.offsetMax = new Vector2(-10f, -10f);

      VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 8f;
      layout.childAlignment = TextAnchor.UpperCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;
      layout.padding = new RectOffset(5, 5, 5, 5);

      // Title
      CreateLabel(contentObj.transform, "DEBUG PANEL", 24, FontStyle.Bold, CheatColor, 35f);
      CreateLabel(contentObj.transform, "F1: Toggle | F5: Money | F7: God", 12, FontStyle.Normal, Color.gray, 20f);

      CreateSpacer(contentObj.transform, 10f);

      // FPS Counter
      _fpsText = CreateLabel(contentObj.transform, "FPS: --", 18, FontStyle.Bold, Color.green, 25f);

      // Entity count
      _entityCountText = CreateLabel(contentObj.transform, "Entities: 0", 16, FontStyle.Normal, Color.white, 22f);

      CreateSpacer(contentObj.transform, 10f);

      // Game state info
      _stateText = CreateLabel(contentObj.transform, "", 14, FontStyle.Normal, Color.white, 120f);
      _stateText.alignment = TextAnchor.UpperLeft;

      CreateSpacer(contentObj.transform, 10f);

      // Cheats section
      CreateLabel(contentObj.transform, "CHEATS", 18, FontStyle.Bold, CheatColor, 25f);

      // God Mode toggle
      _godModeToggle = CreateToggle(contentObj.transform, "God Mode (No Penalties)", DebugService.GodModeEnabled, (v) =>
      {
        DebugService.SetGodMode(v);
      });

      CreateSpacer(contentObj.transform, 5f);

      // Cheat buttons
      CreateButton(contentObj.transform, "+1000 Money (F5)", ButtonColor, () => DebugService.AddMoney(1000));
      CreateButton(contentObj.transform, "+10000 Money", ButtonColor, () => DebugService.AddMoney(10000));
      CreateButton(contentObj.transform, "Complete Quests (F6)", ButtonColor, () => DebugService.CompleteAllQuests());
      CreateButton(contentObj.transform, "Skip Day (F8)", ButtonColor, () => DebugService.SkipToDay(GameStateService.Instance.DayNumber + 1));
      CreateButton(contentObj.transform, "Skip to Day 10", ButtonColor, () => DebugService.SkipToDay(10));
      CreateButton(contentObj.transform, "Max All Upgrades", ButtonColor, () => DebugService.MaxUpgrades());

      CreateSpacer(contentObj.transform, 10f);

      // Danger zone
      CreateLabel(contentObj.transform, "DANGER ZONE", 16, FontStyle.Bold, DangerColor, 22f);
      CreateButton(contentObj.transform, "Reset Save", DangerColor, () => DebugService.ResetSave());

      CreateSpacer(contentObj.transform, 10f);

      // Hint
      CreateLabel(contentObj.transform, "Press F1 to close", 12, FontStyle.Italic, Color.gray, 20f);
    }

    private Text CreateLabel(Transform parent, string text, int fontSize, FontStyle style, Color color, float height)
    {
      GameObject obj = new GameObject("Label");
      obj.transform.SetParent(parent, false);

      Text label = obj.AddComponent<Text>();
      label.text = text;
      label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      label.fontSize = fontSize;
      label.fontStyle = style;
      label.color = color;
      label.alignment = TextAnchor.MiddleCenter;
      label.supportRichText = true;

      LayoutElement layout = obj.AddComponent<LayoutElement>();
      layout.preferredHeight = height;

      return label;
    }

    private void CreateButton(Transform parent, string label, Color color, System.Action onClick)
    {
      GameObject obj = new GameObject("Button_" + label);
      obj.transform.SetParent(parent, false);

      Image bg = obj.AddComponent<Image>();
      bg.color = color;

      Button btn = obj.AddComponent<Button>();
      btn.targetGraphic = bg;

      ColorBlock colors = btn.colors;
      colors.highlightedColor = color * 1.2f;
      colors.pressedColor = color * 0.8f;
      btn.colors = colors;

      btn.onClick.AddListener(() => onClick?.Invoke());

      LayoutElement layout = obj.AddComponent<LayoutElement>();
      layout.preferredHeight = 32f;

      // Text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(obj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 14;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;
    }

    private Toggle CreateToggle(Transform parent, string label, bool initialValue, System.Action<bool> onChanged)
    {
      GameObject obj = new GameObject("Toggle_" + label);
      obj.transform.SetParent(parent, false);

      LayoutElement layout = obj.AddComponent<LayoutElement>();
      layout.preferredHeight = 30f;

      // Background
      Image bg = obj.AddComponent<Image>();
      bg.color = new Color(0.2f, 0.2f, 0.25f);

      // Checkmark area
      GameObject checkArea = new GameObject("CheckArea");
      checkArea.transform.SetParent(obj.transform, false);

      Image checkBg = checkArea.AddComponent<Image>();
      checkBg.color = initialValue ? CheatColor : new Color(0.3f, 0.3f, 0.35f);

      RectTransform checkRect = checkArea.GetComponent<RectTransform>();
      checkRect.anchorMin = new Vector2(0f, 0.15f);
      checkRect.anchorMax = new Vector2(0.12f, 0.85f);
      checkRect.offsetMin = new Vector2(8f, 0f);
      checkRect.offsetMax = new Vector2(0f, 0f);

      // Checkmark text
      GameObject checkText = new GameObject("Checkmark");
      checkText.transform.SetParent(checkArea.transform, false);

      Text check = checkText.AddComponent<Text>();
      check.text = initialValue ? "✓" : "";
      check.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      check.fontSize = 16;
      check.color = Color.white;
      check.alignment = TextAnchor.MiddleCenter;

      RectTransform checkTextRect = checkText.GetComponent<RectTransform>();
      checkTextRect.anchorMin = Vector2.zero;
      checkTextRect.anchorMax = Vector2.one;
      checkTextRect.offsetMin = Vector2.zero;
      checkTextRect.offsetMax = Vector2.zero;

      // Label
      GameObject labelObj = new GameObject("Label");
      labelObj.transform.SetParent(obj.transform, false);

      Text labelText = labelObj.AddComponent<Text>();
      labelText.text = label;
      labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      labelText.fontSize = 14;
      labelText.color = Color.white;
      labelText.alignment = TextAnchor.MiddleLeft;

      RectTransform labelRect = labelObj.GetComponent<RectTransform>();
      labelRect.anchorMin = new Vector2(0.15f, 0f);
      labelRect.anchorMax = new Vector2(1f, 1f);
      labelRect.offsetMin = Vector2.zero;
      labelRect.offsetMax = new Vector2(-5f, 0f);

      // Toggle component
      Toggle toggle = obj.AddComponent<Toggle>();
      toggle.isOn = initialValue;
      toggle.graphic = checkBg;

      toggle.onValueChanged.AddListener((v) =>
      {
        check.text = v ? "✓" : "";
        checkBg.color = v ? CheatColor : new Color(0.3f, 0.3f, 0.35f);
        onChanged?.Invoke(v);
      });

      return toggle;
    }

    private void CreateSpacer(Transform parent, float height)
    {
      GameObject obj = new GameObject("Spacer");
      obj.transform.SetParent(parent, false);

      LayoutElement layout = obj.AddComponent<LayoutElement>();
      layout.preferredHeight = height;
    }

    private void OnDestroy()
    {
      if (_instance == this)
        _instance = null;
    }
  }
}
#endif
