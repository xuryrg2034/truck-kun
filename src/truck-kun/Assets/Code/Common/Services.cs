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
    /// <summary>
    /// Time since last Update (for per-frame logic)
    /// </summary>
    float DeltaTime { get; }

    /// <summary>
    /// Time since last FixedUpdate (for physics)
    /// </summary>
    float FixedDeltaTime { get; }

    /// <summary>
    /// Unscaled delta time (ignores Time.timeScale)
    /// </summary>
    float UnscaledDeltaTime { get; }

    /// <summary>
    /// Current time scale (1.0 = normal, 0.5 = slow-mo)
    /// </summary>
    float TimeScale { get; set; }

    /// <summary>
    /// Total elapsed game time
    /// </summary>
    float Time { get; }
  }

  public class UnityTimeService : ITimeService
  {
    public float DeltaTime => UnityEngine.Time.deltaTime;
    public float FixedDeltaTime => UnityEngine.Time.fixedDeltaTime;
    public float UnscaledDeltaTime => UnityEngine.Time.unscaledDeltaTime;
    public float Time => UnityEngine.Time.time;

    public float TimeScale
    {
      get => UnityEngine.Time.timeScale;
      set => UnityEngine.Time.timeScale = Mathf.Max(0f, value);
    }
  }
}
