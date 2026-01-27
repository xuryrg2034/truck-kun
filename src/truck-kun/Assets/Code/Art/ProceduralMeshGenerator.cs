using System.Collections.Generic;
using UnityEngine;

namespace Code.Art
{
  /// <summary>
  /// Generates simple low-poly meshes procedurally for MVP
  /// </summary>
  public static class ProceduralMeshGenerator
  {
    #region Basic Shapes

    /// <summary>
    /// Create a simple box mesh
    /// </summary>
    public static Mesh CreateBox(Vector3 size)
    {
      Mesh mesh = new Mesh();
      mesh.name = "ProceduralBox";

      float w = size.x / 2f;
      float h = size.y / 2f;
      float d = size.z / 2f;

      Vector3[] vertices = new Vector3[]
      {
        // Front
        new Vector3(-w, -h, -d), new Vector3(-w, h, -d), new Vector3(w, h, -d), new Vector3(w, -h, -d),
        // Back
        new Vector3(w, -h, d), new Vector3(w, h, d), new Vector3(-w, h, d), new Vector3(-w, -h, d),
        // Top
        new Vector3(-w, h, -d), new Vector3(-w, h, d), new Vector3(w, h, d), new Vector3(w, h, -d),
        // Bottom
        new Vector3(-w, -h, d), new Vector3(-w, -h, -d), new Vector3(w, -h, -d), new Vector3(w, -h, d),
        // Left
        new Vector3(-w, -h, d), new Vector3(-w, h, d), new Vector3(-w, h, -d), new Vector3(-w, -h, -d),
        // Right
        new Vector3(w, -h, -d), new Vector3(w, h, -d), new Vector3(w, h, d), new Vector3(w, -h, d),
      };

      int[] triangles = new int[]
      {
        0, 1, 2, 0, 2, 3,       // Front
        4, 5, 6, 4, 6, 7,       // Back
        8, 9, 10, 8, 10, 11,    // Top
        12, 13, 14, 12, 14, 15, // Bottom
        16, 17, 18, 16, 18, 19, // Left
        20, 21, 22, 20, 22, 23, // Right
      };

      mesh.vertices = vertices;
      mesh.triangles = triangles;
      mesh.RecalculateNormals();

      return mesh;
    }

    /// <summary>
    /// Create a cylinder mesh
    /// </summary>
    public static Mesh CreateCylinder(float radius, float height, int segments = 12)
    {
      Mesh mesh = new Mesh();
      mesh.name = "ProceduralCylinder";

      List<Vector3> vertices = new List<Vector3>();
      List<int> triangles = new List<int>();

      float halfHeight = height / 2f;

      // Bottom center
      vertices.Add(new Vector3(0, -halfHeight, 0));
      // Top center
      vertices.Add(new Vector3(0, halfHeight, 0));

      // Generate circle vertices
      for (int i = 0; i <= segments; i++)
      {
        float angle = (float)i / segments * Mathf.PI * 2f;
        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        vertices.Add(new Vector3(x, -halfHeight, z)); // Bottom ring
        vertices.Add(new Vector3(x, halfHeight, z));  // Top ring
      }

      // Triangles
      for (int i = 0; i < segments; i++)
      {
        int bottomLeft = 2 + i * 2;
        int bottomRight = 2 + (i + 1) * 2;
        int topLeft = 3 + i * 2;
        int topRight = 3 + (i + 1) * 2;

        // Side quad
        triangles.Add(bottomLeft);
        triangles.Add(topLeft);
        triangles.Add(topRight);
        triangles.Add(bottomLeft);
        triangles.Add(topRight);
        triangles.Add(bottomRight);

        // Bottom cap
        triangles.Add(0);
        triangles.Add(bottomRight);
        triangles.Add(bottomLeft);

        // Top cap
        triangles.Add(1);
        triangles.Add(topLeft);
        triangles.Add(topRight);
      }

      mesh.vertices = vertices.ToArray();
      mesh.triangles = triangles.ToArray();
      mesh.RecalculateNormals();

      return mesh;
    }

