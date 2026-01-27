using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Art
{
  /// <summary>
  /// Preview script for testing procedural models in Editor
  /// Add to any GameObject and click context menu to spawn models
  /// </summary>
  public class ModelPreview : MonoBehaviour
  {
    [Header("Preview Settings")]
    [SerializeField] private bool _autoSpawnOnStart = false;
    [SerializeField] private bool _spawnStreetScene = true;

    private void Start()
    {
      if (_autoSpawnOnStart)
      {
        SpawnAllModels();
      }
    }

    [ContextMenu("Spawn All Models")]
    public void SpawnAllModels()
    {
      ClearChildren();

      if (_spawnStreetScene)
      {
        SpawnStreetScene();
      }
      else
      {
        SpawnIndividualModels();
      }
    }

    [ContextMenu("Spawn Street Scene")]
    public void SpawnStreetScene()
    {
      ClearChildren();

      // Street with buildings
      ModelFactory.CreateStreetScene(Vector3.zero, 100f).transform.SetParent(transform, false);

      // Truck
      GameObject truck = ModelFactory.CreatePlayerTruck(new Vector3(0f, 0f, -20f));
      truck.transform.SetParent(transform, false);

      // NPCs
      SpawnNPCsOnSidewalk();
    }

    [ContextMenu("Spawn Individual Models")]
    public void SpawnIndividualModels()
    {
      ClearChildren();

      // Truck
      GameObject truck = ProceduralMeshGenerator.CreateTruck(transform);
      truck.transform.localPosition = new Vector3(0f, 0f, 0f);

      // NPCs in a row
      GameObject student = ProceduralMeshGenerator.CreateStudentNerd(transform);
      student.transform.localPosition = new Vector3(-3f, 0f, 3f);

      GameObject salaryman = ProceduralMeshGenerator.CreateSalaryman(transform);
      salaryman.transform.localPosition = new Vector3(0f, 0f, 3f);

      GameObject grandma = ProceduralMeshGenerator.CreateGrandma(transform);
      grandma.transform.localPosition = new Vector3(3f, 0f, 3f);

      // Road section
      GameObject road = ProceduralMeshGenerator.CreateRoadSection(20f, transform);
      road.transform.localPosition = new Vector3(0f, 0f, -5f);

      // Building
      GameObject building = ProceduralMeshGenerator.CreateBuilding(8f, 12f, 6f, new Color(0.7f, 0.65f, 0.6f), transform);
      building.transform.localPosition = new Vector3(-10f, 0f, 0f);
    }

    [ContextMenu("Spawn Hub Interior")]
    public void SpawnHubInterior()
    {
      ClearChildren();
      ModelFactory.CreateHubInterior(Vector3.zero).transform.SetParent(transform, false);
    }

    [ContextMenu("Clear All")]
    public void ClearChildren()
    {
      while (transform.childCount > 0)
      {
        DestroyImmediate(transform.GetChild(0).gameObject);
      }
    }

    private void SpawnNPCsOnSidewalk()
    {
      PedestrianKind[] kinds = { PedestrianKind.StudentNerd, PedestrianKind.Salaryman, PedestrianKind.Grandma };

      // Left sidewalk NPCs
      for (int i = 0; i < 10; i++)
      {
        float z = Random.Range(-40f, 40f);
        PedestrianKind kind = kinds[Random.Range(0, kinds.Length)];

        GameObject npc = ModelFactory.CreateNPC(kind, new Vector3(-5.5f, 0f, z));
        npc.transform.SetParent(transform, false);
        npc.transform.localRotation = Quaternion.Euler(0f, Random.Range(-30f, 30f), 0f);
      }

      // Right sidewalk NPCs
      for (int i = 0; i < 10; i++)
      {
        float z = Random.Range(-40f, 40f);
        PedestrianKind kind = kinds[Random.Range(0, kinds.Length)];

        GameObject npc = ModelFactory.CreateNPC(kind, new Vector3(5.5f, 0f, z));
        npc.transform.SetParent(transform, false);
        npc.transform.localRotation = Quaternion.Euler(0f, 180f + Random.Range(-30f, 30f), 0f);
      }
    }

#if UNITY_EDITOR
    [UnityEditor.MenuItem("Truck-kun/Preview Models")]
    public static void CreatePreviewObject()
    {
      GameObject previewObj = new GameObject("[ModelPreview]");
      ModelPreview preview = previewObj.AddComponent<ModelPreview>();
      preview.SpawnAllModels();

      UnityEditor.Selection.activeGameObject = previewObj;
      UnityEditor.SceneView.lastActiveSceneView?.FrameSelected();
    }

    [UnityEditor.MenuItem("Truck-kun/Preview Hub")]
    public static void CreateHubPreview()
    {
      GameObject previewObj = new GameObject("[HubPreview]");
      ModelPreview preview = previewObj.AddComponent<ModelPreview>();
      preview.SpawnHubInterior();

      UnityEditor.Selection.activeGameObject = previewObj;
      UnityEditor.SceneView.lastActiveSceneView?.FrameSelected();
    }
#endif
  }
}
