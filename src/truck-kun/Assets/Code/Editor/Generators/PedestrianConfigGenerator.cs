#if UNITY_EDITOR
using Code.Gameplay.Features.Pedestrian;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.Generators
{
  /// <summary>
  /// Editor tool to generate PedestrianConfig asset.
  /// Menu: Truck-kun > Generate Pedestrian Config
  /// </summary>
  public static class PedestrianConfigGenerator
  {
    private const string ConfigFolder = "Assets/Resources/Configs";

    [MenuItem("Truck-kun/Generate Pedestrian Config")]
    public static void GeneratePedestrianConfig()
    {
      EnsureFolderExists();

      string path = $"{ConfigFolder}/PedestrianConfig.asset";

      // Check if already exists
      PedestrianConfig existing = AssetDatabase.LoadAssetAtPath<PedestrianConfig>(path);
      if (existing != null)
      {
        Debug.LogWarning($"[PedestrianConfigGenerator] Config already exists at {path}");
        Selection.activeObject = existing;
        EditorGUIUtility.PingObject(existing);
        return;
      }

      // Create new config
      PedestrianConfig config = ScriptableObject.CreateInstance<PedestrianConfig>();

      AssetDatabase.CreateAsset(config, path);
      AssetDatabase.SaveAssets();

      Selection.activeObject = config;
      EditorGUIUtility.PingObject(config);

      Debug.Log($"[PedestrianConfigGenerator] Created PedestrianConfig at {path}");
      Debug.Log("Don't forget to assign pedestrian prefabs to the config!");
    }

    [MenuItem("Truck-kun/Generate Pedestrian Prefabs (Placeholder)")]
    public static void GeneratePlaceholderPedestrians()
    {
      string prefabFolder = "Assets/Prefabs/Pedestrians";

      if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
        AssetDatabase.CreateFolder("Assets", "Prefabs");

      if (!AssetDatabase.IsValidFolder(prefabFolder))
        AssetDatabase.CreateFolder("Assets/Prefabs", "Pedestrians");

      // Generate placeholder for each pedestrian type
      GeneratePlaceholderPedestrian(PedestrianKind.StudentNerd, prefabFolder, new Color(0.95f, 0.95f, 1f), 0.85f);
      GeneratePlaceholderPedestrian(PedestrianKind.Salaryman, prefabFolder, new Color(0.4f, 0.4f, 0.45f), 1f);
      GeneratePlaceholderPedestrian(PedestrianKind.Grandma, prefabFolder, new Color(1f, 0.7f, 0.8f), 0.8f);
      GeneratePlaceholderPedestrian(PedestrianKind.OldMan, prefabFolder, new Color(0.6f, 0.45f, 0.3f), 0.9f);
      GeneratePlaceholderPedestrian(PedestrianKind.Teenager, prefabFolder, new Color(0.2f, 0.8f, 0.4f), 0.95f);

      AssetDatabase.Refresh();
      Debug.Log("[PedestrianConfigGenerator] Placeholder pedestrian prefabs created!");
    }

    private static void GeneratePlaceholderPedestrian(PedestrianKind kind, string folder, Color color, float scale)
    {
      GameObject pedestrian = new GameObject($"Pedestrian_{kind}");

      // Body (capsule)
      GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
      body.name = "Body";
      body.transform.SetParent(pedestrian.transform);
      body.transform.localPosition = new Vector3(0f, 1f, 0f);
      body.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

      // Head (sphere)
      GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      head.name = "Head";
      head.transform.SetParent(pedestrian.transform);
      head.transform.localPosition = new Vector3(0f, 1.75f, 0f);
      head.transform.localScale = new Vector3(0.35f, 0.35f, 0.35f);

      // Remove colliders from visual parts
      Object.DestroyImmediate(body.GetComponent<Collider>());
      Object.DestroyImmediate(head.GetComponent<Collider>());

      // Add single capsule collider to root
      CapsuleCollider collider = pedestrian.AddComponent<CapsuleCollider>();
      collider.height = 2f;
      collider.radius = 0.3f;
      collider.center = new Vector3(0f, 1f, 0f);

      // Set material
      Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
      if (mat.shader == null)
        mat = new Material(Shader.Find("Standard"));
      mat.color = color;

      body.GetComponent<Renderer>().sharedMaterial = mat;
      head.GetComponent<Renderer>().sharedMaterial = mat;

      // Save material
      string matFolder = $"{folder}/Materials";
      if (!AssetDatabase.IsValidFolder(matFolder))
        AssetDatabase.CreateFolder(folder, "Materials");

      AssetDatabase.CreateAsset(mat, $"{matFolder}/{kind}_Mat.mat");

      // Apply scale
      pedestrian.transform.localScale = Vector3.one * scale;

      // Set layer
      int pedestrianLayer = LayerMask.NameToLayer("Pedestrian");
      if (pedestrianLayer != -1)
      {
        pedestrian.layer = pedestrianLayer;
        body.layer = pedestrianLayer;
        head.layer = pedestrianLayer;
      }

      // Save prefab
      string prefabPath = $"{folder}/{kind}.prefab";
      PrefabUtility.SaveAsPrefabAsset(pedestrian, prefabPath);

      Object.DestroyImmediate(pedestrian);

      Debug.Log($"[PedestrianConfigGenerator] Created placeholder: {kind}");
    }

    private static void EnsureFolderExists()
    {
      if (!AssetDatabase.IsValidFolder("Assets/Resources"))
        AssetDatabase.CreateFolder("Assets", "Resources");

      if (!AssetDatabase.IsValidFolder(ConfigFolder))
        AssetDatabase.CreateFolder("Assets/Resources", "Configs");
    }
  }
}
#endif
