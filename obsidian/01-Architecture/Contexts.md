---
tags: [architecture, ecs, context]
aliases: [GameContext, MetaContext, InputContext]
related: [[ECS-Pattern]]
---

# Contexts

Контексты разделяют сущности по доменам.

## Контексты проекта

| Контекст | Назначение | Примеры |
|----------|------------|---------|
| **Game** | Игровые объекты | Hero, Pedestrians, Obstacles |
| **Meta** | Мета-прогресс | Валюта, квесты, апгрейды |

## Инициализация

```csharp
// В DI-инсталлере
public void BindContexts()
{
    var contexts = Contexts.sharedInstance;

    Container.BindInstance(contexts).AsSingle();
    Container.BindInstance(contexts.game).AsSingle();
    Container.BindInstance(contexts.meta).AsSingle();
}
```

## GameContext

Содержит все игровые сущности:

- **Hero** — транспорт игрока
- **Pedestrian** — NPC пешеходы
- **Obstacle** — препятствия

Компоненты: `WorldPosition`, `Velocity`, `View`, `Rigidbody`

## MetaContext

Содержит мета-данные сессии:

- **Money** — текущий баланс
- **DaySession** — параметры дня
- **Quest** — ежедневные квесты
- **Upgrade** — прогресс апгрейдов

## Связанное

- [[ECS-Pattern]] — общая архитектура
- [[Economy]] — работа с MetaContext
- [[Quest]] — квесты в MetaContext
