using System.Collections.Generic;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest;
using Entitas;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.QuestUI
{
  public class QuestUIController : MonoBehaviour
  {
    private MetaContext _meta;
    private IGroup<MetaEntity> _activeQuests;
    private readonly Dictionary<int, QuestItemView> _questViews = new();
    private readonly List<MetaEntity> _questBuffer = new(8);

    private Canvas _canvas;
    private RectTransform _panelRect;
    private VerticalLayoutGroup _layoutGroup;

    private bool _initialized;

    public void Initialize(MetaContext meta)
    {
      _meta = meta;
      _activeQuests = meta.GetGroup(MetaMatcher.AllOf(MetaMatcher.DailyQuest, MetaMatcher.ActiveQuest, MetaMatcher.QuestProgress));

      CreateUI();
      RefreshAllQuests();

      _initialized = true;
    }

    private void CreateUI()
    {
      // Create Canvas
      GameObject canvasObj = new GameObject("QuestUICanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 100;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Create Panel (top-left corner)
      GameObject panelObj = new GameObject("QuestPanel");
      panelObj.transform.SetParent(canvasObj.transform, false);

      Image panelBg = panelObj.AddComponent<Image>();
      panelBg.color = new Color(0f, 0f, 0f, 0.5f);

      _panelRect = panelObj.GetComponent<RectTransform>();
      _panelRect.anchorMin = new Vector2(0f, 1f);
      _panelRect.anchorMax = new Vector2(0f, 1f);
      _panelRect.pivot = new Vector2(0f, 1f);
      _panelRect.anchoredPosition = new Vector2(20f, -20f);
      _panelRect.sizeDelta = new Vector2(220f, 0f);

      _layoutGroup = panelObj.AddComponent<VerticalLayoutGroup>();
      _layoutGroup.spacing = 5f;
      _layoutGroup.padding = new RectOffset(10, 10, 10, 10);
      _layoutGroup.childAlignment = TextAnchor.UpperLeft;
      _layoutGroup.childControlWidth = true;
      _layoutGroup.childControlHeight = false;
      _layoutGroup.childForceExpandWidth = true;
      _layoutGroup.childForceExpandHeight = false;

      ContentSizeFitter fitter = panelObj.AddComponent<ContentSizeFitter>();
      fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
      fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

      // Create header
      CreateHeader(panelObj.transform);
    }

    private void CreateHeader(Transform parent)
    {
      GameObject headerObj = new GameObject("Header");
      headerObj.transform.SetParent(parent, false);

      Text headerText = headerObj.AddComponent<Text>();
      headerText.text = "DAILY QUEST";
      headerText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      headerText.fontSize = 20;
      headerText.fontStyle = FontStyle.Bold;
      headerText.color = Color.white;
      headerText.alignment = TextAnchor.MiddleLeft;

      RectTransform headerRect = headerObj.GetComponent<RectTransform>();
      headerRect.sizeDelta = new Vector2(200f, 30f);

      LayoutElement headerLayout = headerObj.AddComponent<LayoutElement>();
      headerLayout.minHeight = 30f;
      headerLayout.preferredHeight = 30f;
    }

    private void LateUpdate()
    {
      if (!_initialized || _meta == null)
        return;

      UpdateQuestProgress();
    }

    private void UpdateQuestProgress()
    {
      foreach (MetaEntity quest in _activeQuests.GetEntities(_questBuffer))
      {
        int questId = quest.id.Value;

        if (!_questViews.TryGetValue(questId, out QuestItemView view))
        {
          view = CreateQuestView(quest);
          _questViews[questId] = view;
        }

        view.UpdateProgress(
          quest.questProgress.CurrentCount,
          quest.dailyQuest.RequiredCount,
          quest.isQuestCompleted);
      }
    }

    private void RefreshAllQuests()
    {
      // Clear existing views
      foreach (QuestItemView view in _questViews.Values)
        view.Destroy();

      _questViews.Clear();

      // Create views for all active quests
      foreach (MetaEntity quest in _activeQuests.GetEntities(_questBuffer))
      {
        QuestItemView view = CreateQuestView(quest);
        _questViews[quest.id.Value] = view;
      }
    }

    private QuestItemView CreateQuestView(MetaEntity quest)
    {
      return QuestItemView.Create(
        _panelRect,
        quest.id.Value,
        quest.dailyQuest.TargetType,
        quest.questProgress.CurrentCount,
        quest.dailyQuest.RequiredCount);
    }

    private void OnDestroy()
    {
      foreach (QuestItemView view in _questViews.Values)
      {
        if (view != null)
          view.Destroy();
      }

      _questViews.Clear();
    }
  }
}
