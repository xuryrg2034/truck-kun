using System;
using System.Collections.Generic;
using Code.Audio;
using Code.Configs.Global;
using Code.Gameplay.Features.Feedback.Services;
using Code.Gameplay.Features.Pedestrian;
using Code.Gameplay.Features.Pedestrian.Extensions;
using Entitas;
using UnityEngine;
using AudioHelper = Code.Audio.Audio;

namespace Code.Gameplay.Features.Feedback.Systems
{
  /// <summary>
  /// Handles visual and audio feedback for hit events.
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

        // Play sound
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
}
