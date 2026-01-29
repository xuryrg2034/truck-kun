using System;
using Code.Configs.Global;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Gameplay.Features.Feedback.Services
{
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
}
