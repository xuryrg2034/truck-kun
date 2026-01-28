#if UNITY_EDITOR
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Code.Editor
{
  /// <summary>
  /// Validates current physics settings against reference values.
  /// Access via menu: Tools → Physics → Validate Balance
  /// </summary>
  public static class PhysicsBalanceValidator
  {
    // Reference values (Normal difficulty)
    private static class Reference
    {
      // Movement
      public const float MinForwardSpeed = 5f;
      public const float MaxForwardSpeed = 8f;
      public const float MaxLateralSpeed = 8f;
      public const float ForwardAcceleration = 10f;
      public const float LateralAcceleration = 15f;
      public const float Deceleration = 8f;

      // Physics
      public const float BaseDrag = 0.5f;
      public const float Mass = 1000f;

      // Surfaces
      public const float OilFriction = 0.3f;
      public const float OilDrag = 0.5f;
      public const float GrassFriction = 0.8f;
      public const float GrassDrag = 1.8f;
      public const float IceFriction = 0.15f;
      public const float IceDrag = 0.3f;

      // Performance
      public const float TargetFPS = 60f;
      public const int MaxPedestrians = 12;
      public const int MaxRagdolls = 5;
    }

    [MenuItem("Tools/Physics/Validate Balance")]
    public static void ValidateBalance()
    {
      StringBuilder report = new StringBuilder();
      report.AppendLine("=== PHYSICS BALANCE VALIDATION ===\n");

      int warnings = 0;
      int errors = 0;

      // Check if in play mode
      if (!Application.isPlaying)
      {
        report.AppendLine("⚠️ Run in Play Mode for full validation\n");
        report.AppendLine("Checking static references only...\n");
      }

      // Validate surface triggers in scene
      report.AppendLine("--- Surface Triggers ---");
      var surfaceTriggers = Object.FindObjectsOfType<Code.Gameplay.Features.Surface.SurfaceTrigger>();

      if (surfaceTriggers.Length == 0)
      {
        report.AppendLine("⚠️ No surface triggers found in scene");
        warnings++;
      }
      else
      {
        report.AppendLine($"✓ Found {surfaceTriggers.Length} surface triggers");

        foreach (var trigger in surfaceTriggers)
        {
          var type = trigger.SurfaceType;
          float friction = trigger.FrictionMultiplier;
          float drag = trigger.DragMultiplier;

          string expected = GetExpectedValues(type);
          report.AppendLine($"  {trigger.name}: {type} (friction:{friction:F2}, drag:{drag:F2}) {expected}");
        }
      }

      report.AppendLine();

      // Check for physics bodies
      if (Application.isPlaying && Contexts.sharedInstance != null)
      {
        report.AppendLine("--- Runtime Validation ---");

        var game = Contexts.sharedInstance.game;
        var heroes = game.GetGroup(GameMatcher.Hero);
        var pedestrians = game.GetGroup(GameMatcher.Pedestrian);
        var ragdolls = game.GetGroup(GameMatcher.Ragdolled);

        int heroCount = heroes.count;
        int pedCount = pedestrians.count;
        int ragdollCount = ragdolls.count;

        report.AppendLine($"Heroes: {heroCount}");
        report.AppendLine($"Pedestrians: {pedCount} (max: {Reference.MaxPedestrians})");
        report.AppendLine($"Ragdolls: {ragdollCount} (max: {Reference.MaxRagdolls})");

        if (pedCount > Reference.MaxPedestrians)
        {
          report.AppendLine($"⚠️ Pedestrian count exceeds max ({pedCount} > {Reference.MaxPedestrians})");
          warnings++;
        }

        if (ragdollCount > Reference.MaxRagdolls)
        {
          report.AppendLine($"⚠️ Ragdoll count exceeds max ({ragdollCount} > {Reference.MaxRagdolls})");
          warnings++;
        }

        // Check hero physics
        foreach (var hero in heroes)
        {
          if (!hero.isPhysicsBody)
          {
            report.AppendLine("❌ Hero is not marked as PhysicsBody!");
            errors++;
          }

          if (!hero.hasRigidbody)
          {
            report.AppendLine("❌ Hero has no Rigidbody component!");
            errors++;
          }

          if (hero.hasPhysicsVelocity)
          {
            var vel = hero.physicsVelocity.Value;
            float speed = vel.magnitude;
            report.AppendLine($"Hero velocity: {vel} ({speed:F1} m/s)");

            if (speed > Reference.MaxForwardSpeed * 2)
            {
              report.AppendLine($"⚠️ Hero speed unusually high: {speed:F1} m/s");
              warnings++;
            }
          }

          if (hero.hasSurfaceModifier)
          {
            var surface = hero.surfaceModifier;
            report.AppendLine($"Current surface: {surface.SurfaceType} (friction:{surface.FrictionMultiplier:F2}, drag:{surface.DragMultiplier:F2})");
          }

          break;
        }
      }

      report.AppendLine();

      // Summary
      report.AppendLine("--- Summary ---");
      if (errors > 0)
        report.AppendLine($"❌ {errors} error(s) found");
      if (warnings > 0)
        report.AppendLine($"⚠️ {warnings} warning(s) found");
      if (errors == 0 && warnings == 0)
        report.AppendLine("✓ All checks passed!");

      report.AppendLine("\n--- Reference Values (Normal Difficulty) ---");
      report.AppendLine($"MinForwardSpeed: {Reference.MinForwardSpeed}");
      report.AppendLine($"MaxForwardSpeed: {Reference.MaxForwardSpeed}");
      report.AppendLine($"MaxLateralSpeed: {Reference.MaxLateralSpeed}");
      report.AppendLine($"ForwardAcceleration: {Reference.ForwardAcceleration}");
      report.AppendLine($"LateralAcceleration: {Reference.LateralAcceleration}");
      report.AppendLine($"BaseDrag: {Reference.BaseDrag}");
      report.AppendLine($"Mass: {Reference.Mass}");

      Debug.Log(report.ToString());
    }

    private static string GetExpectedValues(Code.Gameplay.Features.Physics.SurfaceType type)
    {
      return type switch
      {
        Code.Gameplay.Features.Physics.SurfaceType.Oil =>
          $"[expected: friction={Reference.OilFriction}, drag={Reference.OilDrag}]",
        Code.Gameplay.Features.Physics.SurfaceType.Grass =>
          $"[expected: friction={Reference.GrassFriction}, drag={Reference.GrassDrag}]",
        Code.Gameplay.Features.Physics.SurfaceType.Ice =>
          $"[expected: friction={Reference.IceFriction}, drag={Reference.IceDrag}]",
        _ => ""
      };
    }

    [MenuItem("Tools/Physics/Log Current Settings")]
    public static void LogCurrentSettings()
    {
      if (!Application.isPlaying)
      {
        Debug.LogWarning("[Balance] Run in Play Mode to see current settings");
        return;
      }

      if (Contexts.sharedInstance == null)
      {
        Debug.LogWarning("[Balance] Contexts not initialized");
        return;
      }

      var game = Contexts.sharedInstance.game;
      var heroes = game.GetGroup(GameMatcher.Hero);

      foreach (var hero in heroes)
      {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("=== CURRENT HERO PHYSICS STATE ===");

        if (hero.hasWorldPosition)
          sb.AppendLine($"Position: {hero.worldPosition.Value}");

        if (hero.hasPhysicsVelocity)
          sb.AppendLine($"Velocity: {hero.physicsVelocity.Value} ({hero.physicsVelocity.Value.magnitude:F1} m/s)");

        if (hero.hasSurfaceModifier)
        {
          var sm = hero.surfaceModifier;
          sb.AppendLine($"Surface: {sm.SurfaceType}");
          sb.AppendLine($"  Friction: {sm.FrictionMultiplier}");
          sb.AppendLine($"  Drag: {sm.DragMultiplier}");
        }

        if (hero.hasRigidbody)
        {
          var rb = hero.rigidbody.Value;
          sb.AppendLine($"Rigidbody:");
          sb.AppendLine($"  Mass: {rb.mass}");
          sb.AppendLine($"  Drag: {rb.linearDamping}");
          sb.AppendLine($"  Angular Drag: {rb.angularDamping}");
          sb.AppendLine($"  Interpolation: {rb.interpolation}");
          sb.AppendLine($"  Collision Detection: {rb.collisionDetectionMode}");
        }

        Debug.Log(sb.ToString());
        break;
      }
    }

    [MenuItem("Tools/Physics/Export Balance Snapshot")]
    public static void ExportSnapshot()
    {
      if (!Application.isPlaying)
      {
        Debug.LogWarning("[Balance] Run in Play Mode to export snapshot");
        return;
      }

      string timestamp = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
      string path = $"Assets/Balance/Snapshots/snapshot_{timestamp}.json";

      // Ensure directory exists
      System.IO.Directory.CreateDirectory("Assets/Balance/Snapshots");

      // Create snapshot data
      var snapshot = new BalanceSnapshot();

      if (Contexts.sharedInstance != null)
      {
        var game = Contexts.sharedInstance.game;

        // Entity counts
        snapshot.HeroCount = game.GetGroup(GameMatcher.Hero).count;
        snapshot.PedestrianCount = game.GetGroup(GameMatcher.Pedestrian).count;
        snapshot.RagdollCount = game.GetGroup(GameMatcher.Ragdolled).count;

        // Hero state
        foreach (var hero in game.GetGroup(GameMatcher.Hero))
        {
          if (hero.hasPhysicsVelocity)
          {
            snapshot.HeroSpeed = hero.physicsVelocity.Value.magnitude;
          }
          if (hero.hasSurfaceModifier)
          {
            snapshot.CurrentSurface = hero.surfaceModifier.SurfaceType.ToString();
            snapshot.CurrentFriction = hero.surfaceModifier.FrictionMultiplier;
            snapshot.CurrentDrag = hero.surfaceModifier.DragMultiplier;
          }
          break;
        }
      }

      // FPS
      snapshot.FPS = 1f / Time.unscaledDeltaTime;
      snapshot.Timestamp = timestamp;

      string json = JsonUtility.ToJson(snapshot, true);
      System.IO.File.WriteAllText(path, json);

      AssetDatabase.Refresh();
      Debug.Log($"[Balance] Snapshot exported to {path}");
    }

    [System.Serializable]
    private class BalanceSnapshot
    {
      public string Timestamp;
      public float FPS;
      public int HeroCount;
      public int PedestrianCount;
      public int RagdollCount;
      public float HeroSpeed;
      public string CurrentSurface;
      public float CurrentFriction;
      public float CurrentDrag;
    }
  }
}
#endif
