using System;
using Code.Common.Services;
using UnityEngine;

namespace Code.Gameplay.Features.Economy
{
  internal enum DayState
  {
    Running,
    Finished
  }

  [Serializable]
  internal class DaySessionSettings
  {
    public float DurationSeconds = 60f;
  }

  internal interface IDaySessionService
  {
    DayState State { get; }
    float RemainingTime { get; }
    void StartDay();
    bool Tick();
    void FinishDay();
  }

  internal class DaySessionService : IDaySessionService
  {
    private readonly ITimeService _timeService;
    private readonly DaySessionSettings _settings;
    private DayState _state;
    private float _remainingTime;

    public DaySessionService(ITimeService timeService, DaySessionSettings settings)
    {
      _timeService = timeService;
      _settings = settings;
    }

    public DayState State => _state;
    public float RemainingTime => _remainingTime;

    public void StartDay()
    {
      _remainingTime = Mathf.Max(0f, _settings.DurationSeconds);
      _state = _remainingTime > 0f ? DayState.Running : DayState.Finished;
    }

    public bool Tick()
    {
      if (_state != DayState.Running)
        return false;

      _remainingTime = Mathf.Max(0f, _remainingTime - _timeService.DeltaTime);

      if (_remainingTime <= 0f)
      {
        FinishDay();
        return true;
      }

      return false;
    }

    public void FinishDay()
    {
      _remainingTime = 0f;
      _state = DayState.Finished;
    }
  }
}
