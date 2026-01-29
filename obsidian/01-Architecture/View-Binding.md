---
tags: [architecture, view, unity]
aliases: [EntityBehaviour, View Binding]
related: [[ECS-Pattern]]
---

# View Binding

Паттерн связывания ECS-сущностей с Unity-объектами.

## Архитектура

```
┌──────────────┐     ViewPath      ┌──────────────────┐
│   Entity     │ ──────────────►   │  BindViewSystem  │
│  (ECS data)  │                   └────────┬─────────┘
└──────────────┘                            │
                                            ▼
                               ┌────────────────────────┐
                               │   EntityViewFactory    │
                               │  (загрузка префаба)    │
                               └────────────┬───────────┘
                                            │
                                            ▼
                               ┌────────────────────────┐
                               │   EntityBehaviour      │
                               │  (MonoBehaviour-мост)  │
                               └────────────────────────┘
```

## EntityBehaviour

Мост между ECS и Unity GameObject.

```csharp
public class EntityBehaviour : MonoBehaviour
{
    private GameEntity _entity;

    public GameEntity Entity => _entity;

    public void SetEntity(GameEntity entity)
    {
        _entity = entity;
        _entity.Retain(this);
        _entity.AddView(gameObject);
    }

    public void Release()
    {
        _entity.Release(this);
        _entity = null;
    }
}
```

## Путь в коде

`Assets/Code/Infrastructure/View/EntityBehaviour.cs`

## Используется в

- [[Physics]] — PhysicsCollisionHandler
- [[Pedestrian]] — PedestrianView
- Все сущности с визуальным представлением

## Связанное

- [[ECS-Pattern]] — общая архитектура