    #endregion

    #region Truck Model (~200 polys)

    /// <summary>
    /// Create a low-poly truck mesh (anime/isekai style)
    /// </summary>
    public static GameObject CreateTruck(Transform parent = null)
    {
      GameObject truck = new GameObject("Truck");
      if (parent != null)
        truck.transform.SetParent(parent, false);

      // Main body color
      Material bodyMat = CreateFlatMaterial(new Color(0.2f, 0.4f, 0.8f)); // Blue
      Material wheelMat = CreateFlatMaterial(new Color(0.15f, 0.15f, 0.15f)); // Dark gray
      Material grillMat = CreateFlatMaterial(new Color(0.7f, 0.7f, 0.7f)); // Silver
      Material glassMat = CreateFlatMaterial(new Color(0.6f, 0.8f, 0.9f, 0.8f)); // Light blue glass

      // Cabin (front part)
      GameObject cabin = CreateMeshObject("Cabin", CreateTruckCabin(), bodyMat);
      cabin.transform.SetParent(truck.transform, false);
      cabin.transform.localPosition = new Vector3(0f, 0.8f, 1.2f);

      // Windshield
      GameObject windshield = CreateMeshObject("Windshield", CreateBox(new Vector3(1.6f, 0.8f, 0.1f)), glassMat);
      windshield.transform.SetParent(truck.transform, false);
      windshield.transform.localPosition = new Vector3(0f, 1.4f, 1.8f);
      windshield.transform.localRotation = Quaternion.Euler(-15f, 0f, 0f);

      // Cargo bed
      GameObject cargo = CreateMeshObject("CargoBed", CreateBox(new Vector3(1.8f, 1.2f, 2.5f)), bodyMat);
      cargo.transform.SetParent(truck.transform, false);
      cargo.transform.localPosition = new Vector3(0f, 0.9f, -0.8f);

      // Grill
      GameObject grill = CreateMeshObject("Grill", CreateBox(new Vector3(1.4f, 0.5f, 0.1f)), grillMat);
      grill.transform.SetParent(truck.transform, false);
      grill.transform.localPosition = new Vector3(0f, 0.5f, 2.0f);

      // Bumper
      GameObject bumper = CreateMeshObject("Bumper", CreateBox(new Vector3(1.8f, 0.2f, 0.15f)), grillMat);
      bumper.transform.SetParent(truck.transform, false);
      bumper.transform.localPosition = new Vector3(0f, 0.2f, 2.05f);

      // Wheels
      CreateWheel(truck.transform, wheelMat, new Vector3(-0.8f, 0.3f, 1.3f));  // Front left
      CreateWheel(truck.transform, wheelMat, new Vector3(0.8f, 0.3f, 1.3f));   // Front right
      CreateWheel(truck.transform, wheelMat, new Vector3(-0.8f, 0.3f, -0.5f)); // Rear left
      CreateWheel(truck.transform, wheelMat, new Vector3(0.8f, 0.3f, -0.5f));  // Rear right
      CreateWheel(truck.transform, wheelMat, new Vector3(-0.8f, 0.3f, -1.3f)); // Rear left 2
      CreateWheel(truck.transform, wheelMat, new Vector3(0.8f, 0.3f, -1.3f));  // Rear right 2

      // Headlights
      Material lightMat = CreateFlatMaterial(new Color(1f, 1f, 0.8f));
      CreateMeshObjectAt(truck.transform, "HeadlightL", CreateBox(new Vector3(0.25f, 0.2f, 0.05f)), lightMat, new Vector3(-0.55f, 0.6f, 2.0f));
      CreateMeshObjectAt(truck.transform, "HeadlightR", CreateBox(new Vector3(0.25f, 0.2f, 0.05f)), lightMat, new Vector3(0.55f, 0.6f, 2.0f));

      // Add collider
      BoxCollider collider = truck.AddComponent<BoxCollider>();
      collider.center = new Vector3(0f, 0.8f, 0.2f);
      collider.size = new Vector3(1.8f, 1.6f, 4.5f);

      return truck;
    }

