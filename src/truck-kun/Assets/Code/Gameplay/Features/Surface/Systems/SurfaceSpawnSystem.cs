using System.Collections.Generic;
using Code.Gameplay.Features.Physics;
using Code.Gameplay.Features.Surface.Factory;
using Entitas;
using UnityEngine;

namespace Code.Gameplay.Features.Surface.Systems
{
  /// <summary>
  /// Spawns surface hazards ahead of the hero
  /// </summary>
  public class SurfaceSpawnSystem : IExecuteSystem
  {
    private readonly SurfaceSpawnSettings _settings;
    private readonly IGroup<GameEntity> _heroes;
    private readonly List<GameEntity> _heroBuffer = new(1);
    private readonly List<GameObject> _activeSurfaces = new(16);

    private float _nextSpawnZ;
    private float _totalWeight;

    public SurfaceSpawnSystem(GameContext game, SurfaceSpawnSettings settings)
    {
      _settings = settings;
      _heroes = game.GetGroup(GameMatcher.AllOf(GameMatcher.Hero, GameMatcher.WorldPosition));

      _totalWeight = _settings.OilWeight + _settings.GrassWeight +
                     _settings.PuddleWeight + _settings.IceWeight;
    }

    public void Execute()
    {
      if (!_settings.EnableSpawning)
        return;

      GameEntity hero = null;
      foreach (GameEntity h in _heroes.GetEntities(_heroBuffer))
      {
        hero = h;
        break;
      }

      if (hero == null)
        return;

      Vector3 heroPos = hero.worldPosition.Value;

      if (_nextSpawnZ < heroPos.z)
        _nextSpawnZ = heroPos.z + _settings.MinSpawnInterval;

      float spawnZ = heroPos.z + _settings.SpawnDistanceAhead;
      while (_nextSpawnZ < spawnZ)
      {
        if (Random.value < _settings.SpawnChance)
        {
          SpawnSurface(_nextSpawnZ, heroPos.x);
        }

        _nextSpawnZ += Random.Range(_settings.MinSpawnInterval, _settings.MaxSpawnInterval);
      }

      DespawnOldSurfaces(heroPos.z - _settings.DespawnDistanceBehind);
    }

    private void DespawnOldSurfaces(float despawnZ)
    {
      for (int i = _activeSurfaces.Count - 1; i >= 0; i--)
      {
        GameObject surface = _activeSurfaces[i];
        if (surface == null)
        {
          _activeSurfaces.RemoveAt(i);
          continue;
        }

        if (surface.transform.position.z < despawnZ)
        {
          Object.Destroy(surface);
          _activeSurfaces.RemoveAt(i);
        }
      }
    }

    private void SpawnSurface(float z, float heroX)
    {
      SurfaceType type = SelectRandomSurfaceType();

      float length = Random.Range(_settings.MinLength, _settings.MaxLength);
      float width = _settings.Width;

      float halfRoad = 3f;
      float margin = _settings.LateralMargin + width * 0.5f;
      float x = Random.Range(-halfRoad + margin, halfRoad - margin);

      Vector3 position = new Vector3(x, _settings.HeightOffset, z);

      GameObject surface = SurfaceFactory.CreateSurface(type, position, length, width);
      _activeSurfaces.Add(surface);
    }

    private SurfaceType SelectRandomSurfaceType()
    {
      float random = Random.value * _totalWeight;
      float cumulative = 0f;

      cumulative += _settings.OilWeight;
      if (random < cumulative) return SurfaceType.Oil;

      cumulative += _settings.GrassWeight;
      if (random < cumulative) return SurfaceType.Grass;

      cumulative += _settings.PuddleWeight;
      if (random < cumulative) return SurfaceType.Puddle;

      return SurfaceType.Ice;
    }
  }
}
