using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Pedestrian.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.QuestUI
{
  public class QuestItemView : MonoBehaviour
  {
    private Image _typeIcon;
    private Text _typeLabel;
    private Text _progressText;
    private Image _progressBarFill;
    private Image _completedCheckmark;
    private RectTransform _progressBarBackground;

    private int _questId;
    private bool _isCompleted;

    private static readonly Color ProgressBarBgColor = new(0.3f, 0.3f, 0.3f, 1f);
    private static readonly Color ProgressBarFillColor = new(0.4f, 0.8f, 0.4f, 1f);
    private static readonly Color CompletedColor = new(0.2f, 1f, 0.2f, 1f);

    public int QuestId => _questId;

    public static QuestItemView Create(Transform parent, int questId, PedestrianKind targetType, int current, int required)
    {
      GameObject root = new GameObject($"QuestItem_{questId}");
      root.transform.SetParent(parent, false);

      RectTransform rootRect = root.AddComponent<RectTransform>();
      rootRect.sizeDelta = new Vector2(200f, 60f);

      VerticalLayoutGroup layout = root.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 4f;
      layout.childAlignment = TextAnchor.UpperLeft;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;
      layout.childForceExpandHeight = false;
      layout.padding = new RectOffset(5, 5, 5, 5);

      LayoutElement rootLayout = root.AddComponent<LayoutElement>();
      rootLayout.minHeight = 60f;
      rootLayout.preferredHeight = 60f;

      QuestItemView view = root.AddComponent<QuestItemView>();
      view._questId = questId;

      view.CreateTopRow(root.transform, targetType);
      view.CreateProgressBar(root.transform);
      view.CreateProgressText(root.transform, current, required, targetType);

      view.UpdateProgress(current, required, false);

      return view;
    }

    private void CreateTopRow(Transform parent, PedestrianKind targetType)
    {
      GameObject rowObj = new GameObject("TopRow");
      rowObj.transform.SetParent(parent, false);

      HorizontalLayoutGroup rowLayout = rowObj.AddComponent<HorizontalLayoutGroup>();
      rowLayout.spacing = 8f;
      rowLayout.childAlignment = TextAnchor.MiddleLeft;
      rowLayout.childControlWidth = false;
      rowLayout.childControlHeight = false;
      rowLayout.childForceExpandWidth = false;
      rowLayout.childForceExpandHeight = false;

      LayoutElement rowLayoutElement = rowObj.AddComponent<LayoutElement>();
      rowLayoutElement.minHeight = 28f;
      rowLayoutElement.preferredHeight = 28f;

      CreateTypeIcon(rowObj.transform, targetType);
      CreateTypeLabel(rowObj.transform, targetType);
      CreateCheckmark(rowObj.transform);
    }

    private void CreateTypeIcon(Transform parent, PedestrianKind targetType)
    {
      GameObject iconObj = new GameObject("TypeIcon");
      iconObj.transform.SetParent(parent, false);

      _typeIcon = iconObj.AddComponent<Image>();
      _typeIcon.color = GetTypeColor(targetType);

      RectTransform iconRect = iconObj.GetComponent<RectTransform>();
      iconRect.sizeDelta = new Vector2(24f, 24f);

      LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
      iconLayout.minWidth = 24f;
      iconLayout.minHeight = 24f;
      iconLayout.preferredWidth = 24f;
      iconLayout.preferredHeight = 24f;
    }

    private void CreateTypeLabel(Transform parent, PedestrianKind targetType)
    {
      GameObject labelObj = new GameObject("TypeLabel");
      labelObj.transform.SetParent(parent, false);

      _typeLabel = labelObj.AddComponent<Text>();
      _typeLabel.text = targetType.GetDisplayNameRu();
      _typeLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _typeLabel.fontSize = 16;
      _typeLabel.fontStyle = FontStyle.Bold;
      _typeLabel.color = Color.white;
      _typeLabel.alignment = TextAnchor.MiddleLeft;

      RectTransform labelRect = labelObj.GetComponent<RectTransform>();
      labelRect.sizeDelta = new Vector2(120f, 24f);

      LayoutElement labelLayout = labelObj.AddComponent<LayoutElement>();
      labelLayout.minWidth = 120f;
      labelLayout.minHeight = 24f;
      labelLayout.preferredWidth = 120f;
      labelLayout.preferredHeight = 24f;
    }

    private void CreateProgressBar(Transform parent)
    {
      GameObject barBg = new GameObject("ProgressBarBg");
      barBg.transform.SetParent(parent, false);

      Image bgImage = barBg.AddComponent<Image>();
      bgImage.color = ProgressBarBgColor;

      _progressBarBackground = barBg.GetComponent<RectTransform>();
      _progressBarBackground.sizeDelta = new Vector2(0f, 10f);

      LayoutElement barLayout = barBg.AddComponent<LayoutElement>();
      barLayout.minHeight = 10f;
      barLayout.preferredHeight = 10f;
      barLayout.flexibleWidth = 1f;

      GameObject barFill = new GameObject("ProgressBarFill");
      barFill.transform.SetParent(barBg.transform, false);

      _progressBarFill = barFill.AddComponent<Image>();
      _progressBarFill.color = ProgressBarFillColor;

      RectTransform fillRect = barFill.GetComponent<RectTransform>();
      fillRect.anchorMin = Vector2.zero;
      fillRect.anchorMax = new Vector2(0f, 1f);
      fillRect.pivot = new Vector2(0f, 0.5f);
      fillRect.offsetMin = Vector2.zero;
      fillRect.offsetMax = Vector2.zero;
    }

    private void CreateProgressText(Transform parent, int current, int required, PedestrianKind targetType)
    {
      GameObject textObj = new GameObject("ProgressText");
      textObj.transform.SetParent(parent, false);

      _progressText = textObj.AddComponent<Text>();
      _progressText.text = $"{current}/{required}";
      _progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _progressText.fontSize = 14;
      _progressText.color = new Color(0.8f, 0.8f, 0.8f);
      _progressText.alignment = TextAnchor.MiddleLeft;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.sizeDelta = new Vector2(0f, 18f);

      LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
      textLayout.minHeight = 18f;
      textLayout.preferredHeight = 18f;
      textLayout.flexibleWidth = 1f;
    }

    private void CreateCheckmark(Transform parent)
    {
      GameObject checkObj = new GameObject("Checkmark");
      checkObj.transform.SetParent(parent, false);

      _completedCheckmark = checkObj.AddComponent<Image>();
      _completedCheckmark.color = CompletedColor;

      RectTransform checkRect = checkObj.GetComponent<RectTransform>();
      checkRect.sizeDelta = new Vector2(20f, 20f);

      LayoutElement checkLayout = checkObj.AddComponent<LayoutElement>();
      checkLayout.minWidth = 20f;
      checkLayout.minHeight = 20f;
      checkLayout.preferredWidth = 20f;
      checkLayout.preferredHeight = 20f;

      checkObj.SetActive(false);
    }

    public void UpdateProgress(int current, int required, bool isCompleted)
    {
      _progressText.text = $"{current}/{required}";

      float progress = required > 0 ? Mathf.Clamp01((float)current / required) : 0f;

      RectTransform fillRect = _progressBarFill.GetComponent<RectTransform>();
      fillRect.anchorMax = new Vector2(progress, 1f);

      if (isCompleted && !_isCompleted)
      {
        _isCompleted = true;
        _completedCheckmark.gameObject.SetActive(true);
        _progressBarFill.color = CompletedColor;
        _progressText.text = $"{current}/{required} OK!";
        _progressText.color = CompletedColor;
      }
    }

    private static Color GetTypeColor(PedestrianKind kind)
    {
      // Get color based on type visual data
      PedestrianVisualData data = PedestrianVisualData.Default(kind);

      // Brighten color for UI visibility
      Color c = data.Color;
      float maxComponent = Mathf.Max(c.r, c.g, c.b);
      if (maxComponent < 0.5f)
      {
        float boost = 0.5f / maxComponent;
        c.r = Mathf.Clamp01(c.r * boost);
        c.g = Mathf.Clamp01(c.g * boost);
        c.b = Mathf.Clamp01(c.b * boost);
      }

      return c;
    }

    public void Destroy()
    {
      if (gameObject != null)
        Destroy(gameObject);
    }
  }
}
