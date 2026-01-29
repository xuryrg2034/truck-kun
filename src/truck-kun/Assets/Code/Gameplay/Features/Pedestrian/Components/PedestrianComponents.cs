using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Code.Gameplay.Features.Pedestrian
{
  /// <summary>
  /// Marks entity as a pedestrian NPC
  /// </summary>
  [Game]
  public class Pedestrian : IComponent { }

  /// <summary>
  /// Type/kind of pedestrian for visual and behavior differentiation
  /// </summary>
  [Game]
  public class PedestrianType : IComponent
  {
    public PedestrianKind Value;
  }

  /// <summary>
  /// Pedestrian that is crossing the road
  /// </summary>
  [Game]
  public class CrossingPedestrian : IComponent
  {
    public float StartX;
    public float TargetX;
    public float Speed;
    public bool MovingRight;
  }
}
