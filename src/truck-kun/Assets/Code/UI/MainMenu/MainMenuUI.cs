using Code.Audio;
using Code.Infrastructure;
using Code.UI.Settings;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.MainMenu
{
  /// <summary>
  /// Main menu UI with Continue/New Game/Settings options
  /// </summary>
  public class MainMenuUI : MonoBehaviour
  {
    private Canvas _canvas;
    private Button _continueButton;
    private Button _newGameButton;
    private Button _settingsButton;
    private Text _highScoreText;
    private Text _lastSaveText;
    private Text _versionText;
    private SettingsPanel _settingsPanel;

    private static readonly Color TitleColor = new Color(0.95f, 0.25f, 0.25f);
    private static readonly Color TitleGlowColor = new Color(1f, 0.4f, 0.2f);
    private static readonly Color ContinueColor = new Color(0.2f, 0.65f, 0.3f);
    private static readonly Color NewGameColor = new Color(0.25f, 0.5f, 0.75f);
    private static readonly Color SettingsColor = new Color(0.5f, 0.45f, 0.55f);
    private static readonly Color DangerColor = new Color(0.75f, 0.25f, 0.25f);

    private void Start()
    {
      CreateUI();
      UpdateUI();
      SetupMusic();
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

      // Background with gradient effect
      CreateBackground(canvasObj.transform);

      // Decorative elements
      CreateDecorativeElements(canvasObj.transform);

      // Content container
      GameObject contentObj = new GameObject("Content");
      contentObj.transform.SetParent(canvasObj.transform, false);

      RectTransform contentRect = contentObj.AddComponent<RectTransform>();
      contentRect.anchorMin = new Vector2(0.5f, 0.5f);
      contentRect.anchorMax = new Vector2(0.5f, 0.5f);
      contentRect.sizeDelta = new Vector2(500f, 700f);

      VerticalLayoutGroup layout = contentObj.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 15f;
      layout.childAlignment = TextAnchor.MiddleCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;
      layout.padding = new RectOffset(20, 20, 0, 0);

      // Title
      CreateTitle(contentObj.transform);

      // Spacer
      CreateSpacer(contentObj.transform, 50f);

      // Continue button (if save exists)
      _continueButton = CreateMenuButton(contentObj.transform, "ПРОДОЛЖИТЬ", ContinueColor, OnContinueClicked, "arrow_right");

      // New Game button
      _newGameButton = CreateMenuButton(contentObj.transform, "НОВАЯ ИГРА", NewGameColor, OnNewGameClicked, "plus");

      // Settings button
      _settingsButton = CreateMenuButton(contentObj.transform, "НАСТРОЙКИ", SettingsColor, OnSettingsClicked, "gear");

      // Spacer
      CreateSpacer(contentObj.transform, 20f);

      // High score display
      _highScoreText = CreateInfoText(contentObj.transform, "");

      // Last save info
      _lastSaveText = CreateInfoText(contentObj.transform, "");

      // Spacer
      CreateSpacer(contentObj.transform, 30f);

      // Quit button
      CreateMenuButton(contentObj.transform, "ВЫХОД", DangerColor, OnQuitClicked, "exit");

      // Version text at bottom
      CreateVersionText(canvasObj.transform);

      // Settings panel component
      _settingsPanel = gameObject.AddComponent<SettingsPanel>();
    }

    private void CreateBackground(Transform parent)
    {
      // Dark gradient background
      GameObject bgObj = new GameObject("Background");
      bgObj.transform.SetParent(parent, false);
      bgObj.transform.SetAsFirstSibling();

      Image bgImage = bgObj.AddComponent<Image>();

      // Create gradient texture
      Texture2D gradientTex = new Texture2D(1, 256);
      for (int y = 0; y < 256; y++)
      {
        float t = y / 255f;
        Color color = Color.Lerp(
          new Color(0.02f, 0.02f, 0.05f),
          new Color(0.08f, 0.05f, 0.12f),
          t
        );
        gradientTex.SetPixel(0, y, color);
      }
      gradientTex.Apply();

      bgImage.sprite = Sprite.Create(gradientTex, new Rect(0, 0, 1, 256), new Vector2(0.5f, 0.5f));
      bgImage.type = Image.Type.Sliced;

      RectTransform bgRect = bgObj.GetComponent<RectTransform>();
      bgRect.anchorMin = Vector2.zero;
      bgRect.anchorMax = Vector2.one;
      bgRect.offsetMin = Vector2.zero;
      bgRect.offsetMax = Vector2.zero;

      // Vignette overlay
      GameObject vignetteObj = new GameObject("Vignette");
      vignetteObj.transform.SetParent(bgObj.transform, false);

      Image vignetteImage = vignetteObj.AddComponent<Image>();
      vignetteImage.color = new Color(0f, 0f, 0f, 0.3f);

      RectTransform vignetteRect = vignetteObj.GetComponent<RectTransform>();
      vignetteRect.anchorMin = Vector2.zero;
      vignetteRect.anchorMax = Vector2.one;
      vignetteRect.offsetMin = Vector2.zero;
      vignetteRect.offsetMax = Vector2.zero;
    }

    private void CreateDecorativeElements(Transform parent)
    {
      // Top-left decorative line
      CreateDecorativeLine(parent, new Vector2(0f, 1f), new Vector2(0.3f, 1f), new Vector2(0f, -50f), TitleColor * 0.5f);

      // Bottom-right decorative line
      CreateDecorativeLine(parent, new Vector2(0.7f, 0f), new Vector2(1f, 0f), new Vector2(0f, 50f), TitleColor * 0.5f);
    }

    private void CreateDecorativeLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offset, Color color)
    {
      GameObject lineObj = new GameObject("DecorativeLine");
      lineObj.transform.SetParent(parent, false);

      Image lineImage = lineObj.AddComponent<Image>();
      lineImage.color = color;

      RectTransform lineRect = lineObj.GetComponent<RectTransform>();
      lineRect.anchorMin = anchorMin;
      lineRect.anchorMax = anchorMax;
      lineRect.anchoredPosition = offset;
      lineRect.sizeDelta = new Vector2(0f, 3f);
    }

    private void CreateTitle(Transform parent)
    {
      // Title container
      GameObject titleContainer = new GameObject("TitleContainer");
      titleContainer.transform.SetParent(parent, false);

      LayoutElement titleLayout = titleContainer.AddComponent<LayoutElement>();
      titleLayout.preferredHeight = 180f;

      // Glow effect behind title
      GameObject glowObj = new GameObject("TitleGlow");
      glowObj.transform.SetParent(titleContainer.transform, false);

      Text glowText = glowObj.AddComponent<Text>();
      glowText.text = "TRUCK-KUN";
      glowText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      glowText.fontSize = 76;
      glowText.fontStyle = FontStyle.Bold;
      glowText.color = new Color(TitleGlowColor.r, TitleGlowColor.g, TitleGlowColor.b, 0.3f);
      glowText.alignment = TextAnchor.MiddleCenter;

      RectTransform glowRect = glowObj.GetComponent<RectTransform>();
      glowRect.anchorMin = new Vector2(0f, 0.45f);
      glowRect.anchorMax = new Vector2(1f, 1f);
      glowRect.offsetMin = new Vector2(4, -4);
      glowRect.offsetMax = new Vector2(4, -4);

      // Main title
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(titleContainer.transform, false);

      Text titleText = titleObj.AddComponent<Text>();
      titleText.text = "TRUCK-KUN";
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 76;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = TitleColor;
      titleText.alignment = TextAnchor.MiddleCenter;

      Outline titleOutline = titleObj.AddComponent<Outline>();
      titleOutline.effectColor = Color.black;
      titleOutline.effectDistance = new Vector2(3, -3);

      Shadow titleShadow = titleObj.AddComponent<Shadow>();
      titleShadow.effectColor = new Color(0, 0, 0, 0.5f);
      titleShadow.effectDistance = new Vector2(5, -5);

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 0.45f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.offsetMin = Vector2.zero;
      titleRect.offsetMax = Vector2.zero;

      // Subtitle "RISING"
      GameObject risingObj = new GameObject("Rising");
      risingObj.transform.SetParent(titleContainer.transform, false);

      Text risingText = risingObj.AddComponent<Text>();
      risingText.text = "RISING";
      risingText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      risingText.fontSize = 36;
      risingText.fontStyle = FontStyle.Bold;
      risingText.color = TitleGlowColor;
      risingText.alignment = TextAnchor.MiddleCenter;

      Outline risingOutline = risingObj.AddComponent<Outline>();
      risingOutline.effectColor = Color.black;
      risingOutline.effectDistance = new Vector2(2, -2);

      RectTransform risingRect = risingObj.GetComponent<RectTransform>();
      risingRect.anchorMin = new Vector2(0f, 0.25f);
      risingRect.anchorMax = new Vector2(1f, 0.5f);
      risingRect.offsetMin = Vector2.zero;
      risingRect.offsetMax = Vector2.zero;

      // Tagline
      GameObject taglineObj = new GameObject("Tagline");
      taglineObj.transform.SetParent(titleContainer.transform, false);

      Text taglineText = taglineObj.AddComponent<Text>();
      taglineText.text = "~ Isekai Express Delivery ~";
      taglineText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      taglineText.fontSize = 20;
      taglineText.fontStyle = FontStyle.Italic;
      taglineText.color = new Color(0.6f, 0.6f, 0.65f);
      taglineText.alignment = TextAnchor.MiddleCenter;

      RectTransform taglineRect = taglineObj.GetComponent<RectTransform>();
      taglineRect.anchorMin = new Vector2(0f, 0f);
      taglineRect.anchorMax = new Vector2(1f, 0.25f);
      taglineRect.offsetMin = Vector2.zero;
      taglineRect.offsetMax = Vector2.zero;
    }

    private Button CreateMenuButton(Transform parent, string label, Color color, System.Action onClick, string icon = null)
    {
      GameObject buttonObj = new GameObject("Button_" + label.Replace(" ", ""));
      buttonObj.transform.SetParent(parent, false);

      Image buttonBg = buttonObj.AddComponent<Image>();
      buttonBg.color = color;

      Button button = buttonObj.AddComponent<Button>();
      button.targetGraphic = buttonBg;

      ColorBlock colors = button.colors;
      colors.normalColor = color;
      colors.highlightedColor = color * 1.25f;
      colors.pressedColor = color * 0.75f;
      colors.disabledColor = new Color(0.25f, 0.25f, 0.25f, 0.5f);
      colors.fadeDuration = 0.1f;
      button.colors = colors;

      button.onClick.AddListener(() =>
      {
        PlayClickSound();
        onClick?.Invoke();
      });

      LayoutElement layout = buttonObj.AddComponent<LayoutElement>();
      layout.preferredHeight = 65f;

      // Button text
      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 30;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      Shadow textShadow = textObj.AddComponent<Shadow>();
      textShadow.effectColor = new Color(0, 0, 0, 0.5f);
      textShadow.effectDistance = new Vector2(2, -2);

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
      text.color = new Color(0.65f, 0.65f, 0.7f);
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

    private void CreateVersionText(Transform parent)
    {
      GameObject versionObj = new GameObject("VersionText");
      versionObj.transform.SetParent(parent, false);

      _versionText = versionObj.AddComponent<Text>();
      _versionText.text = $"v{Application.version} | Truck-kun Rising";
      _versionText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _versionText.fontSize = 16;
      _versionText.color = new Color(0.4f, 0.4f, 0.45f);
      _versionText.alignment = TextAnchor.LowerRight;

      RectTransform versionRect = versionObj.GetComponent<RectTransform>();
      versionRect.anchorMin = new Vector2(1f, 0f);
      versionRect.anchorMax = new Vector2(1f, 0f);
      versionRect.pivot = new Vector2(1f, 0f);
      versionRect.anchoredPosition = new Vector2(-20f, 20f);
      versionRect.sizeDelta = new Vector2(300f, 30f);
    }

    private void SetupMusic()
    {
      // Play main menu music through AudioService
      Code.Audio.Audio.PlayMusic(MusicType.MainMenu);
    }

    private void PlayClickSound()
    {
      // Simple click sound
      AudioSource.PlayClipAtPoint(CreateClickSound(), Camera.main?.transform.position ?? Vector3.zero,
        SettingsService.Instance.GetSFXVolume());
    }

    private AudioClip CreateClickSound()
    {
      int sampleRate = 44100;
      int samples = sampleRate / 10; // 0.1 second

      AudioClip clip = AudioClip.Create("Click", samples, 1, sampleRate, false);
      float[] data = new float[samples];

      for (int i = 0; i < samples; i++)
      {
        float t = (float)i / samples;
        float envelope = 1f - t;
        data[i] = Mathf.Sin(2 * Mathf.PI * 800f * t) * envelope * 0.3f;
      }

      clip.SetData(data, 0);
      return clip;
    }

    private void UpdateUI()
    {
      GameStateService state = GameStateService.Instance;
      bool hasSave = state.HasSaveData();

      // Show/hide continue button
      _continueButton.gameObject.SetActive(hasSave);

      if (hasSave)
      {
        _lastSaveText.text = $"День {state.DayNumber} | {state.PlayerMoney}¥";
        _lastSaveText.gameObject.SetActive(true);
      }
      else
      {
        _lastSaveText.gameObject.SetActive(false);
      }

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
      GameStateService.Instance.Load();
      SceneTransitionService.Instance.LoadHub();
    }

    private void OnNewGameClicked()
    {
      if (GameStateService.Instance.HasSaveData())
        ShowNewGameConfirmation();
      else
        StartNewGame();
    }

    private void OnSettingsClicked()
    {
      _settingsPanel.Show(_canvas.transform, () =>
      {
        // Settings closed - update UI in case save was deleted
        UpdateUI();
      }, showDeleteSave: true);
    }

    private void ShowNewGameConfirmation()
    {
      GameObject dialogObj = new GameObject("ConfirmDialog");
      dialogObj.transform.SetParent(_canvas.transform, false);

      // Overlay
      GameObject overlayObj = new GameObject("Overlay");
      overlayObj.transform.SetParent(dialogObj.transform, false);

      Image overlayImage = overlayObj.AddComponent<Image>();
      overlayImage.color = new Color(0f, 0f, 0f, 0.8f);

      RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
      overlayRect.anchorMin = Vector2.zero;
      overlayRect.anchorMax = Vector2.one;
      overlayRect.offsetMin = Vector2.zero;
      overlayRect.offsetMax = Vector2.zero;

      Button overlayButton = overlayObj.AddComponent<Button>();
      overlayButton.onClick.AddListener(() => Destroy(dialogObj));

      // Panel
      GameObject panelObj = new GameObject("Panel");
      panelObj.transform.SetParent(dialogObj.transform, false);

      Image panelImage = panelObj.AddComponent<Image>();
      panelImage.color = new Color(0.12f, 0.12f, 0.15f, 0.98f);

      RectTransform panelRect = panelObj.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(450f, 220f);

      // Warning text
      GameObject warningObj = new GameObject("WarningText");
      warningObj.transform.SetParent(panelObj.transform, false);

      Text warningText = warningObj.AddComponent<Text>();
      warningText.text = "Начать новую игру?\n\nТекущий прогресс будет потерян!";
      warningText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      warningText.fontSize = 24;
      warningText.color = Color.white;
      warningText.alignment = TextAnchor.MiddleCenter;

      RectTransform warningRect = warningObj.GetComponent<RectTransform>();
      warningRect.anchorMin = new Vector2(0f, 0.45f);
      warningRect.anchorMax = new Vector2(1f, 1f);
      warningRect.offsetMin = new Vector2(20f, 0f);
      warningRect.offsetMax = new Vector2(-20f, -15f);

      // Buttons
      GameObject buttonsObj = new GameObject("Buttons");
      buttonsObj.transform.SetParent(panelObj.transform, false);

      RectTransform buttonsRect = buttonsObj.AddComponent<RectTransform>();
      buttonsRect.anchorMin = new Vector2(0f, 0f);
      buttonsRect.anchorMax = new Vector2(1f, 0.45f);
      buttonsRect.offsetMin = new Vector2(20f, 15f);
      buttonsRect.offsetMax = new Vector2(-20f, 0f);

      HorizontalLayoutGroup buttonsLayout = buttonsObj.AddComponent<HorizontalLayoutGroup>();
      buttonsLayout.spacing = 20f;
      buttonsLayout.childAlignment = TextAnchor.MiddleCenter;
      buttonsLayout.childControlWidth = true;
      buttonsLayout.childControlHeight = true;
      buttonsLayout.childForceExpandWidth = true;

      CreateDialogButton(buttonsObj.transform, "ОТМЕНА", SettingsColor, () => Destroy(dialogObj));
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
      button.onClick.AddListener(() =>
      {
        PlayClickSound();
        onClick?.Invoke();
      });

      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
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

    private void OnDestroy()
    {
      // Music continues playing through AudioService
    }
  }
}
