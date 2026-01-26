using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.HubUI
{
  public abstract class HubPanelBase : MonoBehaviour
  {
    protected Canvas _canvas;
    protected GameObject _panel;
    protected System.Action _onClose;

    public bool IsOpen => _panel != null && _panel.activeSelf;

    public virtual void Show(System.Action onClose = null)
    {
      _onClose = onClose;

      if (_panel == null)
        CreatePanel();

      _panel.SetActive(true);
      OnShow();
    }

    public virtual void Hide()
    {
      if (_panel != null)
        _panel.SetActive(false);

      OnHide();
      _onClose?.Invoke();
    }

    public virtual void Close()
    {
      if (_panel != null)
      {
        Destroy(_panel);
        _panel = null;
      }

      _onClose?.Invoke();
    }

    protected abstract void CreatePanel();
    protected virtual void OnShow() { }
    protected virtual void OnHide() { }

    protected void SetupCanvas()
    {
      if (_canvas != null)
        return;

      GameObject canvasObj = new GameObject($"{GetType().Name}Canvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 200;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();
    }

    protected GameObject CreatePanelBase(string title, Color accentColor, Vector2 size)
    {
      SetupCanvas();

      GameObject panel = new GameObject("Panel");
      panel.transform.SetParent(_canvas.transform, false);

      Image panelBg = panel.AddComponent<Image>();
      panelBg.color = new Color(0.1f, 0.1f, 0.15f, 0.95f);

      RectTransform panelRect = panel.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = size;

      // Title bar
      GameObject titleBar = new GameObject("TitleBar");
      titleBar.transform.SetParent(panel.transform, false);

      Image titleBg = titleBar.AddComponent<Image>();
      titleBg.color = accentColor;

      RectTransform titleRect = titleBar.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 1f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.pivot = new Vector2(0.5f, 1f);
      titleRect.sizeDelta = new Vector2(0f, 60f);
      titleRect.anchoredPosition = Vector2.zero;

      // Title text
      GameObject titleTextObj = new GameObject("TitleText");
      titleTextObj.transform.SetParent(titleBar.transform, false);

      Text titleText = titleTextObj.AddComponent<Text>();
      titleText.text = title;
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 32;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = Color.white;
      titleText.alignment = TextAnchor.MiddleCenter;

      RectTransform titleTextRect = titleTextObj.GetComponent<RectTransform>();
      titleTextRect.anchorMin = Vector2.zero;
      titleTextRect.anchorMax = Vector2.one;
      titleTextRect.offsetMin = Vector2.zero;
      titleTextRect.offsetMax = Vector2.zero;

      return panel;
    }

    protected Button CreateButton(Transform parent, string label, Color color, System.Action onClick)
    {
      GameObject buttonObj = new GameObject("Button_" + label.Replace(" ", ""));
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = color;

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;

      ColorBlock colors = button.colors;
      colors.normalColor = color;
      colors.highlightedColor = color * 1.2f;
      colors.pressedColor = color * 0.8f;
      colors.disabledColor = new Color(0.3f, 0.3f, 0.3f);
      button.colors = colors;

      button.onClick.AddListener(() => onClick?.Invoke());

      // Button text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
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

      return button;
    }

    protected Text CreateText(Transform parent, string content, int fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(parent, false);

      Text text = textObj.AddComponent<Text>();
      text.text = content;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = fontSize;
      text.color = color;
      text.alignment = alignment;

      return text;
    }

    protected void CreateCloseButton(Transform parent, System.Action onClick = null)
    {
      GameObject buttonObj = new GameObject("CloseButton");
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = new Color(0.6f, 0.2f, 0.2f);

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;
      button.onClick.AddListener(() =>
      {
        onClick?.Invoke();
        Close();
      });

      RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
      buttonRect.anchorMin = new Vector2(0.5f, 0f);
      buttonRect.anchorMax = new Vector2(0.5f, 0f);
      buttonRect.pivot = new Vector2(0.5f, 0f);
      buttonRect.anchoredPosition = new Vector2(0f, 20f);
      buttonRect.sizeDelta = new Vector2(150f, 50f);

      // Text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = "ЗАКРЫТЬ";
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 22;
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
      if (_panel != null)
        Destroy(_panel);
    }
  }
}
