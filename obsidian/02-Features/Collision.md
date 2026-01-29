---
tags: [feature, ecs, collision]
aliases: [CollisionFeature, столкновения]
related: [[Physics]], [[Pedestrian]], [[Economy]]
---

# Collision Feature

Обработка столкновений Hero с пешеходами и препятствиями.

## Системы

| Система | Назначение |
|---------|------------|
| CollisionDetectionSystem | Обнаружение столкновений |
| HitProcessingSystem | Обработка попаданий |
| DamageSystem | Применение урона |

## Компоненты

- `HitEvent` — событие столкновения
- `CollisionData` — данные столкновения
- `DamageDealt` — нанесённый урон

## Зависимости

- **Получает от:** [[Physics]] — столкновения Rigidbody
- **Триггерит:** [[Economy]] — награды за попадания
- **Триггерит:** [[Quest]] — прогресс квестов
- **Триггерит:** [[Feedback]] — эффекты

## Flow столкновения

```
PhysicsCollisionHandler → HitEvent → HitProcessingSystem →
    → MoneyService.Add()
    → QuestService.OnPedestrianHit()
    → HitEffectService.PlayEffect()
```

## Путь в коде

`Assets/Code/Gameplay/Features/Collision/`