    private static Mesh CreateTruckCabin()
    {
      // Simple cabin shape - tapered box
      Mesh mesh = new Mesh();
      mesh.name = "TruckCabin";

      Vector3[] vertices = new Vector3[]
      {
        // Bottom (wider)
        new Vector3(-0.9f, -0.5f, -0.6f),
        new Vector3(0.9f, -0.5f, -0.6f),
        new Vector3(0.9f, -0.5f, 0.6f),
        new Vector3(-0.9f, -0.5f, 0.6f),
        // Top (narrower, tilted forward)
        new Vector3(-0.8f, 0.6f, -0.4f),
        new Vector3(0.8f, 0.6f, -0.4f),
        new Vector3(0.8f, 0.5f, 0.7f),
        new Vector3(-0.8f, 0.5f, 0.7f),
      };

      int[] triangles = new int[]
      {
        // Front
        0, 4, 5, 0, 5, 1,
        // Back
        2, 6, 7, 2, 7, 3,
        // Top
        4, 7, 6, 4, 6, 5,
        // Bottom
        0, 1, 2, 0, 2, 3,
        // Left
        0, 3, 7, 0, 7, 4,
        // Right
        1, 5, 6, 1, 6, 2,
      };

      mesh.vertices = vertices;
      mesh.triangles = triangles;
      mesh.RecalculateNormals();

      return mesh;
    }

    private static void CreateWheel(Transform parent, Material mat, Vector3 position)
    {
      GameObject wheel = CreateMeshObject("Wheel", CreateCylinder(0.3f, 0.2f, 8), mat);
      wheel.transform.SetParent(parent, false);
      wheel.transform.localPosition = position;
      wheel.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
    }

    #endregion

    #region NPC Models (~150 polys each)

    /// <summary>
    /// Create a low-poly student character
    /// </summary>
    public static GameObject CreateStudentNerd(Transform parent = null)
    {
      GameObject npc = new GameObject("StudentNerd");
      if (parent != null)
        npc.transform.SetParent(parent, false);

      Material skinMat = CreateFlatMaterial(new Color(0.95f, 0.85f, 0.75f));
      Material shirtMat = CreateFlatMaterial(new Color(0.95f, 0.95f, 1f)); // White shirt
      Material pantsMat = CreateFlatMaterial(new Color(0.2f, 0.2f, 0.3f)); // Dark pants
      Material hairMat = CreateFlatMaterial(new Color(0.1f, 0.1f, 0.15f)); // Black hair
      Material glassesMat = CreateFlatMaterial(new Color(0.3f, 0.3f, 0.35f));

      // Body (slightly hunched)
      GameObject body = CreateMeshObject("Body", CreateBox(new Vector3(0.4f, 0.5f, 0.25f)), shirtMat);
      body.transform.SetParent(npc.transform, false);
      body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
      body.transform.localRotation = Quaternion.Euler(8f, 0f, 0f); // Slouching

      // Head
      GameObject head = CreateMeshObject("Head", CreateBox(new Vector3(0.28f, 0.32f, 0.26f)), skinMat);
      head.transform.SetParent(npc.transform, false);
      head.transform.localPosition = new Vector3(0f, 1.15f, 0.05f);

      // Hair (messy)
      GameObject hair = CreateMeshObject("Hair", CreateBox(new Vector3(0.3f, 0.15f, 0.28f)), hairMat);
      hair.transform.SetParent(npc.transform, false);
      hair.transform.localPosition = new Vector3(0f, 1.35f, 0f);

      // Glasses
      GameObject glasses = CreateMeshObject("Glasses", CreateBox(new Vector3(0.3f, 0.08f, 0.05f)), glassesMat);
      glasses.transform.SetParent(npc.transform, false);
      glasses.transform.localPosition = new Vector3(0f, 1.18f, 0.14f);

      // Legs
      CreateMeshObjectAt(npc.transform, "LegL", CreateBox(new Vector3(0.12f, 0.45f, 0.12f)), pantsMat, new Vector3(-0.1f, 0.22f, 0f));
      CreateMeshObjectAt(npc.transform, "LegR", CreateBox(new Vector3(0.12f, 0.45f, 0.12f)), pantsMat, new Vector3(0.1f, 0.22f, 0f));

      // Book/bag
      GameObject book = CreateMeshObject("Book", CreateBox(new Vector3(0.2f, 0.25f, 0.05f)), new Color(0.6f, 0.3f, 0.2f));
      book.transform.SetParent(npc.transform, false);
      book.transform.localPosition = new Vector3(0.25f, 0.65f, 0.1f);

      AddNPCCollider(npc);
      return npc;
    }

