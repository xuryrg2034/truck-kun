using Code.Infrastructure.Systems;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.Gameplay.Input
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
      Add(systems.Create<CleanupInputSystem>());
    }
  }

  public class InitializeInputSystem : IInitializeSystem
  {
    private readonly IInputService _input;

    public InitializeInputSystem(IInputService input)
    {
      _input = input;
    }

    public void Initialize() => _input.Enable();
  }

  public class EmitInputSystem : IExecuteSystem
  {
    private readonly InputContext _inputContext;
    private readonly IInputService _input;

    public EmitInputSystem(InputContext inputContext, IInputService input)
    {
      _inputContext = inputContext;
      _input = input;
    }

    public void Execute()
    {
      InputEntity entity = _inputContext.CreateEntity();
      entity.AddMoveInput(_input.Move);
    }
  }

  public class CleanupInputSystem : ICleanupSystem
  {
    private readonly InputContext _inputContext;

    public CleanupInputSystem(InputContext inputContext)
    {
      _inputContext = inputContext;
    }

    public void Cleanup() => _inputContext.DestroyAllEntities();
  }
}
