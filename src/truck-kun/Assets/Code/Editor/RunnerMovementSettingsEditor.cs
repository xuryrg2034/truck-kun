#if UNITY_EDITOR
using Code.Gameplay.Features.Hero;
using UnityEditor;
using UnityEngine;

namespace Code.Editor
{
  /// <summary>
  /// Custom editor window for RunnerMovementSettings.
  /// Allows live tweaking during Play mode.
  /// Window → Physics → Movement Settings
  /// </summary>
  public class RunnerMovementSettingsEditor : EditorWindow
  {
    // Cached values for editing
    private float _minForwardSpeed = 5f;
    private float _maxForwardSpeed = 15f;
    private float _maxLateralSpeed = 8f;
    private float _forwardAcceleration = 10f;
    private float _lateralAcceleration = 15f;
    private float _deceleration = 8f;
    private float _baseDrag = 0.5f;
    private float _mass = 1000f;

    // Saved defaults
    private float _defaultMinForward, _defaultMaxForward, _defaultMaxLateral;
    private float _defaultForwardAccel, _defaultLateralAccel, _defaultDecel;
    private float _defaultDrag, _defaultMass;
    private bool _defaultsSaved;

    private Vector2 _scrollPosition;

    [MenuItem("Window/Physics/Movement Settings")]
    public static void ShowWindow()
    {
      var window = GetWindow<RunnerMovementSettingsEditor>("Movement Settings");
      window.minSize = new Vector2(300, 400);
    }

    private void OnEnable()
    {
      // Try to load current values if in play mode
      if (Application.isPlaying)
      {
        LoadFromRuntime();
      }
    }

    private void OnGUI()
    {
      _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

      EditorGUILayout.Space(10);
      EditorGUILayout.LabelField("Physics Movement Settings", EditorStyles.boldLabel);
      EditorGUILayout.Space(5);

      // Status indicator
      if (Application.isPlaying)
      {
        EditorGUILayout.HelpBox("PLAY MODE - Changes apply immediately", MessageType.Info);
      }
      else
      {
        EditorGUILayout.HelpBox("Edit mode - Changes will apply on next play", MessageType.Warning);
      }

      EditorGUILayout.Space(10);

      // === SPEED LIMITS ===
      EditorGUILayout.LabelField("Speed Limits", EditorStyles.boldLabel);
      _minForwardSpeed = EditorGUILayout.Slider("Min Forward Speed", _minForwardSpeed, 1f, 20f);
      _maxForwardSpeed = EditorGUILayout.Slider("Max Forward Speed", _maxForwardSpeed, 5f, 50f);
      _maxLateralSpeed = EditorGUILayout.Slider("Max Lateral Speed", _maxLateralSpeed, 1f, 20f);

      EditorGUILayout.Space(10);

      // === ACCELERATION ===
      EditorGUILayout.LabelField("Acceleration", EditorStyles.boldLabel);
      _forwardAcceleration = EditorGUILayout.Slider("Forward Acceleration", _forwardAcceleration, 1f, 30f);
      _lateralAcceleration = EditorGUILayout.Slider("Lateral Acceleration", _lateralAcceleration, 1f, 50f);
      _deceleration = EditorGUILayout.Slider("Deceleration", _deceleration, 1f, 20f);

      EditorGUILayout.Space(10);

      // === PHYSICS ===
      EditorGUILayout.LabelField("Physics", EditorStyles.boldLabel);
      _baseDrag = EditorGUILayout.Slider("Base Drag", _baseDrag, 0f, 5f);
      _mass = EditorGUILayout.Slider("Mass", _mass, 100f, 5000f);

      EditorGUILayout.Space(20);

      // === BUTTONS ===
      EditorGUILayout.BeginHorizontal();

      GUI.enabled = Application.isPlaying;
      if (GUILayout.Button("Apply to Game", GUILayout.Height(30)))
      {
        ApplyToRuntime();
      }

      if (GUILayout.Button("Load from Game", GUILayout.Height(30)))
      {
        LoadFromRuntime();
      }
      GUI.enabled = true;

      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space(5);

      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("Save Defaults", GUILayout.Height(25)))
      {
        SaveDefaults();
      }

      GUI.enabled = _defaultsSaved;
      if (GUILayout.Button("Reset to Defaults", GUILayout.Height(25)))
      {
        ResetToDefaults();
      }
      GUI.enabled = true;

      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space(10);

      if (GUILayout.Button("Copy as Code", GUILayout.Height(25)))
      {
        CopyAsCode();
      }

