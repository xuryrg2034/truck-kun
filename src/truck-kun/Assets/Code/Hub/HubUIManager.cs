using System.Collections.Generic;
using Code.Gameplay.Features.Economy.Services;
using Code.Meta.Upgrades;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Code.Hub
{
  public class HubUIManager : MonoBehaviour
  {
    public static HubUIManager Instance { get; private set; }

    private Canvas _canvas;
    private Text _interactPrompt;
    private Text _balanceText;
    private GameObject _currentPanel;

    private IMoneyService _moneyService;
    private IUpgradeService _upgradeService;

    public void Initialize(IMoneyService moneyService, IUpgradeService upgradeService = null)
    {
      Instance = this;
      _moneyService = moneyService;
      _upgradeService = upgradeService;
      CreateUI();
    }

    private void CreateUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("HubUICanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 100;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Balance display (top right)
      CreateBalanceDisplay(canvasObj.transform);

      // Interact prompt (bottom center)
      CreateInteractPrompt(canvasObj.transform);
    }

    private void CreateBalanceDisplay(Transform parent)
    {
      GameObject balanceObj = new GameObject("Balance");
      balanceObj.transform.SetParent(parent, false);

      Image bg = balanceObj.AddComponent<Image>();
      bg.color = new Color(0f, 0f, 0f, 0.6f);

      RectTransform rect = balanceObj.GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(1f, 1f);
      rect.anchorMax = new Vector2(1f, 1f);
      rect.pivot = new Vector2(1f, 1f);
      rect.anchoredPosition = new Vector2(-20f, -20f);
      rect.sizeDelta = new Vector2(200f, 50f);

      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(balanceObj.transform, false);

      _balanceText = textObj.AddComponent<Text>();
      _balanceText.text = "0¥";
      _balanceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _balanceText.fontSize = 32;
      _balanceText.color = new Color(1f, 0.85f, 0.2f);
      _balanceText.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;
    }

    private void CreateInteractPrompt(Transform parent)
    {
      GameObject promptObj = new GameObject("InteractPrompt");
      promptObj.transform.SetParent(parent, false);

      Image bg = promptObj.AddComponent<Image>();
      bg.color = new Color(0f, 0f, 0f, 0.7f);

      RectTransform rect = promptObj.GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(0.5f, 0f);
      rect.anchorMax = new Vector2(0.5f, 0f);
      rect.pivot = new Vector2(0.5f, 0f);
      rect.anchoredPosition = new Vector2(0f, 50f);
      rect.sizeDelta = new Vector2(400f, 60f);

      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(promptObj.transform, false);

      _interactPrompt = textObj.AddComponent<Text>();
      _interactPrompt.text = "";
      _interactPrompt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _interactPrompt.fontSize = 28;
      _interactPrompt.color = Color.white;
      _interactPrompt.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;

      promptObj.SetActive(false);
    }

    private void LateUpdate()
    {
      if (_moneyService != null && _balanceText != null)
      {
        _balanceText.text = $"{_moneyService.Balance}¥";
      }
    }

    public void ShowInteractPrompt(string text)
    {
      if (_interactPrompt != null)
      {
        _interactPrompt.text = text;
        _interactPrompt.transform.parent.gameObject.SetActive(true);
      }
    }

    public void HideInteractPrompt()
    {
      if (_interactPrompt != null)
      {
        _interactPrompt.transform.parent.gameObject.SetActive(false);
      }
    }

    public void ShowZonePanel(ZoneType type)
    {
      CloseCurrentPanel();

      switch (type)
      {
        case ZoneType.Food:
          ShowFoodPanel();
          break;
        case ZoneType.Quests:
          ShowQuestsPanel();
          break;
        case ZoneType.Garage:
          ShowGaragePanel();
          break;
        case ZoneType.StartDay:
          StartDay();
          break;
      }
    }

    public void CloseCurrentPanel()
    {
      if (_currentPanel != null)
      {
        Destroy(_currentPanel);
        _currentPanel = null;
      }
    }

    private void ShowFoodPanel()
    {
      _currentPanel = CreateSimplePanel("FOOD SHOP", "Buy food to restore energy\n\n(Coming soon)", new Color(0.8f, 0.5f, 0.2f));
    }

    private void ShowQuestsPanel()
    {
      _currentPanel = CreateSimplePanel("QUEST BOARD", "View available missions\n\n(Coming soon)", new Color(0.2f, 0.6f, 0.8f));
    }

    private void ShowGaragePanel()
    {
      if (_upgradeService == null)
      {
        _currentPanel = CreateSimplePanel("ГАРАЖ", "Апгрейды недоступны", new Color(0.5f, 0.5f, 0.5f));
        return;
      }

      _currentPanel = CreateGaragePanel();
    }

    private GameObject CreateGaragePanel()
    {
      GameObject panel = new GameObject("GaragePanel");
      panel.transform.SetParent(_canvas.transform, false);

      Image panelBg = panel.AddComponent<Image>();
      panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

      RectTransform panelRect = panel.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(600f, 500f);

      // Title
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(panel.transform, false);

      Image titleBg = titleObj.AddComponent<Image>();
      titleBg.color = new Color(0.5f, 0.5f, 0.5f);

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 1f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.pivot = new Vector2(0.5f, 1f);
      titleRect.sizeDelta = new Vector2(0f, 60f);
      titleRect.anchoredPosition = Vector2.zero;

      GameObject titleTextObj = new GameObject("Text");
      titleTextObj.transform.SetParent(titleObj.transform, false);

      Text titleText = titleTextObj.AddComponent<Text>();
      titleText.text = "ГАРАЖ";
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 36;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = Color.white;
      titleText.alignment = TextAnchor.MiddleCenter;

      RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
      titleTextRect.anchorMin = Vector2.zero;
      titleTextRect.anchorMax = Vector2.one;
      titleTextRect.offsetMin = Vector2.zero;
      titleTextRect.offsetMax = Vector2.zero;

      // Upgrades container
      GameObject upgradesContainer = new GameObject("UpgradesContainer");
      upgradesContainer.transform.SetParent(panel.transform, false);

      RectTransform containerRect = upgradesContainer.AddComponent<RectTransform>();
      containerRect.anchorMin = new Vector2(0f, 0.15f);
      containerRect.anchorMax = new Vector2(1f, 0.88f);
      containerRect.offsetMin = new Vector2(20f, 0f);
      containerRect.offsetMax = new Vector2(-20f, 0f);

      VerticalLayoutGroup containerLayout = upgradesContainer.AddComponent<VerticalLayoutGroup>();
      containerLayout.spacing = 15f;
      containerLayout.padding = new RectOffset(10, 10, 10, 10);
      containerLayout.childAlignment = TextAnchor.UpperCenter;
      containerLayout.childControlWidth = true;
      containerLayout.childControlHeight = false;
      containerLayout.childForceExpandWidth = true;

      // Create upgrade items
      IReadOnlyList<UpgradeInfo> upgrades = _upgradeService.GetAllUpgrades();
      foreach (UpgradeInfo upgrade in upgrades)
      {
        CreateUpgradeItem(upgradesContainer.transform, upgrade);
      }

      // Close button
      CreateCloseButton(panel.transform);

      return panel;
    }

    private void CreateUpgradeItem(Transform parent, UpgradeInfo upgrade)
    {
      GameObject itemObj = new GameObject($"Upgrade_{upgrade.Type}");
      itemObj.transform.SetParent(parent, false);

      Image itemBg = itemObj.AddComponent<Image>();
      itemBg.color = new Color(0.15f, 0.15f, 0.2f, 1f);

      LayoutElement layoutEl = itemObj.AddComponent<LayoutElement>();
      layoutEl.preferredHeight = 90f;

      // Name and description
      GameObject infoObj = new GameObject("Info");
      infoObj.transform.SetParent(itemObj.transform, false);

      RectTransform infoRect = infoObj.AddComponent<RectTransform>();
      infoRect.anchorMin = new Vector2(0f, 0f);
      infoRect.anchorMax = new Vector2(0.6f, 1f);
      infoRect.offsetMin = new Vector2(15f, 10f);
      infoRect.offsetMax = new Vector2(0f, -10f);

      VerticalLayoutGroup infoLayout = infoObj.AddComponent<VerticalLayoutGroup>();
      infoLayout.spacing = 5f;
      infoLayout.childAlignment = TextAnchor.MiddleLeft;
      infoLayout.childControlWidth = true;
      infoLayout.childControlHeight = false;

      // Name
      GameObject nameObj = new GameObject("Name");
      nameObj.transform.SetParent(infoObj.transform, false);

      Text nameText = nameObj.AddComponent<Text>();
      nameText.text = upgrade.Name;
      nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      nameText.fontSize = 24;
      nameText.fontStyle = FontStyle.Bold;
      nameText.color = Color.white;

      LayoutElement nameLayout = nameObj.AddComponent<LayoutElement>();
      nameLayout.preferredHeight = 28f;

      // Level
      GameObject levelObj = new GameObject("Level");
      levelObj.transform.SetParent(infoObj.transform, false);

      Text levelText = levelObj.AddComponent<Text>();
      string levelStr = upgrade.IsMaxed ? "MAX" : $"Ур. {upgrade.CurrentLevel}/{upgrade.MaxLevel}";
      string bonusStr = upgrade.CurrentBonus > 0 ? $" (+{upgrade.CurrentBonus * 100:0}%)" : "";
      levelText.text = $"{levelStr}{bonusStr}";
      levelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      levelText.fontSize = 18;
      levelText.color = upgrade.IsMaxed ? new Color(1f, 0.85f, 0.2f) : new Color(0.7f, 0.7f, 0.7f);

      LayoutElement levelLayout = levelObj.AddComponent<LayoutElement>();
      levelLayout.preferredHeight = 22f;

      // Description
      GameObject descObj = new GameObject("Desc");
      descObj.transform.SetParent(infoObj.transform, false);

      Text descText = descObj.AddComponent<Text>();
      descText.text = upgrade.Description;
      descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      descText.fontSize = 14;
      descText.color = new Color(0.6f, 0.6f, 0.6f);

      LayoutElement descLayout = descObj.AddComponent<LayoutElement>();
      descLayout.preferredHeight = 18f;

      // Buy button
      if (!upgrade.IsMaxed)
      {
        GameObject buttonObj = new GameObject("BuyButton");
        buttonObj.transform.SetParent(itemObj.transform, false);

        Image buttonBg = buttonObj.AddComponent<Image>();
        bool canAfford = _moneyService.Balance >= upgrade.NextCost;
        buttonBg.color = canAfford ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.4f, 0.4f, 0.4f);

        Button button = buttonObj.AddComponent<Button>();
        button.targetGraphic = buttonBg;
        button.interactable = canAfford;

        UpgradeType type = upgrade.Type;
        button.onClick.AddListener(() => OnUpgradePurchase(type));

        RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.65f, 0.2f);
        buttonRect.anchorMax = new Vector2(0.95f, 0.8f);
        buttonRect.offsetMin = Vector2.zero;
        buttonRect.offsetMax = Vector2.zero;

        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);

        Text buttonText = buttonTextObj.AddComponent<Text>();
        buttonText.text = $"{upgrade.NextCost}¥";
        buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        buttonText.fontSize = 22;
        buttonText.fontStyle = FontStyle.Bold;
        buttonText.color = Color.white;
        buttonText.alignment = TextAnchor.MiddleCenter;

        RectTransform buttonTextRect = buttonTextObj.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
      }
      else
      {
        // Max level indicator
        GameObject maxObj = new GameObject("MaxIndicator");
        maxObj.transform.SetParent(itemObj.transform, false);

        Text maxText = maxObj.AddComponent<Text>();
        maxText.text = "MAX";
        maxText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        maxText.fontSize = 24;
        maxText.fontStyle = FontStyle.Bold;
        maxText.color = new Color(1f, 0.85f, 0.2f);
        maxText.alignment = TextAnchor.MiddleCenter;

        RectTransform maxRect = maxObj.GetComponent<RectTransform>();
        maxRect.anchorMin = new Vector2(0.65f, 0.2f);
        maxRect.anchorMax = new Vector2(0.95f, 0.8f);
        maxRect.offsetMin = Vector2.zero;
        maxRect.offsetMax = Vector2.zero;
      }
    }

    private void OnUpgradePurchase(UpgradeType type)
    {
      if (_upgradeService == null)
        return;

      if (_upgradeService.PurchaseUpgrade(type))
      {
        // Refresh garage panel
        CloseCurrentPanel();
        _currentPanel = CreateGaragePanel();
      }
    }

    private void StartDay()
    {
      SceneManager.LoadScene("GameScene");
    }

    private GameObject CreateSimplePanel(string title, string description, Color accentColor)
    {
      GameObject panel = new GameObject("Panel");
      panel.transform.SetParent(_canvas.transform, false);

      Image panelBg = panel.AddComponent<Image>();
      panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

      RectTransform panelRect = panel.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(500f, 400f);

      // Title
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(panel.transform, false);

      Image titleBg = titleObj.AddComponent<Image>();
      titleBg.color = accentColor;

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 1f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.pivot = new Vector2(0.5f, 1f);
      titleRect.sizeDelta = new Vector2(0f, 60f);
      titleRect.anchoredPosition = Vector2.zero;

      GameObject titleTextObj = new GameObject("Text");
      titleTextObj.transform.SetParent(titleObj.transform, false);

      Text titleText = titleTextObj.AddComponent<Text>();
      titleText.text = title;
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 36;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = Color.white;
      titleText.alignment = TextAnchor.MiddleCenter;

      RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
      titleTextRect.anchorMin = Vector2.zero;
      titleTextRect.anchorMax = Vector2.one;
      titleTextRect.offsetMin = Vector2.zero;
      titleTextRect.offsetMax = Vector2.zero;

      // Description
      GameObject descObj = new GameObject("Description");
      descObj.transform.SetParent(panel.transform, false);

      Text descText = descObj.AddComponent<Text>();
      descText.text = description;
      descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      descText.fontSize = 24;
      descText.color = Color.white;
      descText.alignment = TextAnchor.MiddleCenter;

      RectTransform descRect = descObj.GetComponent<RectTransform>();
      descRect.anchorMin = new Vector2(0f, 0.2f);
      descRect.anchorMax = new Vector2(1f, 0.85f);
      descRect.offsetMin = new Vector2(20f, 0f);
      descRect.offsetMax = new Vector2(-20f, 0f);

      // Close button
      CreateCloseButton(panel.transform);

      return panel;
    }

    private void CreateCloseButton(Transform parent)
    {
      GameObject buttonObj = new GameObject("CloseButton");
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = new Color(0.6f, 0.2f, 0.2f);

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;
      button.onClick.AddListener(CloseCurrentPanel);

      RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
      buttonRect.anchorMin = new Vector2(0.5f, 0f);
      buttonRect.anchorMax = new Vector2(0.5f, 0f);
      buttonRect.pivot = new Vector2(0.5f, 0f);
      buttonRect.anchoredPosition = new Vector2(0f, 20f);
      buttonRect.sizeDelta = new Vector2(150f, 50f);

      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = "CLOSE";
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 24;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;
    }

    private void OnDestroy()
    {
      if (Instance == this)
        Instance = null;
    }
  }
}
