using Code.Gameplay.Features.Pedestrian;
using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Code.Gameplay.Features.Collision
{
  /// <summary>
  /// Flag marking an entity as hit (pedestrian was struck by hero)
  /// </summary>
  [Game]
  public class Hit : IComponent { }

  /// <summary>
  /// Event created when a collision occurs.
  /// Consumed by FeedbackFeature, QuestFeature, etc.
  /// </summary>
  [Game]
  public class HitEvent : IComponent
  {
    public PedestrianKind PedestrianType;
    public int PedestrianId;
  }

  /// <summary>
  /// Extended collision data for VFX and scoring.
  /// </summary>
  [Game]
  public class CollisionImpact : IComponent
  {
    public float Force;
    public Vector3 Point;
    public Vector3 Normal;
  }
}
