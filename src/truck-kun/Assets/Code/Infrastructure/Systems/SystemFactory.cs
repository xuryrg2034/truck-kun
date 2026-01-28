using Entitas;
using Zenject;

namespace Code.Infrastructure.Systems
{
  public interface ISystemFactory
  {
    T Create<T>() where T : ISystem;
  }

  public class SystemFactory : ISystemFactory
  {
    private readonly DiContainer _container;

    public SystemFactory(DiContainer container)
    {
      _container = container;
    }

    public T Create<T>() where T : ISystem =>
      _container.Instantiate<T>();
  }
}
