---
tags: [feature, ecs, surface]
aliases: [SurfaceFeature, поверхности]
related: [[Physics]]
---

# Surface Feature

Типы поверхностей, влияющие на физику.

## Системы

| Система | Назначение |
|---------|------------|
| SurfaceDetectionSystem | Определение поверхности |
| SurfaceModifierApplySystem | Применение модификаторов |

## Типы поверхностей

| Тип | Скорость | Управление |
|-----|----------|------------|
| Asphalt | x1.0 | x1.0 |
| Dirt | x0.8 | x0.9 |
| Ice | x0.7 | x0.5 |
| Boost | x1.5 | x1.0 |

## Компоненты

- `SurfaceType` — тип текущей поверхности
- `SurfaceModifier` — модификаторы скорости/управления
- `BoostZone` — зона ускорения

## Зависимости

- **Влияет на:** [[Physics]] — модификаторы движения

## Путь в коде

`Assets/Code/Gameplay/Features/Surface/`
