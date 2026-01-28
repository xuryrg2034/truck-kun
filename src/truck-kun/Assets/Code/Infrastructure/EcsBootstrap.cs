using Code.Balance;
using Code.Gameplay;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Quest;
using Code.Gameplay.Input;
using Code.Infrastructure.Systems;
using Code.Meta.Difficulty;
using Code.Meta.Upgrades;
using Code.UI.EndDayScreen;
using Code.UI.QuestUI;
using UnityEngine;
using Zenject;

namespace Code.Infrastructure
{
  /// <summary>
  /// Coordinates ECS gameplay systems.
  /// Receives all dependencies via Zenject injection from GameplaySceneInstaller.
  /// </summary>
  public class EcsBootstrap : MonoBehaviour
  {
    // Injected dependencies
    [Inject] private Contexts _contexts;
    [Inject] private IBalanceProvider _balanceProvider;
    [Inject] private IDifficultyService _difficultyService;
    [Inject] private IInputService _inputService;
    [Inject] private IDaySessionService _daySessionService;
    [Inject] private IMoneyService _moneyService;
    [Inject] private IQuestService _questService;
    [Inject] private IUpgradeService _upgradeService;
    [Inject] private ISystemFactory _systemFactory;

    // Injected settings (modified in-place for difficulty/upgrades)
    [Inject] private RunnerMovementSettings _runnerMovement;
    [Inject] private PedestrianSpawnSettings _pedestrianSpawnSettings;
    [Inject] private QuestSettings _questSettings;
    [Inject] private EconomySettings _economySettings;

    // Runtime state
    private BattleFeature _battleFeature;
    private QuestUIController _questUI;
    private EndDayController _endDayController;
    private DayCounterUI _dayCounterUI;
    private bool _dayFinishedHandled;
    private bool _physicsEnabled = true;

    [Inject]
    public void Construct()
    {
      // Get current day from persistent state
      GameStateService gameState = GameStateService.Instance;
      int currentDay = gameState.DayNumber;

      // Apply difficulty scaling to settings
      ApplyDifficultyScaling(currentDay);

      // Apply upgrades from persistent state
      ApplyUpgradesToSettings();

      // Initialize upgrade service
      _upgradeService.Initialize();

      // Create and initialize ECS systems
      _battleFeature = _systemFactory.Create<BattleFeature>();
      _battleFeature.Initialize();
      Debug.Log("[EcsBootstrap] BattleFeature initialized (Physics runs in FixedUpdate)");

      // Initialize UI
      InitializeQuestUI();
      InitializeEndDayScreen();
      InitializeDayCounterUI(currentDay);

      // Handle milestone bonus
      HandleMilestoneBonus(currentDay);

      // Start the day
      _daySessionService.StartDay();

      if (_daySessionService.State == DayState.Finished)
        HandleDayFinished();

      DifficultyConfig difficulty = _difficultyService.CurrentDifficulty;
      Debug.Log($"[EcsBootstrap] Day {currentDay} started. Balance: {gameState.PlayerMoney}¥. " +
                $"Difficulty: Quests={difficulty.QuestCount}, SpawnInterval={difficulty.PedestrianSpawnInterval:F2}");
    }

    private void ApplyDifficultyScaling(int dayNumber)
    {
      _difficultyService.ApplyDifficulty(dayNumber);
      DifficultyConfig difficulty = _difficultyService.CurrentDifficulty;

      // Apply to pedestrian settings
      _pedestrianSpawnSettings.SpawnInterval = difficulty.PedestrianSpawnInterval;
      _pedestrianSpawnSettings.MaxActive = difficulty.MaxPedestrians;
      _pedestrianSpawnSettings.CrossingChance = difficulty.CrossingChance;

      // Apply to movement
      _runnerMovement.ForwardSpeed *= difficulty.ForwardSpeedMultiplier;

      // Apply to quests
      _questSettings.MinQuestsPerDay = difficulty.QuestCount;
      _questSettings.MaxQuestsPerDay = difficulty.QuestCount;
    }

