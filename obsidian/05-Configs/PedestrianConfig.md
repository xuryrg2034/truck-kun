---
tags: [config, scriptableobject]
aliases: [Pedestrian Config, конфиг пешеходов]
---

# Pedestrian Config

Конфигурация пешеходов.

## Поля

| Поле | Тип | Описание |
|------|-----|----------|
| SpawnWeights | Dictionary | Веса спавна по типам |
| MoveSpeed | float | Скорость движения |
| Prefabs | GameObject[] | Префабы пешеходов |

## Типы пешеходов

| Kind | Вес | Очки |
|------|-----|------|
| StudentNerd | 30% | 100 |
| Teenager | 25% | 120 |
| Salaryman | 20% | 150 |
| OldMan | 15% | 180 |
| Grandma | 10% | 200 |

## Используется в

- [[Pedestrian]] — спавн и движение
- [[Quest]] — цели квестов

## Путь

- **Скрипт:** `Assets/Code/Configs/Pedestrian/PedestrianConfig.cs`
- **Asset:** `Assets/Resources/Configs/PedestrianConfig.asset`
