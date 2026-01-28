using UnityEngine;
using AudioHelper = Code.Audio.Audio;

namespace Code.Art.VFX
{
  /// <summary>
  /// Controls hit particle effects when colliding with pedestrians.
  /// Spawns sparks, debris particles and triggers camera shake.
  /// </summary>
  public class HitEffectController : MonoBehaviour
  {
    [Header("Particles")]
    [SerializeField] private ParticleSystem _sparksPrefab;
    [SerializeField] private ParticleSystem _debrisPrefab;
    [SerializeField] private ParticleSystem _bloodPrefab;

    [Header("Settings")]
    [SerializeField] private int _sparksCount = 15;
    [SerializeField] private int _debrisCount = 8;
    [SerializeField] private float _effectScale = 1f;

    [Header("Camera Shake")]
    [SerializeField] private bool _enableCameraShake = true;
    [SerializeField] private float _shakeIntensity = 0.3f;
    [SerializeField] private float _shakeDuration = 0.2f;

    private static HitEffectController _instance;
    public static HitEffectController Instance => _instance;

    // Object pools
    private ParticleSystem[] _sparksPool;
    private ParticleSystem[] _debrisPool;
    private int _sparksIndex;
    private int _debrisIndex;
    private const int PoolSize = 5;

    private void Awake()
    {
      if (_instance != null && _instance != this)
      {
        Destroy(gameObject);
        return;
      }
      _instance = this;

      InitializePools();
    }

    private void InitializePools()
    {
      // Create pools for particle systems
      if (_sparksPrefab != null)
      {
        _sparksPool = new ParticleSystem[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
          _sparksPool[i] = Instantiate(_sparksPrefab, transform);
          _sparksPool[i].gameObject.SetActive(false);
        }
      }

      if (_debrisPrefab != null)
      {
        _debrisPool = new ParticleSystem[PoolSize];
        for (int i = 0; i < PoolSize; i++)
        {
          _debrisPool[i] = Instantiate(_debrisPrefab, transform);
          _debrisPool[i].gameObject.SetActive(false);
        }
      }
    }

    /// <summary>
    /// Play hit effect at specified position
    /// </summary>
    public void PlayHitEffect(Vector3 position, Vector3 normal, float impactForce)
    {
      float intensity = Mathf.Clamp01(impactForce / 10f);

      // Sparks
      PlayPooledEffect(_sparksPool, ref _sparksIndex, position, normal,
        Mathf.RoundToInt(_sparksCount * intensity));

      // Debris
      PlayPooledEffect(_debrisPool, ref _debrisIndex, position, normal,
        Mathf.RoundToInt(_debrisCount * intensity));

      // Camera shake
      if (_enableCameraShake)
      {
        CameraShakeController.Instance?.Shake(_shakeIntensity * intensity, _shakeDuration);
      }

      // Sound effect
      AudioHelper.Hit(intensity);

      Debug.Log($"[HitEffect] Played at {position} with intensity {intensity:F2}");
    }

    private void PlayPooledEffect(ParticleSystem[] pool, ref int index, Vector3 position,
      Vector3 normal, int count)
    {
      if (pool == null || pool.Length == 0)
        return;

      ParticleSystem ps = pool[index];
      index = (index + 1) % pool.Length;

      ps.transform.position = position;
      ps.transform.rotation = Quaternion.LookRotation(normal);
      ps.transform.localScale = Vector3.one * _effectScale;

      var emission = ps.emission;
      var burst = emission.GetBurst(0);
      burst.count = count;
      emission.SetBurst(0, burst);

      ps.gameObject.SetActive(true);
      ps.Play();
    }

    /// <summary>
    /// Create default hit effect prefabs if none assigned
    /// </summary>
    [ContextMenu("Create Default Prefabs")]
    public void CreateDefaultPrefabs()
    {
      if (_sparksPrefab == null)
        _sparksPrefab = CreateSparksPrefab();

      if (_debrisPrefab == null)
        _debrisPrefab = CreateDebrisPrefab();
    }

    private ParticleSystem CreateSparksPrefab()
    {
      GameObject go = new GameObject("Sparks");
      go.transform.SetParent(transform);

      ParticleSystem ps = go.AddComponent<ParticleSystem>();
      var main = ps.main;
      main.duration = 0.5f;
      main.loop = false;
      main.startLifetime = 0.3f;
      main.startSpeed = 8f;
      main.startSize = 0.1f;
      main.startColor = new Color(1f, 0.8f, 0.3f);
      main.gravityModifier = 1f;
      main.simulationSpace = ParticleSystemSimulationSpace.World;

      var emission = ps.emission;
      emission.rateOverTime = 0;
      emission.SetBursts(new[] { new ParticleSystem.Burst(0, _sparksCount) });

      var shape = ps.shape;
      shape.shapeType = ParticleSystemShapeType.Cone;
      shape.angle = 45f;
      shape.radius = 0.2f;

      var colorOverLifetime = ps.colorOverLifetime;
      colorOverLifetime.enabled = true;
      Gradient gradient = new Gradient();
      gradient.SetKeys(
        new[] { new GradientColorKey(Color.white, 0), new GradientColorKey(new Color(1f, 0.5f, 0f), 1) },
        new[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(0, 1) }
      );
      colorOverLifetime.color = gradient;

      var renderer = go.GetComponent<ParticleSystemRenderer>();
      renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));

      go.SetActive(false);
      return ps;
    }

    private ParticleSystem CreateDebrisPrefab()
    {
      GameObject go = new GameObject("Debris");
      go.transform.SetParent(transform);

      ParticleSystem ps = go.AddComponent<ParticleSystem>();
      var main = ps.main;
      main.duration = 1f;
      main.loop = false;
      main.startLifetime = 1f;
      main.startSpeed = 5f;
      main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);
      main.startColor = new Color(0.4f, 0.35f, 0.3f);
      main.gravityModifier = 2f;
      main.simulationSpace = ParticleSystemSimulationSpace.World;

      var emission = ps.emission;
      emission.rateOverTime = 0;
      emission.SetBursts(new[] { new ParticleSystem.Burst(0, _debrisCount) });

      var shape = ps.shape;
      shape.shapeType = ParticleSystemShapeType.Cone;
      shape.angle = 60f;
      shape.radius = 0.3f;

      var rotation = ps.rotationOverLifetime;
      rotation.enabled = true;
      rotation.z = new ParticleSystem.MinMaxCurve(-180f, 180f);

      var renderer = go.GetComponent<ParticleSystemRenderer>();
      renderer.renderMode = ParticleSystemRenderMode.Mesh;
      renderer.mesh = CreateCubeMesh();
      renderer.material = new Material(Shader.Find("Standard"));

      go.SetActive(false);
      return ps;
    }

    private Mesh CreateCubeMesh()
    {
      GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
      Mesh mesh = temp.GetComponent<MeshFilter>().sharedMesh;
      DestroyImmediate(temp);
      return mesh;
    }

    private void OnDestroy()
    {
      if (_instance == this)
        _instance = null;
    }
  }
}
