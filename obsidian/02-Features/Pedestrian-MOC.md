---
tags: [moc, pedestrian]
aliases: [Пешеходы MOC, Pedestrian Map]
---

# Pedestrian — Карта связей

> Открой **Local Graph** (Ctrl+Shift+G) чтобы увидеть только связи пешеходов.

## Core

- [[Pedestrian]] — главная Feature

## Связанные Features

- [[Collision]] — обработка столкновений с пешеходами
- [[Quest]] — квесты на сбор пешеходов
- [[Economy]] — награды за пешеходов
- [[Feedback]] — эффекты при попадании

## Сервисы

- [[QuestService]] — прогресс квестов по типам
- [[HitEffectService]] — эффекты попаданий

## Конфиги

- [[PedestrianConfig]] — настройки спавна и типов

## Типы пешеходов

| Тип | Очки | Квесты |
|-----|------|--------|
| StudentNerd | 100 | "Сбить студентов" |
| Teenager | 120 | — |
| Salaryman | 150 | "Сбить офисных" |
| OldMan | 180 | — |
| Grandma | 200 | "Сбить бабушек" |

## Код

```
Assets/Code/Gameplay/Features/Pedestrian/
├── Components/
├── Data/
├── Factory/
├── Systems/
├── Extensions/
└── PedestrianFeature.cs
```
