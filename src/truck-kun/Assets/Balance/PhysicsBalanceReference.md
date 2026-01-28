# Physics Balance Reference

## Overview

This document contains recommended physics parameters for the hybrid physics system.
All values have been tested in the PhysicsTestScene.

**Last Updated:** 2025-01-27
**Version:** 1.0

---

## Movement Parameters

### Forward Movement

| Parameter | Easy | Normal | Hard | Description |
|-----------|------|--------|------|-------------|
| MinForwardSpeed | 4.0 | 5.0 | 6.0 | Minimum constant speed (m/s) |
| MaxForwardSpeed | 10.0 | 8.0 | 7.0 | Maximum speed (m/s) |
| ForwardAcceleration | 12.0 | 10.0 | 8.0 | Acceleration rate (m/s²) |
| Deceleration | 6.0 | 8.0 | 10.0 | Natural slowdown rate (m/s²) |

### Lateral Movement

| Parameter | Easy | Normal | Hard | Description |
|-----------|------|--------|------|-------------|
| MaxLateralSpeed | 10.0 | 8.0 | 6.0 | Max side movement speed (m/s) |
| LateralAcceleration | 20.0 | 15.0 | 10.0 | Side acceleration (m/s²) |

### Physics

| Parameter | Easy | Normal | Hard | Description |
|-----------|------|--------|------|-------------|
| BaseDrag | 0.3 | 0.5 | 0.7 | Air resistance |
| Mass | 800 | 1000 | 1200 | Vehicle mass (kg) |
| AngularDrag | 0.05 | 0.05 | 0.05 | Rotation damping |

---

## Surface Modifiers

### Surface Types

| Surface | Friction | Drag | Effect |
|---------|----------|------|--------|
| Normal | 1.0 | 1.0 | Standard handling |
| Oil | 0.3 | 0.5 | Very slippery, less control |
| Grass | 0.8 | 1.8 | Slightly slippery, slows down |
| Ice | 0.15 | 0.3 | Extremely slippery |
| Puddle | 0.85 | 1.3 | Slight slip, moderate slowdown |

### Effect Breakdown

**Oil:**
- Friction 0.3 = 70% reduction in lateral control
- Drag 0.5 = 50% less natural slowdown
- Player slides through corners
- Recovery distance: ~5-10m after exiting

**Grass:**
- Friction 0.8 = 20% less control
- Drag 1.8 = 80% more resistance
- Significant speed loss (~40%)
- Good for slowing runaway players

**Ice:**
- Friction 0.15 = 85% control reduction
- Drag 0.3 = minimal slowdown
- Extremely dangerous, nearly uncontrollable
- Use sparingly in level design

---

## Collision Settings

| Parameter | Value | Description |
|-----------|-------|-------------|
| UsePhysicsCollision | true | Use Unity physics for detection |
| MinImpactForce | 0.5 | Minimum velocity to register hit |
| StrongImpactForce | 5.0 | Threshold for "strong" hit VFX |

### Collision Reliability

- Physics-based collisions work at speeds up to 30 m/s
- At higher speeds, consider ContinuousSpeculative mode
- Recommended FixedUpdate timestep: 0.02 (50 Hz)

---

## Ragdoll Settings

| Parameter | Value | Description |
|-----------|-------|-------------|
| HitForce | 800 | Base launch force (N) |
| UpwardForce | 300 | Vertical component (N) |
| TorqueForce | 200 | Spin force (N·m) |
| DespawnDelay | 2.5 | Time before removal (s) |
| MaxActiveRagdolls | 5 | Performance limit |
| RagdollDrag | 0.5 | Air resistance |
| EnableFadeOut | true | Visual fade before despawn |

---

## Performance Targets

| Metric | Target | Notes |
|--------|--------|-------|
| FPS | 60+ | On target hardware |
| Physics Step | 20ms | FixedUpdate budget |
| Max Rigidbodies | 20 | Active at once |
| Max Pedestrians | 12 | Spawned NPCs |

### Optimization Tips

1. Use `MaxActiveRagdolls` to limit flying NPCs
2. Despawn pedestrians behind camera
3. Pool NPC GameObjects
4. Use layer-based collision matrix

---

## Balance Checklist

### Forward Movement
- [ ] Speed feels comfortable (not too fast/slow)
- [ ] Acceleration is smooth
- [ ] Min/Max speeds are correct
- [ ] No jitter at constant speed

### Lateral Movement
- [ ] Control is precise on Normal surface
- [ ] Oil gives obvious sliding effect
- [ ] Grass noticeably slows maneuvers
- [ ] Ice is challenging but not impossible

### Collisions
- [ ] Collisions register reliably
- [ ] No missed NPCs at high speed
- [ ] No false positives
- [ ] Ragdoll launches look natural

### Performance
- [ ] 60 FPS with 12 active NPCs
- [ ] No lag spikes on collision
- [ ] FixedUpdate doesn't overrun

---

## Difficulty Scaling

### Easy Mode
- Higher max speed (fun factor)
- More forgiving surfaces (higher friction)
- Slower NPC spawn
- Wider road margins

### Normal Mode
- Balanced challenge
- Standard surface effects
- Medium NPC density
- Standard road width

### Hard Mode
- Lower max speed (precision required)
- Punishing surfaces (lower friction)
- High NPC density
- Narrower effective road

---

## Code Reference

```csharp
// EcsBootstrap.cs - CreateSettingsFromBalance()
_runnerMovement = new RunnerMovementSettings
{
  // Speed
  MinForwardSpeed = 5f,
  MaxForwardSpeed = 8f,
  MaxLateralSpeed = 8f,

  // Acceleration
  ForwardAcceleration = 10f,
  LateralAcceleration = 15f,
  Deceleration = 8f,

  // Physics
  BaseDrag = 0.5f,
  Mass = 1000f,
  AngularDrag = 0.05f,
  UseContinuousCollision = true
};

// Surface defaults - SurfaceTrigger.cs
GetDefaultModifiers(SurfaceType type) => type switch
{
  SurfaceType.Normal => (1.0f, 1.0f),
  SurfaceType.Oil    => (0.3f, 0.5f),
  SurfaceType.Grass  => (0.8f, 1.8f),
  SurfaceType.Ice    => (0.15f, 0.3f),
  SurfaceType.Puddle => (0.85f, 1.3f),
  _ => (1.0f, 1.0f)
};
```

---

## Testing Procedure

1. **Generate Test Scene**
   - Menu: Tools → Physics → Generate Test Scene
   - Scene contains all surface types and test NPCs

2. **Run Balance Test**
   - Play the test scene
   - Press F1 for debug panel
   - Drive through all zones
   - Note handling differences

3. **Verify Collisions**
   - Hit test NPCs
   - Check console for collision logs
   - Verify ragdoll behavior

4. **Performance Check**
   - Monitor FPS in debug panel
   - Note any frame drops
   - Check Physics Step time

---

## Changelog

### v1.0 (2025-01-27)
- Initial balance values
- Surface modifiers defined
- Ragdoll parameters tuned
- Performance targets set