    private void ApplyUpgradesToSettings()
    {
      GameStateService state = GameStateService.Instance;

      // Apply speed upgrade
      float speedBonus = GetUpgradeBonus(UpgradeType.SpeedBoost, state.GetUpgradeLevel(UpgradeType.SpeedBoost));
      if (speedBonus > 0)
      {
        float multiplier = 1f + speedBonus;
        _runnerMovement.ForwardSpeed *= multiplier;
        _runnerMovement.MinForwardSpeed *= multiplier;
        _runnerMovement.MaxForwardSpeed *= multiplier;
        _runnerMovement.ForwardAcceleration *= (1f + speedBonus * 0.5f);
        Debug.Log($"[Upgrades] SpeedBoost applied: +{speedBonus * 100:F0}% speed");
      }

      // Apply maneuverability upgrade
      float lateralBonus = GetUpgradeBonus(UpgradeType.Maneuverability, state.GetUpgradeLevel(UpgradeType.Maneuverability));
      if (lateralBonus > 0)
      {
        float multiplier = 1f + lateralBonus;
        _runnerMovement.LateralSpeed *= multiplier;
        _runnerMovement.MaxLateralSpeed *= multiplier;
        _runnerMovement.LateralAcceleration *= multiplier;
        _runnerMovement.Deceleration *= (1f + lateralBonus * 0.3f);
        Debug.Log($"[Upgrades] Maneuverability applied: +{lateralBonus * 100:F0}% lateral control");
      }

      // Set starting money from state
      _economySettings.StartingMoney = state.PlayerMoney;
    }

    private float GetUpgradeBonus(UpgradeType type, int level)
    {
      if (level <= 0)
        return 0f;

      UpgradeDefinitionBalance upgrade = _balanceProvider.Balance.GetUpgrade(type);
      return upgrade?.GetBonus(level) ?? 0f;
    }

    private void Update()
    {
      if (_battleFeature == null || _daySessionService == null)
        return;

      if (_daySessionService.State == DayState.Finished)
      {
        HandleDayFinished();
        return;
      }

      if (_daySessionService.Tick())
      {
        HandleDayFinished();
        return;
      }

      _battleFeature.Execute();
      _battleFeature.Cleanup();
    }

    private void FixedUpdate()
    {
      if (!_physicsEnabled || _battleFeature == null || _daySessionService == null)
        return;

      if (_daySessionService.State == DayState.Finished)
        return;

      _battleFeature.FixedExecute();
    }

    private void OnDestroy()
    {
      if (_battleFeature != null)
      {
        _battleFeature.TearDown();
        _battleFeature = null;
      }

      (_inputService as System.IDisposable)?.Dispose();
    }

    private void InitializeQuestUI()
    {
      GameObject questUIObj = new GameObject("QuestUI");
      questUIObj.transform.SetParent(transform, false);

      _questUI = questUIObj.AddComponent<QuestUIController>();
      _questUI.Initialize(_contexts.meta);
    }

    private void InitializeEndDayScreen()
    {
      GameObject endDayObj = new GameObject("EndDayScreen");
      endDayObj.transform.SetParent(transform, false);

      _endDayController = endDayObj.AddComponent<EndDayController>();
      _endDayController.Initialize(_contexts.meta, _moneyService, _questService);
    }

    private void HandleDayFinished()
    {
      if (_dayFinishedHandled)
        return;

      _dayFinishedHandled = true;
      _endDayController.Show();
    }

    private void InitializeDayCounterUI(int dayNumber)
    {
      GameObject dayCounterObj = new GameObject("DayCounterUI");
      dayCounterObj.transform.SetParent(transform, false);

      _dayCounterUI = dayCounterObj.AddComponent<DayCounterUI>();

      DifficultyConfig difficulty = _difficultyService.CurrentDifficulty;
      _dayCounterUI.Initialize(
        dayNumber,
        difficulty.IsMilestoneDay,
        difficulty.MilestoneBonus
      );
    }

    private void HandleMilestoneBonus(int dayNumber)
    {
      if (!_difficultyService.IsMilestoneDay(dayNumber))
        return;

      int bonus = _difficultyService.GetMilestoneBonus(dayNumber);
      if (bonus > 0)
      {
        GameStateService.Instance.AddMoney(bonus);
        Debug.Log($"[EcsBootstrap] Milestone Day {dayNumber}! Bonus: +{bonus}");
      }
    }
  }
}