    /// <summary>
    /// Create a low-poly salaryman character
    /// </summary>
    public static GameObject CreateSalaryman(Transform parent = null)
    {
      GameObject npc = new GameObject("Salaryman");
      if (parent != null)
        npc.transform.SetParent(parent, false);

      Material skinMat = CreateFlatMaterial(new Color(0.92f, 0.82f, 0.72f));
      Material suitMat = CreateFlatMaterial(new Color(0.15f, 0.15f, 0.2f)); // Dark suit
      Material shirtMat = CreateFlatMaterial(new Color(0.9f, 0.9f, 0.95f)); // White shirt
      Material tieMat = CreateFlatMaterial(new Color(0.7f, 0.2f, 0.2f)); // Red tie
      Material hairMat = CreateFlatMaterial(new Color(0.1f, 0.08f, 0.05f));
      Material briefcaseMat = CreateFlatMaterial(new Color(0.25f, 0.15f, 0.1f));

      // Body (suit jacket)
      GameObject body = CreateMeshObject("Body", CreateBox(new Vector3(0.45f, 0.55f, 0.22f)), suitMat);
      body.transform.SetParent(npc.transform, false);
      body.transform.localPosition = new Vector3(0f, 0.75f, 0f);

      // Shirt visible
      GameObject shirt = CreateMeshObject("Shirt", CreateBox(new Vector3(0.2f, 0.3f, 0.05f)), shirtMat);
      shirt.transform.SetParent(npc.transform, false);
      shirt.transform.localPosition = new Vector3(0f, 0.85f, 0.12f);

      // Tie
      GameObject tie = CreateMeshObject("Tie", CreateBox(new Vector3(0.06f, 0.25f, 0.02f)), tieMat);
      tie.transform.SetParent(npc.transform, false);
      tie.transform.localPosition = new Vector3(0f, 0.78f, 0.13f);

      // Head
      GameObject head = CreateMeshObject("Head", CreateBox(new Vector3(0.26f, 0.3f, 0.24f)), skinMat);
      head.transform.SetParent(npc.transform, false);
      head.transform.localPosition = new Vector3(0f, 1.2f, 0f);

      // Hair (neat)
      GameObject hair = CreateMeshObject("Hair", CreateBox(new Vector3(0.27f, 0.1f, 0.25f)), hairMat);
      hair.transform.SetParent(npc.transform, false);
      hair.transform.localPosition = new Vector3(0f, 1.38f, -0.02f);

      // Legs
      CreateMeshObjectAt(npc.transform, "LegL", CreateBox(new Vector3(0.14f, 0.48f, 0.14f)), suitMat, new Vector3(-0.12f, 0.24f, 0f));
      CreateMeshObjectAt(npc.transform, "LegR", CreateBox(new Vector3(0.14f, 0.48f, 0.14f)), suitMat, new Vector3(0.12f, 0.24f, 0f));

      // Briefcase
      GameObject briefcase = CreateMeshObject("Briefcase", CreateBox(new Vector3(0.3f, 0.22f, 0.08f)), briefcaseMat);
      briefcase.transform.SetParent(npc.transform, false);
      briefcase.transform.localPosition = new Vector3(0.35f, 0.4f, 0f);

      AddNPCCollider(npc);
      return npc;
    }

