using System.Collections.Generic;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest;
using Code.Infrastructure;
using Entitas;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.EndDayScreen
{
  public class EndDayController : MonoBehaviour
  {
    private Canvas _canvas;
    private GameObject _panel;
    private Text _titleText;
    private Transform _questsContainer;
    private Transform _violationsContainer;
    private Text _earnedText;
    private Text _penaltiesText;
    private Text _totalText;
    private Text _balanceText;
    private Button _continueButton;

    private MetaContext _meta;
    private IMoneyService _moneyService;
    private IQuestService _questService;

    private bool _isShown;

    private static readonly Color SuccessColor = new(0.2f, 0.9f, 0.2f, 1f);
    private static readonly Color FailColor = new(0.9f, 0.2f, 0.2f, 1f);
    private static readonly Color GoldColor = new(1f, 0.85f, 0.2f, 1f);

    public void Initialize(MetaContext meta, IMoneyService moneyService, IQuestService questService)
    {
      _meta = meta;
      _moneyService = moneyService;
      _questService = questService;

      CreateUI();
      Hide();
    }

    public void Show()
    {
      if (_isShown)
        return;

      _isShown = true;

      // Sync final balance to GameState
      SyncToGameState();

      PopulateData();
      _panel.SetActive(true);
    }

    private void SyncToGameState()
    {
      GameStateService state = GameStateService.Instance;

      // Update balance in persistent state
      state.PlayerMoney = _moneyService.Balance;

      // Track earnings history
      state.AddEarnings(_moneyService.EarnedToday, _moneyService.PenaltiesToday);

      // Save immediately
      state.Save();
    }

    public void Hide()
    {
      _isShown = false;
      if (_panel != null)
        _panel.SetActive(false);
    }

    private void CreateUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("EndDayCanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 1000;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Background Panel
      _panel = new GameObject("Panel");
      _panel.transform.SetParent(canvasObj.transform, false);

      Image panelBg = _panel.AddComponent<Image>();
      panelBg.color = new Color(0.05f, 0.05f, 0.1f, 0.95f);

      RectTransform panelRect = _panel.GetComponent<RectTransform>();
      panelRect.anchorMin = Vector2.zero;
      panelRect.anchorMax = Vector2.one;
      panelRect.offsetMin = Vector2.zero;
      panelRect.offsetMax = Vector2.zero;

      // Content Container
      GameObject content = new GameObject("Content");
      content.transform.SetParent(_panel.transform, false);

      RectTransform contentRect = content.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0.5f, 0.5f);
      contentRect.anchorMax = new Vector2(0.5f, 0.5f);
      contentRect.sizeDelta = new Vector2(600f, 700f);
      contentRect.anchoredPosition = Vector2.zero;

      VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
      contentLayout.spacing = 20f;
      contentLayout.padding = new RectOffset(30, 30, 30, 30);
      contentLayout.childAlignment = TextAnchor.UpperCenter;
      contentLayout.childControlWidth = true;
      contentLayout.childControlHeight = false;
      contentLayout.childForceExpandWidth = true;
      contentLayout.childForceExpandHeight = false;

      // Title with day number
      int dayNum = GameStateService.Instance.DayNumber;
      _titleText = CreateText(content.transform, $"ДЕНЬ {dayNum} ЗАВЕРШЁН", 48, FontStyle.Bold, Color.white);
      SetLayoutHeight(_titleText.gameObject, 60f);

      // Separator
      CreateSeparator(content.transform);

      // Quests Section
      Text questsHeader = CreateText(content.transform, "ВЫПОЛНЕННЫЕ ЗАДАНИЯ", 24, FontStyle.Bold, SuccessColor);
      SetLayoutHeight(questsHeader.gameObject, 35f);

      GameObject questsScroll = CreateScrollContainer(content.transform, "QuestsContainer", 120f);
      _questsContainer = questsScroll.transform.GetChild(0).GetChild(0);

      // Violations Section
      Text violationsHeader = CreateText(content.transform, "НАРУШЕНИЯ", 24, FontStyle.Bold, FailColor);
      SetLayoutHeight(violationsHeader.gameObject, 35f);

      GameObject violationsScroll = CreateScrollContainer(content.transform, "ViolationsContainer", 80f);
      _violationsContainer = violationsScroll.transform.GetChild(0).GetChild(0);

      // Separator
      CreateSeparator(content.transform);

      // Financial Summary
      GameObject financialSection = new GameObject("FinancialSection");
      financialSection.transform.SetParent(content.transform, false);

      VerticalLayoutGroup finLayout = financialSection.AddComponent<VerticalLayoutGroup>();
      finLayout.spacing = 8f;
      finLayout.childAlignment = TextAnchor.MiddleCenter;
      finLayout.childControlWidth = true;
      finLayout.childControlHeight = false;

      LayoutElement finLayoutEl = financialSection.AddComponent<LayoutElement>();
      finLayoutEl.preferredHeight = 120f;

      _earnedText = CreateText(financialSection.transform, "Заработано: +0¥", 28, FontStyle.Normal, SuccessColor);
      _penaltiesText = CreateText(financialSection.transform, "Штрафы: -0¥", 28, FontStyle.Normal, FailColor);

      CreateSeparator(financialSection.transform, 2f);

      _totalText = CreateText(financialSection.transform, "Итого: +0¥", 32, FontStyle.Bold, GoldColor);
      _balanceText = CreateText(financialSection.transform, "Баланс: 0¥", 36, FontStyle.Bold, Color.white);

      // Continue Button
      _continueButton = CreateButton(content.transform, "Вернуться в хаб", OnContinueClicked);
    }

    private void PopulateData()
    {
      ClearContainer(_questsContainer);
      ClearContainer(_violationsContainer);

      // Quests
      IReadOnlyList<QuestProgressInfo> quests = _questService.GetQuestProgress();

      foreach (QuestProgressInfo quest in quests)
      {
        if (quest.IsCompleted)
        {
          int reward = GetQuestReward(quest.QuestId);

          string typeLabel = quest.TargetType == PedestrianKind.Target ? "целей" : "NPC";
          string text = $"✓ Сбито {quest.RequiredCount} {typeLabel}  +{reward}¥";
          CreateText(_questsContainer, text, 20, FontStyle.Normal, SuccessColor);
        }
        else
        {
          string typeLabel = quest.TargetType == PedestrianKind.Target ? "целей" : "NPC";
          string text = $"✗ Сбито {quest.CurrentCount}/{quest.RequiredCount} {typeLabel}";
          CreateText(_questsContainer, text, 20, FontStyle.Normal, new Color(0.6f, 0.6f, 0.6f));
        }
      }

      if (quests.Count == 0)
      {
        CreateText(_questsContainer, "Нет заданий", 20, FontStyle.Italic, new Color(0.5f, 0.5f, 0.5f));
      }

      // Violations
      int penalties = _moneyService.PenaltiesToday;
      int violationCount = penalties > 0 ? Mathf.CeilToInt(penalties / 100f) : 0;

      if (violationCount > 0)
      {
        string text = $"✗ Сбито {violationCount} запрещённых NPC  -{penalties}¥";
        CreateText(_violationsContainer, text, 20, FontStyle.Normal, FailColor);
      }
      else
      {
        CreateText(_violationsContainer, "Нет нарушений - чистый заезд!", 20, FontStyle.Normal, new Color(0.5f, 0.8f, 0.5f));
      }

      // Financial Summary
      int earned = _moneyService.EarnedToday;
      int penaltiesAmount = _moneyService.PenaltiesToday;
      int total = earned - penaltiesAmount;
      int balance = _moneyService.Balance;

      _earnedText.text = $"Заработано: +{earned}¥";
      _penaltiesText.text = $"Штрафы: -{penaltiesAmount}¥";

      _totalText.text = total >= 0 ? $"Итого: +{total}¥" : $"Итого: {total}¥";
      _totalText.color = total >= 0 ? SuccessColor : FailColor;

      _balanceText.text = $"Баланс: {balance}¥";
    }

    private int GetQuestReward(int questId)
    {
      MetaEntity quest = _meta.GetEntityWithId(questId);
      if (quest != null && quest.hasDailyQuest)
        return quest.dailyQuest.Reward;

      return 0;
    }

    private void OnContinueClicked()
    {
      // State already synced in Show(), use transition service
      SceneTransitionService.Instance.LoadHub();
    }

    private Text CreateText(Transform parent, string content, int fontSize, FontStyle style, Color color)
    {
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(parent, false);

      Text text = textObj.AddComponent<Text>();
      text.text = content;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = fontSize;
      text.fontStyle = style;
      text.color = color;
      text.alignment = TextAnchor.MiddleCenter;

      LayoutElement layout = textObj.AddComponent<LayoutElement>();
      layout.preferredHeight = fontSize + 10;

      return text;
    }

    private void CreateSeparator(Transform parent, float height = 3f)
    {
      GameObject sep = new GameObject("Separator");
      sep.transform.SetParent(parent, false);

      Image sepImage = sep.AddComponent<Image>();
      sepImage.color = new Color(1f, 1f, 1f, 0.2f);

      LayoutElement layout = sep.AddComponent<LayoutElement>();
      layout.preferredHeight = height;
      layout.flexibleWidth = 1f;
    }

    private GameObject CreateScrollContainer(Transform parent, string name, float height)
    {
      GameObject container = new GameObject(name);
      container.transform.SetParent(parent, false);

      Image containerBg = container.AddComponent<Image>();
      containerBg.color = new Color(0f, 0f, 0f, 0.3f);

      LayoutElement containerLayout = container.AddComponent<LayoutElement>();
      containerLayout.preferredHeight = height;
      containerLayout.flexibleWidth = 1f;

      // Viewport
      GameObject viewport = new GameObject("Viewport");
      viewport.transform.SetParent(container.transform, false);

      Image viewportImage = viewport.AddComponent<Image>();
      viewportImage.color = Color.clear;

      Mask mask = viewport.AddComponent<Mask>();
      mask.showMaskGraphic = false;

      RectTransform viewportRect = viewport.GetComponent<RectTransform>();
      viewportRect.anchorMin = Vector2.zero;
      viewportRect.anchorMax = Vector2.one;
      viewportRect.offsetMin = new Vector2(10, 5);
      viewportRect.offsetMax = new Vector2(-10, -5);

      // Content
      GameObject content = new GameObject("Content");
      content.transform.SetParent(viewport.transform, false);

      RectTransform contentRect = content.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0, 1);
      contentRect.anchorMax = new Vector2(1, 1);
      contentRect.pivot = new Vector2(0.5f, 1f);
      contentRect.anchoredPosition = Vector2.zero;

      VerticalLayoutGroup contentLayout = content.AddComponent<VerticalLayoutGroup>();
      contentLayout.spacing = 5f;
      contentLayout.childAlignment = TextAnchor.UpperLeft;
      contentLayout.childControlWidth = true;
      contentLayout.childControlHeight = false;
      contentLayout.childForceExpandWidth = true;

      ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
      fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

      // ScrollRect
      ScrollRect scroll = container.AddComponent<ScrollRect>();
      scroll.content = contentRect;
      scroll.viewport = viewportRect;
      scroll.horizontal = false;
      scroll.vertical = true;
      scroll.scrollSensitivity = 20f;

      return container;
    }

    private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
      GameObject buttonObj = new GameObject("Button");
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = new Color(0.2f, 0.5f, 0.8f, 1f);

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;

      ColorBlock colors = button.colors;
      colors.normalColor = new Color(0.2f, 0.5f, 0.8f);
      colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f);
      colors.pressedColor = new Color(0.15f, 0.4f, 0.7f);
      button.colors = colors;

      button.onClick.AddListener(onClick);

      LayoutElement buttonLayout = buttonObj.AddComponent<LayoutElement>();
      buttonLayout.preferredHeight = 60f;
      buttonLayout.preferredWidth = 300f;

      // Button Text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 28;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;

      return button;
    }

    private void SetLayoutHeight(GameObject obj, float height)
    {
      LayoutElement layout = obj.GetComponent<LayoutElement>();
      if (layout == null)
        layout = obj.AddComponent<LayoutElement>();

      layout.preferredHeight = height;
    }

    private void ClearContainer(Transform container)
    {
      for (int i = container.childCount - 1; i >= 0; i--)
        Destroy(container.GetChild(i).gameObject);
    }
  }
}
