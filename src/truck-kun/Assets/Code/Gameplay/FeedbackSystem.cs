using System;
using System.Collections.Generic;
using Code.Common;
using Code.Gameplay.Features.Collision;
using Code.Gameplay.Features.Economy;
using Code.Gameplay.Features.Pedestrian;
using Code.Infrastructure.Systems;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Feedback
{
  #region Enums

  public enum SFXType
  {
    Hit,
    HitProtected,
    QuestComplete,
    MoneyGain,
    MoneyLoss
  }

  #endregion

  #region Settings

  [Serializable]
  public class FeedbackSettings
  {
    [Header("Particles")]
    public int ParticleBurstCount = 15;
    public float ParticleLifetime = 1f;
    public float ParticleSpeed = 3f;
    public float ParticleGravity = 2f;
    public float ParticleSize = 0.15f;

    [Header("Floating Text")]
    public float FloatSpeed = 2f;
    public float FloatDuration = 1.2f;
    public int FontSize = 32;

    [Header("Colors")]
    public Color RewardColor = new Color(0.2f, 1f, 0.3f);
    public Color PenaltyColor = new Color(1f, 0.3f, 0.2f);
    public Color NeutralHitColor = Color.white;

    [Header("Audio")]
    public float SFXVolume = 0.7f;
  }

  #endregion

  #region Audio Service

  public interface IAudioService
  {
    void PlaySFX(SFXType type);
    void PlaySFX(SFXType type, Vector3 position);
    void SetVolume(float volume);
  }

  public class AudioService : IAudioService
  {
    private readonly FeedbackSettings _settings;
    private GameObject _audioSourceObject;
    private AudioSource _audioSource;

    // Simple generated sounds (no AudioClip files needed)
    private readonly Dictionary<SFXType, float[]> _generatedSounds = new();

    public AudioService(FeedbackSettings settings = null)
    {
      _settings = settings ?? new FeedbackSettings();
      CreateAudioSource();
      GenerateSounds();
    }

    private void CreateAudioSource()
    {
      _audioSourceObject = new GameObject("FeedbackAudioSource");
      UnityEngine.Object.DontDestroyOnLoad(_audioSourceObject);
      _audioSource = _audioSourceObject.AddComponent<AudioSource>();
      _audioSource.playOnAwake = false;
      _audioSource.volume = _settings.SFXVolume;
    }

    private void GenerateSounds()
    {
      // Generate simple procedural sounds
      int sampleRate = 44100;

      // Hit sound - short impact
      _generatedSounds[SFXType.Hit] = GenerateImpactSound(sampleRate, 0.1f, 200f, 100f);

      // Hit protected - lower, more "wrong" sound
      _generatedSounds[SFXType.HitProtected] = GenerateImpactSound(sampleRate, 0.15f, 150f, 80f);

      // Money gain - rising tone
      _generatedSounds[SFXType.MoneyGain] = GenerateToneSound(sampleRate, 0.15f, 400f, 600f);

      // Money loss - falling tone
      _generatedSounds[SFXType.MoneyLoss] = GenerateToneSound(sampleRate, 0.2f, 300f, 150f);

      // Quest complete - happy chord
      _generatedSounds[SFXType.QuestComplete] = GenerateChordSound(sampleRate, 0.3f);
    }

    private float[] GenerateImpactSound(int sampleRate, float duration, float startFreq, float endFreq)
    {
      int samples = (int)(sampleRate * duration);
      float[] sound = new float[samples];

      for (int i = 0; i < samples; i++)
      {
        float t = i / (float)samples;
        float freq = Mathf.Lerp(startFreq, endFreq, t);
        float amplitude = 1f - t; // Fade out
        amplitude *= amplitude; // Exponential decay

        sound[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * amplitude * 0.5f;
        // Add noise for impact feel
        sound[i] += UnityEngine.Random.Range(-0.1f, 0.1f) * amplitude;
      }

      return sound;
    }

    private float[] GenerateToneSound(int sampleRate, float duration, float startFreq, float endFreq)
    {
      int samples = (int)(sampleRate * duration);
      float[] sound = new float[samples];

      for (int i = 0; i < samples; i++)
      {
        float t = i / (float)samples;
        float freq = Mathf.Lerp(startFreq, endFreq, t);
        float amplitude = 1f - (t * t); // Gradual fade

        sound[i] = Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * amplitude * 0.3f;
      }

      return sound;
    }

    private float[] GenerateChordSound(int sampleRate, float duration)
    {
      int samples = (int)(sampleRate * duration);
      float[] sound = new float[samples];

      float[] frequencies = { 523.25f, 659.25f, 783.99f }; // C5, E5, G5 chord

      for (int i = 0; i < samples; i++)
      {
        float t = i / (float)samples;
        float amplitude = 1f - (t * t);

        foreach (float freq in frequencies)
        {
          sound[i] += Mathf.Sin(2f * Mathf.PI * freq * i / sampleRate) * amplitude * 0.2f;
        }
      }

      return sound;
    }

    public void PlaySFX(SFXType type)
    {
      if (_audioSource == null)
        return;

      if (!_generatedSounds.TryGetValue(type, out float[] samples))
        return;

      AudioClip clip = AudioClip.Create($"SFX_{type}", samples.Length, 1, 44100, false);
      clip.SetData(samples, 0);

      _audioSource.PlayOneShot(clip, _settings.SFXVolume);
    }

    public void PlaySFX(SFXType type, Vector3 position)
    {
      // For now, just play non-positional
      PlaySFX(type);
    }

    public void SetVolume(float volume)
    {
      if (_audioSource != null)
        _audioSource.volume = Mathf.Clamp01(volume);
    }
  }

  #endregion

  #region Hit Effect Service

  public interface IHitEffectService
  {
    void SpawnHitEffect(Vector3 position, PedestrianKind kind, bool isViolation);
  }

  public class HitEffectService : IHitEffectService
  {
    private readonly FeedbackSettings _settings;
    private GameObject _particleTemplate;

    public HitEffectService(FeedbackSettings settings = null)
    {
      _settings = settings ?? new FeedbackSettings();
    }

    public void SpawnHitEffect(Vector3 position, PedestrianKind kind, bool isViolation)
    {
      // Create particle system
      GameObject particleObj = new GameObject("HitEffect");
      particleObj.transform.position = position;

      ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();

      // Get color based on pedestrian type
      Color particleColor = isViolation
        ? _settings.PenaltyColor
        : PedestrianVisualData.Default(kind).Color;

      ConfigureParticleSystem(ps, particleColor);

      // Auto destroy
      UnityEngine.Object.Destroy(particleObj, _settings.ParticleLifetime + 0.5f);
    }

    private void ConfigureParticleSystem(ParticleSystem ps, Color color)
    {
      // Stop auto-play to configure
      ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      // Main module
      ParticleSystem.MainModule main = ps.main;
      main.duration = 0.1f;
      main.loop = false;
      main.startLifetime = _settings.ParticleLifetime;
      main.startSpeed = _settings.ParticleSpeed;
      main.startSize = _settings.ParticleSize;
      main.startColor = color;
      main.gravityModifier = _settings.ParticleGravity;
      main.simulationSpace = ParticleSystemSimulationSpace.World;
      main.maxParticles = _settings.ParticleBurstCount + 5;

      // Emission - burst
      ParticleSystem.EmissionModule emission = ps.emission;
      emission.enabled = true;
      emission.rateOverTime = 0;
      emission.SetBursts(new ParticleSystem.Burst[]
      {
        new ParticleSystem.Burst(0f, _settings.ParticleBurstCount)
      });

      // Shape - sphere for explosion effect
      ParticleSystem.ShapeModule shape = ps.shape;
      shape.enabled = true;
      shape.shapeType = ParticleSystemShapeType.Sphere;
      shape.radius = 0.3f;

      // Size over lifetime - shrink
      ParticleSystem.SizeOverLifetimeModule sizeOverLifetime = ps.sizeOverLifetime;
      sizeOverLifetime.enabled = true;
      AnimationCurve sizeCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(1f, 0f)
      );
      sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

      // Color over lifetime - fade out
      ParticleSystem.ColorOverLifetimeModule colorOverLifetime = ps.colorOverLifetime;
      colorOverLifetime.enabled = true;
      Gradient gradient = new Gradient();
      gradient.SetKeys(
        new GradientColorKey[] { new GradientColorKey(color, 0f), new GradientColorKey(color, 1f) },
        new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
      );
      colorOverLifetime.color = gradient;

      // Renderer
      ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
      renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
      renderer.material.color = color;

      // Play
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
    private readonly FeedbackSettings _settings;
    private readonly List<FloatingTextInstance> _activeTexts = new(16);
    private Camera _mainCamera;

    public FloatingTextService(FeedbackSettings settings = null)
    {
      _settings = settings ?? new FeedbackSettings();
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
        Duration = _settings.FloatDuration,
        Canvas = textObj.GetComponentInParent<Canvas>()
      };

      _activeTexts.Add(instance);
    }

    public void SpawnMoneyText(Vector3 worldPosition, int amount, bool isGain)
    {
      string prefix = isGain ? "+" : "";
      string text = $"{prefix}{amount}";
      Color color = isGain ? _settings.RewardColor : _settings.PenaltyColor;

      SpawnFloatingText(worldPosition, text, color);
    }

    private void EnsureCamera()
    {
      if (_mainCamera == null)
        _mainCamera = Camera.main;
    }

    private GameObject CreateTextObject(string text, Color color)
    {
      // Create canvas for this text
      GameObject canvasObj = new GameObject("FloatingTextCanvas");

      Canvas canvas = canvasObj.AddComponent<Canvas>();
      canvas.renderMode = RenderMode.WorldSpace;
      canvas.sortingOrder = 500;

      // Scale canvas to be visible in world space
      canvasObj.transform.localScale = Vector3.one * 0.01f;

      // Create text
      GameObject textObj = new GameObject("FloatingText");
      textObj.transform.SetParent(canvasObj.transform, false);

      UnityEngine.UI.Text uiText = textObj.AddComponent<UnityEngine.UI.Text>();
      uiText.text = text;
      uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
      uiText.fontSize = _settings.FontSize * 3; // Larger for world space
      uiText.fontStyle = FontStyle.Bold;
      uiText.color = color;
      uiText.alignment = TextAnchor.MiddleCenter;

      // Add outline for visibility
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
          // Destroy and remove
          if (instance.GameObject != null)
            UnityEngine.Object.Destroy(instance.GameObject);

          _activeTexts.RemoveAt(i);
          continue;
        }

        // Update position (float upward)
        Vector3 offset = Vector3.up * (_settings.FloatSpeed * elapsed);
        instance.GameObject.transform.position = instance.WorldPosition + offset;

        // Face camera
        if (_mainCamera != null)
        {
          instance.GameObject.transform.rotation = _mainCamera.transform.rotation;
        }

        // Fade out
        float alpha = 1f - (t * t);
        UnityEngine.UI.Text text = instance.GameObject.GetComponentInChildren<UnityEngine.UI.Text>();
        if (text != null)
        {
          Color c = text.color;
          c.a = alpha;
          text.color = c;
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

  public class HitFeedbackSystem : ReactiveSystem<GameEntity>
  {
    private readonly GameContext _game;
    private readonly IAudioService _audioService;
    private readonly IHitEffectService _hitEffectService;
    private readonly IFloatingTextService _floatingTextService;
    private readonly EconomySettings _economySettings;
    private readonly IGroup<GameEntity> _pedestrians;
    private readonly List<GameEntity> _pedBuffer = new(32);

    public HitFeedbackSystem(
      GameContext game,
      IAudioService audioService,
      IHitEffectService hitEffectService,
      IFloatingTextService floatingTextService,
      EconomySettings economySettings = null) : base(game)
    {
      _game = game;
      _audioService = audioService;
      _hitEffectService = hitEffectService;
      _floatingTextService = floatingTextService;
      _economySettings = economySettings ?? new EconomySettings();
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

        // Find pedestrian position
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

        // Play sound
        _audioService.PlaySFX(isViolation ? SFXType.HitProtected : SFXType.Hit);

        // Spawn floating text
        if (isViolation)
        {
          int penalty = _economySettings.ViolationPenalty;
          _floatingTextService.SpawnMoneyText(hitPosition + Vector3.up, -penalty, false);
          _audioService.PlaySFX(SFXType.MoneyLoss);
        }
        else
        {
          // Show small reward indicator for quest progress
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
      // Update floating texts
      if (_floatingTextService is FloatingTextService service)
      {
        service.Update();
      }
    }
  }

  #endregion
}