    /// <summary>
    /// Create a low-poly grandma character
    /// </summary>
    public static GameObject CreateGrandma(Transform parent = null)
    {
      GameObject npc = new GameObject("Grandma");
      if (parent != null)
        npc.transform.SetParent(parent, false);

      Material skinMat = CreateFlatMaterial(new Color(0.9f, 0.8f, 0.7f));
      Material dressMat = CreateFlatMaterial(new Color(0.5f, 0.35f, 0.5f)); // Purple dress
      Material scarfMat = CreateFlatMaterial(new Color(0.8f, 0.75f, 0.6f)); // Beige scarf
      Material hairMat = CreateFlatMaterial(new Color(0.7f, 0.7f, 0.75f)); // Gray hair
      Material caneMat = CreateFlatMaterial(new Color(0.35f, 0.2f, 0.1f)); // Brown cane

      // Body (dress, slightly hunched)
      GameObject body = CreateMeshObject("Body", CreateBox(new Vector3(0.4f, 0.6f, 0.3f)), dressMat);
      body.transform.SetParent(npc.transform, false);
      body.transform.localPosition = new Vector3(0f, 0.6f, 0f);
      body.transform.localRotation = Quaternion.Euler(10f, 0f, 0f); // Hunched

      // Head
      GameObject head = CreateMeshObject("Head", CreateBox(new Vector3(0.24f, 0.26f, 0.22f)), skinMat);
      head.transform.SetParent(npc.transform, false);
      head.transform.localPosition = new Vector3(0f, 1.05f, 0.08f);

      // Headscarf
      GameObject scarf = CreateMeshObject("Scarf", CreateBox(new Vector3(0.28f, 0.2f, 0.26f)), scarfMat);
      scarf.transform.SetParent(npc.transform, false);
      scarf.transform.localPosition = new Vector3(0f, 1.15f, 0.02f);

      // Hair (bun visible)
      GameObject hair = CreateMeshObject("Hair", CreateBox(new Vector3(0.12f, 0.1f, 0.12f)), hairMat);
      hair.transform.SetParent(npc.transform, false);
      hair.transform.localPosition = new Vector3(0f, 1.18f, -0.12f);

      // Skirt extension
      GameObject skirt = CreateMeshObject("Skirt", CreateBox(new Vector3(0.45f, 0.35f, 0.35f)), dressMat);
      skirt.transform.SetParent(npc.transform, false);
      skirt.transform.localPosition = new Vector3(0f, 0.2f, 0f);

      // Legs (shorter)
      CreateMeshObjectAt(npc.transform, "LegL", CreateBox(new Vector3(0.1f, 0.3f, 0.1f)), skinMat, new Vector3(-0.08f, 0.02f, 0f));
      CreateMeshObjectAt(npc.transform, "LegR", CreateBox(new Vector3(0.1f, 0.3f, 0.1f)), skinMat, new Vector3(0.08f, 0.02f, 0f));

      // Walking cane
      GameObject cane = CreateMeshObject("Cane", CreateCylinder(0.02f, 0.8f, 6), caneMat);
      cane.transform.SetParent(npc.transform, false);
      cane.transform.localPosition = new Vector3(0.3f, 0.4f, 0.15f);
      cane.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);

      // Cane handle
      GameObject handle = CreateMeshObject("CaneHandle", CreateBox(new Vector3(0.08f, 0.03f, 0.03f)), caneMat);
      handle.transform.SetParent(npc.transform, false);
      handle.transform.localPosition = new Vector3(0.35f, 0.82f, 0.15f);

      AddNPCCollider(npc);
      return npc;
    }

    private static void AddNPCCollider(GameObject npc)
    {
      CapsuleCollider collider = npc.AddComponent<CapsuleCollider>();
      collider.center = new Vector3(0f, 0.7f, 0f);
      collider.radius = 0.25f;
      collider.height = 1.5f;
    }

