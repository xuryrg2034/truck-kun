#if UNITY_EDITOR
using Code.Gameplay.Features.Obstacle;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.Generators
{
  /// <summary>
  /// Editor tool to generate obstacle prefabs with all required components and settings.
  /// Menu: Truck-kun > Generate Obstacle Prefabs
  /// </summary>
  public static class ObstaclePrefabGenerator
  {
    private const string PrefabFolder = "Assets/Prefabs/Obstacles";
    private const string MaterialFolder = "Assets/Prefabs/Obstacles/Materials";
    private const string ObstacleLayer = "Obstacle";

    // Default settings
    private static class Defaults
    {
      // Ramp
      public const float RampAngle = 15f;
      public const float RampWidth = 4f;
      public const float RampLength = 3f;
      public const float RampHeight = 0.8f;

      // Barrier
      public const float BarrierMass = 200f;
      public const float BarrierDrag = 2f;
      public const float BarrierAngularDrag = 1f;
      public const float BarrierSpeedPenalty = 0.3f;
      public const float BarrierWidth = 2f;
      public const float BarrierHeight = 1f;
      public const float BarrierDepth = 0.5f;

      // SpeedBump
      public const float SpeedBumpWidth = 6f;
      public const float SpeedBumpHeight = 0.12f;
      public const float SpeedBumpDepth = 0.4f;
      public const float SpeedBumpImpulse = 300f;
      public const float SpeedBumpPenalty = 0.1f;

      // Hole
      public const float HoleWidth = 3f;
      public const float HoleDepth = 4f;
      public const float HoleTriggerHeight = 2f;
      public const float HoleDownForce = 500f;
      public const float HoleSpeedPenalty = 0.5f;
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/All", priority = 100)]
    public static void GenerateAll()
    {
      EnsureFoldersExist();
      GenerateRamp();
      GenerateBarrier();
      GenerateSpeedBump();
      GenerateHole();
      AssetDatabase.Refresh();
      Debug.Log("<color=green>[ObstacleGenerator]</color> All obstacle prefabs generated in " + PrefabFolder);
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/Ramp")]
    public static void GenerateRamp()
    {
      EnsureFoldersExist();

      GameObject ramp = new GameObject("Ramp");

      // Create ramp visual (rotated cube)
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
      visual.name = "Visual";
      visual.transform.SetParent(ramp.transform);
      visual.transform.localScale = new Vector3(Defaults.RampWidth, 0.1f, Defaults.RampLength);
      visual.transform.localPosition = new Vector3(0f, Defaults.RampHeight * 0.5f, 0f);
      visual.transform.localRotation = Quaternion.Euler(-Defaults.RampAngle, 0f, 0f);

      // Remove default collider from visual
      Object.DestroyImmediate(visual.GetComponent<Collider>());

      // Add box collider to root (angled)
      BoxCollider collider = ramp.AddComponent<BoxCollider>();
      collider.size = new Vector3(Defaults.RampWidth, 0.1f, Defaults.RampLength);
      collider.center = new Vector3(0f, Defaults.RampHeight * 0.5f, 0f);

      // Rotate the entire object to create ramp angle
      ramp.transform.rotation = Quaternion.Euler(-Defaults.RampAngle, 0f, 0f);

      // Set material (gray concrete)
      Material mat = CreateMaterial("Ramp_Mat", new Color(0.5f, 0.5f, 0.5f));
      visual.GetComponent<Renderer>().sharedMaterial = mat;

      // Add ObstacleBehaviour with settings
      ObstacleBehaviour behaviour = ramp.AddComponent<ObstacleBehaviour>();
      SetObstacleBehaviourValues(behaviour, ObstacleKind.Ramp, isPassable: true,
        rampAngle: Defaults.RampAngle);

      // Set layer
      SetLayerRecursive(ramp, ObstacleLayer);

      // Reset rotation before saving (rotation is part of design)
      ramp.transform.rotation = Quaternion.Euler(-Defaults.RampAngle, 0f, 0f);

      SavePrefab(ramp, "Ramp");
      Object.DestroyImmediate(ramp);

      Debug.Log("[ObstacleGenerator] Ramp prefab created");
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/Barrier")]
    public static void GenerateBarrier()
    {
      EnsureFoldersExist();

      GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
      barrier.name = "Barrier";
      barrier.transform.localScale = new Vector3(Defaults.BarrierWidth, Defaults.BarrierHeight, Defaults.BarrierDepth);

      // Adjust collider center so bottom is at y=0
      BoxCollider collider = barrier.GetComponent<BoxCollider>();
      // Collider is auto-generated, just position the object
      barrier.transform.position = new Vector3(0f, Defaults.BarrierHeight * 0.5f, 0f);

      // Add dynamic Rigidbody
      Rigidbody rb = barrier.AddComponent<Rigidbody>();
      rb.mass = Defaults.BarrierMass;
      rb.linearDamping = Defaults.BarrierDrag;
      rb.angularDamping = Defaults.BarrierAngularDrag;
      rb.useGravity = true;
      rb.isKinematic = false;
      rb.interpolation = RigidbodyInterpolation.Interpolate;
      rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

      // Set material (orange/warning)
      Material mat = CreateMaterial("Barrier_Mat", new Color(1f, 0.5f, 0f));
      barrier.GetComponent<Renderer>().sharedMaterial = mat;

      // Add ObstacleBehaviour with settings
      ObstacleBehaviour behaviour = barrier.AddComponent<ObstacleBehaviour>();
      SetObstacleBehaviourValues(behaviour, ObstacleKind.Barrier, isPassable: false,
        barrierMass: Defaults.BarrierMass,
        barrierSpeedPenalty: Defaults.BarrierSpeedPenalty);

      // Set layer
      SetLayerRecursive(barrier, ObstacleLayer);

      SavePrefab(barrier, "Barrier");
      Object.DestroyImmediate(barrier);

      Debug.Log("[ObstacleGenerator] Barrier prefab created");
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/SpeedBump")]
    public static void GenerateSpeedBump()
    {
      EnsureFoldersExist();

      GameObject speedBump = new GameObject("SpeedBump");

      // Create elongated cylinder for bump shape
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      visual.name = "Visual";
      visual.transform.SetParent(speedBump.transform);
      visual.transform.localPosition = new Vector3(0f, Defaults.SpeedBumpHeight * 0.5f, 0f);
      visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
      visual.transform.localScale = new Vector3(Defaults.SpeedBumpHeight, Defaults.SpeedBumpWidth * 0.5f, Defaults.SpeedBumpDepth);

      // Remove default collider from visual
      Object.DestroyImmediate(visual.GetComponent<Collider>());

      // Add box collider to root (simpler collision)
      BoxCollider collider = speedBump.AddComponent<BoxCollider>();
      collider.size = new Vector3(Defaults.SpeedBumpWidth, Defaults.SpeedBumpHeight, Defaults.SpeedBumpDepth);
      collider.center = new Vector3(0f, Defaults.SpeedBumpHeight * 0.5f, 0f);

      // Set material (yellow/warning stripes)
      Material mat = CreateMaterial("SpeedBump_Mat", new Color(1f, 0.9f, 0.2f));
      visual.GetComponent<Renderer>().sharedMaterial = mat;

      // Add ObstacleBehaviour with settings
      ObstacleBehaviour behaviour = speedBump.AddComponent<ObstacleBehaviour>();
      SetObstacleBehaviourValues(behaviour, ObstacleKind.SpeedBump, isPassable: true,
        speedBumpImpulse: Defaults.SpeedBumpImpulse,
        speedBumpPenalty: Defaults.SpeedBumpPenalty);

      // Set layer
      SetLayerRecursive(speedBump, ObstacleLayer);

      SavePrefab(speedBump, "SpeedBump");
      Object.DestroyImmediate(speedBump);

      Debug.Log("[ObstacleGenerator] SpeedBump prefab created");
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/Hole")]
    public static void GenerateHole()
    {
      EnsureFoldersExist();

      GameObject hole = new GameObject("Hole");

      // Create visual (dark quad on ground)
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
      visual.name = "Visual";
      visual.transform.SetParent(hole.transform);
      visual.transform.localPosition = new Vector3(0f, 0.01f, 0f);
      visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      visual.transform.localScale = new Vector3(Defaults.HoleWidth, Defaults.HoleDepth, 1f);

      // Remove collider from visual
      Object.DestroyImmediate(visual.GetComponent<Collider>());

      // Add trigger collider
      BoxCollider triggerCollider = hole.AddComponent<BoxCollider>();
      triggerCollider.size = new Vector3(Defaults.HoleWidth, Defaults.HoleTriggerHeight, Defaults.HoleDepth);
      triggerCollider.center = new Vector3(0f, Defaults.HoleTriggerHeight * 0.5f, 0f);
      triggerCollider.isTrigger = true;

      // Set material (dark/black)
      Material mat = CreateMaterial("Hole_Mat", new Color(0.05f, 0.05f, 0.05f));
      visual.GetComponent<Renderer>().sharedMaterial = mat;

      // Add ObstacleBehaviour with settings
      ObstacleBehaviour behaviour = hole.AddComponent<ObstacleBehaviour>();
      SetObstacleBehaviourValues(behaviour, ObstacleKind.Hole, isPassable: true,
        holeDownForce: Defaults.HoleDownForce,
        holeSpeedPenalty: Defaults.HoleSpeedPenalty);

      // Set layer
      SetLayerRecursive(hole, ObstacleLayer);

      SavePrefab(hole, "Hole");
      Object.DestroyImmediate(hole);

      Debug.Log("[ObstacleGenerator] Hole prefab created");
    }

    private static void SetObstacleBehaviourValues(
      ObstacleBehaviour behaviour,
      ObstacleKind kind,
      bool isPassable,
      float rampAngle = 15f,
      float barrierMass = 200f,
      float barrierSpeedPenalty = 0.3f,
      float speedBumpImpulse = 200f,
      float speedBumpPenalty = 0.1f,
      float holeDownForce = 500f,
      float holeSpeedPenalty = 0.5f)
    {
      SerializedObject so = new SerializedObject(behaviour);

      // Set kind
      SerializedProperty kindProp = so.FindProperty("_kind");
      if (kindProp != null)
        kindProp.enumValueIndex = (int)kind;

      // Set passable
      SerializedProperty passableProp = so.FindProperty("_isPassable");
      if (passableProp != null)
        passableProp.boolValue = isPassable;

      // Ramp settings
      SerializedProperty rampAngleProp = so.FindProperty("_rampAngle");
      if (rampAngleProp != null)
        rampAngleProp.floatValue = rampAngle;

      // Barrier settings
      SerializedProperty barrierMassProp = so.FindProperty("_barrierMass");
      if (barrierMassProp != null)
        barrierMassProp.floatValue = barrierMass;

      SerializedProperty barrierPenaltyProp = so.FindProperty("_barrierSpeedPenalty");
      if (barrierPenaltyProp != null)
        barrierPenaltyProp.floatValue = barrierSpeedPenalty;

      // SpeedBump settings
      SerializedProperty speedBumpImpulseProp = so.FindProperty("_speedBumpImpulse");
      if (speedBumpImpulseProp != null)
        speedBumpImpulseProp.floatValue = speedBumpImpulse;

      SerializedProperty speedBumpPenaltyProp = so.FindProperty("_speedBumpPenalty");
      if (speedBumpPenaltyProp != null)
        speedBumpPenaltyProp.floatValue = speedBumpPenalty;

      // Hole settings
      SerializedProperty holeDownForceProp = so.FindProperty("_holeDownForce");
      if (holeDownForceProp != null)
        holeDownForceProp.floatValue = holeDownForce;

      SerializedProperty holeSpeedPenaltyProp = so.FindProperty("_holeSpeedPenalty");
      if (holeSpeedPenaltyProp != null)
        holeSpeedPenaltyProp.floatValue = holeSpeedPenalty;

      so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static Material CreateMaterial(string name, Color color)
    {
      // Try URP first, fallback to Standard
      Shader shader = Shader.Find("Universal Render Pipeline/Lit");
      if (shader == null)
        shader = Shader.Find("Standard");

      Material mat = new Material(shader);
      mat.color = color;

      // For URP
      if (mat.HasProperty("_BaseColor"))
        mat.SetColor("_BaseColor", color);

      string matPath = $"{MaterialFolder}/{name}.mat";

      // Check if material already exists
      Material existingMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
      if (existingMat != null)
      {
        existingMat.color = color;
        if (existingMat.HasProperty("_BaseColor"))
          existingMat.SetColor("_BaseColor", color);
        return existingMat;
      }

      AssetDatabase.CreateAsset(mat, matPath);
      return mat;
    }

    private static void SetLayerRecursive(GameObject obj, string layerName)
    {
      int layer = LayerMask.NameToLayer(layerName);
      if (layer == -1)
      {
        Debug.LogWarning($"[ObstacleGenerator] Layer '{layerName}' not found! Please create it in Tags & Layers.");
        return;
      }

      obj.layer = layer;
      foreach (Transform child in obj.transform)
      {
        SetLayerRecursive(child.gameObject, layerName);
      }
    }

    private static void EnsureFoldersExist()
    {
      if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        AssetDatabase.CreateFolder("Assets", "Prefabs");

      if (!AssetDatabase.IsValidFolder(PrefabFolder))
        AssetDatabase.CreateFolder("Assets/Prefabs", "Obstacles");

      if (!AssetDatabase.IsValidFolder(MaterialFolder))
        AssetDatabase.CreateFolder(PrefabFolder, "Materials");
    }

    private static void SavePrefab(GameObject obj, string name)
    {
      string path = $"{PrefabFolder}/{name}.prefab";

      // Remove existing prefab if any
      GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
      if (existingPrefab != null)
      {
        AssetDatabase.DeleteAsset(path);
      }

      PrefabUtility.SaveAsPrefabAsset(obj, path);
    }
  }
}
#endif
