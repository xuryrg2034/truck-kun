using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Art
{
  /// <summary>
  /// Factory for creating game models
  /// </summary>
  public static class ModelFactory
  {
    /// <summary>
    /// Create player truck model
    /// </summary>
    public static GameObject CreatePlayerTruck(Vector3 position)
    {
      GameObject truck = ProceduralMeshGenerator.CreateTruck();
      truck.transform.position = position;
      truck.name = "PlayerTruck";
      return truck;
    }

    /// <summary>
    /// Create NPC model based on type
    /// </summary>
    public static GameObject CreateNPC(PedestrianKind kind, Vector3 position)
    {
      GameObject npc;

      switch (kind)
      {
        case PedestrianKind.StudentNerd:
          npc = ProceduralMeshGenerator.CreateStudentNerd();
          break;
        case PedestrianKind.Salaryman:
          npc = ProceduralMeshGenerator.CreateSalaryman();
          break;
        case PedestrianKind.Grandma:
          npc = ProceduralMeshGenerator.CreateGrandma();
          break;
        default:
          // Default to salaryman
          npc = ProceduralMeshGenerator.CreateSalaryman();
          break;
      }

      npc.transform.position = position;
      return npc;
    }

    /// <summary>
    /// Create road segment
    /// </summary>
    public static GameObject CreateRoad(Vector3 position, float length = 20f)
    {
      GameObject road = ProceduralMeshGenerator.CreateRoadSection(length);
      road.transform.position = position;
      return road;
    }

    /// <summary>
    /// Create sidewalk segment
    /// </summary>
    public static GameObject CreateSidewalk(Vector3 position, float length = 20f, bool leftSide = true)
    {
      GameObject sidewalk = ProceduralMeshGenerator.CreateSidewalk(length, 2f);
      sidewalk.transform.position = position;

      if (!leftSide)
      {
        sidewalk.transform.localScale = new Vector3(-1f, 1f, 1f);
      }

      return sidewalk;
    }

    /// <summary>
    /// Create building for background
    /// </summary>
    public static GameObject CreateBuilding(Vector3 position, float width, float height, Color color)
    {
      GameObject building = ProceduralMeshGenerator.CreateBuilding(width, height, 5f, color);
      building.transform.position = position;
      return building;
    }

    /// <summary>
    /// Create a complete street scene segment
    /// </summary>
    public static GameObject CreateStreetScene(Vector3 position, float length = 50f)
    {
      GameObject scene = new GameObject("StreetScene");
      scene.transform.position = position;

      // Road
      GameObject road = ProceduralMeshGenerator.CreateRoadSection(length);
      road.transform.SetParent(scene.transform, false);

      // Left sidewalk
      GameObject sidewalkL = ProceduralMeshGenerator.CreateSidewalk(length, 2.5f);
      sidewalkL.transform.SetParent(scene.transform, false);
      sidewalkL.transform.localPosition = new Vector3(-6f, 0f, 0f);

      // Right sidewalk
      GameObject sidewalkR = ProceduralMeshGenerator.CreateSidewalk(length, 2.5f);
      sidewalkR.transform.SetParent(scene.transform, false);
      sidewalkR.transform.localPosition = new Vector3(6f, 0f, 0f);

      // Buildings on left
      Color[] buildingColors = new Color[]
      {
        new Color(0.7f, 0.65f, 0.6f),
        new Color(0.6f, 0.55f, 0.5f),
        new Color(0.65f, 0.6f, 0.55f),
        new Color(0.55f, 0.5f, 0.45f),
      };

      float buildingZ = -length / 2f + 5f;
      int colorIndex = 0;
      while (buildingZ < length / 2f)
      {
        float width = Random.Range(6f, 10f);
        float height = Random.Range(8f, 15f);

        // Left side
        GameObject buildingL = ProceduralMeshGenerator.CreateBuilding(width, height, 6f, buildingColors[colorIndex % buildingColors.Length]);
        buildingL.transform.SetParent(scene.transform, false);
        buildingL.transform.localPosition = new Vector3(-12f, 0f, buildingZ);

        // Right side
        GameObject buildingR = ProceduralMeshGenerator.CreateBuilding(width, height, 6f, buildingColors[(colorIndex + 2) % buildingColors.Length]);
        buildingR.transform.SetParent(scene.transform, false);
        buildingR.transform.localPosition = new Vector3(12f, 0f, buildingZ);

        buildingZ += width + 2f;
        colorIndex++;
      }

      return scene;
    }

    /// <summary>
    /// Create hub interior
    /// </summary>
    public static GameObject CreateHubInterior(Vector3 position)
    {
      GameObject hub = new GameObject("HubInterior");
      hub.transform.position = position;

      // Room
      GameObject room = ProceduralMeshGenerator.CreateHubRoom();
      room.transform.SetParent(hub.transform, false);

      // Quest board
      GameObject questBoard = ProceduralMeshGenerator.CreateQuestBoard();
      questBoard.transform.SetParent(hub.transform, false);
      questBoard.transform.localPosition = new Vector3(0f, 0f, 7f);

      // Desk
      GameObject desk = ProceduralMeshGenerator.CreateDesk();
      desk.transform.SetParent(hub.transform, false);
      desk.transform.localPosition = new Vector3(-4f, 0f, 3f);

      // Garage area (truck)
      GameObject garageTruck = ProceduralMeshGenerator.CreateTruck();
      garageTruck.transform.SetParent(hub.transform, false);
      garageTruck.transform.localPosition = new Vector3(4f, 0f, -3f);
      garageTruck.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
      garageTruck.transform.localScale = Vector3.one * 0.7f; // Smaller in garage

      return hub;
    }
  }
}
