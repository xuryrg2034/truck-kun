using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Code.Gameplay.Features.Economy
{
  [Meta, Unique]
  public class PlayerMoney : IComponent
  {
    public int Amount;
  }

  [Meta, Unique]
  public class EarnedThisDay : IComponent
  {
    public int Amount;
  }

  [Meta, Unique]
  public class PenaltiesThisDay : IComponent
  {
    public int Amount;
  }
}
