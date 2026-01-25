using Code.Gameplay.Features.Economy;
using Code.Meta.Upgrades;
using UnityEngine;

namespace Code.Hub
{
  public class HubBootstrap : MonoBehaviour
  {
    private IMoneyService _moneyService;
    private IUpgradeService _upgradeService;
    private HubController _player;
    private HubUIManager _uiManager;
    private Camera _mainCamera;

    private void Awake()
    {
      // Create services for hub (loads persisted data)
      _moneyService = new HubMoneyService();
      _upgradeService = new HubUpgradeService(_moneyService);
      _upgradeService.Initialize();

      CreateEnvironment();
      CreatePlayer();
      CreateCamera();
      CreateZones();
      CreateUI();
    }

    private void CreateEnvironment()
    {
      // Floor
      GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
      floor.name = "Floor";
      floor.transform.position = Vector3.zero;
      floor.transform.localScale = new Vector3(3f, 1f, 3f);

      Renderer floorRenderer = floor.GetComponent<Renderer>();
      Material floorMat = new Material(Shader.Find("Standard"));
      floorMat.color = new Color(0.3f, 0.3f, 0.35f);
      floorRenderer.material = floorMat;

      // Walls
      CreateWall("WallNorth", new Vector3(0f, 2.5f, 15f), new Vector3(30f, 5f, 0.5f));
      CreateWall("WallSouth", new Vector3(0f, 2.5f, -15f), new Vector3(30f, 5f, 0.5f));
      CreateWall("WallEast", new Vector3(15f, 2.5f, 0f), new Vector3(0.5f, 5f, 30f));
      CreateWall("WallWest", new Vector3(-15f, 2.5f, 0f), new Vector3(0.5f, 5f, 30f));
    }

    private void CreateWall(string name, Vector3 position, Vector3 scale)
    {
      GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
      wall.name = name;
      wall.transform.position = position;
      wall.transform.localScale = scale;

      Renderer renderer = wall.GetComponent<Renderer>();
      Material mat = new Material(Shader.Find("Standard"));
      mat.color = new Color(0.4f, 0.4f, 0.45f);
      renderer.material = mat;
    }

    private void CreatePlayer()
    {
      _player = HubController.Create(new Vector3(0f, 1f, 0f));
    }

    private void CreateCamera()
    {
      GameObject cameraObj = new GameObject("MainCamera");
      cameraObj.tag = "MainCamera";

      _mainCamera = cameraObj.AddComponent<Camera>();
      _mainCamera.clearFlags = CameraClearFlags.SolidColor;
      _mainCamera.backgroundColor = new Color(0.1f, 0.1f, 0.15f);
      _mainCamera.fieldOfView = 60f;

      cameraObj.AddComponent<AudioListener>();

      // Position camera behind and above player
      cameraObj.transform.position = new Vector3(0f, 12f, -18f);
      cameraObj.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

      _player.SetCamera(cameraObj.transform);
    }

    private void CreateZones()
    {
      Transform zonesParent = new GameObject("Zones").transform;

      // Food zone (orange table)
      InteractableZone foodZone = InteractableZone.Create(
        zonesParent,
        ZoneType.Food,
        "Food Shop",
        new Vector3(-8f, 0.75f, 8f),
        new Vector3(3f, 1.5f, 2f),
        new Color(0.8f, 0.5f, 0.2f)
      );
      foodZone.Initialize(OnZoneInteract);

      // Quest board zone (blue)
      InteractableZone questZone = InteractableZone.Create(
        zonesParent,
        ZoneType.Quests,
        "Quest Board",
        new Vector3(0f, 1.5f, 10f),
        new Vector3(4f, 3f, 0.3f),
        new Color(0.2f, 0.5f, 0.8f)
      );
      questZone.Initialize(OnZoneInteract);

      // Garage zone (gray)
      InteractableZone garageZone = InteractableZone.Create(
        zonesParent,
        ZoneType.Garage,
        "Garage",
        new Vector3(8f, 1f, 8f),
        new Vector3(4f, 2f, 4f),
        new Color(0.5f, 0.5f, 0.5f)
      );
      garageZone.Initialize(OnZoneInteract);

      // Start Day zone (green, exit door)
      InteractableZone startDayZone = InteractableZone.Create(
        zonesParent,
        ZoneType.StartDay,
        "Start Day",
        new Vector3(0f, 1.5f, -12f),
        new Vector3(3f, 3f, 1f),
        new Color(0.2f, 0.7f, 0.3f)
      );
      startDayZone.Initialize(OnZoneInteract);
    }

    private void CreateUI()
    {
      GameObject uiObj = new GameObject("HubUI");
      _uiManager = uiObj.AddComponent<HubUIManager>();
      _uiManager.Initialize(_moneyService, _upgradeService);
    }

    private void OnZoneInteract(ZoneType type)
    {
      _uiManager.ShowZonePanel(type);
    }
  }

  /// <summary>
  /// Simple money service for hub that persists balance via PlayerPrefs
  /// </summary>
  public class HubMoneyService : IMoneyService
  {
    private const string BalanceKey = "PlayerBalance";

    public int Balance { get; private set; }
    public int EarnedToday => 0;
    public int PenaltiesToday => 0;

    public HubMoneyService()
    {
      Balance = PlayerPrefs.GetInt(BalanceKey, 0);
    }

    public void AddMoney(int amount)
    {
      if (amount <= 0)
        return;

      Balance += amount;
      Save();
    }

    public bool SpendMoney(int amount)
    {
      if (amount <= 0)
        return true;

      if (Balance < amount)
        return false;

      Balance -= amount;
      Save();
      return true;
    }

    public int GetBalance()
    {
      return Balance;
    }

    public void ApplyPenalty(int amount)
    {
      if (amount <= 0)
        return;

      Balance = Mathf.Max(0, Balance - amount);
      Save();
    }

    public void ResetDayEarnings()
    {
      // Not needed in hub
    }

    public void Initialize()
    {
      // Already initialized in constructor
    }

    private void Save()
    {
      PlayerPrefs.SetInt(BalanceKey, Balance);
      PlayerPrefs.Save();
    }
  }
}
