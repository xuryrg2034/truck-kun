using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Debugging
{
#if UNITY_EDITOR || DEVELOPMENT_BUILD
  /// <summary>
  /// Main controller that creates and manages all debug tools.
  /// Add to scene or it will be auto-created.
  ///
  /// Hotkeys:
  /// - F1: Toggle debug panel
  /// - F2: Toggle gizmos in Game view
  /// - F3: Log physics snapshot
  /// </summary>
  public class DebugPhysicsController : MonoBehaviour
  {
    [Header("Auto-Create Components")]
    [SerializeField] private bool _createVisualizer = true;
    [SerializeField] private bool _createUI = true;

    [Header("References (auto-filled)")]
    [SerializeField] private DebugPhysicsVisualizer _visualizer;
    [SerializeField] private DebugPhysicsUI _debugUI;

    private static DebugPhysicsController _instance;
    public static DebugPhysicsController Instance => _instance;

    public DebugPhysicsVisualizer Visualizer => _visualizer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoCreate()
    {
      // Only create if not already exists and in development
      if (_instance != null)
        return;

      if (FindObjectOfType<DebugPhysicsController>() != null)
        return;

      GameObject go = new GameObject("[DebugPhysics]");
      go.AddComponent<DebugPhysicsController>();
      DontDestroyOnLoad(go);

      UnityEngine.Debug.Log("[DebugPhysics] Auto-created debug controller. Press F1 for debug panel.");
    }

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }

      _instance = this;

      if (_createVisualizer && _visualizer == null)
      {
        _visualizer = gameObject.AddComponent<DebugPhysicsVisualizer>();
      }

      if (_createUI && _debugUI == null)
      {
        _debugUI = gameObject.AddComponent<DebugPhysicsUI>();
      }
    }

    private void Update()
    {
      Keyboard kb = Keyboard.current;
      if (kb == null)
        return;

      // F2 - Toggle Gizmos in Game view
      if (kb.f2Key.wasPressedThisFrame)
      {
        // Note: Gizmos in Game view require "Gizmos" button in Game view to be enabled
        UnityEngine.Debug.Log("[DebugPhysics] Gizmos visibility toggled. Make sure 'Gizmos' button is enabled in Game view.");
      }

      // F3 - Quick snapshot
      if (kb.f3Key.wasPressedThisFrame)
      {
        LogQuickSnapshot();
      }
    }

    private void LogQuickSnapshot()
    {
      if (Contexts.sharedInstance == null)
        return;

      var game = Contexts.sharedInstance.game;
      var heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));

      foreach (var hero in heroes)
      {
        Vector3 pos = hero.worldPosition.Value;
        Vector3 vel = hero.hasPhysicsVelocity ? hero.physicsVelocity.Value : Vector3.zero;

        string surface = "Normal";
        float friction = 1f, drag = 1f;
        if (hero.hasSurfaceModifier)
        {
          surface = hero.surfaceModifier.SurfaceType.ToString();
          friction = hero.surfaceModifier.FrictionMultiplier;
          drag = hero.surfaceModifier.DragMultiplier;
        }

        UnityEngine.Debug.Log(
          $"<color=cyan>[SNAPSHOT F3]</color>\n" +
          $"Position: {pos}\n" +
          $"Velocity: {vel} ({vel.magnitude:F1} m/s)\n" +
          $"Surface: {surface} (friction:{friction:F2}, drag:{drag:F2})");
        break;
      }
    }

    private void OnDestroy()
    {
      if (_instance == this)
        _instance = null;
    }
  }
#endif
}
