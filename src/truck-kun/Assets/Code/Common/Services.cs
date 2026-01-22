using UnityEngine;

namespace Code.Common
{
  public interface IIdentifierService
  {
    int Next();
  }

  public class IdentifierService : IIdentifierService
  {
    private int _next = 1;
    public int Next() => _next++;
  }

  public interface ITimeService
  {
    float DeltaTime { get; }
  }

  public class UnityTimeService : ITimeService
  {
    public float DeltaTime => Time.deltaTime;
  }
}
