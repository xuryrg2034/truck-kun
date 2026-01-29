using System;
using System.Collections.Generic;
using Code.Audio;
using Code.Common.Components;
using Code.Configs.Global;
using AudioHelper = Code.Audio.Audio;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Pedestrian;
using Code.Infrastructure.Systems;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Feedback
{
  #region Hit Effect Service

  public interface IHitEffectService
  {
    void SpawnHitEffect(Vector3 position, PedestrianKind kind, bool isViolation);
  }

  public class HitEffectService : IHitEffectService
  {
    private readonly FeedbackConfig _config;

    public HitEffectService(FeedbackConfig config)
    {
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "FeedbackConfig is required! Assign it in LevelConfig.");
    }

    public void SpawnHitEffect(Vector3 position, PedestrianKind kind, bool isViolation)
    {
      GameObject particleObj = new GameObject("HitEffect");
      particleObj.transform.position = position;

      ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

      Color particleColor = isViolation
        ? _config.PenaltyColor
        : PedestrianVisualData.Default(kind).Color;

      ConfigureParticleSystem(ps, particleColor);

      UnityEngine.Object.Destroy(particleObj, _config.ParticleLifetime + 0.5f);
    }

    private void ConfigureParticleSystem(ParticleSystem ps, Color color)
    {
      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      ParticleSystem.MainModule main = ps.main;
      main.duration = 0.1f;
      main.loop = false;
      main.startLifetime = _config.ParticleLifetime;
      main.startSpeed = _config.ParticleSpeed;
      main.startSize = _config.ParticleSize;
      main.startColor = color;
      main.gravityModifier = _config.ParticleGravity;
      main.simulationSpace = ParticleSystemSimulationSpace.World;
      main.maxParticles = _config.ParticleBurstCount + 5;

      ParticleSystem.EmissionModule emission = ps.emission;
      emission.enabled = true;
      emission.rateOverTime = 0;
      emission.SetBursts(new ParticleSystem.Burst[]
      {
        new ParticleSystem.Burst(0f, _config.ParticleBurstCount)
      });

      ParticleSystem.ShapeModule shape = ps.shape;
      shape.enabled = true;
      shape.shapeType = ParticleSystemShapeType.Sphere;
      shape.radius = 0.3f;

      ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
      sizeOverLifetime.enabled = true;
      AnimationCurve sizeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(1f, 0f)
      );
      sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

      ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
      colorOverLifetime.enabled = true;
      Gradient gradient = new Gradient();
      gradient.SetKeys(
        new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
      );
      colorOverLifetime.color = gradient;

      ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
      renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
      renderer.material.color = color;

      ps.Play();
    }
  }

  #endregion

  #region Floating Text Service

  public interface IFloatingTextService
  {
    void SpawnFloatingText(Vector3 worldPosition, string text, Color color);
    void SpawnMoneyText(Vector3 worldPosition, int amount, bool isGain);
  }

  public class FloatingTextService : IFloatingTextService
  {
    private readonly FeedbackConfig _config;
    private readonly List<FloatingTextInstance> _activeTexts = new(16);
    private Camera _mainCamera;

    public FloatingTextService(FeedbackConfig config)
    {
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "FeedbackConfig is required! Assign it in LevelConfig.");
    }

    public void SpawnFloatingText(Vector3 worldPosition, string text, Color color)
    {
      EnsureCamera();

      GameObject textObj = CreateTextObject(text, color);
      FloatingTextInstance instance = new FloatingTextInstance
      {
        GameObject = textObj,
        WorldPosition = worldPosition,
        StartTime = Time.time,
        Duration = _config.TextDuration,
        Canvas = textObj.GetComponentInParent<Canvas>()
      };

      _activeTexts.Add(instance);
    }

    public void SpawnMoneyText(Vector3 worldPosition, int amount, bool isGain)
    {
      string prefix = isGain ? "+" : "";
      string text = $"{prefix}{amount}";
      Color color = isGain ? _config.RewardColor : _config.PenaltyColor;

      SpawnFloatingText(worldPosition, text, color);
    }

    private void EnsureCamera()
    {
      if (_mainCamera == null)
        _mainCamera = Camera.main;
    }

    private GameObject CreateTextObject(string text, Color color)
    {
      GameObject canvasObj = new GameObject("FloatingTextCanvas");

      Canvas canvas = canvasObj.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.WorldSpace;
      canvas.sortingOrder = 500;

      canvasObj.transform.localScale = Vector3.one * 0.01f;

      GameObject textObj = new GameObject("FloatingText");
      textObj.transform.SetParent(canvasObj.transform, false);

      UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
      uiText.text = text;
      uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      uiText.fontSize = _config.FontSize * 3;
      uiText.fontStyle = FontStyle.Bold;
      uiText.color = color;
      uiText.alignment = TextAnchor.MiddleCenter;

      UnityEngine.UI.Outline outline = textObj.AddComponent<UnityEngine.UI.Outline>();
      outline.effectColor = Color.black;
      outline.effectDistance = new Vector2(2, -2);

      RectTransform textRect = textObj.GetComponent<RectTransform>();
      textRect.sizeDelta = new Vector2(300f, 100f);

      return canvasObj;
    }

    public void Update()
    {
      EnsureCamera();

      for (int i = _activeTexts.Count - 1; i >= 0; i--)
      {
        FloatingTextInstance instance = _activeTexts[i];

        float elapsed = Time.time - instance.StartTime;
        float t = elapsed / instance.Duration;

        if (t >= 1f)
        {
          if (instance.GameObject != null)
            UnityEngine.Object.Destroy(instance.GameObject);

          _activeTexts.RemoveAt(i);
          continue;
        }

        Vector3 offset = Vector3.up * (_config.TextRiseSpeed * elapsed);
        instance.GameObject.transform.position = instance.WorldPosition + offset;

        if (_mainCamera != null)
        {
          instance.GameObject.transform.rotation = _mainCamera.transform.rotation;
        }

        float alpha = 1f - (t * t);
        UnityEngine.UI.Text uiText = instance.GameObject.GetComponentInChildren<UnityEngine.UI.Text>();
        if (uiText != null)
        {
          Color c = uiText.color;
          c.a = alpha;
          uiText.color = c;
        }
      }
    }

    private class FloatingTextInstance
    {
      public GameObject GameObject;
      public Vector3 WorldPosition;
      public float StartTime;
      public float Duration;
      public Canvas Canvas;
    }
  }

  #endregion

  #region Feature

  public sealed class FeedbackFeature : Feature
  {
    public FeedbackFeature(ISystemFactory systems)
    {
      Add(systems.Create<HitFeedbackSystem>());
      Add(systems.Create<FloatingTextUpdateSystem>());
    }
  }

  #endregion

  #region Systems

  /// <summary>
  /// Handles visual and audio feedback for hit events.
  /// Uses Code.Audio.Audio static helper for sounds.
  /// </summary>
  public class HitFeedbackSystem : ReactiveSystem<GameEntity>
  {
    private readonly IHitEffectService _hitEffectService;
    private readonly IFloatingTextService _floatingTextService;
    private readonly EconomyConfig _economyConfig;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _pedBuffer = new(32);

    public HitFeedbackSystem(
      GameContext game,
      IHitEffectService hitEffectService,
      IFloatingTextService floatingTextService,
      EconomyConfig economyConfig) : base(game)
    {
      _hitEffectService = hitEffectService;
      _floatingTextService = floatingTextService;
      _economyConfig = economyConfig ?? throw new ArgumentNullException(nameof(economyConfig),
        "EconomyConfig is required! Assign it in LevelConfig.");
      _pedestrians = game.GetGroup(GameMatcher.AllOf(GameMatcher.Pedestrian, GameMatcher.WorldPosition, GameMatcher.Id));
    }

    protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    {
      return context.CreateCollector(GameMatcher.HitEvent.Added());
    }

    protected override bool Filter(GameEntity entity)
    {
      return entity.hasHitEvent;
    }

    protected override void Execute(List<GameEntity> entities)
    {
      foreach (GameEntity hitEvent in entities)
      {
        PedestrianKind kind = hitEvent.hitEvent.PedestrianType;
        int pedestrianId = hitEvent.hitEvent.PedestrianId;

        Vector3 hitPosition = Vector3.zero;
        bool foundPosition = false;

        foreach (GameEntity ped in _pedestrians.GetEntities(_pedBuffer))
        {
          if (ped.id.Value == pedestrianId)
          {
            hitPosition = ped.worldPosition.Value;
            foundPosition = true;
            break;
          }
        }

        if (!foundPosition)
          continue;

        bool isViolation = kind.IsProtectedType();

        // Spawn particle effect
        _hitEffectService.SpawnHitEffect(hitPosition, kind, isViolation);

        // Play sound via new AudioService
        if (isViolation)
        {
          AudioHelper.PlaySFX(SFXType.Violation);
        }
        else
        {
          AudioHelper.PlaySFX(SFXType.Hit);
        }

        // Spawn floating text
        if (isViolation)
        {
          _floatingTextService.SpawnMoneyText(hitPosition + Vector3.up, -_economyConfig.ViolationPenalty, false);
          AudioHelper.PlaySFX(SFXType.MoneyLoss);
        }
        else
        {
          _floatingTextService.SpawnFloatingText(
            hitPosition + Vector3.up,
            kind.GetDisplayNameRu(),
            PedestrianVisualData.Default(kind).Color
          );
        }
      }
    }
  }

  public class FloatingTextUpdateSystem : IExecuteSystem
  {
    private readonly IFloatingTextService _floatingTextService;

    public FloatingTextUpdateSystem(IFloatingTextService floatingTextService)
    {
      _floatingTextService = floatingTextService;
    }

    public void Execute()
    {
      if (_floatingTextService is FloatingTextService service)
      {
        service.Update();
      }
    }
  }

  #endregion
}
