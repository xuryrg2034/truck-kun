using System.Collections.Generic;
using Code.Infrastructure.View;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian.Systems
{
  /// <summary>
  /// Moves crossing pedestrians by directly setting velocity.
  /// Should be added to PhysicsFeature to run in FixedUpdate.
  /// </summary>
  public class PedestrianForceMovementSystem : IExecuteSystem
  {
    private readonly PedestrianPhysicsSettings _settings;
    private readonly IGroup<GameEntity> _crossingPedestrians;
    private readonly List<GameEntity> _buffer = new(16);

    public PedestrianForceMovementSystem(
      GameContext game,
      PedestrianPhysicsSettings settings)
    {
      _settings = settings;

      // Only process crossing pedestrians that have View and are not hit
      _crossingPedestrians = game.GetGroup(
        GameMatcher.AllOf(
          GameMatcher.Pedestrian,
          GameMatcher.CrossingPedestrian,
          GameMatcher.View)
        .NoneOf(GameMatcher.Hit, GameMatcher.Ragdolled));
    }

    public void Execute()
    {
      foreach (GameEntity pedestrian in _crossingPedestrians.GetEntities(_buffer))
      {
        MovePedestrian(pedestrian);
      }
    }

    private void MovePedestrian(GameEntity pedestrian)
    {
      if (!pedestrian.hasView)
        return;

      IEntityView view = pedestrian.view.Value;
      if (view is not Component component)
        return;

      Rigidbody rb = component.GetComponent<Rigidbody>();
      if (rb == null || rb.isKinematic)
        return;

      // Get crossing data
      float targetX = pedestrian.crossingPedestrian.TargetX;
      float speed = pedestrian.crossingPedestrian.Speed;
      bool movingRight = pedestrian.crossingPedestrian.MovingRight;

      // Check if reached target
      float currentX = component.transform.position.x;
      bool reachedTarget = movingRight
        ? currentX >= targetX
        : currentX <= targetX;

      if (reachedTarget)
      {
        // Stop horizontal movement and remove crossing component
        rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        pedestrian.RemoveCrossingPedestrian();

        // Rotate back to face forward
        if (pedestrian.hasPedestrianType)
        {
          PedestrianVisualData data = PedestrianVisualData.Default(pedestrian.pedestrianType.Value);
          component.transform.rotation = Quaternion.Euler(data.ForwardTilt, 0f, 0f);
        }
        return;
      }

      // Set velocity directly — reliable for arcade movement
      float moveSpeed = Mathf.Min(speed, _settings.MaxSpeed);
      float direction = movingRight ? 1f : -1f;

      rb.WakeUp();
      rb.linearVelocity = new Vector3(
        direction * moveSpeed,
        rb.linearVelocity.y,
        0f
      );

      // Sync world position with physics
      if (pedestrian.hasWorldPosition)
      {
        pedestrian.ReplaceWorldPosition(component.transform.position);
      }
    }
  }
}
