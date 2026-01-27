using System;
using System.Collections.Generic;
using UnityEngine;

namespace Code.Art.VFX
{
  /// <summary>
  /// Lightweight tween system - alternative to DOTween
  /// </summary>
  public class TweenSystem : MonoBehaviour
  {
    private static TweenSystem _instance;
    private readonly List<Tween> _activeTweens = new();
    private readonly List<Tween> _toRemove = new();

    public static TweenSystem Instance
    {
      get
      {
        if (_instance == null)
        {
          GameObject go = new GameObject("[TweenSystem]");
          _instance = go.AddComponent<TweenSystem>();
          DontDestroyOnLoad(go);
        }
        return _instance;
      }
    }

    private void Update()
    {
      float dt = Time.unscaledDeltaTime;

      foreach (Tween tween in _activeTweens)
      {
        if (tween.Target == null || !tween.IsActive)
        {
          _toRemove.Add(tween);
          continue;
        }

        tween.Update(tween.UseUnscaledTime ? dt : Time.deltaTime);

        if (tween.IsComplete)
          _toRemove.Add(tween);
      }

      foreach (Tween t in _toRemove)
        _activeTweens.Remove(t);

      _toRemove.Clear();
    }

    public void AddTween(Tween tween)
    {
      _activeTweens.Add(tween);
    }

    public void KillTweensOf(object target)
    {
      foreach (Tween t in _activeTweens)
      {
        if (t.Target == target)
          t.Kill();
      }
    }
  }

  public abstract class Tween
  {
    public object Target { get; protected set; }
    public bool IsActive { get; protected set; } = true;
    public bool IsComplete { get; protected set; }
    public bool UseUnscaledTime { get; set; }

    protected float Duration;
    protected float Elapsed;
    protected EaseType Ease = EaseType.OutQuad;
    protected Action OnCompleteCallback;
    protected int LoopCount;
    protected LoopType LoopMode;
    protected bool PingPong;

    public abstract void Update(float dt);

    public Tween SetEase(EaseType ease)
    {
      Ease = ease;
      return this;
    }

    public Tween SetLoops(int count, LoopType loopType = LoopType.Restart)
    {
      LoopCount = count;
      LoopMode = loopType;
      return this;
    }

    public Tween OnComplete(Action callback)
    {
      OnCompleteCallback = callback;
      return this;
    }

    public Tween SetUnscaledTime(bool unscaled)
    {
      UseUnscaledTime = unscaled;
      return this;
    }

    public void Kill()
    {
      IsActive = false;
    }

