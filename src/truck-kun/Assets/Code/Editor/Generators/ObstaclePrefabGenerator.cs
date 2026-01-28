#if UNITY_EDITOR
using Code.Gameplay.Features.Obstacle;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.Generators
{
  /// <summary>
  /// Editor tool to generate obstacle prefabs.
  /// Menu: Truck-kun > Generate Obstacle Prefabs
  /// </summary>
  public static class ObstaclePrefabGenerator
  {
    private const string PrefabFolder = "Assets/Prefabs/Obstacles";
    private const string ObstacleLayer = "Obstacle";

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/All", priority = 100)]
    public static void GenerateAll()
    {
      EnsureFolderExists();
      GenerateRamp();
      GenerateBarrier();
      GenerateSpeedBump();
      GenerateHole();
      AssetDatabase.Refresh();
      Debug.Log("[ObstacleGenerator] All obstacle prefabs generated!");
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/Ramp")]
    public static void GenerateRamp()
    {
      EnsureFolderExists();

      GameObject ramp = new GameObject("Ramp");

      // Create ramp mesh (wedge shape)
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
      visual.name = "Visual";
      visual.transform.SetParent(ramp.transform);
      visual.transform.localPosition = new Vector3(0f, 0.25f, 0f);
      visual.transform.localScale = new Vector3(4f, 0.5f, 3f);
      visual.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);

      // Remove default collider from visual
      Object.DestroyImmediate(visual.GetComponent<Collider>());

      // Add mesh collider to parent
      MeshCollider meshCollider = ramp.AddComponent<MeshCollider>();
      meshCollider.sharedMesh = CreateRampMesh();
      meshCollider.convex = true;

      // Set material
      SetObstacleMaterial(visual, new Color(0.6f, 0.6f, 0.6f));

      // Add behaviour
      ObstacleBehaviour behaviour = ramp.AddComponent<ObstacleBehaviour>();
      SetObstacleKind(behaviour, ObstacleKind.Ramp);

      // Set layer
      SetLayerRecursive(ramp, ObstacleLayer);

      SavePrefab(ramp, "Ramp");
      Object.DestroyImmediate(ramp);

      Debug.Log("[ObstacleGenerator] Ramp prefab created");
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/Barrier")]
    public static void GenerateBarrier()
    {
      EnsureFolderExists();

      GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
      barrier.name = "Barrier";
      barrier.transform.localScale = new Vector3(2f, 1f, 0.5f);

      // Position so bottom is at y=0
      MeshFilter mf = barrier.GetComponent<MeshFilter>();
      barrier.transform.position = new Vector3(0f, 0.5f, 0f);

      // Add Rigidbody (dynamic)
      Rigidbody rb = barrier.AddComponent<Rigidbody>();
      rb.mass = 200f;
      rb.linearDamping = 2f;
      rb.angularDamping = 1f;
      rb.useGravity = true;
      rb.isKinematic = false;

      // Set material (orange/warning color)
      SetObstacleMaterial(barrier, new Color(1f, 0.5f, 0f));

      // Add behaviour
      ObstacleBehaviour behaviour = barrier.AddComponent<ObstacleBehaviour>();
      SetObstacleKind(behaviour, ObstacleKind.Barrier);

      // Set layer
      SetLayerRecursive(barrier, ObstacleLayer);

      SavePrefab(barrier, "Barrier");
      Object.DestroyImmediate(barrier);

      Debug.Log("[ObstacleGenerator] Barrier prefab created");
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/SpeedBump")]
    public static void GenerateSpeedBump()
    {
      EnsureFolderExists();

      GameObject speedBump = new GameObject("SpeedBump");

      // Create elongated bump shape
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
      visual.name = "Visual";
      visual.transform.SetParent(speedBump.transform);
      visual.transform.localPosition = new Vector3(0f, 0.075f, 0f);
      visual.transform.localScale = new Vector3(0.15f, 3f, 0.3f); // Rotated cylinder
      visual.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);

      // Remove default collider
      Object.DestroyImmediate(visual.GetComponent<Collider>());

      // Add box collider to parent (simpler collision)
      BoxCollider boxCollider = speedBump.AddComponent<BoxCollider>();
      boxCollider.size = new Vector3(6f, 0.15f, 0.4f);
      boxCollider.center = new Vector3(0f, 0.075f, 0f);

      // Set material (yellow/warning)
      SetObstacleMaterial(visual, new Color(1f, 0.9f, 0.2f));

      // Add behaviour
      ObstacleBehaviour behaviour = speedBump.AddComponent<ObstacleBehaviour>();
      SetObstacleKind(behaviour, ObstacleKind.SpeedBump);

      // Set layer
      SetLayerRecursive(speedBump, ObstacleLayer);

      SavePrefab(speedBump, "SpeedBump");
      Object.DestroyImmediate(speedBump);

      Debug.Log("[ObstacleGenerator] SpeedBump prefab created");
    }

    [MenuItem("Truck-kun/Generate Obstacle Prefabs/Hole")]
    public static void GenerateHole()
    {
      EnsureFolderExists();

      GameObject hole = new GameObject("Hole");

      // Visual representation (dark plane)
      GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
      visual.name = "Visual";
      visual.transform.SetParent(hole.transform);
      visual.transform.localPosition = new Vector3(0f, 0.01f, 0f);
      visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
      visual.transform.localScale = new Vector3(3f, 4f, 1f);

      // Remove collider from visual
      Object.DestroyImmediate(visual.GetComponent<Collider>());

      // Add trigger collider
      BoxCollider triggerCollider = hole.AddComponent<BoxCollider>();
      triggerCollider.size = new Vector3(3f, 2f, 4f);
      triggerCollider.center = new Vector3(0f, 1f, 0f);
      triggerCollider.isTrigger = true;

      // Set material (dark/black)
      SetObstacleMaterial(visual, new Color(0.1f, 0.1f, 0.1f));

      // Add behaviour
      ObstacleBehaviour behaviour = hole.AddComponent<ObstacleBehaviour>();
      SetObstacleKind(behaviour, ObstacleKind.Hole);

      // Set layer
      SetLayerRecursive(hole, ObstacleLayer);

      SavePrefab(hole, "Hole");
      Object.DestroyImmediate(hole);

      Debug.Log("[ObstacleGenerator] Hole prefab created");
    }

    private static Mesh CreateRampMesh()
    {
      // Create a simple ramp/wedge mesh
      Mesh mesh = new Mesh();
      mesh.name = "RampMesh";

      float width = 4f;
      float length = 3f;
      float height = 0.5f;

      Vector3[] vertices = new Vector3[]
      {
        // Bottom face
        new Vector3(-width/2, 0, -length/2),
        new Vector3(width/2, 0, -length/2),
        new Vector3(width/2, 0, length/2),
        new Vector3(-width/2, 0, length/2),

        // Top edge (front)
        new Vector3(-width/2, height, length/2),
        new Vector3(width/2, height, length/2),

        // Back edge (at ground level, same as bottom back)
        new Vector3(-width/2, 0, -length/2),
        new Vector3(width/2, 0, -length/2),
      };

      int[] triangles = new int[]
      {
        // Bottom
        0, 2, 1,
        0, 3, 2,

        // Ramp surface (inclined)
        0, 4, 3,
        0, 1, 5,
        0, 5, 4,
        1, 2, 5,
        2, 4, 5,
        2, 3, 4,

        // Left side
        0, 3, 4,

        // Right side
        1, 5, 2,
      };

      mesh.vertices = vertices;
      mesh.triangles = triangles;
      mesh.RecalculateNormals();
      mesh.RecalculateBounds();

      return mesh;
    }

    private static void SetObstacleMaterial(GameObject obj, Color color)
    {
      Renderer renderer = obj.GetComponent<Renderer>();
      if (renderer != null)
      {
        // Create simple material
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null)
          mat = new Material(Shader.Find("Standard"));

        mat.color = color;
        renderer.sharedMaterial = mat;

        // Save material as asset
        string matPath = $"{PrefabFolder}/Materials";
        if (!AssetDatabase.IsValidFolder(matPath))
          AssetDatabase.CreateFolder(PrefabFolder, "Materials");

        string matAssetPath = $"{matPath}/{obj.transform.root.name}_Mat.mat";
        AssetDatabase.CreateAsset(mat, matAssetPath);
      }
    }

    private static void SetObstacleKind(ObstacleBehaviour behaviour, ObstacleKind kind)
    {
      // Use SerializedObject to set private field
      SerializedObject so = new SerializedObject(behaviour);
      SerializedProperty kindProp = so.FindProperty("_kind");
      if (kindProp != null)
      {
        kindProp.enumValueIndex = (int)kind;
        so.ApplyModifiedPropertiesWithoutUndo();
      }
    }

    private static void SetLayerRecursive(GameObject obj, string layerName)
    {
      int layer = LayerMask.NameToLayer(layerName);
      if (layer == -1)
      {
        Debug.LogWarning($"[ObstacleGenerator] Layer '{layerName}' not found! Create it in Tags & Layers.");
        return;
      }

      obj.layer = layer;
      foreach (Transform child in obj.transform)
      {
        SetLayerRecursive(child.gameObject, layerName);
      }
    }

    private static void EnsureFolderExists()
    {
      if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        AssetDatabase.CreateFolder("Assets", "Prefabs");

      if (!AssetDatabase.IsValidFolder(PrefabFolder))
        AssetDatabase.CreateFolder("Assets/Prefabs", "Obstacles");
    }

    private static void SavePrefab(GameObject obj, string name)
    {
      string path = $"{PrefabFolder}/{name}.prefab";

      // Reset position before saving
      obj.transform.position = Vector3.zero;

      PrefabUtility.SaveAsPrefabAsset(obj, path);
    }
  }
}
#endif
