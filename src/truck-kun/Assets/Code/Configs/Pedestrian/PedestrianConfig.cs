using System.Collections.Generic;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Configs.Pedestrian
{
  /// <summary>
  /// Configuration for pedestrian visuals and spawn weights
  /// </summary>
  [CreateAssetMenu(fileName = "PedestrianConfig", menuName = "Truck-kun/Pedestrian Config")]
  public class PedestrianConfig : ScriptableObject
  {
    [Header("Visual Settings")]
    [SerializeField] private List<PedestrianVisualData> _visualData = new()
    {
      PedestrianVisualData.Default(PedestrianKind.StudentNerd),
      PedestrianVisualData.Default(PedestrianKind.Salaryman),
      PedestrianVisualData.Default(PedestrianKind.Grandma),
      PedestrianVisualData.Default(PedestrianKind.OldMan),
      PedestrianVisualData.Default(PedestrianKind.Teenager)
    };

    [Header("Spawn Weights")]
    [SerializeField] private List<PedestrianSpawnWeight> _spawnWeights = new()
    {
      new PedestrianSpawnWeight(PedestrianKind.StudentNerd, 0.25f),
      new PedestrianSpawnWeight(PedestrianKind.Salaryman, 0.30f),
      new PedestrianSpawnWeight(PedestrianKind.Grandma, 0.15f),
      new PedestrianSpawnWeight(PedestrianKind.OldMan, 0.10f),
      new PedestrianSpawnWeight(PedestrianKind.Teenager, 0.20f)
    };

    public IReadOnlyList<PedestrianVisualData> VisualData => _visualData;
    public IReadOnlyList<PedestrianSpawnWeight> SpawnWeights => _spawnWeights;

    public PedestrianVisualData GetVisualData(PedestrianKind kind)
    {
      foreach (PedestrianVisualData data in _visualData)
      {
        if (data.Kind == kind)
          return data;
      }
      return PedestrianVisualData.Default(kind);
    }

    public PedestrianKind SelectRandomKind()
    {
      return SelectRandomKind(null);
    }

    /// <summary>
    /// Select random pedestrian kind, filtered by allowed types.
    /// Skips types that don't have a prefab assigned.
    /// </summary>
    public PedestrianKind SelectRandomKind(List<PedestrianKind> allowedKinds)
    {
      if (_spawnWeights.Count == 0)
        return PedestrianKind.StudentNerd;

      // Build filtered list of spawn weights
      List<PedestrianSpawnWeight> filtered = new();
      foreach (PedestrianSpawnWeight sw in _spawnWeights)
      {
        // Skip if not in allowed list (when list is specified)
        if (allowedKinds != null && allowedKinds.Count > 0 && !allowedKinds.Contains(sw.Kind))
          continue;

        // Skip if no prefab assigned
        if (!HasPrefab(sw.Kind))
          continue;

        filtered.Add(sw);
      }

      // Fallback: if all filtered out, try to find any type with prefab
      if (filtered.Count == 0)
      {
        foreach (PedestrianSpawnWeight sw in _spawnWeights)
        {
          if (HasPrefab(sw.Kind))
            return sw.Kind;
        }
        return PedestrianKind.StudentNerd;
      }

      // Calculate total weight
      float totalWeight = 0f;
      foreach (PedestrianSpawnWeight sw in filtered)
        totalWeight += sw.Weight;

      if (totalWeight <= 0f)
        return filtered[0].Kind;

      // Weighted random selection
      float random = Random.value * totalWeight;
      float cumulative = 0f;

      foreach (PedestrianSpawnWeight sw in filtered)
      {
        cumulative += sw.Weight;
        if (random <= cumulative)
          return sw.Kind;
      }

      return filtered[^1].Kind;
    }

    /// <summary>
    /// Check if a pedestrian kind has a prefab assigned
    /// </summary>
    public bool HasPrefab(PedestrianKind kind)
    {
      foreach (PedestrianVisualData data in _visualData)
      {
        if (data.Kind == kind)
          return data.Prefab != null;
      }
      return false;
    }

    /// <summary>
    /// Get list of kinds that have prefabs assigned
    /// </summary>
    public List<PedestrianKind> GetAvailableKinds()
    {
      List<PedestrianKind> available = new();
      foreach (PedestrianVisualData data in _visualData)
      {
        if (data.Prefab != null)
          available.Add(data.Kind);
      }
      return available;
    }
  }
}