    protected float EvaluateEase(float t)
    {
      return Ease switch
      {
        EaseType.Linear => t,
        EaseType.InQuad => t * t,
        EaseType.OutQuad => 1f - (1f - t) * (1f - t),
        EaseType.InOutQuad => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f,
        EaseType.InCubic => t * t * t,
        EaseType.OutCubic => 1f - Mathf.Pow(1f - t, 3f),
        EaseType.InOutCubic => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f,
        EaseType.InBack => 2.70158f * t * t * t - 1.70158f * t * t,
        EaseType.OutBack => 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f),
        EaseType.InElastic => t == 0f ? 0f : t == 1f ? 1f : -Mathf.Pow(2f, 10f * t - 10f) * Mathf.Sin((t * 10f - 10.75f) * (2f * Mathf.PI / 3f)),
        EaseType.OutElastic => t == 0f ? 0f : t == 1f ? 1f : Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * (2f * Mathf.PI / 3f)) + 1f,
        EaseType.OutBounce => EaseOutBounce(t),
        _ => t
      };
    }

    private static float EaseOutBounce(float t)
    {
      const float n1 = 7.5625f;
      const float d1 = 2.75f;

      if (t < 1f / d1)
        return n1 * t * t;
      if (t < 2f / d1)
        return n1 * (t -= 1.5f / d1) * t + 0.75f;
      if (t < 2.5f / d1)
        return n1 * (t -= 2.25f / d1) * t + 0.9375f;
      return n1 * (t -= 2.625f / d1) * t + 0.984375f;
    }
  }

  public enum EaseType
  {
    Linear,
    InQuad, OutQuad, InOutQuad,
    InCubic, OutCubic, InOutCubic,
    InBack, OutBack,
    InElastic, OutElastic,
    OutBounce
  }

  public enum LoopType
  {
    Restart,
    PingPong
  }

  #region Tween Types

  public class TweenFloat : Tween
  {
    private readonly Action<float> _setter;
    private readonly float _from;
    private readonly float _to;

    public TweenFloat(object target, float from, float to, float duration, Action<float> setter)
    {
      Target = target;
      _from = from;
      _to = to;
      Duration = duration;
      _setter = setter;
    }

    public override void Update(float dt)
    {
      Elapsed += dt;
      float t = Mathf.Clamp01(Elapsed / Duration);

      if (LoopMode == LoopType.PingPong && PingPong)
        t = 1f - t;

      float eased = EvaluateEase(t);
      _setter?.Invoke(Mathf.LerpUnclamped(_from, _to, eased));

      if (Elapsed >= Duration)
      {
        if (LoopCount != 0)
        {
          Elapsed = 0f;
          if (LoopCount > 0) LoopCount--;
          if (LoopMode == LoopType.PingPong) PingPong = !PingPong;
        }
        else
        {
          IsComplete = true;
          OnCompleteCallback?.Invoke();
        }
      }
    }
  }

  public class TweenVector3 : Tween
  {
    private readonly Action<Vector3> _setter;
    private readonly Vector3 _from;
    private readonly Vector3 _to;

    public TweenVector3(object target, Vector3 from, Vector3 to, float duration, Action<Vector3> setter)
    {
      Target = target;
      _from = from;
      _to = to;
      Duration = duration;
      _setter = setter;
    }

    public override void Update(float dt)
    {
      Elapsed += dt;
      float t = Mathf.Clamp01(Elapsed / Duration);

      if (LoopMode == LoopType.PingPong && PingPong)
        t = 1f - t;

      float eased = EvaluateEase(t);
      _setter?.Invoke(Vector3.LerpUnclamped(_from, _to, eased));

      if (Elapsed >= Duration)
      {
        if (LoopCount != 0)
        {
          Elapsed = 0f;
          if (LoopCount > 0) LoopCount--;
          if (LoopMode == LoopType.PingPong) PingPong = !PingPong;
        }
        else
        {
          IsComplete = true;
          OnCompleteCallback?.Invoke();
        }
      }
    }
  }

  public class TweenColor : Tween
  {
    private readonly Action<Color> _setter;
    private readonly Color _from;
    private readonly Color _to;

    public TweenColor(object target, Color from, Color to, float duration, Action<Color> setter)
    {
      Target = target;
      _from = from;
      _to = to;
      Duration = duration;
      _setter = setter;
    }

    public override void Update(float dt)
    {
      Elapsed += dt;
      float t = Mathf.Clamp01(Elapsed / Duration);
      float eased = EvaluateEase(t);
      _setter?.Invoke(Color.LerpUnclamped(_from, _to, eased));

      if (Elapsed >= Duration)
      {
        IsComplete = true;
        OnCompleteCallback?.Invoke();
      }
    }
  }

  #endregion

  #region Extensions

  public static class TweenExtensions
  {
    // Transform extensions
    public static Tween TweenScale(this Transform transform, Vector3 to, float duration)
    {
      var tween = new TweenVector3(transform, transform.localScale, to, duration, v => transform.localScale = v);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    public static Tween TweenPosition(this Transform transform, Vector3 to, float duration)
    {
      var tween = new TweenVector3(transform, transform.position, to, duration, v => transform.position = v);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    public static Tween TweenLocalPosition(this Transform transform, Vector3 to, float duration)
    {
      var tween = new TweenVector3(transform, transform.localPosition, to, duration, v => transform.localPosition = v);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    public static Tween TweenRotation(this Transform transform, Vector3 to, float duration)
    {
      var tween = new TweenVector3(transform, transform.eulerAngles, to, duration, v => transform.eulerAngles = v);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    public static Tween TweenLocalRotation(this Transform transform, Vector3 to, float duration)
    {
      var tween = new TweenVector3(transform, transform.localEulerAngles, to, duration, v => transform.localEulerAngles = v);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    // CanvasGroup extensions
    public static Tween TweenFade(this CanvasGroup canvasGroup, float to, float duration)
    {
      var tween = new TweenFloat(canvasGroup, canvasGroup.alpha, to, duration, v => canvasGroup.alpha = v);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    // Generic
    public static Tween TweenValue(this MonoBehaviour mb, float from, float to, float duration, Action<float> onUpdate)
    {
      var tween = new TweenFloat(mb, from, to, duration, onUpdate);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    public static void KillTweens(this Transform transform)
    {
      TweenSystem.Instance.KillTweensOf(transform);
    }

    public static void KillTweens(this CanvasGroup canvasGroup)
    {
      TweenSystem.Instance.KillTweensOf(canvasGroup);
    }

    // Punch effects
    public static Tween PunchScale(this Transform transform, Vector3 punch, float duration)
    {
      Vector3 original = transform.localScale;
      var tween = new TweenFloat(transform, 0f, 1f, duration, t =>
      {
        float curve = Mathf.Sin(t * Mathf.PI) * (1f - t);
        transform.localScale = original + punch * curve;
      });
      tween.OnComplete(() => transform.localScale = original);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    public static Tween PunchPosition(this Transform transform, Vector3 punch, float duration)
    {
      Vector3 original = transform.localPosition;
      var tween = new TweenFloat(transform, 0f, 1f, duration, t =>
      {
        float curve = Mathf.Sin(t * Mathf.PI) * (1f - t);
        transform.localPosition = original + punch * curve;
      });
      tween.OnComplete(() => transform.localPosition = original);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }

    public static Tween ShakePosition(this Transform transform, float strength, float duration, int vibrato = 10)
    {
      Vector3 original = transform.localPosition;
      var tween = new TweenFloat(transform, 0f, 1f, duration, t =>
      {
        float decay = 1f - t;
        float shake = Mathf.Sin(t * vibrato * Mathf.PI * 2f) * strength * decay;
        transform.localPosition = original + new Vector3(
          UnityEngine.Random.Range(-1f, 1f) * shake,
          UnityEngine.Random.Range(-1f, 1f) * shake,
          0f
        );
      });
      tween.OnComplete(() => transform.localPosition = original);
      TweenSystem.Instance.AddTween(tween);
      return tween;
    }
  }

  #endregion
}
