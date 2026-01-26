using Code.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Meta.Economy
{
  public static class EconomyConstants
  {
    public const int DailyFoodCost = 100;
    public const int StartingMoney = 1000;
    public const int MinimumRequiredMoney = 100;
  }

  /// <summary>
  /// Panel shown when player runs out of money (Game Over)
  /// </summary>
  public class GameOverPanel : MonoBehaviour
  {
    private Canvas _canvas;
    private GameObject _panel;

    private static readonly Color GameOverColor = new Color(0.6f, 0.15f, 0.15f);
    private static readonly Color RestartButtonColor = new Color(0.2f, 0.5f, 0.7f);
    private static readonly Color MenuButtonColor = new Color(0.4f, 0.4f, 0.45f);

    public bool IsShowing => _panel != null && _panel.activeSelf;

    public void Show()
    {
      if (_panel == null)
        CreatePanel();

      _panel.SetActive(true);
      Time.timeScale = 0f;
      Cursor.lockState = CursorLockMode.None;
      Cursor.visible = true;
    }

    public void Hide()
    {
      if (_panel != null)
        _panel.SetActive(false);

      Time.timeScale = 1f;
    }

    private void CreatePanel()
    {
      CreateCanvas();

      _panel = new GameObject("GameOverPanel");
      _panel.transform.SetParent(_canvas.transform, false);

      // Full screen dark overlay
      GameObject overlay = new GameObject("Overlay");
      overlay.transform.SetParent(_panel.transform, false);

      Image overlayImage = overlay.AddComponent<Image>();
      overlayImage.color = new Color(0f, 0f, 0f, 0.85f);

      RectTransform overlayRect = overlay.GetComponent<RectTransform>();
      overlayRect.anchorMin = Vector2.zero;
      overlayRect.anchorMax = Vector2.one;
      overlayRect.offsetMin = Vector2.zero;
      overlayRect.offsetMax = Vector2.zero;

      // Content container
      GameObject content = new GameObject("Content");
      content.transform.SetParent(_panel.transform, false);

      RectTransform contentRect = content.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0.5f, 0.5f);
      contentRect.anchorMax = new Vector2(0.5f, 0.5f);
      contentRect.sizeDelta = new Vector2(600f, 450f);

      // Panel background
      Image contentBg = content.AddComponent<Image>();
      contentBg.color = new Color(0.12f, 0.12f, 0.15f, 0.98f);

      // Title bar
      GameObject titleBar = new GameObject("TitleBar");
      titleBar.transform.SetParent(content.transform, false);

      Image titleBg = titleBar.AddComponent<Image>();
      titleBg.color = GameOverColor;

      RectTransform titleRect = titleBar.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 1f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.pivot = new Vector2(0.5f, 1f);
      titleRect.sizeDelta = new Vector2(0f, 70f);
      titleRect.anchoredPosition = Vector2.zero;

      // Title text
      CreateText(titleBar.transform, "GAME OVER", 38, Color.white, TextAnchor.MiddleCenter, true);

      // Main message
      GameObject messageContainer = new GameObject("MessageContainer");
      messageContainer.transform.SetParent(content.transform, false);

      RectTransform msgRect = messageContainer.AddComponent<RectTransform>();
      msgRect.anchorMin = new Vector2(0f, 0.4f);
      msgRect.anchorMax = new Vector2(1f, 0.85f);
      msgRect.offsetMin = new Vector2(30f, 0f);
      msgRect.offsetMax = new Vector2(-30f, -70f);

      VerticalLayoutGroup msgLayout = messageContainer.AddComponent<VerticalLayoutGroup>();
      msgLayout.spacing = 15f;
      msgLayout.childAlignment = TextAnchor.MiddleCenter;
      msgLayout.childControlWidth = true;
      msgLayout.childControlHeight = false;
      msgLayout.childForceExpandWidth = true;

      // Skull icon
      Text skullText = CreateText(messageContainer.transform, "💀", 64, new Color(0.8f, 0.3f, 0.3f));
      LayoutElement skullLayout = skullText.gameObject.AddComponent<LayoutElement>();
      skullLayout.preferredHeight = 80f;

      // Message text
      Text msgText = CreateText(messageContainer.transform,
        "У вас закончились деньги!\n\nНе хватает даже на еду...",
        26, new Color(0.85f, 0.85f, 0.85f));
      LayoutElement msgTextLayout = msgText.gameObject.AddComponent<LayoutElement>();
      msgTextLayout.preferredHeight = 100f;

      // Balance display
      int currentBalance = GameStateService.Instance.PlayerMoney;
      Text balanceText = CreateText(messageContainer.transform,
        $"Баланс: {currentBalance}¥ (нужно минимум {EconomyConstants.MinimumRequiredMoney}¥)",
        22, new Color(0.9f, 0.4f, 0.4f));
      LayoutElement balanceLayout = balanceText.gameObject.AddComponent<LayoutElement>();
      balanceLayout.preferredHeight = 35f;

      // Buttons container
      GameObject buttonsContainer = new GameObject("ButtonsContainer");
      buttonsContainer.transform.SetParent(content.transform, false);

      RectTransform buttonsRect = buttonsContainer.AddComponent<RectTransform>();
      buttonsRect.anchorMin = new Vector2(0f, 0f);
      buttonsRect.anchorMax = new Vector2(1f, 0.35f);
      buttonsRect.offsetMin = new Vector2(40f, 30f);
      buttonsRect.offsetMax = new Vector2(-40f, 0f);

      VerticalLayoutGroup buttonsLayout = buttonsContainer.AddComponent<VerticalLayoutGroup>();
      buttonsLayout.spacing = 15f;
      buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
      buttonsLayout.childControlWidth = true;
      buttonsLayout.childControlHeight = false;
      buttonsLayout.childForceExpandWidth = true;

      // Restart button
      CreateButton(buttonsContainer.transform, "НАЧАТЬ ЗАНОВО", RestartButtonColor, OnRestartClicked);

      // Main menu button
      CreateButton(buttonsContainer.transform, "ГЛАВНОЕ МЕНЮ", MenuButtonColor, OnMainMenuClicked);
    }

    private void CreateCanvas()
    {
      GameObject canvasObj = new GameObject("GameOverCanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 1000;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();
    }

    private Text CreateText(Transform parent, string content, int fontSize, Color color,
      TextAnchor alignment = TextAnchor.MiddleCenter, bool bold = false)
    {
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(parent, false);

      Text text = textObj.AddComponent<Text>();
      text.text = content;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = fontSize;
      text.color = color;
      text.alignment = alignment;
      if (bold)
        text.fontStyle = FontStyle.Bold;

      RectTransform rect = textObj.GetComponent<RectTransform>();
      rect.anchorMin = Vector2.zero;
      rect.anchorMax = Vector2.one;
      rect.offsetMin = Vector2.zero;
      rect.offsetMax = Vector2.zero;

      return text;
    }

    private Button CreateButton(Transform parent, string label, Color color, System.Action onClick)
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
      button.colors = colors;

      button.onClick.AddListener(() => onClick?.Invoke());

      LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
      layout.preferredHeight = 55f;

      // Button text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 26;
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

    private void OnRestartClicked()
    {
      Time.timeScale = 1f;
      GameStateService.Instance.ResetProgress();
      SceneTransitionService.Instance.LoadHub();
    }

    private void OnMainMenuClicked()
    {
      Time.timeScale = 1f;
      // Load main menu scene if exists, otherwise just reload hub
      SceneTransitionService.Instance.LoadHub();
    }

    private void OnDestroy()
    {
      Time.timeScale = 1f;
    }
  }
}
