using Code.Infrastructure;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Code.UI.Settings
{
  /// <summary>
  /// Pause menu that appears when ESC is pressed during gameplay
  /// </summary>
  public class PauseMenu : MonoBehaviour
  {
    private static PauseMenu _instance;

    private static readonly Color ButtonColor = new Color(0.25f, 0.5f, 0.75f);
    private static readonly Color SettingsColor = new Color(0.5f, 0.45f, 0.55f);
    private static readonly Color DangerColor = new Color(0.75f, 0.25f, 0.25f);
    private static readonly Color PanelColor = new Color(0.1f, 0.1f, 0.12f, 0.95f);

    private Canvas _canvas;
    private GameObject _menuPanel;
    private SettingsPanel _settingsPanel;
    private bool _isPaused;

    public static bool IsPaused => _instance != null && _instance._isPaused;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
      // Only create in gameplay scenes (not main menu or hub)
      string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
      if (sceneName == "MainMenuScene" || sceneName == "HubScene")
        return;

      if (_instance != null)
        return;

      GameObject go = new GameObject("[PauseMenu]");
      _instance = go.AddComponent<PauseMenu>();
      DontDestroyOnLoad(go);

      UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
      // Destroy pause menu when returning to main menu
      if (scene.name == "MainMenuScene" && _instance != null)
      {
        Destroy(_instance.gameObject);
        _instance = null;
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
      }
    }

    private void Awake()
    {
      _settingsPanel = gameObject.AddComponent<SettingsPanel>();
      CreateUI();
    }

    private void Update()
    {
      // Check for ESC key using new Input System
      if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
      {
        if (_settingsPanel.IsOpen)
        {
          _settingsPanel.Hide();
        }
        else if (_isPaused)
        {
          Resume();
        }
        else
        {
          Pause();
        }
      }
    }

    private void CreateUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("PauseCanvas");
      canvasObj.transform.SetParent(transform, false);

      _canvas = canvasObj.AddComponent<Canvas>();
      _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      _canvas.sortingOrder = 1000;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);
      scaler.matchWidthOrHeight = 0.5f;

      canvasObj.AddComponent<GraphicRaycaster>();

      // Menu panel (hidden by default)
      CreateMenuPanel(canvasObj.transform);

      _canvas.gameObject.SetActive(false);
    }

    private void CreateMenuPanel(Transform parent)
    {
      _menuPanel = new GameObject("MenuPanel");
      _menuPanel.transform.SetParent(parent, false);

      // Overlay
      GameObject overlayObj = new GameObject("Overlay");
      overlayObj.transform.SetParent(_menuPanel.transform, false);

      Image overlayImage = overlayObj.AddComponent<Image>();
      overlayImage.color = new Color(0f, 0f, 0f, 0.7f);

      RectTransform overlayRect = overlayObj.GetComponent<RectTransform>();
      overlayRect.anchorMin = Vector2.zero;
      overlayRect.anchorMax = Vector2.one;
      overlayRect.offsetMin = Vector2.zero;
      overlayRect.offsetMax = Vector2.zero;

      // Panel
      GameObject panelObj = new GameObject("Panel");
      panelObj.transform.SetParent(_menuPanel.transform, false);

      Image panelImage = panelObj.AddComponent<Image>();
      panelImage.color = PanelColor;

      RectTransform panelRect = panelObj.GetComponent<RectTransform>();
      panelRect.anchorMin = new Vector2(0.5f, 0.5f);
      panelRect.anchorMax = new Vector2(0.5f, 0.5f);
      panelRect.sizeDelta = new Vector2(400f, 380f);

      // Title
      GameObject titleObj = new GameObject("Title");
      titleObj.transform.SetParent(panelObj.transform, false);

      Text titleText = titleObj.AddComponent<Text>();
      titleText.text = "ПАУЗА";
      titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      titleText.fontSize = 42;
      titleText.fontStyle = FontStyle.Bold;
      titleText.color = Color.white;
      titleText.alignment = TextAnchor.MiddleCenter;

      RectTransform titleRect = titleObj.GetComponent<RectTransform>();
      titleRect.anchorMin = new Vector2(0f, 0.82f);
      titleRect.anchorMax = new Vector2(1f, 1f);
      titleRect.offsetMin = Vector2.zero;
      titleRect.offsetMax = Vector2.zero;

      // Buttons container
      GameObject buttonsObj = new GameObject("Buttons");
      buttonsObj.transform.SetParent(panelObj.transform, false);

      RectTransform buttonsRect = buttonsObj.AddComponent<RectTransform>();
      buttonsRect.anchorMin = new Vector2(0.1f, 0.08f);
      buttonsRect.anchorMax = new Vector2(0.9f, 0.8f);
      buttonsRect.offsetMin = Vector2.zero;
      buttonsRect.offsetMax = Vector2.zero;

      VerticalLayoutGroup layout = buttonsObj.AddComponent<VerticalLayoutGroup>();
      layout.spacing = 15f;
      layout.childAlignment = TextAnchor.MiddleCenter;
      layout.childControlWidth = true;
      layout.childControlHeight = false;
      layout.childForceExpandWidth = true;
      layout.padding = new RectOffset(10, 10, 10, 10);

      // Buttons
      CreateMenuButton(buttonsObj.transform, "ПРОДОЛЖИТЬ", ButtonColor, Resume);
      CreateMenuButton(buttonsObj.transform, "НАСТРОЙКИ", SettingsColor, OpenSettings);
      CreateMenuButton(buttonsObj.transform, "В ГЛАВНОЕ МЕНЮ", DangerColor, ReturnToMainMenu);
    }

    private void CreateMenuButton(Transform parent, string label, Color color, System.Action onClick)
    {
      GameObject buttonObj = new GameObject("Button_" + label);
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
      layout.preferredHeight = 60f;

      GameObject textObj = new GameObject("Text");
      textObj.transform.SetParent(buttonObj.transform, false);

      Text text = textObj.AddComponent<Text>();
      text.text = label;
      text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      text.fontSize = 26;
      text.fontStyle = FontStyle.Bold;
      text.color = Color.white;
      text.alignment = TextAnchor.MiddleCenter;

      Shadow shadow = textObj.AddComponent<Shadow>();
      shadow.effectColor = new Color(0, 0, 0, 0.5f);
      shadow.effectDistance = new Vector2(2, -2);

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.anchorMin = Vector2.zero;
      textRect.anchorMax = Vector2.one;
      textRect.offsetMin = Vector2.zero;
      textRect.offsetMax = Vector2.zero;
    }

    public void Pause()
    {
      _isPaused = true;
      Time.timeScale = 0f;
      _canvas.gameObject.SetActive(true);
      _menuPanel.SetActive(true);

      Debug.Log("[PauseMenu] Game paused");
    }

    public void Resume()
    {
      _isPaused = false;
      Time.timeScale = 1f;
      _canvas.gameObject.SetActive(false);

      Debug.Log("[PauseMenu] Game resumed");
    }

    private void OpenSettings()
    {
      _menuPanel.SetActive(false);
      _settingsPanel.Show(_canvas.transform, () =>
      {
        _menuPanel.SetActive(true);
      });
    }

    private void ReturnToMainMenu()
    {
      Resume(); // Restore time scale

      // Save game state before leaving
      GameStateService.Instance.Save();

      // Load main menu
      SceneTransitionService.Instance.LoadMainMenu();
    }

    private void OnDestroy()
    {
      // Make sure time scale is restored
      Time.timeScale = 1f;

      if (_instance == this)
        _instance = null;
    }
  }
}
