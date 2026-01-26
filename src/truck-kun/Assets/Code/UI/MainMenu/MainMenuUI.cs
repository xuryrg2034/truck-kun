using Code.Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.MainMenu
{
  /// <summary>
  /// Main menu UI with Continue/New Game options
  /// </summary>
  public class MainMenuUI : MonoBehaviour
  {
    private Canvas _canvas;
    private Button _continueButton;
    private Button _newGameButton;
    private Text _highScoreText;
    private Text _lastSaveText;

    private static readonly Color TitleColor = new Color(0.9f, 0.3f, 0.3f);
    private static readonly Color ContinueColor = new Color(0.2f, 0.6f, 0.3f);
    private static readonly Color NewGameColor = new Color(0.3f, 0.5f, 0.7f);
    private static readonly Color DangerColor = new Color(0.7f, 0.3f, 0.3f);

    private void Start()
    {
      CreateUI();
      UpdateUI();
    }

    private void CreateUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("MainMenuCanvas");
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
      GameObject bgObj = new GameObject("Background");
      bgObj.transform.SetParent(canvasObj.transform, false);

      Image bgImage = bgObj.AddComponent<Image>();
      bgImage.color = new Color(0.08f, 0.08f, 0.12f, 1f);

      RectTransform bgRect = bgObj.GetComponent<RectTransform>();
      bgRect.anchorMin = Vector2.zero;
      bgRect.anchorMax = Vector2.one;
      bgRect.offsetMin = Vector2.zero;
      bgRect.offsetMax = Vector2.zero;

      // Content container
      GameObject contentObj = new GameObject("Content");
      contentObj.transform.SetParent(canvasObj.transform, false);

      RectTransform contentRect = contentObj.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0.5f, 0.5f);
      contentRect.anchorMax = new Vector2(0.5f, 0.5f);
      contentRect.sizeDelta = new Vector2(500f, 600f);

      VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 20f;
      layout.childAlignment = TextAnchor.MiddleCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;

      // Title
      CreateTitle(contentObj.transform);

      // Spacer
      CreateSpacer(contentObj.transform, 40f);

      // Continue button (if save exists)
      _continueButton = CreateMenuButton(contentObj.transform, "ПРОДОЛЖИТЬ", ContinueColor, OnContinueClicked);

      // New Game button
      _newGameButton = CreateMenuButton(contentObj.transform, "НОВАЯ ИГРА", NewGameColor, OnNewGameClicked);

      // Spacer
      CreateSpacer(contentObj.transform, 30f);

      // High score display
      _highScoreText = CreateInfoText(contentObj.transform, "");

      // Last save info
      _lastSaveText = CreateInfoText(contentObj.transform, "");

      // Spacer
      CreateSpacer(contentObj.transform, 40f);

      // Quit button
      CreateMenuButton(contentObj.transform, "ВЫХОД", DangerColor, OnQuitClicked);
    }

    private void CreateTitle(Transform parent)
    {
      // Title container
      GameObject titleContainer = new GameObject("TitleContainer");
      titleContainer.transform.SetParent(parent, false);

      LayoutElement titleLayout = titleContainer.AddComponent<LayoutElement>();
      titleLayout.preferredHeight = 150f;

      // Main title
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(titleContainer.transform, false);

      Text titleText = titleObj.AddComponent<Text>();
      titleText.text = "TRUCK-KUN";
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 72;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = TitleColor;
      titleText.alignment = TextAnchor.MiddleCenter;

      Outline titleOutline = titleObj.AddComponent<Outline>();
      titleOutline.effectColor = Color.black;
      titleOutline.effectDistance = new Vector2(4, -4);

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 0.5f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.offsetMin = Vector2.zero;
      titleRect.offsetMax = Vector2.zero;

      // Subtitle
      GameObject subtitleObj = new GameObject("Subtitle");
      subtitleObj.transform.SetParent(titleContainer.transform, false);

      Text subtitleText = subtitleObj.AddComponent<Text>();
      subtitleText.text = "Isekai Express Delivery";
      subtitleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      subtitleText.fontSize = 24;
      subtitleText.fontStyle = FontStyle.Italic;
      subtitleText.color = new Color(0.7f, 0.7f, 0.7f);
      subtitleText.alignment = TextAnchor.MiddleCenter;

      RectTransform subtitleRect = subtitleObj.GetComponent<RectTransform>();
      subtitleRect.anchorMin = new Vector2(0f, 0f);
      subtitleRect.anchorMax = new Vector2(1f, 0.4f);
      subtitleRect.offsetMin = Vector2.zero;
      subtitleRect.offsetMax = Vector2.zero;
    }

    private Button CreateMenuButton(Transform parent, string label, Color color, System.Action onClick)
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
      colors.disabledColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
      button.colors = colors;

      button.onClick.AddListener(() => onClick?.Invoke());

      LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
      layout.preferredHeight = 70f;

      // Button text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 32;
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

    private Text CreateInfoText(Transform parent, string content)
    {
      GameObject textObj = new GameObject("InfoText");
      textObj.transform.SetParent(parent, false);

      Text text = textObj.AddComponent<Text>();
      text.text = content;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 22;
      text.color = new Color(0.6f, 0.6f, 0.6f);
      text.alignment = TextAnchor.MiddleCenter;

      LayoutElement layout = textObj.AddComponent<LayoutElement>();
      layout.preferredHeight = 30f;

      return text;
    }

    private void CreateSpacer(Transform parent, float height)
    {
      GameObject spacer = new GameObject("Spacer");
      spacer.transform.SetParent(parent, false);

      LayoutElement layout = spacer.AddComponent<LayoutElement>();
      layout.preferredHeight = height;
    }

    private void UpdateUI()
    {
      GameStateService state = GameStateService.Instance;
      bool hasSave = state.HasSaveData();

      // Show/hide continue button
      _continueButton.gameObject.SetActive(hasSave);

      if (hasSave)
      {
        // Show save info
        _lastSaveText.text = $"День {state.DayNumber} | {state.PlayerMoney}¥";
        _lastSaveText.gameObject.SetActive(true);
      }
      else
      {
        _lastSaveText.gameObject.SetActive(false);
      }

      // Show high score if any
      if (state.HighScore > 0)
      {
        _highScoreText.text = $"Рекорд: День {state.HighScore}";
        _highScoreText.gameObject.SetActive(true);
      }
      else
      {
        _highScoreText.gameObject.SetActive(false);
      }
    }

    private void OnContinueClicked()
    {
      // Load saved game and go to hub
      GameStateService.Instance.Load();
      SceneTransitionService.Instance.LoadHub();
    }

    private void OnNewGameClicked()
    {
      bool hasSave = GameStateService.Instance.HasSaveData();

      if (hasSave)
      {
        // Show confirmation dialog
        ShowNewGameConfirmation();
      }
      else
      {
        StartNewGame();
      }
    }

    private void ShowNewGameConfirmation()
    {
      // Create confirmation dialog
      GameObject dialogObj = new GameObject("ConfirmDialog");
      dialogObj.transform.SetParent(_canvas.transform, false);

      // Overlay
      GameObject overlayObj = new GameObject("Overlay");
      overlayObj.transform.SetParent(dialogObj.transform, false);

      Image overlayImage = overlayObj.AddComponent<Image>();
      overlayImage.color = new Color(0f, 0f, 0f, 0.7f);

      RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
      overlayRect.anchorMin = Vector2.zero;
      overlayRect.anchorMax = Vector2.one;
      overlayRect.offsetMin = Vector2.zero;
      overlayRect.offsetMax = Vector2.zero;

      Button overlayButton = overlayObj.AddComponent<Button>();
      overlayButton.onClick.AddListener(() => Destroy(dialogObj));

      // Dialog panel
      GameObject panelObj = new GameObject("Panel");
      panelObj.transform.SetParent(dialogObj.transform, false);

      Image panelImage = panelObj.AddComponent<Image>();
      panelImage.color = new Color(0.15f, 0.15f, 0.2f, 1f);

      RectTransform panelRect = panelObj.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(450f, 250f);

      // Warning text
      GameObject warningObj = new GameObject("WarningText");
      warningObj.transform.SetParent(panelObj.transform, false);

      Text warningText = warningObj.AddComponent<Text>();
      warningText.text = "Начать новую игру?\n\nТекущий прогресс будет потерян!";
      warningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      warningText.fontSize = 26;
      warningText.color = Color.white;
      warningText.alignment = TextAnchor.MiddleCenter;

      RectTransform warningRect = warningObj.GetComponent<RectTransform>();
      warningRect.anchorMin = new Vector2(0f, 0.4f);
      warningRect.anchorMax = new Vector2(1f, 1f);
      warningRect.offsetMin = new Vector2(20f, 0f);
      warningRect.offsetMax = new Vector2(-20f, -20f);

      // Buttons container
      GameObject buttonsObj = new GameObject("Buttons");
      buttonsObj.transform.SetParent(panelObj.transform, false);

      RectTransform buttonsRect = buttonsObj.AddComponent<RectTransform>();
      buttonsRect.anchorMin = new Vector2(0f, 0f);
      buttonsRect.anchorMax = new Vector2(1f, 0.4f);
      buttonsRect.offsetMin = new Vector2(20f, 20f);
      buttonsRect.offsetMax = new Vector2(-20f, 0f);

      HorizontalLayoutGroup buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
      buttonsLayout.spacing = 20f;
      buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
      buttonsLayout.childControlWidth = true;
      buttonsLayout.childControlHeight = true;
      buttonsLayout.childForceExpandWidth = true;

      // Cancel button
      CreateDialogButton(buttonsObj.transform, "ОТМЕНА", new Color(0.4f, 0.4f, 0.4f), () =>
      {
        Destroy(dialogObj);
      });

      // Confirm button
      CreateDialogButton(buttonsObj.transform, "НАЧАТЬ", DangerColor, () =>
      {
        Destroy(dialogObj);
        StartNewGame();
      });
    }

    private void CreateDialogButton(Transform parent, string label, Color color, System.Action onClick)
    {
      GameObject buttonObj = new GameObject("Button_" + label);
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = color;

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;
      button.onClick.AddListener(() => onClick?.Invoke());

      // Text
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
    }

    private void StartNewGame()
    {
      GameStateService.Instance.ResetProgress();
      SceneTransitionService.Instance.LoadHub();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
      UnityEditor.EditorApplication.isPlaying = false;
#else
      Application.Quit();
#endif
    }
  }
}
