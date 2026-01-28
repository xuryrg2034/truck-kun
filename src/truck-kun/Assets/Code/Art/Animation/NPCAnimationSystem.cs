using System.Collections.Generic;
using Code.Art.Animation;
using Code.Infrastructure.View;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian
{
  /// <summary>
  /// Updates NPC animations based on movement state.
  /// Add to PedestrianFeature.
  /// </summary>
  public class NPCAnimationSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _buffer = new(32);

    public NPCAnimationSystem(GameContext game)
    {
      _pedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.View, GameMatcher.MoveSpeed)
        .NoneOf(GameMatcher.Hit));
    }

    public void Execute()
    {
      foreach (GameEntity pedestrian in _pedestrians.GetEntities(_buffer))
      {
        if (!pedestrian.hasView)
          continue;

        IEntityView view = pedestrian.view.Value;
        if (view is not Component component)
          continue;

        // Get or add animator
        NPCAnimator animator = component.GetComponent<NPCAnimator>();
        if (animator == null)
        {
          animator = component.gameObject.AddComponent<NPCAnimator>();
        }

        // Update animation state
        float speed = pedestrian.hasMoveSpeed ? pedestrian.moveSpeed.Value : 0f;
        bool isMoving = speed > 0.1f;

        animator.SetWalking(isMoving, speed);
      }
    }
  }

  /// <summary>
  /// Disables animation on hit pedestrians.
  /// </summary>
  public class DisableAnimationOnHitSystem : IExecuteSystem
  {
    private readonly IGroup<GameEntity> _hitPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    public DisableAnimationOnHitSystem(GameContext game)
    {
      _hitPedestrians = game.GetGroup(GameMatcher
        .AllOf(GameMatcher.Pedestrian, GameMatcher.Hit, GameMatcher.View));
    }

    public void Execute()
    {
      foreach (GameEntity pedestrian in _hitPedestrians.GetEntities(_buffer))
      {
        if (!pedestrian.hasView)
          continue;

        IEntityView view = pedestrian.view.Value;
        if (view is not Component component)
          continue;

        NPCAnimator animator = component.GetComponent<NPCAnimator>();
        if (animator != null)
        {
          animator.PlayHitReaction();
        }
      }
    }
  }
}
