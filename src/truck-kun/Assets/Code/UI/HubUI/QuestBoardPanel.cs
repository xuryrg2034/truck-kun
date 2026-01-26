using Code.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.HubUI
{
  public class QuestBoardPanel : HubPanelBase
  {
    private static readonly Color AccentColor = new Color(0.2f, 0.5f, 0.8f);
    private static readonly Color StartButtonColor = new Color(0.2f, 0.6f, 0.3f);

    protected override void CreatePanel()
    {
      _panel = CreatePanelBase("ДОСКА ЗАДАНИЙ", AccentColor, new Vector2(550f, 450f));
      CreateContent();
      CreateCloseButton(_panel.transform);
    }

    private void CreateContent()
    {
      // Content container
      GameObject content = new GameObject("Content");
      content.transform.SetParent(_panel.transform, false);

      RectTransform contentRect = content.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0f, 0.18f);
      contentRect.anchorMax = new Vector2(1f, 0.88f);
      contentRect.offsetMin = new Vector2(25f, 0f);
      contentRect.offsetMax = new Vector2(-25f, 0f);

      VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 15f;
      layout.padding = new RectOffset(10, 10, 15, 10);
      layout.childAlignment = TextAnchor.UpperCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;

      // Header
      Text headerText = CreateText(content.transform,
        "Доступные задания на сегодня:", 22, Color.white);
      headerText.fontStyle = FontStyle.Bold;
      LayoutElement headerLayout = headerText.gameObject.AddComponent<LayoutElement>();
      headerLayout.preferredHeight = 30f;

      // Quest preview container
      GameObject questsContainer = new GameObject("QuestsPreview");
      questsContainer.transform.SetParent(content.transform, false);

      Image questsBg = questsContainer.AddComponent<Image>();
      questsBg.color = new Color(0.12f, 0.12f, 0.18f);

      LayoutElement questsLayout = questsContainer.AddComponent<LayoutElement>();
      questsLayout.preferredHeight = 180f;

      VerticalLayoutGroup questsVLayout = questsContainer.AddComponent<VerticalLayoutGroup>();
      questsVLayout.spacing = 10f;
      questsVLayout.padding = new RectOffset(15, 15, 15, 15);
      questsVLayout.childAlignment = TextAnchor.UpperLeft;
      questsVLayout.childControlWidth = true;
      questsVLayout.childControlHeight = false;

      // Quest 1 - Target quest
      CreateQuestPreview(questsContainer.transform,
        "Цель дня",
        "Сбей определённое количество целей",
        new Color(0.2f, 0.8f, 0.3f));

      // Divider
      GameObject divider = new GameObject("Divider");
      divider.transform.SetParent(questsContainer.transform, false);

      Image dividerImg = divider.AddComponent<Image>();
      dividerImg.color = new Color(1f, 1f, 1f, 0.1f);

      LayoutElement dividerLayout = divider.AddComponent<LayoutElement>();
      dividerLayout.preferredHeight = 2f;

      // Warning about forbidden
      CreateQuestPreview(questsContainer.transform,
        "⚠ Внимание",
        "Избегай красных пешеходов - штраф!",
        new Color(0.9f, 0.3f, 0.3f));

      // Info text
      Text infoText = CreateText(content.transform,
        "Квесты генерируются автоматически\nпри начале нового дня",
        18, new Color(0.6f, 0.6f, 0.6f));
      LayoutElement infoLayout = infoText.gameObject.AddComponent<LayoutElement>();
      infoLayout.preferredHeight = 50f;

      // Start button
      GameObject buttonContainer = new GameObject("ButtonContainer");
      buttonContainer.transform.SetParent(content.transform, false);

      LayoutElement buttonContainerLayout = buttonContainer.AddComponent<LayoutElement>();
      buttonContainerLayout.preferredHeight = 60f;

      Button startButton = CreateButton(buttonContainer.transform, "НАЧАТЬ НОВЫЙ ДЕНЬ", StartButtonColor, OnStartDay);

      RectTransform buttonRect = startButton.GetComponent<RectTransform>();
      buttonRect.anchorMin = new Vector2(0.15f, 0f);
      buttonRect.anchorMax = new Vector2(0.85f, 1f);
      buttonRect.offsetMin = Vector2.zero;
      buttonRect.offsetMax = Vector2.zero;
    }

    private void CreateQuestPreview(Transform parent, string title, string description, Color accentColor)
    {
      GameObject questItem = new GameObject("QuestItem");
      questItem.transform.SetParent(parent, false);

      LayoutElement itemLayout = questItem.AddComponent<LayoutElement>();
      itemLayout.preferredHeight = 55f;

      HorizontalLayoutGroup hLayout = questItem.AddComponent<HorizontalLayoutGroup>();
      hLayout.spacing = 10f;
      hLayout.childAlignment = TextAnchor.MiddleLeft;
      hLayout.childControlWidth = false;
      hLayout.childControlHeight = true;

      // Color indicator
      GameObject indicator = new GameObject("Indicator");
      indicator.transform.SetParent(questItem.transform, false);

      Image indicatorImg = indicator.AddComponent<Image>();
      indicatorImg.color = accentColor;

      LayoutElement indicatorLayout = indicator.AddComponent<LayoutElement>();
      indicatorLayout.preferredWidth = 6f;
      indicatorLayout.preferredHeight = 50f;

      // Text container
      GameObject textContainer = new GameObject("TextContainer");
      textContainer.transform.SetParent(questItem.transform, false);

      VerticalLayoutGroup textLayout = textContainer.AddComponent<VerticalLayoutGroup>();
      textLayout.spacing = 3f;
      textLayout.childAlignment = TextAnchor.MiddleLeft;
      textLayout.childControlWidth = true;
      textLayout.childControlHeight = false;

      LayoutElement textContainerLayout = textContainer.AddComponent<LayoutElement>();
      textContainerLayout.flexibleWidth = 1f;

      // Title
      Text titleText = CreateText(textContainer.transform, title, 20, accentColor, TextAnchor.MiddleLeft);
      titleText.fontStyle = FontStyle.Bold;
      LayoutElement titleLayout = titleText.gameObject.AddComponent<LayoutElement>();
      titleLayout.preferredHeight = 24f;

      // Description
      Text descText = CreateText(textContainer.transform, description, 16,
        new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleLeft);
      LayoutElement descLayout = descText.gameObject.AddComponent<LayoutElement>();
      descLayout.preferredHeight = 20f;
    }

    private void OnStartDay()
    {
      GameStateService.Instance.IncrementDay();
      SceneTransitionService.Instance.LoadGameplay();
    }
  }
}