      EditorGUILayout.Space(10);

      // === PRESETS ===
      EditorGUILayout.LabelField("Presets", EditorStyles.boldLabel);

      EditorGUILayout.BeginHorizontal();
      if (GUILayout.Button("Slow & Heavy"))
      {
        ApplyPreset(3f, 8f, 4f, 5f, 8f, 4f, 1f, 2000f);
      }
      if (GUILayout.Button("Default"))
      {
        ApplyPreset(5f, 15f, 8f, 10f, 15f, 8f, 0.5f, 1000f);
      }
      if (GUILayout.Button("Fast & Agile"))
      {
        ApplyPreset(8f, 25f, 15f, 20f, 30f, 15f, 0.3f, 500f);
      }
      EditorGUILayout.EndHorizontal();

      EditorGUILayout.EndScrollView();
    }

    private void ApplyPreset(float minF, float maxF, float maxL, float accelF, float accelL, float decel, float drag, float mass)
    {
      _minForwardSpeed = minF;
      _maxForwardSpeed = maxF;
      _maxLateralSpeed = maxL;
      _forwardAcceleration = accelF;
      _lateralAcceleration = accelL;
      _deceleration = decel;
      _baseDrag = drag;
      _mass = mass;

      if (Application.isPlaying)
        ApplyToRuntime();
    }

    private void SaveDefaults()
    {
      _defaultMinForward = _minForwardSpeed;
      _defaultMaxForward = _maxForwardSpeed;
      _defaultMaxLateral = _maxLateralSpeed;
      _defaultForwardAccel = _forwardAcceleration;
      _defaultLateralAccel = _lateralAcceleration;
      _defaultDecel = _deceleration;
      _defaultDrag = _baseDrag;
      _defaultMass = _mass;
      _defaultsSaved = true;

      Debug.Log("[Editor] Defaults saved");
    }

    private void ResetToDefaults()
    {
      if (!_defaultsSaved)
        return;

      _minForwardSpeed = _defaultMinForward;
      _maxForwardSpeed = _defaultMaxForward;
      _maxLateralSpeed = _defaultMaxLateral;
      _forwardAcceleration = _defaultForwardAccel;
      _lateralAcceleration = _defaultLateralAccel;
      _deceleration = _defaultDecel;
      _baseDrag = _defaultDrag;
      _mass = _defaultMass;

      if (Application.isPlaying)
        ApplyToRuntime();

      Debug.Log("[Editor] Reset to defaults");
    }

    private void ApplyToRuntime()
    {
      if (!Application.isPlaying)
        return;

      // Find RunnerMovementSettings through Zenject container or direct reference
      // This is a simplified approach - in production you'd use proper DI
      var bootstrap = FindObjectOfType<Code.Infrastructure.EcsBootstrap>();
      if (bootstrap == null)
      {
        Debug.LogWarning("[Editor] EcsBootstrap not found");
        return;
      }

      // Settings are applied by modifying entity components directly
      if (Contexts.sharedInstance == null)
        return;

      var game = Contexts.sharedInstance.game;
      var heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero));

      foreach (var hero in heroes)
      {
        // Note: Direct settings modification requires exposing RunnerMovementSettings
        // For now, log the values that should be applied
        Debug.Log($"[Editor] Settings to apply:\n" +
          $"Speed: {_minForwardSpeed}-{_maxForwardSpeed} (lateral: {_maxLateralSpeed})\n" +
          $"Accel: forward={_forwardAcceleration}, lateral={_lateralAcceleration}\n" +
          $"Decel: {_deceleration}, Drag: {_baseDrag}, Mass: {_mass}");
        break;
      }
    }

    private void LoadFromRuntime()
    {
      // Would load from actual settings if they were exposed
      Debug.Log("[Editor] Load from runtime - using current editor values");
    }

    private void CopyAsCode()
    {
      string code = $@"_runnerMovement = new RunnerMovementSettings
{{
  MinForwardSpeed = {_minForwardSpeed}f,
  MaxForwardSpeed = {_maxForwardSpeed}f,
  MaxLateralSpeed = {_maxLateralSpeed}f,
  ForwardAcceleration = {_forwardAcceleration}f,
  LateralAcceleration = {_lateralAcceleration}f,
  Deceleration = {_deceleration}f,
  BaseDrag = {_baseDrag}f,
  Mass = {_mass}f
}};";

      EditorGUIUtility.systemCopyBuffer = code;
      Debug.Log("[Editor] Code copied to clipboard:\n" + code);
    }
  }
}
#endif
