using System;
using System.Collections.Generic;
using Code.Audio;
using Code.Infrastructure;
using Code.Meta.Upgrades;
using Code.UI.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Hub
{
  /// <summary>
  /// Hub UI - upgrade shop and day management
  /// </summary>
  public class HubUI : MonoBehaviour
  {
    private static readonly Color AccentColor = new Color(0.2f, 0.65f, 0.3f);
    private static readonly Color ButtonColor = new Color(0.25f, 0.5f, 0.75f);
    private static readonly Color UpgradeColor = new Color(0.4f, 0.35f, 0.5f);
    private static readonly Color DisabledColor = new Color(0.3f, 0.3f, 0.35f);
    private static readonly Color DangerColor = new Color(0.75f, 0.25f, 0.25f);

    private Canvas _canvas;
    private Text _moneyText;
    private Text _dayText;
    private GameObject _upgradesContainer;
    private List<UpgradeUIItem> _upgradeItems = new();
    private SettingsPanel _settingsPanel;

    private void Start()
    {
      _settingsPanel = gameObject.AddComponent<SettingsPanel>();
      CreateUI();
      UpdateUI();

      // Play hub music
      Code.Audio.Audio.PlayMusic(MusicType.Hub);

      // Subscribe to money changes
      GameStateService.Instance.OnMoneyChanged += OnMoneyChanged;
    }

    private void OnDestroy()
    {
      if (GameStateService.Instance != null)
        GameStateService.Instance.OnMoneyChanged -= OnMoneyChanged;
    }

    private void CreateUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("HubCanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 100;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Background
      // CreateBackground(canvasObj.transform);
      

      // Header
      CreateHeader(canvasObj.transform);

      // Main content area
      CreateMainContent(canvasObj.transform);

      // Footer with Start Day button
      CreateFooter(canvasObj.transform);
    }

    // private void CreateBackground(Transform parent)
    // {
    //   GameObject bgObj = new GameObject("Background");
    //   bgObj.transform.SetParent(parent, false);
    //   bgObj.transform.SetAsFirstSibling();
    //
    //   Image bgImage = bgObj.AddComponent<Image>();
    //
    //   // Create gradient texture
    //   Texture2D gradientTex = new Texture2D(1, 256);
    //   for (int y = 0; y < 256; y++)
    //   {
    //     float t = y / 255f;
    //     Color color = Color.Lerp(
    //       new Color(0.03f, 0.02f, 0.05f),
    //       new Color(0.1f, 0.06f, 0.14f),
    //       t
    //     );
    //     gradientTex.SetPixel(0, y, color);
    //   }
    //   gradientTex.Apply();
    //
    //   bgImage.sprite = Sprite.Create(gradientTex, new Rect(0, 0, 1, 256), new Vector2(0.5f, 0.5f));
    //   bgImage.type = Image.Type.Sliced;
    //
    //   RectTransform bgRect = bgObj.GetComponent<RectTransform>();
    //   bgRect.anchorMin = Vector2.zero;
    //   bgRect.anchorMax = Vector2.one;
    //   bgRect.offsetMin = Vector2.zero;
    //   bgRect.offsetMax = Vector2.zero;
    // }

    private void CreateHeader(Transform parent)
    {
      GameObject headerObj = new GameObject("Header");
      headerObj.transform.SetParent(parent, false);

      Image headerBg = headerObj.AddComponent<Image>();
      headerBg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);

      RectTransform headerRect = headerObj.GetComponent<RectTransform>();
      headerRect.anchorMin = new Vector2(0f, 0.9f);
      headerRect.anchorMax = new Vector2(1f, 1f);
      headerRect.offsetMin = Vector2.zero;
      headerRect.offsetMax = Vector2.zero;

      // Title
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(headerObj.transform, false);

      Text titleText = titleObj.AddComponent<Text>();
      titleText.text = "ХАБ TRUCK-KUN";
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 36;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = AccentColor;
      titleText.alignment = TextAnchor.MiddleLeft;

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 0f);
      titleRect.anchorMax = new Vector2(0.4f, 1f);
      titleRect.offsetMin = new Vector2(40f, 0f);
      titleRect.offsetMax = Vector2.zero;

      // Day info
      GameObject dayObj = new GameObject("DayText");
      dayObj.transform.SetParent(headerObj.transform, false);

      _dayText = dayObj.AddComponent<Text>();
      _dayText.text = "День 1";
      _dayText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _dayText.fontSize = 28;
      _dayText.color = Color.white;
      _dayText.alignment = TextAnchor.MiddleCenter;

      RectTransform dayRect = dayObj.GetComponent<RectTransform>();
      dayRect.anchorMin = new Vector2(0.4f, 0f);
      dayRect.anchorMax = new Vector2(0.6f, 1f);
      dayRect.offsetMin = Vector2.zero;
      dayRect.offsetMax = Vector2.zero;

      // Money display
      GameObject moneyObj = new GameObject("MoneyText");
      moneyObj.transform.SetParent(headerObj.transform, false);

      _moneyText = moneyObj.AddComponent<Text>();
      _moneyText.text = "1000 ¥";
      _moneyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _moneyText.fontSize = 32;
      _moneyText.fontStyle = FontStyle.Bold;
      _moneyText.color = new Color(1f, 0.85f, 0.2f);
      _moneyText.alignment = TextAnchor.MiddleRight;

      RectTransform moneyRect = moneyObj.GetComponent<RectTransform>();
      moneyRect.anchorMin = new Vector2(0.6f, 0f);
      moneyRect.anchorMax = new Vector2(1f, 1f);
      moneyRect.offsetMin = Vector2.zero;
      moneyRect.offsetMax = new Vector2(-40f, 0f);
    }

    private void CreateMainContent(Transform parent)
    {
      GameObject contentObj = new GameObject("Content");
      contentObj.transform.SetParent(parent, false);

      RectTransform contentRect = contentObj.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0f, 0.15f);
      contentRect.anchorMax = new Vector2(1f, 0.88f);
      contentRect.offsetMin = new Vector2(40f, 20f);
      contentRect.offsetMax = new Vector2(-40f, -20f);

      // Section title
      GameObject sectionTitleObj = new GameObject("SectionTitle");
      sectionTitleObj.transform.SetParent(contentObj.transform, false);

      Text sectionTitle = sectionTitleObj.AddComponent<Text>();
      sectionTitle.text = "УЛУЧШЕНИЯ";
      sectionTitle.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      sectionTitle.fontSize = 28;
      sectionTitle.fontStyle = FontStyle.Bold;
      sectionTitle.color = Color.white;
      sectionTitle.alignment = TextAnchor.MiddleLeft;

      RectTransform sectionTitleRect = sectionTitleObj.GetComponent<RectTransform>();
      sectionTitleRect.anchorMin = new Vector2(0f, 0.92f);
      sectionTitleRect.anchorMax = new Vector2(1f, 1f);
      sectionTitleRect.offsetMin = Vector2.zero;
      sectionTitleRect.offsetMax = Vector2.zero;

      // Upgrades container
      _upgradesContainer = new GameObject("UpgradesContainer");
      _upgradesContainer.transform.SetParent(contentObj.transform, false);

      RectTransform upgradesRect = _upgradesContainer.AddComponent<RectTransform>();
      upgradesRect.anchorMin = new Vector2(0f, 0f);
      upgradesRect.anchorMax = new Vector2(1f, 0.9f);
      upgradesRect.offsetMin = Vector2.zero;
      upgradesRect.offsetMax = Vector2.zero;

      GridLayoutGroup grid = _upgradesContainer.AddComponent<GridLayoutGroup>();
      grid.cellSize = new Vector2(400f, 130f);
      grid.spacing = new Vector2(30f, 20f);
      grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
      grid.startAxis = GridLayoutGroup.Axis.Horizontal;
      grid.childAlignment = TextAnchor.UpperCenter;
      grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
      grid.constraintCount = 3;

      // Create upgrade items
      CreateUpgradeItems();
    }

    private void CreateUpgradeItems()
    {
      // Define available upgrades (using existing UpgradeType enum)
      var upgrades = new[]
      {
        (UpgradeType.SpeedBoost, "СКОРОСТЬ", "Увеличивает скорость грузовика", 200, 1.2f),
        (UpgradeType.Maneuverability, "МАНЁВРЕННОСТЬ", "Улучшает управляемость", 250, 1.15f),
        (UpgradeType.MoneyMultiplier, "ДОХОД", "Увеличивает награду x1.1", 300, 1.25f),
      };

      foreach (var (type, name, desc, baseCost, costMultiplier) in upgrades)
      {
        CreateUpgradeItem(type, name, desc, baseCost, costMultiplier);
      }
    }

    private void CreateUpgradeItem(UpgradeType type, string name, string description, int baseCost, float costMultiplier)
    {
      GameObject itemObj = new GameObject($"Upgrade_{type}");
      itemObj.transform.SetParent(_upgradesContainer.transform, false);

      Image itemBg = itemObj.AddComponent<Image>();
      itemBg.color = UpgradeColor;

      // Name
      GameObject nameObj = new GameObject("Name");
      nameObj.transform.SetParent(itemObj.transform, false);

      Text nameText = nameObj.AddComponent<Text>();
      nameText.text = name;
      nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      nameText.fontSize = 22;
      nameText.fontStyle = FontStyle.Bold;
      nameText.color = Color.white;
      nameText.alignment = TextAnchor.MiddleLeft;

      RectTransform nameRect = nameObj.GetComponent<RectTransform>();
      nameRect.anchorMin = new Vector2(0f, 0.65f);
      nameRect.anchorMax = new Vector2(0.7f, 1f);
      nameRect.offsetMin = new Vector2(15f, 0f);
      nameRect.offsetMax = new Vector2(-5f, -10f);

      // Level
      GameObject levelObj = new GameObject("Level");
      levelObj.transform.SetParent(itemObj.transform, false);

      Text levelText = levelObj.AddComponent<Text>();
      levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      levelText.fontSize = 18;
      levelText.color = AccentColor;
      levelText.alignment = TextAnchor.MiddleRight;

      RectTransform levelRect = levelObj.GetComponent<RectTransform>();
      levelRect.anchorMin = new Vector2(0.7f, 0.65f);
      levelRect.anchorMax = new Vector2(1f, 1f);
      levelRect.offsetMin = new Vector2(0f, 0f);
      levelRect.offsetMax = new Vector2(-15f, -10f);

      // Description
      GameObject descObj = new GameObject("Description");
      descObj.transform.SetParent(itemObj.transform, false);

      Text descText = descObj.AddComponent<Text>();
      descText.text = description;
      descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      descText.fontSize = 14;
      descText.color = new Color(0.7f, 0.7f, 0.75f);
      descText.alignment = TextAnchor.UpperLeft;

      RectTransform descRect = descObj.GetComponent<RectTransform>();
      descRect.anchorMin = new Vector2(0f, 0.35f);
      descRect.anchorMax = new Vector2(1f, 0.65f);
      descRect.offsetMin = new Vector2(15f, 0f);
      descRect.offsetMax = new Vector2(-15f, 0f);

      // Buy button
      GameObject buyBtnObj = new GameObject("BuyButton");
      buyBtnObj.transform.SetParent(itemObj.transform, false);

      Image buyBtnBg = buyBtnObj.AddComponent<Image>();
      buyBtnBg.color = AccentColor;

      Button buyBtn = buyBtnObj.AddComponent<Button>();
      buyBtn.targetGraphic = buyBtnBg;

      RectTransform buyBtnRect = buyBtnObj.GetComponent<RectTransform>();
      buyBtnRect.anchorMin = new Vector2(0.05f, 0.05f);
      buyBtnRect.anchorMax = new Vector2(0.95f, 0.32f);
      buyBtnRect.offsetMin = Vector2.zero;
      buyBtnRect.offsetMax = Vector2.zero;

      // Button text
      GameObject btnTextObj = new GameObject("Text");
      btnTextObj.transform.SetParent(buyBtnObj.transform, false);

      Text btnText = btnTextObj.AddComponent<Text>();
      btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      btnText.fontSize = 16;
      btnText.fontStyle = FontStyle.Bold;
      btnText.color = Color.white;
      btnText.alignment = TextAnchor.MiddleCenter;

      RectTransform btnTextRect = btnTextObj.GetComponent<RectTransform>();
      btnTextRect.anchorMin = Vector2.zero;
      btnTextRect.anchorMax = Vector2.one;
      btnTextRect.offsetMin = Vector2.zero;
      btnTextRect.offsetMax = Vector2.zero;

      // Store upgrade item data
      var item = new UpgradeUIItem
      {
        Type = type,
        BaseCost = baseCost,
        CostMultiplier = costMultiplier,
        LevelText = levelText,
        ButtonText = btnText,
        ButtonImage = buyBtnBg,
        Button = buyBtn
      };
      _upgradeItems.Add(item);

      // Setup button click
      buyBtn.onClick.AddListener(() => OnUpgradeClicked(item));
    }

    private void CreateFooter(Transform parent)
    {
      GameObject footerObj = new GameObject("Footer");
      footerObj.transform.SetParent(parent, false);

      Image footerBg = footerObj.AddComponent<Image>();
      footerBg.color = new Color(0.05f, 0.05f, 0.08f, 0.9f);

      RectTransform footerRect = footerObj.GetComponent<RectTransform>();
      footerRect.anchorMin = new Vector2(0f, 0f);
      footerRect.anchorMax = new Vector2(1f, 0.12f);
      footerRect.offsetMin = Vector2.zero;
      footerRect.offsetMax = Vector2.zero;

      // Back to menu button
      CreateFooterButton(footerObj.transform, "В МЕНЮ", DangerColor, new Vector2(0.02f, 0.15f), new Vector2(0.18f, 0.85f), OnBackToMenuClicked);

      // Settings button
      CreateFooterButton(footerObj.transform, "НАСТРОЙКИ", UpgradeColor, new Vector2(0.2f, 0.15f), new Vector2(0.36f, 0.85f), OnSettingsClicked);

      // Start Day button (large, prominent)
      CreateFooterButton(footerObj.transform, "НАЧАТЬ ДЕНЬ", AccentColor, new Vector2(0.5f, 0.1f), new Vector2(0.98f, 0.9f), OnStartDayClicked, 28);
    }

    private void CreateFooterButton(Transform parent, string label, Color color, Vector2 anchorMin, Vector2 anchorMax, Action onClick, int fontSize = 20)
    {
      GameObject btnObj = new GameObject("Button_" + label);
      btnObj.transform.SetParent(parent, false);

      Image btnBg = btnObj.AddComponent<Image>();
      btnBg.color = color;

      Button btn = btnObj.AddComponent<Button>();
      btn.targetGraphic = btnBg;

      ColorBlock colors = btn.colors;
      colors.normalColor = color;
      colors.highlightedColor = color * 1.2f;
      colors.pressedColor = color * 0.8f;
      btn.colors = colors;

      btn.onClick.AddListener(() => onClick?.Invoke());

      RectTransform btnRect = btnObj.GetComponent<RectTransform>();
      btnRect.anchorMin = anchorMin;
      btnRect.anchorMax = anchorMax;
      btnRect.offsetMin = Vector2.zero;
      btnRect.offsetMax = Vector2.zero;

      // Text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(btnObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = fontSize;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      Shadow shadow = textObj.AddComponent<Shadow>();
      shadow.effectColor = new Color(0, 0, 0, 0.5f);
      shadow.effectDistance = new Vector2(2, -2);

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;
    }

    private void UpdateUI()
    {
      GameStateService state = GameStateService.Instance;

      _dayText.text = $"День {state.DayNumber}";
      _moneyText.text = $"{state.PlayerMoney} ¥";

      // Update upgrade items
      foreach (var item in _upgradeItems)
      {
        int level = state.GetUpgradeLevel(item.Type);
        int cost = CalculateUpgradeCost(item.BaseCost, item.CostMultiplier, level);
        bool canAfford = state.PlayerMoney >= cost;

        item.LevelText.text = $"Ур. {level}";
        item.ButtonText.text = $"КУПИТЬ: {cost} ¥";

        item.Button.interactable = canAfford;
        item.ButtonImage.color = canAfford ? AccentColor : DisabledColor;
      }
    }

    private int CalculateUpgradeCost(int baseCost, float multiplier, int currentLevel)
    {
      return Mathf.RoundToInt(baseCost * Mathf.Pow(multiplier, currentLevel));
    }

    private void OnMoneyChanged(int newAmount)
    {
      UpdateUI();
    }

    private void OnUpgradeClicked(UpgradeUIItem item)
    {
      GameStateService state = GameStateService.Instance;
      int level = state.GetUpgradeLevel(item.Type);
      int cost = CalculateUpgradeCost(item.BaseCost, item.CostMultiplier, level);

      if (state.SpendMoney(cost))
      {
        state.SetUpgradeLevel(item.Type, level + 1);
        state.Save();
        UpdateUI();
        Debug.Log($"[Hub] Upgraded {item.Type} to level {level + 1}");
      }
    }

    private void OnBackToMenuClicked()
    {
      GameStateService.Instance.Save();
      SceneTransitionService.Instance.LoadMainMenu();
    }

    private void OnSettingsClicked()
    {
      _settingsPanel.Show(_canvas.transform);
    }

    private void OnStartDayClicked()
    {
      GameStateService.Instance.Save();
      SceneTransitionService.Instance.LoadGameplay();
    }

    private class UpgradeUIItem
    {
      public UpgradeType Type;
      public int BaseCost;
      public float CostMultiplier;
      public Text LevelText;
      public Text ButtonText;
      public Image ButtonImage;
      public Button Button;
    }
  }
}
