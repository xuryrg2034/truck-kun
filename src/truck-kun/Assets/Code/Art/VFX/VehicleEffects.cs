using UnityEngine;

namespace Code.Art.VFX
{
  /// <summary>
  /// Visual effects for the vehicle - dust, trails, etc.
  /// Attach to hero prefab.
  /// </summary>
  public class VehicleEffects : MonoBehaviour
  {
    [Header("Dust Effect")]
    [SerializeField] private bool _enableDust = true;
    [SerializeField] private ParticleSystem _dustParticles;
    [SerializeField] private float _dustSpeedThreshold = 2f;
    [SerializeField] private float _dustEmissionRate = 20f;

    [Header("Trail Effect")]
    [SerializeField] private bool _enableTrail = false;
    [SerializeField] private TrailRenderer _leftTrail;
    [SerializeField] private TrailRenderer _rightTrail;

    [Header("Speed Lines")]
    [SerializeField] private bool _enableSpeedLines = true;
    [SerializeField] private ParticleSystem _speedLines;
    [SerializeField] private float _speedLinesThreshold = 8f;

    private Rigidbody _rigidbody;
    private ParticleSystem.EmissionModule _dustEmission;
    private ParticleSystem.EmissionModule _speedLinesEmission;

    private void Start()
    {
      _rigidbody = GetComponent<Rigidbody>();

      // Setup dust
      if (_dustParticles == null && _enableDust)
        CreateDustEffect();

      if (_dustParticles != null)
        _dustEmission = _dustParticles.emission;

      // Setup trails
      if (_enableTrail && _leftTrail == null)
        CreateTrailEffects();

      // Setup speed lines
      if (_speedLines == null && _enableSpeedLines)
        CreateSpeedLinesEffect();

      if (_speedLines != null)
        _speedLinesEmission = _speedLines.emission;
    }

    private void Update()
    {
      if (_rigidbody == null)
        return;

      float speed = _rigidbody.linearVelocity.magnitude;

      UpdateDustEffect(speed);
      UpdateTrailEffect(speed);
      UpdateSpeedLinesEffect(speed);
    }

    private void UpdateDustEffect(float speed)
    {
      if (!_enableDust || _dustParticles == null)
        return;

      bool shouldEmit = speed > _dustSpeedThreshold;
      float rate = shouldEmit ? _dustEmissionRate * (speed / 10f) : 0f;

      _dustEmission.rateOverTime = rate;
    }

    private void UpdateTrailEffect(float speed)
    {
      if (!_enableTrail)
        return;

      bool shouldShow = speed > _dustSpeedThreshold;

      if (_leftTrail != null)
        _leftTrail.emitting = shouldShow;
      if (_rightTrail != null)
        _rightTrail.emitting = shouldShow;
    }

    private void UpdateSpeedLinesEffect(float speed)
    {
      if (!_enableSpeedLines || _speedLines == null)
        return;

      bool shouldEmit = speed > _speedLinesThreshold;
      float rate = shouldEmit ? 30f * ((speed - _speedLinesThreshold) / 10f) : 0f;

      _speedLinesEmission.rateOverTime = rate;
    }

    #region Effect Creation

    private void CreateDustEffect()
    {
      GameObject dustObj = new GameObject("DustEffect");
      dustObj.transform.SetParent(transform);
      dustObj.transform.localPosition = new Vector3(0, 0.1f, -1.5f); // Behind vehicle

      _dustParticles = dustObj.AddComponent<ParticleSystem>();
      _dustParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      var main = _dustParticles.main;
      main.duration = 1f;
      main.loop = true;
      main.startLifetime = 1.5f;
      main.startSpeed = 1f;
      main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.8f);
      main.startColor = new Color(0.6f, 0.55f, 0.5f, 0.4f);
      main.gravityModifier = -0.1f;
      main.simulationSpace = ParticleSystemSimulationSpace.World;

      var emission = _dustParticles.emission;
      emission.rateOverTime = 0;

      var shape = _dustParticles.shape;
      shape.shapeType = ParticleSystemShapeType.Box;
      shape.scale = new Vector3(1.5f, 0.1f, 0.5f);

      var velocityOverLifetime = _dustParticles.velocityOverLifetime;
      velocityOverLifetime.enabled = true;
      velocityOverLifetime.space = ParticleSystemSimulationSpace.Local;
      velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);
      velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.5f, 1f);
      velocityOverLifetime.z = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f);

      var sizeOverLifetime = _dustParticles.sizeOverLifetime;
      sizeOverLifetime.enabled = true;
      sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, 2f);

      var colorOverLifetime = _dustParticles.colorOverLifetime;
      colorOverLifetime.enabled = true;
      Gradient gradient = new Gradient();
      gradient.SetKeys(
        new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(Color.white, 1) },
        new[] { new GradientAlphaKey(0.4f, 0), new GradientAlphaKey(0, 1) }
      );
      colorOverLifetime.color = gradient;

      var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
      renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
      renderer.material.SetFloat("_Mode", 2); // Fade
    }

    private void CreateTrailEffects()
    {
      // Left trail
      GameObject leftObj = new GameObject("LeftTrail");
      leftObj.transform.SetParent(transform);
      leftObj.transform.localPosition = new Vector3(-0.6f, 0.05f, -1f);

      _leftTrail = leftObj.AddComponent<TrailRenderer>();
      SetupTrail(_leftTrail);

      // Right trail
      GameObject rightObj = new GameObject("RightTrail");
      rightObj.transform.SetParent(transform);
      rightObj.transform.localPosition = new Vector3(0.6f, 0.05f, -1f);

      _rightTrail = rightObj.AddComponent<TrailRenderer>();
      SetupTrail(_rightTrail);
    }

    private void SetupTrail(TrailRenderer trail)
    {
      trail.time = 0.5f;
      trail.startWidth = 0.3f;
      trail.endWidth = 0f;
      trail.material = new Material(Shader.Find("Sprites/Default"));

      Gradient gradient = new Gradient();
      gradient.SetKeys(
        new[] { new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 0), new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 1) },
        new[] { new GradientAlphaKey(0.5f, 0), new GradientAlphaKey(0, 1) }
      );
      trail.colorGradient = gradient;
      trail.emitting = false;
    }

    private void CreateSpeedLinesEffect()
    {
      GameObject linesObj = new GameObject("SpeedLines");
      linesObj.transform.SetParent(transform);
      linesObj.transform.localPosition = new Vector3(0, 1f, 2f); // In front of vehicle

      _speedLines = linesObj.AddComponent<ParticleSystem>();
      _speedLines.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

      var main = _speedLines.main;
      main.duration = 1f;
      main.loop = true;
      main.startLifetime = 0.3f;
      main.startSpeed = -20f; // Moving towards camera
      main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
      main.startColor = new Color(1f, 1f, 1f, 0.3f);
      main.simulationSpace = ParticleSystemSimulationSpace.Local;

      var emission = _speedLines.emission;
      emission.rateOverTime = 0;

      var shape = _speedLines.shape;
      shape.shapeType = ParticleSystemShapeType.Box;
      shape.scale = new Vector3(4f, 3f, 0.1f);

      var renderer = linesObj.GetComponent<ParticleSystemRenderer>();
      renderer.renderMode = ParticleSystemRenderMode.Stretch;
      renderer.lengthScale = 10f;
      renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
    }

    #endregion
  }
}
