using System;
using UnityEngine;

namespace Code.Gameplay.Features.Surface
{
  [Serializable]
  public class SurfaceSpawnSettings
  {
    [Header("Spawn Control")]
    [Tooltip("Enable surface hazard spawning")]
    public bool EnableSpawning = true;

    [Tooltip("Chance to spawn a surface per spawn check (0-1)")]
    [Range(0f, 1f)]
    public float SpawnChance = 0.15f;

    [Tooltip("Minimum distance between surfaces")]
    public float MinSpawnInterval = 30f;

    [Tooltip("Maximum distance between surfaces")]
    public float MaxSpawnInterval = 60f;

    [Header("Surface Dimensions")]
    [Tooltip("Minimum length of surface patch")]
    public float MinLength = 3f;

    [Tooltip("Maximum length of surface patch")]
    public float MaxLength = 8f;

    [Tooltip("Width of surface patch")]
    public float Width = 3f;

    [Header("Road Bounds")]
    [Tooltip("Distance ahead of hero to spawn")]
    public float SpawnDistanceAhead = 50f;

    [Tooltip("Distance behind hero to despawn")]
    public float DespawnDistanceBehind = 20f;

    [Tooltip("Lateral margin from road edge")]
    public float LateralMargin = 1f;

    [Header("Surface Type Weights")]
    [Tooltip("Weight for Oil surfaces")]
    public float OilWeight = 1f;

    [Tooltip("Weight for Grass surfaces")]
    public float GrassWeight = 0.5f;

    [Tooltip("Weight for Puddle surfaces")]
    public float PuddleWeight = 0.7f;

    [Tooltip("Weight for Ice surfaces (rare)")]
    public float IceWeight = 0.2f;

    [Header("Visual")]
    [Tooltip("Height offset above road")]
    public float HeightOffset = 0.01f;
  }
}
