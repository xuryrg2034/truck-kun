using Code.Gameplay.Features.Economy.Services;
using Code.Hub;
using Code.Infrastructure;
using Code.Meta.Upgrades;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.HubUI
{
  public class HubMainUI : MonoBehaviour
  {
    private IMoneyService _moneyService;
    private IUpgradeService _upgradeService;

    private Canvas _canvas;
    private Text _balanceText;
    private GameObject _interactPrompt;
    private Text _promptText;

    // Panels
    private UpgradePanel _upgradePanel;
    private FoodPanel _foodPanel;
    private QuestBoardPanel _questBoardPanel;
    private HubPanelBase _currentPanel;

    public static HubMainUI Instance { get; private set; }

    public void Initialize(IMoneyService moneyService, IUpgradeService upgradeService)
    {
      Instance = this;
      _moneyService = moneyService;
      _upgradeService = upgradeService;

      CreateMainUI();
      CreatePanels();
    }

    private void CreateMainUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("HubMainCanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 100;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      CreateBalanceDisplay(canvasObj.transform);
      CreateInteractPrompt(canvasObj.transform);
      CreateHubTitle(canvasObj.transform);
    }

    private void CreateBalanceDisplay(Transform parent)
    {
      GameObject balanceObj = new GameObject("BalanceDisplay");
      balanceObj.transform.SetParent(parent, false);

      Image bg = balanceObj.AddComponent<Image>();
      bg.color = new Color(0f, 0f, 0f, 0.7f);

      RectTransform rect = balanceObj.GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(1f, 1f);
      rect.anchorMax = new Vector2(1f, 1f);
      rect.pivot = new Vector2(1f, 1f);
      rect.anchoredPosition = new Vector2(-20f, -20f);
      rect.sizeDelta = new Vector2(220f, 60f);

      // Icon
      GameObject iconObj = new GameObject("Icon");
      iconObj.transform.SetParent(balanceObj.transform, false);

      Text iconText = iconObj.AddComponent<Text>();
      iconText.text = "💰";
      iconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      iconText.fontSize = 32;
      iconText.alignment = TextAnchor.MiddleCenter;

      RectTransform iconRect = iconObj.GetComponent<RectTransform>();
      iconRect.anchorMin = new Vector2(0f, 0f);
      iconRect.anchorMax = new Vector2(0.25f, 1f);
      iconRect.offsetMin = Vector2.zero;
      iconRect.offsetMax = Vector2.zero;

      // Balance text
      GameObject textObj = new GameObject("BalanceText");
      textObj.transform.SetParent(balanceObj.transform, false);

      _balanceText = textObj.AddComponent<Text>();
      _balanceText.text = "0¥";
      _balanceText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _balanceText.fontSize = 36;
      _balanceText.fontStyle = FontStyle.Bold;
      _balanceText.color = new Color(1f, 0.85f, 0.2f);
      _balanceText.alignment = TextAnchor.MiddleRight;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = new Vector2(0.25f, 0f);
      textRect.anchorMax = new Vector2(1f, 1f);
      textRect.offsetMin = new Vector2(0f, 0f);
      textRect.offsetMax = new Vector2(-15f, 0f);
    }

    private void CreateInteractPrompt(Transform parent)
    {
      _interactPrompt = new GameObject("InteractPrompt");
      _interactPrompt.transform.SetParent(parent, false);

      Image bg = _interactPrompt.AddComponent<Image>();
      bg.color = new Color(0f, 0f, 0f, 0.8f);

      RectTransform rect = _interactPrompt.GetComponent<RectTransform>();
      rect.anchorMin = new Vector2(0.5f, 0f);
      rect.anchorMax = new Vector2(0.5f, 0f);
      rect.pivot = new Vector2(0.5f, 0f);
      rect.anchoredPosition = new Vector2(0f, 60f);
      rect.sizeDelta = new Vector2(450f, 70f);

      // Prompt text
      GameObject textObj = new GameObject("PromptText");
      textObj.transform.SetParent(_interactPrompt.transform, false);

      _promptText = textObj.AddComponent<Text>();
      _promptText.text = "";
      _promptText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _promptText.fontSize = 28;
      _promptText.color = Color.white;
      _promptText.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = new Vector2(10f, 5f);
      textRect.offsetMax = new Vector2(-10f, -5f);

      _interactPrompt.SetActive(false);
    }

    private void CreateHubTitle(Transform parent)
    {
      GameObject titleObj = new GameObject("HubTitle");
      titleObj.transform.SetParent(parent, false);

      Text titleText = titleObj.AddComponent<Text>();
      titleText.text = "ХАБ";
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 42;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = new Color(1f, 1f, 1f, 0.3f);
      titleText.alignment = TextAnchor.UpperLeft;

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 1f);
      titleRect.anchorMax = new Vector2(0f, 1f);
      titleRect.pivot = new Vector2(0f, 1f);
      titleRect.anchoredPosition = new Vector2(30f, -20f);
      titleRect.sizeDelta = new Vector2(200f, 50f);
    }

    private void CreatePanels()
    {
      // Upgrade panel
      GameObject upgradePanelObj = new GameObject("UpgradePanel");
      upgradePanelObj.transform.SetParent(transform, false);
      _upgradePanel = upgradePanelObj.AddComponent<UpgradePanel>();
      _upgradePanel.Initialize(_moneyService, _upgradeService);

      // Food panel
      GameObject foodPanelObj = new GameObject("FoodPanel");
      foodPanelObj.transform.SetParent(transform, false);
      _foodPanel = foodPanelObj.AddComponent<FoodPanel>();
      _foodPanel.Initialize(_moneyService);

      // Quest board panel
      GameObject questPanelObj = new GameObject("QuestBoardPanel");
      questPanelObj.transform.SetParent(transform, false);
      _questBoardPanel = questPanelObj.AddComponent<QuestBoardPanel>();
    }

    private void LateUpdate()
    {
      UpdateBalance();
    }

    private void UpdateBalance()
    {
      if (_balanceText != null)
      {
        // Use GameStateService as primary source
        _balanceText.text = $"{GameStateService.Instance.PlayerMoney}¥";
      }
    }

    public void ShowInteractPrompt(string text)
    {
      if (_interactPrompt != null && _promptText != null)
      {
        _promptText.text = text;
        _interactPrompt.SetActive(true);
      }
    }

    public void HideInteractPrompt()
    {
      if (_interactPrompt != null)
      {
        _interactPrompt.SetActive(false);
      }
    }

    public void ShowZonePanel(ZoneType zoneType)
    {
      CloseCurrentPanel();

      switch (zoneType)
      {
        case ZoneType.Food:
          _currentPanel = _foodPanel;
          _foodPanel.Show(OnPanelClosed);
          break;

        case ZoneType.Quests:
          _currentPanel = _questBoardPanel;
          _questBoardPanel.Show(OnPanelClosed);
          break;

        case ZoneType.Garage:
          _currentPanel = _upgradePanel;
          _upgradePanel.Show(OnPanelClosed);
          break;

        case ZoneType.StartDay:
          // Direct scene load without panel
          GameStateService.Instance.IncrementDay();
          SceneTransitionService.Instance.LoadGameplay();
          break;
      }
    }

    public void CloseCurrentPanel()
    {
      if (_currentPanel != null && _currentPanel.IsOpen)
      {
        _currentPanel.Close();
        _currentPanel = null;
      }
    }

    private void OnPanelClosed()
    {
      _currentPanel = null;
    }

    private void OnDestroy()
    {
      if (Instance == this)
        Instance = null;
    }
  }
}
