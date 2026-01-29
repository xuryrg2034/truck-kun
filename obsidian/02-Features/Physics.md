---
tags: [feature, ecs, physics]
aliases: [PhysicsFeature, физика]
related: [[VehicleConfig]], [[Collision]]
---

# Physics Feature

Управление физикой транспорта героя через Unity Rigidbody.

## Системы

| Система | Назначение |
|---------|------------|
| VelocityApplySystem | Применяет скорость к Rigidbody |
| AccelerationSystem | Рассчитывает ускорение |
| SurfaceModifierSystem | Модификаторы от поверхности |
| SteeringSystem | Управление поворотом |
| DragSystem | Применение сопротивления |
| SpeedLimitSystem | Ограничение скорости |
| PhysicsCleanupSystem | Очистка флагов |

## Компоненты

- `Velocity` — текущая скорость Vector3
- `Acceleration` — ускорение
- `SurfaceModifier` — модификатор от поверхности
- `RigidbodyRef` — ссылка на Unity Rigidbody
- `isHero` — флаг героя

## Зависимости

- **Использует:** [[VehicleConfig]], [[VehicleStats]]
- **Триггерит:** [[Collision]]
- **Зависит от:** Input (стиринг)

## Пайплайн движения

```
Input → SteeringSystem → AccelerationSystem → VelocityApplySystem → Rigidbody
```

## Путь в коде

`Assets/Code/Gameplay/Features/Physics/`
