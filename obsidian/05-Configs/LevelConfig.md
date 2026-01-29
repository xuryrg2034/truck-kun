---
tags: [config, scriptableobject]
aliases: [Level Config, конфиг уровня]
---

# Level Config

Конфигурация уровня/дня.

## Поля

| Поле | Тип | Описание |
|------|-----|----------|
| DayDuration | float | Длительность дня (сек) |
| RoadLength | float | Длина дороги |
| RoadWidth | float | Ширина дороги |
| SpawnRate | float | Частота спавна NPC |

## Используется в

- [[Pedestrian]] — спавн пешеходов
- [[Game-Loop]] — таймер дня
- Level Generator — генерация уровня

## Путь

- **Скрипт:** `Assets/Code/Configs/LevelConfig.cs`
- **Asset:** `Assets/Resources/Configs/LevelConfig.asset`