    #endregion

    #region Environment

    /// <summary>
    /// Create a road section (3 lanes)
    /// </summary>
    public static GameObject CreateRoadSection(float length = 10f, Transform parent = null)
    {
      GameObject road = new GameObject("RoadSection");
      if (parent != null)
        road.transform.SetParent(parent, false);

      Material asphaltMat = CreateFlatMaterial(new Color(0.2f, 0.2f, 0.22f));
      Material lineMat = CreateFlatMaterial(new Color(0.9f, 0.9f, 0.9f));
      Material yellowMat = CreateFlatMaterial(new Color(0.9f, 0.8f, 0.2f));

      float roadWidth = 9f; // 3 lanes * 3m each

      // Main road surface
      GameObject surface = CreateMeshObject("Surface", CreateBox(new Vector3(roadWidth, 0.1f, length)), asphaltMat);
      surface.transform.SetParent(road.transform, false);
      surface.transform.localPosition = new Vector3(0f, 0f, 0f);

      // Center line (yellow, dashed)
      for (float z = -length / 2f + 1f; z < length / 2f; z += 2f)
      {
        GameObject centerLine = CreateMeshObject("CenterLine", CreateBox(new Vector3(0.15f, 0.02f, 1f)), yellowMat);
        centerLine.transform.SetParent(road.transform, false);
        centerLine.transform.localPosition = new Vector3(0f, 0.06f, z);
      }

      // Lane dividers (white, dashed)
      float[] lanePositions = { -3f, 3f };
      foreach (float x in lanePositions)
      {
        for (float z = -length / 2f + 1f; z < length / 2f; z += 3f)
        {
          GameObject laneLine = CreateMeshObject("LaneLine", CreateBox(new Vector3(0.12f, 0.02f, 1.5f)), lineMat);
          laneLine.transform.SetParent(road.transform, false);
          laneLine.transform.localPosition = new Vector3(x, 0.06f, z);
        }
      }

      // Edge lines (solid white)
      CreateMeshObjectAt(road.transform, "EdgeLineL", CreateBox(new Vector3(0.15f, 0.02f, length)), lineMat, new Vector3(-roadWidth / 2f + 0.1f, 0.06f, 0f));
      CreateMeshObjectAt(road.transform, "EdgeLineR", CreateBox(new Vector3(0.15f, 0.02f, length)), lineMat, new Vector3(roadWidth / 2f - 0.1f, 0.06f, 0f));

      return road;
    }

    /// <summary>
    /// Create a sidewalk section
    /// </summary>
    public static GameObject CreateSidewalk(float length = 10f, float width = 2f, Transform parent = null)
    {
      GameObject sidewalk = new GameObject("Sidewalk");
      if (parent != null)
        sidewalk.transform.SetParent(parent, false);

      Material concreteMat = CreateFlatMaterial(new Color(0.6f, 0.58f, 0.55f));
      Material curbMat = CreateFlatMaterial(new Color(0.5f, 0.48f, 0.45f));

      // Main surface
      GameObject surface = CreateMeshObject("Surface", CreateBox(new Vector3(width, 0.15f, length)), concreteMat);
      surface.transform.SetParent(sidewalk.transform, false);
      surface.transform.localPosition = new Vector3(0f, 0.075f, 0f);

      // Curb
      GameObject curb = CreateMeshObject("Curb", CreateBox(new Vector3(0.2f, 0.15f, length)), curbMat);
      curb.transform.SetParent(sidewalk.transform, false);
      curb.transform.localPosition = new Vector3(-width / 2f - 0.1f, 0.075f, 0f);

      return sidewalk;
    }

