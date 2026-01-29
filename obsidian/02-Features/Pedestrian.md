---
tags: [feature, ecs, npc]
aliases: [PedestrianFeature, пешеходы, NPC]
related: [[PedestrianConfig]], [[Collision]], [[Quest]]
---

# Pedestrian Feature

NPC пешеходы — цели для столкновений.

## Системы

| Система | Назначение |
|---------|------------|
| PedestrianSpawnSystem | Спавн пешеходов (Y=0, overlap check) |
| PedestrianCrossingSystem | Движение пересекающих дорогу |
| PedestrianDespawnSystem | Удаление за границей |

## Спавн

Параметры в `PedestrianSpawnConfig`:

| Параметр | Описание |
|----------|----------|
| SpawnY | Фиксированная высота (0 = земля) |
| MinSpawnDistanceAhead | Мин. расстояние перед игроком |
| SpawnZVariation | Разброс по Z |
| CheckOverlap | Проверка препятствий |
| ObstacleLayer | Слой для проверки |

## Компоненты

- `PedestrianKind` — тип пешехода (Студент, Бабушка, etc)
- `PedestrianPoints` — очки за столкновение
- `WorldPosition` — позиция в мире
- `isPedestrian` — флаг

## Типы пешеходов

| Тип | Очки | Цвет |
|-----|------|------|
| StudentNerd | 100 | Голубой |
| Salaryman | 150 | Серый |
| Grandma | 200 | Розовый |
| OldMan | 180 | Коричневый |
| Teenager | 120 | Зелёный |

## Зависимости

- **Использует:** [[PedestrianConfig]]
- **Триггерит:** [[Collision]] при столкновении
- **Связан с:** [[Quest]] — цели квестов
- **Связан с:** [[Economy]] — награды

## Путь в коде

`Assets/Code/Gameplay/Features/Pedestrian/`
