using System;
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian
{
  /// <summary>
  /// Visual and behavioral data for a pedestrian type
  /// </summary>
  [Serializable]
  public class PedestrianVisualData
  {
    [Header("Prefab (drag and drop)")]
    [Tooltip("Optional prefab to use. If not set, procedural model will be generated.")]
    public GameObject Prefab;

    [Header("Settings")]
    public PedestrianKind Kind;
    public Color Color = Color.white;
    public float Scale = 1f;
    public float Speed = 1f;
    public float ForwardTilt = 0f;        // X rotation (bent forward)
    public float BaseSpeed = 1.5f;        // Movement speed
    public PedestrianCategory Category = PedestrianCategory.Normal;
    public string DisplayName = "Pedestrian";

    [Header("Behavior")]
    [Range(0f, 1f)]
    [Tooltip("Chance this pedestrian will cross the road (0 = always stands, 1 = always crosses)")]
    public float CrossingChance = 0.7f;

    public static PedestrianVisualData Default(PedestrianKind kind)
    {
      return kind switch
      {
        PedestrianKind.StudentNerd => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.95f, 0.95f, 1f),  // White/light blue
          Scale = 0.85f,
          ForwardTilt = 15f,                     // Bent forward (backpack)
          BaseSpeed = 2f,
          Category = PedestrianCategory.Normal,
          DisplayName = "Student",
          CrossingChance = 0.9f                  // Students rush across
        },
        PedestrianKind.Salaryman => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.4f, 0.4f, 0.45f), // Gray suit
          Scale = 1f,
          ForwardTilt = 0f,
          BaseSpeed = 1.8f,
          Category = PedestrianCategory.Normal,
          DisplayName = "Salaryman",
          CrossingChance = 0.8f                  // Busy commuters
        },
        PedestrianKind.Grandma => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(1f, 0.7f, 0.8f),    // Pink
          Scale = 0.8f,
          ForwardTilt = 10f,                     // Slightly hunched
          BaseSpeed = 0.8f,                      // Slow
          Category = PedestrianCategory.Protected,
          DisplayName = "Grandma",
          CrossingChance = 0.4f                  // Often just stands
        },
        PedestrianKind.OldMan => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.6f, 0.45f, 0.3f), // Brown
          Scale = 0.9f,
          ForwardTilt = 8f,
          BaseSpeed = 0.9f,
          Category = PedestrianCategory.Protected,
          DisplayName = "Old Man",
          CrossingChance = 0.5f                  // Sometimes crosses slowly
        },
        PedestrianKind.Teenager => new PedestrianVisualData
        {
          Kind = kind,
          Color = new Color(0.2f, 0.8f, 0.4f),  // Bright green
          Scale = 0.95f,
          ForwardTilt = -5f,                     // Leaning back (cool pose)
          BaseSpeed = 2.2f,                      // Fast
          Category = PedestrianCategory.Normal,
          DisplayName = "Teenager",
          CrossingChance = 0.85f                 // Active, always on the move
        },
        _ => new PedestrianVisualData
        {
          Kind = kind,
          Color = Color.white,
          Scale = 1f,
          ForwardTilt = 0f,
          BaseSpeed = 1.5f,
          Category = PedestrianCategory.Normal,
          DisplayName = "Pedestrian",
          CrossingChance = 0.7f
        }
      };
    }
  }

  /// <summary>
  /// Spawn weight for weighted random selection
  /// </summary>
  [Serializable]
  public class PedestrianSpawnWeight
  {
    public PedestrianKind Kind;
    [Range(0f, 1f)] public float Weight = 1f;

    public PedestrianSpawnWeight() { }

    public PedestrianSpawnWeight(PedestrianKind kind, float weight)
    {
      Kind = kind;
      Weight = weight;
    }
  }
}
