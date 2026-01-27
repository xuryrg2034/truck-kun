using UnityEngine;
using UnityEditor;
using System.IO;

namespace Code.Editor
{
  /// <summary>
  /// Utility to save procedurally generated meshes as assets
  /// so they can be used in prefabs
  /// </summary>
  public static class MeshSaver
  {
    private const string DefaultMeshFolder = "Assets/Meshes/Procedural";

    [MenuItem("Truck-kun/Save Meshes From Selected")]
    public static void SaveMeshesFromSelected()
    {
      GameObject selected = Selection.activeGameObject;
      if (selected == null)
      {
        EditorUtility.DisplayDialog("Error", "Please select a GameObject with procedural meshes", "OK");
        return;
      }

      SaveMeshesRecursive(selected, DefaultMeshFolder);
      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      EditorUtility.DisplayDialog("Success",
        $"Meshes saved to {DefaultMeshFolder}\n\nNow you can create a prefab from this GameObject.", "OK");
    }

    [MenuItem("Truck-kun/Save Meshes From Selected", true)]
    public static bool SaveMeshesFromSelectedValidate()
    {
      return Selection.activeGameObject != null;
    }

    /// <summary>
    /// Save all meshes from a GameObject and its children
    /// </summary>
    public static void SaveMeshesRecursive(GameObject root, string folderPath)
    {
      // Ensure folder exists
      EnsureFolderExists(folderPath);

      string baseName = root.name.Replace("[", "").Replace("]", "").Replace(" ", "_");
      int meshIndex = 0;

      // Process all MeshFilters
      MeshFilter[] meshFilters = root.GetComponentsInChildren<MeshFilter>(true);
      foreach (MeshFilter mf in meshFilters)
      {
        if (mf.sharedMesh == null)
          continue;

        // Skip if mesh is already an asset
        if (AssetDatabase.Contains(mf.sharedMesh))
          continue;

        // Save mesh
        Mesh savedMesh = SaveMesh(mf.sharedMesh, folderPath, $"{baseName}_{mf.gameObject.name}_{meshIndex}");
        mf.sharedMesh = savedMesh;
        meshIndex++;
      }

      // Process all SkinnedMeshRenderers (if any)
      SkinnedMeshRenderer[] skinnedRenderers = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
      foreach (SkinnedMeshRenderer smr in skinnedRenderers)
      {
        if (smr.sharedMesh == null)
          continue;

        if (AssetDatabase.Contains(smr.sharedMesh))
          continue;

        Mesh savedMesh = SaveMesh(smr.sharedMesh, folderPath, $"{baseName}_{smr.gameObject.name}_{meshIndex}");
        smr.sharedMesh = savedMesh;
        meshIndex++;
      }

      Debug.Log($"[MeshSaver] Saved {meshIndex} meshes from '{root.name}' to {folderPath}");
    }

    /// <summary>
    /// Save a single mesh as an asset
    /// </summary>
    public static Mesh SaveMesh(Mesh mesh, string folderPath, string meshName)
    {
      EnsureFolderExists(folderPath);

      // Clean up the name
      meshName = meshName.Replace("[", "").Replace("]", "").Replace(" ", "_");
      string assetPath = $"{folderPath}/{meshName}.asset";

      // Make unique path if file exists
      assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

      // Create a copy of the mesh
      Mesh meshCopy = Object.Instantiate(mesh);
      meshCopy.name = meshName;

      // Save as asset
      AssetDatabase.CreateAsset(meshCopy, assetPath);

      Debug.Log($"[MeshSaver] Saved mesh: {assetPath}");
      return meshCopy;
    }

    /// <summary>
    /// Ensure the folder path exists, creating it if necessary
    /// </summary>
    private static void EnsureFolderExists(string folderPath)
    {
      if (AssetDatabase.IsValidFolder(folderPath))
        return;

      string[] folders = folderPath.Split('/');
      string currentPath = folders[0]; // "Assets"

      for (int i = 1; i < folders.Length; i++)
      {
        string nextPath = currentPath + "/" + folders[i];
        if (!AssetDatabase.IsValidFolder(nextPath))
        {
          AssetDatabase.CreateFolder(currentPath, folders[i]);
        }
        currentPath = nextPath;
      }
    }

    /// <summary>
    /// Create a prefab from a GameObject with procedural meshes
    /// Saves meshes first, then creates the prefab
    /// </summary>
    [MenuItem("Truck-kun/Create Prefab With Meshes")]
    public static void CreatePrefabWithMeshes()
    {
      GameObject selected = Selection.activeGameObject;
      if (selected == null)
      {
        EditorUtility.DisplayDialog("Error", "Please select a GameObject", "OK");
        return;
      }

      // Ask for prefab location
      string prefabPath = EditorUtility.SaveFilePanelInProject(
        "Save Prefab",
        selected.name,
        "prefab",
        "Choose where to save the prefab",
        "Assets/Prefabs"
      );

      if (string.IsNullOrEmpty(prefabPath))
        return;

      // Save meshes first
      string meshFolder = Path.GetDirectoryName(prefabPath).Replace("\\", "/") + "/Meshes";
      SaveMeshesRecursive(selected, meshFolder);

      // Save materials too
      SaveMaterialsRecursive(selected, meshFolder);

      // Create prefab
      GameObject prefab = PrefabUtility.SaveAsPrefabAsset(selected, prefabPath);

      AssetDatabase.SaveAssets();
      AssetDatabase.Refresh();

      // Select the created prefab
      Selection.activeObject = prefab;

      EditorUtility.DisplayDialog("Success",
        $"Prefab created: {prefabPath}\nMeshes saved to: {meshFolder}", "OK");
    }

    [MenuItem("Truck-kun/Create Prefab With Meshes", true)]
    public static bool CreatePrefabWithMeshesValidate()
    {
      return Selection.activeGameObject != null;
    }

    /// <summary>
    /// Save materials as assets
    /// </summary>
    private static void SaveMaterialsRecursive(GameObject root, string folderPath)
    {
      EnsureFolderExists(folderPath);

      string baseName = root.name.Replace("[", "").Replace("]", "").Replace(" ", "_");
      int matIndex = 0;

      Renderer[] renderers = root.GetComponentsInChildren<Renderer>(true);
      foreach (Renderer renderer in renderers)
      {
        Material[] materials = renderer.sharedMaterials;
        bool changed = false;

        for (int i = 0; i < materials.Length; i++)
        {
          if (materials[i] == null)
            continue;

          // Skip if material is already an asset
          if (AssetDatabase.Contains(materials[i]))
            continue;

          // Save material
          string matName = $"{baseName}_{renderer.gameObject.name}_Mat{matIndex}";
          matName = matName.Replace("[", "").Replace("]", "").Replace(" ", "_");
          string matPath = $"{folderPath}/{matName}.mat";
          matPath = AssetDatabase.GenerateUniqueAssetPath(matPath);

          Material matCopy = Object.Instantiate(materials[i]);
          matCopy.name = matName;
          AssetDatabase.CreateAsset(matCopy, matPath);

          materials[i] = matCopy;
          changed = true;
          matIndex++;

          Debug.Log($"[MeshSaver] Saved material: {matPath}");
        }

        if (changed)
        {
          renderer.sharedMaterials = materials;
        }
      }
    }
  }
}
