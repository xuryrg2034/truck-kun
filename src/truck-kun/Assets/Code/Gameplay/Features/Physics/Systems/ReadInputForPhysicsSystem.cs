using System.Collections.Generic;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Physics.Systems
{
  /// <summary>
  /// Reads lateral input for physics-based heroes.
  /// Stores input in a temporary component for physics calculations.
  /// </summary>
  public class ReadInputForPhysicsSystem : IExecuteSystem
  {
    private readonly IGroup<InputEntity> _inputs;
    private readonly IGroup<GameEntity> _physicsHeroes;
    private readonly List<InputEntity> _inputBuffer = new(1);
    private readonly List<GameEntity> _heroBuffer = new(4);

    public ReadInputForPhysicsSystem(GameContext game, InputContext input)
    {
      _inputs = input.GetGroup(InputMatcher.MoveInput);
      _physicsHeroes = game.GetGroup(GameMatcher.AllOf(
        GameMatcher.Hero,
        GameMatcher.PhysicsBody,
        GameMatcher.PhysicsVelocity));
    }

    public void Execute()
    {
      // Get current lateral input
      float lateralInput = 0f;
      foreach (InputEntity inputEntity in _inputs.GetEntities(_inputBuffer))
      {
        lateralInput = inputEntity.moveInput.Value.x;
      }

      lateralInput = Mathf.Clamp(lateralInput, -1f, 1f);

      // Store in MoveDirection for physics systems to use
      foreach (GameEntity hero in _physicsHeroes.GetEntities(_heroBuffer))
      {
        // Use MoveDirection.x to store lateral input, z for forward intent
        Vector3 moveDir = new Vector3(lateralInput, 0f, 1f);
        hero.ReplaceMoveDirection(moveDir);
      }
    }
  }
}
