#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using UnityEngine.UI;

namespace Code.DevTools
{
  /// <summary>
  /// Small debug overlay showing FPS and hints
  /// Always visible in development builds
  /// </summary>
  public class DebugOverlay : MonoBehaviour
  {
    private static DebugOverlay _instance;

    private Text _fpsText;
    private Text _hintText;
    private Text _godModeText;

    private float _fps;
    private int _frameCount;
    private float _fpsAccumulator;
    private float _fpsTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
      if (_instance != null)
        return;

      GameObject go = new GameObject("[DebugOverlay]");
      _instance = go.AddComponent<DebugOverlay>();
      DontDestroyOnLoad(go);
    }

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;
      CreateUI();

      DebugService.OnGodModeChanged += OnGodModeChanged;
    }

    private void OnDestroy()
    {
      DebugService.OnGodModeChanged -= OnGodModeChanged;

      if (_instance == this)
        _instance = null;
    }

    private void Update()
    {
      _frameCount++;
      _fpsAccumulator += Time.unscaledDeltaTime;
      _fpsTimer += Time.unscaledDeltaTime;

      if (_fpsTimer >= 0.5f)
      {
        _fps = _frameCount / _fpsAccumulator;
        _frameCount = 0;
        _fpsAccumulator = 0f;
        _fpsTimer = 0f;

        UpdateFPSDisplay();
      }
    }

    private void UpdateFPSDisplay()
    {
      if (_fpsText != null)
      {
        _fpsText.text = $"FPS: {_fps:F0}";
        _fpsText.color = _fps >= 60 ? Color.green : (_fps >= 30 ? Color.yellow : Color.red);
      }
    }

    private void OnGodModeChanged(bool enabled)
    {
      if (_godModeText != null)
      {
        _godModeText.gameObject.SetActive(enabled);
      }
    }

    private void CreateUI()
    {
      // Canvas
      GameObject canvasObj = new GameObject("DebugOverlayCanvas");
      canvasObj.transform.SetParent(transform, false);

      Canvas canvas = canvasObj.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.ScreenSpaceOverlay;
      canvas.sortingOrder = 9998;

      CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
      scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
      scaler.referenceResolution = new Vector2(1920f, 1080f);

      // FPS Text (top-right corner)
      GameObject fpsObj = new GameObject("FPS");
      fpsObj.transform.SetParent(canvasObj.transform, false);

      _fpsText = fpsObj.AddComponent<Text>();
      _fpsText.text = "FPS: --";
      _fpsText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _fpsText.fontSize = 16;
      _fpsText.color = Color.green;
      _fpsText.alignment = TextAnchor.UpperRight;

      Outline fpsOutline = fpsObj.AddComponent<Outline>();
      fpsOutline.effectColor = Color.black;
      fpsOutline.effectDistance = new Vector2(1, -1);

      RectTransform fpsRect = fpsObj.GetComponent<RectTransform>();
      fpsRect.anchorMin = new Vector2(1f, 1f);
      fpsRect.anchorMax = new Vector2(1f, 1f);
      fpsRect.pivot = new Vector2(1f, 1f);
      fpsRect.anchoredPosition = new Vector2(-10f, -10f);
      fpsRect.sizeDelta = new Vector2(100f, 25f);

      // Hint Text (top-right, below FPS)
      GameObject hintObj = new GameObject("Hint");
      hintObj.transform.SetParent(canvasObj.transform, false);

      _hintText = hintObj.AddComponent<Text>();
      _hintText.text = "F1: Debug";
      _hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _hintText.fontSize = 12;
      _hintText.color = new Color(0.7f, 0.7f, 0.7f, 0.7f);
      _hintText.alignment = TextAnchor.UpperRight;

      Outline hintOutline = hintObj.AddComponent<Outline>();
      hintOutline.effectColor = Color.black;
      hintOutline.effectDistance = new Vector2(1, -1);

      RectTransform hintRect = hintObj.GetComponent<RectTransform>();
      hintRect.anchorMin = new Vector2(1f, 1f);
      hintRect.anchorMax = new Vector2(1f, 1f);
      hintRect.pivot = new Vector2(1f, 1f);
      hintRect.anchoredPosition = new Vector2(-10f, -32f);
      hintRect.sizeDelta = new Vector2(100f, 20f);

      // God Mode indicator (top-center)
      GameObject godModeObj = new GameObject("GodMode");
      godModeObj.transform.SetParent(canvasObj.transform, false);

      _godModeText = godModeObj.AddComponent<Text>();
      _godModeText.text = "GOD MODE";
      _godModeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      _godModeText.fontSize = 20;
      _godModeText.fontStyle = FontStyle.Bold;
      _godModeText.color = new Color(1f, 0.8f, 0.2f);
      _godModeText.alignment = TextAnchor.UpperCenter;

      Outline godOutline = godModeObj.AddComponent<Outline>();
      godOutline.effectColor = Color.black;
      godOutline.effectDistance = new Vector2(2, -2);

      RectTransform godRect = godModeObj.GetComponent<RectTransform>();
      godRect.anchorMin = new Vector2(0.5f, 1f);
      godRect.anchorMax = new Vector2(0.5f, 1f);
      godRect.pivot = new Vector2(0.5f, 1f);
      godRect.anchoredPosition = new Vector2(0f, -10f);
      godRect.sizeDelta = new Vector2(200f, 30f);

      godModeObj.SetActive(DebugService.GodModeEnabled);
    }
  }
}
#endif
