---
tags: [moc, economy]
aliases: [Экономика MOC, Economy Map]
---

# Economy — Карта связей

> Открой **Local Graph** (Ctrl+Shift+G) чтобы увидеть только связи экономики.

## Core

- [[Economy]] — главная Feature
- [[MoneyService]] — сервис баланса

## Связанные Features

- [[Collision]] — источник наград
- [[Quest]] — бонусы за квесты
- [[Pedestrian]] — очки за типы

## Game Flow

- [[Economy-Flow]] — баланс экономики
- [[Player-Progression]] — прогрессия через улучшения

## Сервисы

- [[MoneyService]] — управление балансом
- [[UpgradeService]] — покупка улучшений
- [[QuestService]] — бонусы за квесты

## Flow денег

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  Collision  │ ──► │ MoneyService│ ──► │   Balance   │
│  (награда)  │     │   .Add()    │     │   (Meta)    │
└─────────────┘     └─────────────┘     └──────┬──────┘
                                               │
                                               ▼
                                        ┌─────────────┐
                                        │UpgradeService│
                                        │  .Purchase() │
                                        └─────────────┘
```