    /// <summary>
    /// Create a simple building (background)
    /// </summary>
    public static GameObject CreateBuilding(float width, float height, float depth, Color color, Transform parent = null)
    {
      GameObject building = new GameObject("Building");
      if (parent != null)
        building.transform.SetParent(parent, false);

      Material wallMat = CreateFlatMaterial(color);
      Material windowMat = CreateFlatMaterial(new Color(0.4f, 0.5f, 0.6f));
      Material roofMat = CreateFlatMaterial(color * 0.7f);

      // Main structure
      GameObject main = CreateMeshObject("Main", CreateBox(new Vector3(width, height, depth)), wallMat);
      main.transform.SetParent(building.transform, false);
      main.transform.localPosition = new Vector3(0f, height / 2f, 0f);

      // Roof
      GameObject roof = CreateMeshObject("Roof", CreateBox(new Vector3(width + 0.2f, 0.3f, depth + 0.2f)), roofMat);
      roof.transform.SetParent(building.transform, false);
      roof.transform.localPosition = new Vector3(0f, height + 0.15f, 0f);

      // Windows (simple grid)
      int windowRows = Mathf.FloorToInt(height / 2f);
      int windowCols = Mathf.FloorToInt(width / 2f);

      for (int row = 0; row < windowRows; row++)
      {
        for (int col = 0; col < windowCols; col++)
        {
          float wx = -width / 2f + 1f + col * 2f;
          float wy = 1.5f + row * 2f;

          GameObject window = CreateMeshObject("Window", CreateBox(new Vector3(0.8f, 1.2f, 0.05f)), windowMat);
          window.transform.SetParent(building.transform, false);
          window.transform.localPosition = new Vector3(wx, wy, depth / 2f + 0.03f);
        }
      }

      return building;
    }

    #endregion

    #region Hub Environment

    /// <summary>
    /// Create hub room
    /// </summary>
    public static GameObject CreateHubRoom(Transform parent = null)
    {
      GameObject room = new GameObject("HubRoom");
      if (parent != null)
        room.transform.SetParent(parent, false);

      Material floorMat = CreateFlatMaterial(new Color(0.4f, 0.35f, 0.3f));
      Material wallMat = CreateFlatMaterial(new Color(0.7f, 0.65f, 0.6f));
      Material ceilingMat = CreateFlatMaterial(new Color(0.8f, 0.78f, 0.75f));

      float roomWidth = 12f;
      float roomLength = 15f;
      float roomHeight = 4f;

      // Floor
      CreateMeshObjectAt(room.transform, "Floor", CreateBox(new Vector3(roomWidth, 0.2f, roomLength)), floorMat, new Vector3(0f, -0.1f, 0f));

      // Walls
      CreateMeshObjectAt(room.transform, "WallN", CreateBox(new Vector3(roomWidth, roomHeight, 0.3f)), wallMat, new Vector3(0f, roomHeight / 2f, roomLength / 2f));
      CreateMeshObjectAt(room.transform, "WallS", CreateBox(new Vector3(roomWidth, roomHeight, 0.3f)), wallMat, new Vector3(0f, roomHeight / 2f, -roomLength / 2f));
      CreateMeshObjectAt(room.transform, "WallE", CreateBox(new Vector3(0.3f, roomHeight, roomLength)), wallMat, new Vector3(roomWidth / 2f, roomHeight / 2f, 0f));
      CreateMeshObjectAt(room.transform, "WallW", CreateBox(new Vector3(0.3f, roomHeight, roomLength)), wallMat, new Vector3(-roomWidth / 2f, roomHeight / 2f, 0f));

      // Ceiling
      CreateMeshObjectAt(room.transform, "Ceiling", CreateBox(new Vector3(roomWidth, 0.2f, roomLength)), ceilingMat, new Vector3(0f, roomHeight, 0f));

      return room;
    }

