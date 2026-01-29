using System;
using System.Collections.Generic;
using Code.Configs.Pedestrian;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian.Factory
{
  public interface IPedestrianFactory
  {
    GameObject CreatePedestrianVisual(PedestrianKind kind, Vector3 position);
    PedestrianVisualData GetVisualData(PedestrianKind kind);
    PedestrianKind SelectRandomKind();
    PedestrianKind SelectRandomKind(List<PedestrianKind> allowedKinds);
    bool IsProtected(PedestrianKind kind);
    bool HasPrefab(PedestrianKind kind);
  }

  public class PedestrianFactory : IPedestrianFactory
  {
    private readonly PedestrianConfig _config;
    private readonly PedestrianPhysicsSettings _physicsSettings;
    private readonly Dictionary<PedestrianKind, PedestrianVisualData> _visualCache = new();

    public PedestrianFactory(
      PedestrianConfig config,
      PedestrianPhysicsSettings physicsSettings = null)
    {
      _config = config ?? throw new ArgumentNullException(nameof(config),
        "PedestrianConfig is required! Assign it in GameplaySceneInstaller.");
      _physicsSettings = physicsSettings;
      InitializeCache();
    }

    private void InitializeCache()
    {
      // Cache all visual data from config
      foreach (PedestrianKind kind in Enum.GetValues(typeof(PedestrianKind)))
      {
        _visualCache[kind] = _config.GetVisualData(kind);
      }
    }

    public PedestrianVisualData GetVisualData(PedestrianKind kind)
    {
      if (_visualCache.TryGetValue(kind, out PedestrianVisualData data))
        return data;

      return _config.GetVisualData(PedestrianKind.StudentNerd); // Fallback
    }

    public PedestrianKind SelectRandomKind()
    {
      return _config.SelectRandomKind();
    }

    public PedestrianKind SelectRandomKind(List<PedestrianKind> allowedKinds)
    {
      return _config.SelectRandomKind(allowedKinds);
    }

    public bool HasPrefab(PedestrianKind kind)
    {
      return _config.HasPrefab(kind);
    }

    public bool IsProtected(PedestrianKind kind)
    {
      PedestrianVisualData data = GetVisualData(kind);
      return data.Category == PedestrianCategory.Protected;
    }

    public GameObject CreatePedestrianVisual(PedestrianKind kind, Vector3 position)
    {
      PedestrianVisualData data = GetVisualData(kind);

      if (data.Prefab == null)
        return null;

      return CreateFromPrefab(data, position);
    }

    private GameObject CreateFromPrefab(PedestrianVisualData data, Vector3 position)
    {
      GameObject pedestrian = UnityEngine.Object.Instantiate(data.Prefab, position, Quaternion.identity);
      pedestrian.name = $"Pedestrian_{data.DisplayName}";

      // Apply scale
      pedestrian.transform.localScale = Vector3.one * data.Scale;

      // Apply rotation (forward tilt)
      pedestrian.transform.rotation = Quaternion.Euler(data.ForwardTilt, 0f, 0f);

      // Set layer for collision filtering
      pedestrian.layer = LayerMask.NameToLayer("Pedestrian");

      // Ensure collider exists for physics collision detection
      CapsuleCollider collider = pedestrian.GetComponent<CapsuleCollider>();
      if (collider == null)
      {
        collider = pedestrian.AddComponent<CapsuleCollider>();
        collider.height = 2f;
        collider.radius = 0.3f;
        collider.center = new Vector3(0f, 1f, 0f);
      }
      // Explicitly not a trigger - we want OnCollisionEnter, not OnTriggerEnter
      collider.isTrigger = false;

      // Dynamic Rigidbody for full physics simulation
      Rigidbody rb = pedestrian.GetComponent<Rigidbody>();
      if (rb == null)
      {
        rb = pedestrian.AddComponent<Rigidbody>();
      }

      // Configure for force-based movement
      rb.isKinematic = false;
      rb.useGravity = true;
      rb.mass = _physicsSettings?.GetMass(data.Kind) ?? 60f;
      rb.linearDamping = _physicsSettings?.Drag ?? 2f;
      rb.angularDamping = _physicsSettings?.AngularDrag ?? 0.5f;
      rb.interpolation = RigidbodyInterpolation.Interpolate;

      // Freeze all rotation to prevent spinning from collisions
      rb.constraints =
        RigidbodyConstraints.FreezeRotationX |
        RigidbodyConstraints.FreezeRotationY |
        RigidbodyConstraints.FreezeRotationZ;

      return pedestrian;
    }
  }
}
