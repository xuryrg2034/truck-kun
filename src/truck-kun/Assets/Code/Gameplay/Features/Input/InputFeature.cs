using Code.Infrastructure.Systems;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Gameplay.Features.Input
{
  [Input] public class MoveInput : IComponent { public Vector2 Value; }

  public interface IInputService : System.IDisposable
  {
    void Enable();
    void Disable();
    Vector2 Move { get; }
  }

  public class InputSystemService : IInputService
  {
    private readonly InputActionAsset _asset;
    private readonly InputAction _moveAction;
    private bool _isEnabled;

    public InputSystemService(InputActionAsset asset)
    {
      _asset = asset;
      _moveAction = asset != null ? asset.FindAction("Player/Move", true) : null;
    }

    public Vector2 Move => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

    public void Enable()
    {
      if (_isEnabled)
        return;

      _asset?.Enable();
      _isEnabled = true;
    }

    public void Disable()
    {
      if (!_isEnabled)
        return;

      _asset?.Disable();
      _isEnabled = false;
    }

    public void Dispose() => Disable();
  }

  public sealed class InputFeature : Feature
  {
    public InputFeature(ISystemFactory systems)
    {
      Add(systems.Create<InitializeInputSystem>());
      Add(systems.Create<EmitInputSystem>());
      // Removed CleanupInputSystem - input entity is now persistent
    }
  }

  public class InitializeInputSystem : IInitializeSystem
  {
    private readonly IInputService _input;
    private readonly InputContext _inputContext;

    public InitializeInputSystem(IInputService input, InputContext inputContext)
    {
      _input = input;
      _inputContext = inputContext;
    }

    public void Initialize()
    {
      _input.Enable();

      // Create persistent input entity
      InputEntity entity = _inputContext.CreateEntity();
      entity.AddMoveInput(Vector2.zero);
    }
  }

  /// <summary>
  /// Updates the persistent input entity with current input values.
  /// Uses Replace instead of Add to update existing entity.
  /// This ensures input is available for both Update and FixedUpdate systems.
  /// </summary>
  public class EmitInputSystem : IExecuteSystem
  {
    private readonly IInputService _input;
    private readonly IGroup<InputEntity> _inputEntities;

    public EmitInputSystem(InputContext inputContext, IInputService input)
    {
      _input = input;
      _inputEntities = inputContext.GetGroup(InputMatcher.MoveInput);
    }

    public void Execute()
    {
      Vector2 move = _input.Move;

      foreach (InputEntity entity in _inputEntities.GetEntities())
      {
        entity.ReplaceMoveInput(move);
      }
    }
  }

  // CleanupInputSystem removed - input entity persists across frames
  // This fixes timing issues between Update and FixedUpdate
}
