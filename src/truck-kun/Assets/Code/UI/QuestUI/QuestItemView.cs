using Code.Gameplay.Features.Pedestrian;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.QuestUI
{
  public class QuestItemView : MonoBehaviour
  {
    private Image _typeIcon;
    private Text _progressText;
    private Image _progressBarFill;
    private Image _completedCheckmark;
    private RectTransform _progressBarBackground;

    private int _questId;
    private bool _isCompleted;

    private static readonly Color TargetColor = new(0.2f, 0.8f, 0.2f, 1f);
    private static readonly Color ForbiddenColor = new(0.8f, 0.2f, 0.2f, 1f);
    private static readonly Color ProgressBarBgColor = new(0.3f, 0.3f, 0.3f, 1f);
    private static readonly Color ProgressBarFillColor = new(0.4f, 0.8f, 0.4f, 1f);
    private static readonly Color CompletedColor = new(0.2f, 1f, 0.2f, 1f);

    public int QuestId => _questId;

    public static QuestItemView Create(Transform parent, int questId, PedestrianKind targetType, int current, int required)
    {
      GameObject root = new GameObject($"QuestItem_{questId}");
      root.transform.SetParent(parent, false);

      RectTransform rootRect = root.AddComponent<RectTransform>();
      rootRect.sizeDelta = new Vector2(200f, 50f);

      HorizontalLayoutGroup layout = root.AddComponent<HorizontalLayoutGroup>();
      layout.spacing = 8f;
      layout.childAlignment = TextAnchor.MiddleLeft;
      layout.childControlWidth = false;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = false;
      layout.childForceExpandHeight = false;
      layout.padding = new RectOffset(5, 5, 5, 5);

      QuestItemView view = root.AddComponent<QuestItemView>();
      view._questId = questId;

      view.CreateTypeIcon(root.transform, targetType);
      view.CreateProgressBar(root.transform);
      view.CreateProgressText(root.transform, current, required);
      view.CreateCheckmark(root.transform);

      view.UpdateProgress(current, required, false);

      return view;
    }

    private void CreateTypeIcon(Transform parent, PedestrianKind targetType)
    {
      GameObject iconObj = new GameObject("TypeIcon");
      iconObj.transform.SetParent(parent, false);

      _typeIcon = iconObj.AddComponent<Image>();
      _typeIcon.color = targetType == PedestrianKind.Target ? TargetColor : ForbiddenColor;

      RectTransform iconRect = iconObj.GetComponent<RectTransform>();
      iconRect.sizeDelta = new Vector2(36f, 36f);

      LayoutElement iconLayout = iconObj.AddComponent<LayoutElement>();
      iconLayout.minWidth = 36f;
      iconLayout.minHeight = 36f;
      iconLayout.preferredWidth = 36f;
      iconLayout.preferredHeight = 36f;
    }

    private void CreateProgressBar(Transform parent)
    {
      GameObject barBg = new GameObject("ProgressBarBg");
      barBg.transform.SetParent(parent, false);

      Image bgImage = barBg.AddComponent<Image>();
      bgImage.color = ProgressBarBgColor;

      _progressBarBackground = barBg.GetComponent<RectTransform>();
      _progressBarBackground.sizeDelta = new Vector2(80f, 12f);

      LayoutElement barLayout = barBg.AddComponent<LayoutElement>();
      barLayout.minWidth = 80f;
      barLayout.minHeight = 12f;
      barLayout.preferredWidth = 80f;
      barLayout.preferredHeight = 12f;

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

    private void CreateProgressText(Transform parent, int current, int required)
    {
      GameObject textObj = new GameObject("ProgressText");
      textObj.transform.SetParent(parent, false);

      _progressText = textObj.AddComponent<Text>();
      _progressText.text = $"{current}/{required}";
      _progressText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _progressText.fontSize = 18;
      _progressText.color = Color.white;
      _progressText.alignment = TextAnchor.MiddleLeft;

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.sizeDelta = new Vector2(50f, 36f);

      LayoutElement textLayout = textObj.AddComponent<LayoutElement>();
      textLayout.minWidth = 50f;
      textLayout.minHeight = 36f;
      textLayout.preferredWidth = 50f;
      textLayout.preferredHeight = 36f;
    }

    private void CreateCheckmark(Transform parent)
    {
      GameObject checkObj = new GameObject("Checkmark");
      checkObj.transform.SetParent(parent, false);

      _completedCheckmark = checkObj.AddComponent<Image>();
      _completedCheckmark.color = CompletedColor;

      RectTransform checkRect = checkObj.GetComponent<RectTransform>();
      checkRect.sizeDelta = new Vector2(24f, 24f);

      LayoutElement checkLayout = checkObj.AddComponent<LayoutElement>();
      checkLayout.minWidth = 24f;
      checkLayout.minHeight = 24f;
      checkLayout.preferredWidth = 24f;
      checkLayout.preferredHeight = 24f;

      // Create checkmark shape (simple square for now, can be replaced with sprite)
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
      }
    }

    public void Destroy()
    {
      if (gameObject != null)
        Destroy(gameObject);
    }
  }
}
