using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Feedback;
using Code.Gameplay.Features.Hero;
using Code.Gameplay.Features.Movement;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Physics;
using Code.Gameplay.Features.Quest;
using Code.Gameplay.Input;
using Code.Infrastructure.Systems;
using Code.Infrastructure.View;

namespace Code.Gameplay
{
  /// <summary>
  /// Main battle feature orchestrating all gameplay systems.
  ///
  /// Execution model:
  /// - Execute() called in Update - game logic, input, AI
  /// - FixedExecute() called in FixedUpdate - physics systems
  /// - Cleanup() called after each execution cycle
  ///
  /// Pipeline order:
  /// Update:  Input → Hero → Pedestrian → Collision → Feedback → Quest → Economy → BindView → Movement
  /// FixedUpdate: Physics (velocity calculation → apply to Rigidbody → sync position)
  /// </summary>
  public sealed class BattleFeature : Feature
  {
    private readonly PhysicsFeature _physicsFeature;

    public BattleFeature(ISystemFactory systems)
    {
      // === UPDATE SYSTEMS (game logic) ===

      // Input processing
      Add(systems.Create<InputFeature>());

      // Hero initialization and input handling
      Add(systems.Create<HeroFeature>());

      // NPC spawning and AI
      Add(systems.Create<PedestrianFeature>());

      // Collision detection
      Add(systems.Create<CollisionFeature>());

      // Visual/audio feedback after collision
      Add(systems.Create<FeedbackFeature>());

      // Quest tracking
      Add(systems.Create<QuestFeature>());

      // Economy (money, penalties)
      Add(systems.Create<EconomyFeature>());

      // View binding (prefab instantiation)
      Add(systems.Create<BindViewFeature>());

      // Kinematic movement (for NPCs without Rigidbody)
      Add(systems.Create<MovementFeature>());

      // === FIXED UPDATE SYSTEMS (physics) ===
      // PhysicsFeature runs separately in FixedUpdate for deterministic physics
      _physicsFeature = systems.Create<PhysicsFeature>();
    }

    /// <summary>
    /// Initialize all systems including physics
    /// </summary>
    public override void Initialize()
    {
      base.Initialize();
      _physicsFeature.Initialize();
    }

    /// <summary>
    /// Execute game logic systems (call from Update)
    /// </summary>
    public override void Execute()
    {
      base.Execute();
    }

    /// <summary>
    /// Execute physics systems (call from FixedUpdate)
    /// Physics runs at fixed timestep for deterministic behavior
    /// </summary>
    public void FixedExecute()
    {
      _physicsFeature.Execute();
    }

    /// <summary>
    /// Cleanup all systems
    /// </summary>
    public override void Cleanup()
    {
      base.Cleanup();
      _physicsFeature.Cleanup();
    }

    /// <summary>
    /// Tear down all systems
    /// </summary>
    public override void TearDown()
    {
      _physicsFeature.TearDown();
      base.TearDown();
    }
  }
}
