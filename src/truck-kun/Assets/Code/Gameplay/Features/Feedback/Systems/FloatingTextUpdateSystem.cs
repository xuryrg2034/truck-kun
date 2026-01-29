using Code.Gameplay.Features.Feedback.Services;
using Entitas;

namespace Code.Gameplay.Features.Feedback.Systems
{
  public class FloatingTextUpdateSystem : IExecuteSystem
  {
    private readonly IFloatingTextService _floatingTextService;

    public FloatingTextUpdateSystem(IFloatingTextService floatingTextService)
    {
      _floatingTextService = floatingTextService;
    }

    public void Execute()
    {
      _floatingTextService.Update();
    }
  }
}
