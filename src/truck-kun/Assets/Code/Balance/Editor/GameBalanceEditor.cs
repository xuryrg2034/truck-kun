#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Code.Balance.Editor
{
  [CustomEditor(typeof(GameBalance))]
  public class GameBalanceEditor : UnityEditor.Editor
  {
    private GameBalance _balance;

    private bool _movementFoldout = true;
    private bool _pedestriansFoldout = true;
    private bool _pedestrianTypesFoldout;
    private bool _economyFoldout = true;
    private bool _dayFoldout = true;
    private bool _questTargetsFoldout;
    private bool _upgradesFoldout = true;
    private bool _upgradeListFoldout;
    private bool _feedbackFoldout = true;

    private static readonly Color HeaderColor = new Color(0.2f, 0.6f, 0.9f);
    private static readonly Color SectionBgColor = new Color(0.18f, 0.18f, 0.22f);

    private void OnEnable()
    {
      _balance = (GameBalance)target;
    }

    public override void OnInspectorGUI()
    {
      serializedObject.Update();

      DrawHeader();

      EditorGUILayout.Space(10);

      DrawMovementSection();
      DrawPedestriansSection();
      DrawEconomySection();
      DrawDaySection();
      DrawUpgradesSection();
      DrawFeedbackSection();

      EditorGUILayout.Space(20);
      DrawButtons();

      serializedObject.ApplyModifiedProperties();
    }

    private void DrawHeader()
    {
      EditorGUILayout.BeginVertical(EditorStyles.helpBox);

      GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
      {
        fontSize = 18,
        alignment = TextAnchor.MiddleCenter
      };

      EditorGUILayout.LabelField("TRUCK-KUN GAME BALANCE", titleStyle);
      EditorGUILayout.LabelField("Centralized balance configuration", EditorStyles.centeredGreyMiniLabel);

      EditorGUILayout.EndVertical();
    }

    private void DrawMovementSection()
    {
      _movementFoldout = DrawSectionHeader("Movement & Physics", _movementFoldout);

      if (_movementFoldout)
      {
        EditorGUI.indentLevel++;

        SerializedProperty movement = serializedObject.FindProperty("Movement");

        EditorGUILayout.LabelField("Hero Movement", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(movement.FindPropertyRelative("ForwardSpeed"),
          new GUIContent("Forward Speed", "Speed of hero moving forward"));
        EditorGUILayout.PropertyField(movement.FindPropertyRelative("LateralSpeed"),
          new GUIContent("Lateral Speed", "Speed of hero moving sideways"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Road", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(movement.FindPropertyRelative("RoadWidth"),
          new GUIContent("Road Width", "Width of the playable road"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Collision", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(movement.FindPropertyRelative("HitRadius"),
          new GUIContent("Hit Radius", "Radius for detecting hits on pedestrians"));

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
      }
    }

    private void DrawPedestriansSection()
    {
      _pedestriansFoldout = DrawSectionHeader("Pedestrians", _pedestriansFoldout);

      if (_pedestriansFoldout)
      {
        EditorGUI.indentLevel++;

        SerializedProperty pedestrians = serializedObject.FindProperty("Pedestrians");

        EditorGUILayout.LabelField("Spawning", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("SpawnInterval"));
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("SpawnDistanceAhead"));
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("DespawnDistanceBehind"));
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("MaxActive"));
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("LateralMargin"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Crossing", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("CrossingChance"));
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("CrossingSpeedMultiplier"));
        EditorGUILayout.PropertyField(pedestrians.FindPropertyRelative("SidewalkOffset"));

        EditorGUILayout.Space(5);

        // Pedestrian Types
        _pedestrianTypesFoldout = EditorGUILayout.Foldout(_pedestrianTypesFoldout, "Pedestrian Types", true);
        if (_pedestrianTypesFoldout)
        {
          SerializedProperty typeBalances = pedestrians.FindPropertyRelative("TypeBalances");

          // Calculate total weight
          float totalWeight = 0f;
          for (int i = 0; i < typeBalances.arraySize; i++)
          {
            totalWeight += typeBalances.GetArrayElementAtIndex(i).FindPropertyRelative("SpawnWeight").floatValue;
          }

          if (Mathf.Abs(totalWeight - 1f) > 0.01f)
          {
            EditorGUILayout.HelpBox($"Spawn weights sum to {totalWeight:F2}. Should be 1.0", MessageType.Warning);
          }

          EditorGUILayout.PropertyField(typeBalances, true);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
      }
    }

    private void DrawEconomySection()
    {
      _economyFoldout = DrawSectionHeader("Economy", _economyFoldout);

      if (_economyFoldout)
      {
        EditorGUI.indentLevel++;

        SerializedProperty economy = serializedObject.FindProperty("Economy");

        EditorGUILayout.LabelField("Starting", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(economy.FindPropertyRelative("StartingMoney"),
          new GUIContent("Starting Money", "Initial player balance"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Daily Costs", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(economy.FindPropertyRelative("DailyFoodCost"),
          new GUIContent("Food Cost", "Daily food cost"));
        EditorGUILayout.PropertyField(economy.FindPropertyRelative("MinimumRequiredMoney"),
          new GUIContent("Min Required", "Minimum money required (Game Over threshold)"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Penalties & Rewards", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(economy.FindPropertyRelative("ViolationPenalty"),
          new GUIContent("Violation Penalty", "Penalty for hitting protected pedestrians"));
        EditorGUILayout.PropertyField(economy.FindPropertyRelative("BaseQuestReward"),
          new GUIContent("Base Quest Reward", "Base reward for completing quests"));
        EditorGUILayout.PropertyField(economy.FindPropertyRelative("RewardPerTarget"),
          new GUIContent("Per Target Bonus", "Additional reward per target in quest"));

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
      }
    }

    private void DrawDaySection()
    {
      _dayFoldout = DrawSectionHeader("Day & Quests", _dayFoldout);

      if (_dayFoldout)
      {
        EditorGUI.indentLevel++;

        SerializedProperty day = serializedObject.FindProperty("Day");

        EditorGUILayout.LabelField("Session", EditorStyles.miniBoldLabel);
        SerializedProperty duration = day.FindPropertyRelative("DurationSeconds");
        EditorGUILayout.PropertyField(duration, new GUIContent("Duration (sec)"));

        // Show formatted time
        float durationValue = duration.floatValue;
        int minutes = (int)(durationValue / 60);
        int seconds = (int)(durationValue % 60);
        EditorGUILayout.LabelField($"  = {minutes}m {seconds}s", EditorStyles.miniLabel);

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Quests Per Day", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(day.FindPropertyRelative("MinQuestsPerDay"));
        EditorGUILayout.PropertyField(day.FindPropertyRelative("MaxQuestsPerDay"));

        EditorGUILayout.Space(5);

        _questTargetsFoldout = EditorGUILayout.Foldout(_questTargetsFoldout, "Quest Targets", true);
        if (_questTargetsFoldout)
        {
          EditorGUILayout.PropertyField(day.FindPropertyRelative("QuestTargets"), true);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
      }
    }

    private void DrawUpgradesSection()
    {
      _upgradesFoldout = DrawSectionHeader("Upgrades", _upgradesFoldout);

      if (_upgradesFoldout)
      {
        EditorGUI.indentLevel++;

        _upgradeListFoldout = EditorGUILayout.Foldout(_upgradeListFoldout, "Upgrade Definitions", true);
        if (_upgradeListFoldout)
        {
          SerializedProperty upgrades = serializedObject.FindProperty("Upgrades.Upgrades");
          EditorGUILayout.PropertyField(upgrades, true);
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
      }
    }

    private void DrawFeedbackSection()
    {
      _feedbackFoldout = DrawSectionHeader("Feedback Effects", _feedbackFoldout);

      if (_feedbackFoldout)
      {
        EditorGUI.indentLevel++;

        SerializedProperty feedback = serializedObject.FindProperty("Feedback");

        EditorGUILayout.LabelField("Particles", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("ParticleBurstCount"));
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("ParticleLifetime"));
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("ParticleSpeed"));
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("ParticleGravity"));
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("ParticleSize"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Floating Text", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("FloatSpeed"));
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("FloatDuration"));
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("FontSize"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Audio", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("SFXVolume"));

        EditorGUILayout.Space(5);
        EditorGUILayout.LabelField("Colors", EditorStyles.miniBoldLabel);
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("RewardColor"));
        EditorGUILayout.PropertyField(feedback.FindPropertyRelative("PenaltyColor"));

        EditorGUI.indentLevel--;
        EditorGUILayout.Space(10);
      }
    }

    private bool DrawSectionHeader(string title, bool foldout)
    {
      EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

      GUIStyle headerStyle = new GUIStyle(EditorStyles.foldoutHeader)
      {
        fontStyle = FontStyle.Bold,
        fontSize = 12
      };

      foldout = EditorGUILayout.Foldout(foldout, title, true, headerStyle);

      EditorGUILayout.EndHorizontal();

      return foldout;
    }

    private void DrawButtons()
    {
      EditorGUILayout.BeginHorizontal();

      GUI.backgroundColor = new Color(0.9f, 0.5f, 0.5f);
      if (GUILayout.Button("Reset to Defaults", GUILayout.Height(30)))
      {
        if (EditorUtility.DisplayDialog("Reset Balance",
              "Are you sure you want to reset all balance values to defaults?",
              "Reset", "Cancel"))
        {
          Undo.RecordObject(_balance, "Reset GameBalance to Defaults");
          _balance.ResetToDefaults();
          EditorUtility.SetDirty(_balance);
        }
      }

      GUI.backgroundColor = new Color(0.5f, 0.9f, 0.5f);
      if (GUILayout.Button("Save Asset", GUILayout.Height(30)))
      {
        EditorUtility.SetDirty(_balance);
        AssetDatabase.SaveAssets();
        Debug.Log("[GameBalance] Asset saved");
      }

      GUI.backgroundColor = Color.white;

      EditorGUILayout.EndHorizontal();

      EditorGUILayout.Space(5);

      // Quick actions
      EditorGUILayout.BeginHorizontal();

      if (GUILayout.Button("Copy to Resources"))
      {
        CopyToResources();
      }

      if (GUILayout.Button("Validate"))
      {
        ValidateBalance();
      }

      EditorGUILayout.EndHorizontal();
    }

    private void CopyToResources()
    {
      string assetPath = AssetDatabase.GetAssetPath(_balance);
      string resourcesPath = "Assets/Resources/GameBalance.asset";

      // Ensure Resources folder exists
      if (!AssetDatabase.IsValidFolder("Assets/Resources"))
      {
        AssetDatabase.CreateFolder("Assets", "Resources");
      }

      if (assetPath != resourcesPath)
      {
        AssetDatabase.CopyAsset(assetPath, resourcesPath);
        AssetDatabase.Refresh();
        Debug.Log($"[GameBalance] Copied to {resourcesPath}");
      }
      else
      {
        Debug.Log("[GameBalance] Already in Resources folder");
      }
    }

    private void ValidateBalance()
    {
      bool valid = true;

      // Validate pedestrian weights
      float totalWeight = 0f;
      foreach (PedestrianTypeBalance tb in _balance.Pedestrians.TypeBalances)
        totalWeight += tb.SpawnWeight;

      if (Mathf.Abs(totalWeight - 1f) > 0.01f)
      {
        Debug.LogWarning($"[GameBalance] Pedestrian spawn weights sum to {totalWeight:F2}, should be 1.0");
        valid = false;
      }

      // Validate economy
      if (_balance.Economy.StartingMoney < _balance.Economy.DailyFoodCost)
      {
        Debug.LogWarning("[GameBalance] Starting money is less than daily food cost!");
        valid = false;
      }

      // Validate day duration
      if (_balance.Day.DurationSeconds < 10f)
      {
        Debug.LogWarning("[GameBalance] Day duration seems too short (< 10 seconds)");
        valid = false;
      }

      // Validate quest counts
      if (_balance.Day.MinQuestsPerDay > _balance.Day.MaxQuestsPerDay)
      {
        Debug.LogError("[GameBalance] MinQuestsPerDay > MaxQuestsPerDay!");
        valid = false;
      }

      if (valid)
      {
        Debug.Log("[GameBalance] Validation passed!");
      }
    }
  }
}
#endif
