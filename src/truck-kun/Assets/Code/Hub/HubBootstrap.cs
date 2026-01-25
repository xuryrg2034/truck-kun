using Code.Gameplay.Features.Economy;
using Code.Infrastructure;
using Code.Meta.Upgrades;
using Code.UI.HubUI;
using UnityEngine;

namespace Code.Hub
{
  public class HubBootstrap : MonoBehaviour
  {
    private IMoneyService _moneyService;
    private IUpgradeService _upgradeService;
    private HubController _player;
    private HubMainUI _mainUI;
    private Camera _mainCamera;

    private void Awake()
    {
      // Ensure GameStateService is loaded
      GameStateService gameState = GameStateService.Instance;

      // Create services for hub (backed by GameStateService)
      _moneyService = new HubMoneyService(gameState);
      _upgradeService = new HubUpgradeService(_moneyService, gameState);
      _upgradeService.Initialize();

      CreateEnvironment();
      CreatePlayer();
      CreateCamera();
      CreateZones();
      CreateUI();

      Debug.Log($"[HubBootstrap] Hub loaded. Day {gameState.DayNumber}, Balance: {gameState.PlayerMoney}¥");
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
        "Столовая",
        new Vector3(-8f, 0.75f, 8f),
        new Vector3(3f, 1.5f, 2f),
        new Color(0.8f, 0.5f, 0.2f)
      );
      foodZone.Initialize(OnZoneInteract);

      // Quest board zone (blue)
      InteractableZone questZone = InteractableZone.Create(
        zonesParent,
        ZoneType.Quests,
        "Доска заданий",
        new Vector3(0f, 1.5f, 10f),
        new Vector3(4f, 3f, 0.3f),
        new Color(0.2f, 0.5f, 0.8f)
      );
      questZone.Initialize(OnZoneInteract);

      // Garage zone (gray)
      InteractableZone garageZone = InteractableZone.Create(
        zonesParent,
        ZoneType.Garage,
        "Гараж",
        new Vector3(8f, 1f, 8f),
        new Vector3(4f, 2f, 4f),
        new Color(0.5f, 0.5f, 0.5f)
      );
      garageZone.Initialize(OnZoneInteract);

      // Start Day zone (green, exit door)
      InteractableZone startDayZone = InteractableZone.Create(
        zonesParent,
        ZoneType.StartDay,
        "Выход на работу",
        new Vector3(0f, 1.5f, -12f),
        new Vector3(3f, 3f, 1f),
        new Color(0.2f, 0.7f, 0.3f)
      );
      startDayZone.Initialize(OnZoneInteract);
    }

    private void CreateUI()
    {
      GameObject uiObj = new GameObject("HubMainUI");
      _mainUI = uiObj.AddComponent<HubMainUI>();
      _mainUI.Initialize(_moneyService, _upgradeService);
    }

    private void OnZoneInteract(ZoneType type)
    {
      _mainUI.ShowZonePanel(type);
    }
  }

  /// <summary>
  /// Money service for hub backed by GameStateService
  /// </summary>
  public class HubMoneyService : IMoneyService
  {
    private readonly GameStateService _gameState;

    public int Balance => _gameState.PlayerMoney;
    public int EarnedToday => 0;
    public int PenaltiesToday => 0;

    public HubMoneyService(GameStateService gameState)
    {
      _gameState = gameState;
    }

    public void AddMoney(int amount)
    {
      if (amount <= 0)
        return;

      _gameState.AddMoney(amount);
      _gameState.Save();
    }

    public bool SpendMoney(int amount)
    {
      if (amount <= 0)
        return true;

      if (!_gameState.SpendMoney(amount))
        return false;

      _gameState.Save();
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

      _gameState.SpendMoney(amount);
      _gameState.Save();
    }

    public void ResetDayEarnings()
    {
      // Not needed in hub
    }

    public void Initialize()
    {
      // GameStateService handles initialization
    }
  }
}