    /// <summary>
    /// Create a desk/table
    /// </summary>
    public static GameObject CreateDesk(Transform parent = null)
    {
      GameObject desk = new GameObject("Desk");
      if (parent != null)
        desk.transform.SetParent(parent, false);

      Material woodMat = CreateFlatMaterial(new Color(0.5f, 0.35f, 0.2f));

      // Top
      CreateMeshObjectAt(desk.transform, "Top", CreateBox(new Vector3(1.5f, 0.08f, 0.8f)), woodMat, new Vector3(0f, 0.75f, 0f));

      // Legs
      CreateMeshObjectAt(desk.transform, "LegFL", CreateBox(new Vector3(0.08f, 0.75f, 0.08f)), woodMat, new Vector3(-0.65f, 0.375f, 0.3f));
      CreateMeshObjectAt(desk.transform, "LegFR", CreateBox(new Vector3(0.08f, 0.75f, 0.08f)), woodMat, new Vector3(0.65f, 0.375f, 0.3f));
      CreateMeshObjectAt(desk.transform, "LegBL", CreateBox(new Vector3(0.08f, 0.75f, 0.08f)), woodMat, new Vector3(-0.65f, 0.375f, -0.3f));
      CreateMeshObjectAt(desk.transform, "LegBR", CreateBox(new Vector3(0.08f, 0.75f, 0.08f)), woodMat, new Vector3(0.65f, 0.375f, -0.3f));

      return desk;
    }

    /// <summary>
    /// Create a quest board
    /// </summary>
    public static GameObject CreateQuestBoard(Transform parent = null)
    {
      GameObject board = new GameObject("QuestBoard");
      if (parent != null)
        board.transform.SetParent(parent, false);

      Material frameMat = CreateFlatMaterial(new Color(0.4f, 0.3f, 0.2f));
      Material corkMat = CreateFlatMaterial(new Color(0.7f, 0.55f, 0.35f));
      Material paperMat = CreateFlatMaterial(new Color(0.95f, 0.92f, 0.85f));

      // Frame
      CreateMeshObjectAt(board.transform, "Frame", CreateBox(new Vector3(2f, 1.5f, 0.1f)), frameMat, new Vector3(0f, 1.5f, 0f));

      // Cork surface
      CreateMeshObjectAt(board.transform, "Cork", CreateBox(new Vector3(1.8f, 1.3f, 0.05f)), corkMat, new Vector3(0f, 1.5f, 0.06f));

      // Papers/notes
      CreateMeshObjectAt(board.transform, "Note1", CreateBox(new Vector3(0.3f, 0.4f, 0.01f)), paperMat, new Vector3(-0.5f, 1.6f, 0.1f));
      CreateMeshObjectAt(board.transform, "Note2", CreateBox(new Vector3(0.35f, 0.3f, 0.01f)), paperMat, new Vector3(0.3f, 1.7f, 0.1f));
      CreateMeshObjectAt(board.transform, "Note3", CreateBox(new Vector3(0.25f, 0.35f, 0.01f)), paperMat, new Vector3(0f, 1.3f, 0.1f));

      return board;
    }

    #endregion

    #region Helpers

    private static Material CreateFlatMaterial(Color color)
    {
      // Try to use URP unlit shader first, fallback to standard
      Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
      if (shader == null)
        shader = Shader.Find("Unlit/Color");
      if (shader == null)
        shader = Shader.Find("Standard");

      Material mat = new Material(shader);
      mat.color = color;

      if (color.a < 1f)
      {
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
      }

      return mat;
    }

    private static GameObject CreateMeshObject(string name, Mesh mesh, Material mat)
    {
      GameObject obj = new GameObject(name);
      MeshFilter filter = obj.AddComponent<MeshFilter>();
      filter.mesh = mesh;

      MeshRenderer renderer = obj.AddComponent<MeshRenderer>();
      renderer.material = mat;

      return obj;
    }

    private static GameObject CreateMeshObject(string name, Mesh mesh, Color color)
    {
      return CreateMeshObject(name, mesh, CreateFlatMaterial(color));
    }

    private static void CreateMeshObjectAt(Transform parent, string name, Mesh mesh, Material mat, Vector3 localPos)
    {
      GameObject obj = CreateMeshObject(name, mesh, mat);
      obj.transform.SetParent(parent, false);
      obj.transform.localPosition = localPos;
    }

    #endregion
  }
}
