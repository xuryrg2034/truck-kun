---
tags: [index, navigation]
aliases: [Главная, Index]
---

# Truck-kun Rising

> Аркадный раннер в стиле isekai. Entitas ECS + Zenject + Unity 6.

## Навигация по графу

| Действие | Горячая клавиша |
|----------|-----------------|
| **Local Graph** (только связи текущей заметки) | `Ctrl+Shift+G` |
| Global Graph | `Ctrl+G` |
| Quick Switcher | `Ctrl+O` |

### Карты связей (MOC)

Для фокусированного просмотра открой MOC и нажми `Ctrl+Shift+G`:

- [[Pedestrian-MOC]] — всё о пешеходах
- [[Physics-MOC]] — всё о физике
- [[Economy-MOC]] — всё об экономике

---

## Архитектура

- [[ECS-Pattern]] — основные паттерны ECS
- [[Contexts]] — GameContext, MetaContext
- [[View-Binding]] — связь ECS с Unity

## Features

| Feature | Описание |
|---------|----------|
| [[Physics]] | Физика транспорта |
| [[Pedestrian]] | NPC пешеходы |
| [[Collision]] | Обработка столкновений |
| [[Economy]] | Деньги и награды |
| [[Quest]] | Ежедневные квесты |
| [[Feedback]] | VFX, звуки, UI эффекты |
| [[Surface]] | Типы поверхностей |

## Services

| Сервис | Назначение |
|--------|------------|
| [[MoneyService]] | Управление балансом |
| [[QuestService]] | Прогресс квестов |
| [[HitEffectService]] | Эффекты попаданий |
| [[UpgradeService]] | Система улучшений |

## Game Flow

- [[Game-Loop]] — цикл день/хаб/геймплей
- [[Player-Progression]] — прогрессия игрока
- [[Economy-Flow]] — экономический баланс

## Configs

| Конфиг | Путь |
|--------|------|
| [[VehicleConfig]] | `Resources/Configs/VehicleConfig` |
| [[LevelConfig]] | `Resources/Configs/LevelConfig` |
| [[PedestrianConfig]] | `Resources/Configs/PedestrianConfig` |

---

**Инструкции для AI:** [[Claude-Instructions]]
