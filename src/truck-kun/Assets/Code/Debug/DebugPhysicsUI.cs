using System;
using Code.DevTools;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Physics;
using Code.Infrastructure;
using Entitas;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.DebugTools
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  /// <summary>
  /// Runtime debug UI panel for physics tweaking.
  /// Press F1 to toggle visibility.
  /// </summary>
  public class DebugPhysicsUI : MonoBehaviour
  {
    [Header("Toggle")]
    [SerializeField] private Key _toggleKey = Key.F1;

    [Header("Style")]
    [SerializeField] private int _fontSize = 14;

    private bool _isVisible = false;
    private Rect _windowRect = new Rect(10, 10, 320, 700);

    // Cached references
    private GameContext _game;
    private IGroup<GameEntity> _heroes;
    private RunnerMovementSettings _movementSettings;

    // Live tweaking values
    private float _forwardAcceleration;
    private float _lateralAcceleration;
    private float _baseDrag;
    private float _deceleration;
    private float _testFriction = 1f;
    private float _testDrag = 1f;

    // Performance stats
    private float _fps;
    private float _fpsUpdateTime;
    private int _frameCount;
    private int _activeRigidbodies;
    private float _physicsTime;

    // Current values (read-only display)
    private Vector3 _velocity;
    private float _forwardSpeed;
    private float _lateralSpeed;
    private SurfaceType _currentSurface;
    private float _currentFriction;
    private float _currentDrag;

    private GUIStyle _headerStyle;
    private GUIStyle _labelStyle;
    private GUIStyle _valueStyle;
    private bool _stylesInitialized;
    private Vector2 _scrollPosition;

    private void Start()
    {
      if (Contexts.sharedInstance == null)
        return;

      _game = Contexts.sharedInstance.game;
      _heroes = _game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.WorldPosition));

      // Try to find settings via reflection or static reference
      FindMovementSettings();
    }

    private void FindMovementSettings()
    {
      // Look for EcsBootstrap to get settings reference
      var bootstrap = FindObjectOfType<Code.Infrastructure.EcsBootstrap>();
      if (bootstrap != null)
      {
        // Settings are private, we'll use default values for sliders
        // and apply changes via the hero entity directly
        _forwardAcceleration = 10f;
        _lateralAcceleration = 15f;
        _baseDrag = 0.5f;
        _deceleration = 8f;
      }
    }

    private void Update()
    {
      Keyboard kb = Keyboard.current;
      if (kb == null)
        return;

      // Toggle visibility (F1)
      if (kb[_toggleKey].wasPressedThisFrame)
      {
        _isVisible = !_isVisible;
        UnityEngine.Debug.Log($"[DebugUI] Panel {(_isVisible ? "shown" : "hidden")} (F1 to toggle)");
      }

      // Cheat hotkeys (always active)
      if (kb.f5Key.wasPressedThisFrame)
        DebugService.AddMoney(1000);

      if (kb.f6Key.wasPressedThisFrame)
        DebugService.CompleteAllQuests();

      if (kb.f7Key.wasPressedThisFrame)
        DebugService.ToggleGodMode();

      if (kb.f8Key.wasPressedThisFrame)
        DebugService.SkipToDay(GameStateService.Instance.DayNumber + 1);

      // Update stats
      UpdatePerformanceStats();
      if (_isVisible)
        UpdateHeroStats();
    }

    private void UpdatePerformanceStats()
    {
      // FPS calculation
      _frameCount++;
      if (Time.unscaledTime > _fpsUpdateTime + 0.5f)
      {
        _fps = _frameCount / (Time.unscaledTime - _fpsUpdateTime);
        _frameCount = 0;
        _fpsUpdateTime = Time.unscaledTime;

        // Count active rigidbodies
        _activeRigidbodies = FindObjectsOfType<Rigidbody>().Length;
      }

      // Physics time (approximate)
      _physicsTime = Time.fixedDeltaTime * 1000f;
    }

    private void UpdateHeroStats()
    {
      if (_heroes == null)
        return;

      foreach (GameEntity hero in _heroes)
      {
        if (hero.hasPhysicsVelocity)
          _velocity = hero.physicsVelocity.Value;
        else
          _velocity = Vector3.zero;

        _forwardSpeed = _velocity.z;
        _lateralSpeed = _velocity.x;

        if (hero.hasSurfaceModifier)
        {
          _currentSurface = hero.surfaceModifier.SurfaceType;
          _currentFriction = hero.surfaceModifier.FrictionMultiplier;
          _currentDrag = hero.surfaceModifier.DragMultiplier;
        }
        else
        {
          _currentSurface = SurfaceType.Normal;
          _currentFriction = 1f;
          _currentDrag = 1f;
        }

        break;
      }
    }

    private void OnGUI()
    {
      if (!_isVisible)
        return;

      InitStyles();

      _windowRect = GUI.Window(12345, _windowRect, DrawWindow, "Physics Debug (F1)");
    }

    private void InitStyles()
    {
      if (_stylesInitialized)
        return;

      _headerStyle = new GUIStyle(GUI.skin.label)
      {
        fontSize = _fontSize + 2,
        fontStyle = FontStyle.Bold,
        normal = { textColor = Color.cyan }
      };

      _labelStyle = new GUIStyle(GUI.skin.label)
      {
        fontSize = _fontSize,
        normal = { textColor = Color.white }
      };

      _valueStyle = new GUIStyle(GUI.skin.label)
      {
        fontSize = _fontSize,
        fontStyle = FontStyle.Bold,
        normal = { textColor = Color.yellow }
      };

      _stylesInitialized = true;
    }

    private void DrawWindow(int windowId)
    {
      // Scrollable content
      _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

      GUILayout.Space(5);

      // === PERFORMANCE ===
      DrawHeader("PERFORMANCE");
      DrawValueRow("FPS", $"{_fps:F0}");
      DrawValueRow("Physics Step", $"{_physicsTime:F2} ms");
      DrawValueRow("Rigidbodies", $"{_activeRigidbodies}");

      GUILayout.Space(10);

      // === CURRENT VALUES ===
      DrawHeader("CURRENT VALUES");
      DrawValueRow("Velocity X", $"{_lateralSpeed:F2} m/s");
      DrawValueRow("Velocity Z", $"{_forwardSpeed:F2} m/s");
      DrawValueRow("Total Speed", $"{_velocity.magnitude:F2} m/s");

      GUILayout.Space(5);

      Color surfaceColor = GetSurfaceGUIColor(_currentSurface);
      GUI.contentColor = surfaceColor;
      DrawValueRow("Surface", _currentSurface.ToString());
      GUI.contentColor = Color.white;

      DrawValueRow("Friction", $"{_currentFriction:F2}");
      DrawValueRow("Drag", $"{_currentDrag:F2}");

      GUILayout.Space(10);

      // === LIVE TWEAKING ===
      DrawHeader("LIVE TWEAKING");

      GUILayout.BeginHorizontal();
      GUILayout.Label("Forward Accel:", _labelStyle, GUILayout.Width(100));
      _forwardAcceleration = GUILayout.HorizontalSlider(_forwardAcceleration, 1f, 30f, GUILayout.Width(120));
      GUILayout.Label($"{_forwardAcceleration:F1}", _valueStyle, GUILayout.Width(50));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Lateral Accel:", _labelStyle, GUILayout.Width(100));
      _lateralAcceleration = GUILayout.HorizontalSlider(_lateralAcceleration, 1f, 40f, GUILayout.Width(120));
      GUILayout.Label($"{_lateralAcceleration:F1}", _valueStyle, GUILayout.Width(50));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Deceleration:", _labelStyle, GUILayout.Width(100));
      _deceleration = GUILayout.HorizontalSlider(_deceleration, 1f, 20f, GUILayout.Width(120));
      GUILayout.Label($"{_deceleration:F1}", _valueStyle, GUILayout.Width(50));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Base Drag:", _labelStyle, GUILayout.Width(100));
      _baseDrag = GUILayout.HorizontalSlider(_baseDrag, 0f, 5f, GUILayout.Width(120));
      GUILayout.Label($"{_baseDrag:F2}", _valueStyle, GUILayout.Width(50));
      GUILayout.EndHorizontal();

      GUILayout.Space(10);

      // === TEST SURFACE ===
      DrawHeader("TEST SURFACE");

      GUILayout.BeginHorizontal();
      GUILayout.Label("Friction:", _labelStyle, GUILayout.Width(100));
      _testFriction = GUILayout.HorizontalSlider(_testFriction, 0.1f, 2f, GUILayout.Width(120));
      GUILayout.Label($"{_testFriction:F2}", _valueStyle, GUILayout.Width(50));
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Drag:", _labelStyle, GUILayout.Width(100));
      _testDrag = GUILayout.HorizontalSlider(_testDrag, 0.1f, 10f, GUILayout.Width(120));
      GUILayout.Label($"{_testDrag:F2}", _valueStyle, GUILayout.Width(50));
      GUILayout.EndHorizontal();

      GUILayout.Space(5);

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Apply Test Surface", GUILayout.Height(25)))
      {
        ApplyTestSurface();
      }
      if (GUILayout.Button("Reset", GUILayout.Height(25)))
      {
        ResetSurface();
      }
      GUILayout.EndHorizontal();

      GUILayout.Space(10);

      // === PRESETS ===
      DrawHeader("SURFACE PRESETS");

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Normal")) ApplySurfacePreset(SurfaceType.Normal);
      if (GUILayout.Button("Oil")) ApplySurfacePreset(SurfaceType.Oil);
      if (GUILayout.Button("Grass")) ApplySurfacePreset(SurfaceType.Grass);
      if (GUILayout.Button("Ice")) ApplySurfacePreset(SurfaceType.Ice);
      GUILayout.EndHorizontal();

      GUILayout.Space(10);

      // === GAME STATE ===
      DrawHeader("GAME STATE");
      GameStateService state = GameStateService.Instance;
      DrawValueRow("Day", $"{state.DayNumber}");
      DrawValueRow("Money", $"{state.PlayerMoney} ¥");
      bool godMode = DebugService.GodModeEnabled;
      GUI.contentColor = godMode ? Color.yellow : Color.white;
      DrawValueRow("God Mode", godMode ? "ON" : "OFF");
      GUI.contentColor = Color.white;

      GUILayout.Space(10);

      // === CHEATS ===
      DrawHeader("CHEATS (Hotkeys)");
      GUILayout.BeginHorizontal();
      if (GUILayout.Button("+1000¥ (F5)")) DebugService.AddMoney(1000);
      if (GUILayout.Button("+10000¥")) DebugService.AddMoney(10000);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("God Mode (F7)")) DebugService.ToggleGodMode();
      if (GUILayout.Button("Skip Day (F8)")) DebugService.SkipToDay(state.DayNumber + 1);
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      if (GUILayout.Button("Complete Quests (F6)")) DebugService.CompleteAllQuests();
      if (GUILayout.Button("Max Upgrades")) DebugService.MaxUpgrades();
      GUILayout.EndHorizontal();

      GUILayout.Space(5);
      GUI.backgroundColor = new Color(0.7f, 0.2f, 0.2f);
      if (GUILayout.Button("RESET SAVE", GUILayout.Height(25)))
      {
        DebugService.ResetSave();
      }
      GUI.backgroundColor = Color.white;

      GUILayout.Space(10);

      // === ACTIONS ===
      if (GUILayout.Button("Log Current Settings", GUILayout.Height(25)))
      {
        LogCurrentSettings();
      }

      GUILayout.EndScrollView();

      // Make window draggable
      GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }

    private void DrawHeader(string text)
    {
      GUILayout.Label(text, _headerStyle);
    }

    private void DrawValueRow(string label, string value)
    {
      GUILayout.BeginHorizontal();
      GUILayout.Label(label + ":", _labelStyle, GUILayout.Width(100));
      GUILayout.Label(value, _valueStyle);
      GUILayout.EndHorizontal();
    }

    private void ApplyTestSurface()
    {
      if (_heroes == null)
        return;

      foreach (GameEntity hero in _heroes)
      {
        if (hero.hasSurfaceModifier)
        {
          hero.ReplaceSurfaceModifier(_testFriction, _testDrag, SurfaceType.Normal);
          UnityEngine.Debug.Log($"[DebugUI] Applied test surface: friction={_testFriction:F2}, drag={_testDrag:F2}");
        }
        break;
      }
    }

    private void ResetSurface()
    {
      _testFriction = 1f;
      _testDrag = 1f;
      ApplyTestSurface();
    }

    private void ApplySurfacePreset(SurfaceType type)
    {
      var (friction, drag) = Code.Gameplay.Features.Surface.SurfaceTrigger.GetDefaultModifiers(type);
      _testFriction = friction;
      _testDrag = drag;

      if (_heroes == null)
        return;

      foreach (GameEntity hero in _heroes)
      {
        if (hero.hasSurfaceModifier)
        {
          hero.ReplaceSurfaceModifier(friction, drag, type);
          UnityEngine.Debug.Log($"[DebugUI] Applied {type} preset: friction={friction:F2}, drag={drag:F2}");
        }
        break;
      }
    }

    private void LogCurrentSettings()
    {
      UnityEngine.Debug.Log($"=== PHYSICS DEBUG SNAPSHOT ===\n" +
        $"Velocity: ({_velocity.x:F2}, {_velocity.y:F2}, {_velocity.z:F2})\n" +
        $"Speed: {_velocity.magnitude:F2} m/s\n" +
        $"Surface: {_currentSurface}\n" +
        $"Friction: {_currentFriction:F2}\n" +
        $"Drag: {_currentDrag:F2}\n" +
        $"Tweaked Accel: forward={_forwardAcceleration:F1}, lateral={_lateralAcceleration:F1}\n" +
        $"Tweaked Decel: {_deceleration:F1}\n" +
        $"Tweaked Drag: {_baseDrag:F2}");
    }

    private Color GetSurfaceGUIColor(SurfaceType type)
    {
      return type switch
      {
        SurfaceType.Oil => new Color(0.3f, 0.3f, 0.3f),
        SurfaceType.Grass => Color.green,
        SurfaceType.Ice => Color.cyan,
        SurfaceType.Puddle => new Color(0.4f, 0.6f, 1f),
        _ => Color.white
      };
    }
  }
#endif
}
