# Truck-kun Rising

> Аркадный раннер в стиле isekai. Entitas ECS + Zenject + Unity 6.

---

## Quick Reference

| Цель | Файл |
|------|------|
| Конфигурация игры | `Assets/Code/Balance/GameBalance.cs` |
| Точка входа ECS | `Assets/Code/Infrastructure/EcsBootstrap.cs` |
| Порядок систем | `Assets/Code/Gameplay/BattleFeature.cs` |
| NPC логика | `Assets/Code/Gameplay/PedestrianFeature.cs` |
| Физика героя | `Assets/Code/Gameplay/PhysicsFeature.cs` |
| Эффекты | `Assets/Code/Gameplay/FeedbackSystem.cs` |
| Анимации | `Assets/Code/Art/VFX/TweenSystem.cs` |

---

## Архитектура

### Слои

```
┌─────────────────────────────────────────────────────────────┐
│                      UNITY LAYER                            │
│   Scenes (MainMenu, Hub, Battle)                            │
│   Prefabs (PlayerTruck, Pedestrians)                        │
│   MonoBehaviours (CameraFollow, EntityBehaviour)            │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    INFRASTRUCTURE                           │
│   Zenject Container                                         │
│   ├── Services (Time, Identifier, Audio)                    │
│   ├── Factories (System, Pedestrian, Hero)                  │
│   └── Settings (GameBalance, MovementSettings)              │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────┐
│                    ECS LAYER (Entitas)                      │
│                                                             │
│   Contexts: GameContext, InputContext, MetaContext          │
│                                                             │
│   BattleFeature (главная Feature):                          │
│   ├── InputFeature       - чтение ввода                     │
│   ├── HeroFeature        - логика героя                     │
│   ├── PedestrianFeature  - NPC логика                       │
│   ├── CollisionFeature   - столкновения                     │
│   ├── FeedbackFeature    - VFX, звуки                       │
│   ├── QuestFeature       - прогресс квестов                 │
│   ├── EconomyFeature     - экономика                        │
│   ├── BindViewFeature    - привязка View                    │
│   ├── MovementFeature    - применение движения              │
│   └── SurfaceFeature     - поверхности (Oil, Ice, etc.)     │
│                                                             │
│   PhysicsFeature (FixedUpdate):                             │
│   └── Физика героя с Rigidbody                              │
└─────────────────────────────────────────────────────────────┘
```

### Data Flow

```
Input.GetAxis() → InputContext.moveInput
       ↓
HeroFeature: MoveInput → Direction → WorldPosition
       ↓
PedestrianFeature: Spawn NPC, Move crossing
       ↓
CollisionFeature: Hit detection → HitEvent
       ↓
FeedbackFeature: Particles, Sound, FloatingText
       ↓
QuestFeature + EconomyFeature: Progress, Money
       ↓
BindViewFeature: Entity → GameObject
       ↓
MovementFeature: WorldPosition → Transform.position
```

---

## Структура кода

```
Assets/Code/
├── Common/              # Общие компоненты и утилиты
│   ├── CommonComponents.cs    # Destructed, View, ViewPath
│   └── Services.cs            # ITimeService, IIdentifierService
│
├── Infrastructure/      # DI, States, SystemFactory
│   ├── EcsBootstrap.cs        # Инициализация ECS
│   ├── SystemFactory.cs       # Zenject фабрика систем
│   └── EntityBehaviour.cs     # MonoBehaviour-обёртка Entity
│
├── Gameplay/            # Игровая логика
│   ├── BattleFeature.cs       # Главная Feature
│   ├── PhysicsFeature.cs      # Физика героя
│   ├── PhysicsComponents.cs   # Rigidbody, Velocity, Surface
│   └── Features/              # Отдельные фичи
│       ├── Hero/
│       ├── Enemies/
│       ├── Movement/
│       └── Abilities/
│
├── Balance/             # Конфигурация
│   └── GameBalance.cs         # ВСЕ настройки игры
│
├── Art/VFX/             # Визуальные эффекты
│   ├── TweenSystem.cs         # Кастомные анимации
│   └── NPCAnimator.cs         # Анимации NPC
│
└── Generated/           # Автогенерация Entitas (НЕ РЕДАКТИРОВАТЬ!)
```

---

## Правила написания кода

### Компоненты

```csharp
// Расположение: Features/[Name]/[Name]Components.cs
// Атрибуты контекста: [Game], [Input], [Meta]

[Game] public class Speed : IComponent { public float Value; }      // Data
[Game] public class Moving : IComponent { }                         // Flag
[Game, Unique] public class Hero : IComponent { }                   // Unique
```

После добавления — **перегенерировать Entitas** (Jenny → Generate).

### Системы

```csharp
// Расположение: Features/[Name]/Systems/[Name]System.cs

public class MySystem : IExecuteSystem
{
    private readonly IGroup<GameEntity> _entities;
    private readonly ITimeService _time;

    public MySystem(GameContext game, ITimeService time)
    {
        _time = time;
        _entities = game.GetGroup(GameMatcher
            .AllOf(GameMatcher.ComponentA, GameMatcher.ComponentB)
            .NoneOf(GameMatcher.Destructed));  // <- НЕ ЗАБЫВАЙ!
    }

    public void Execute()
    {
        foreach (GameEntity entity in _entities)
        {
            // логика
        }
    }
}
```

Типы систем:
- `IInitializeSystem` — один раз при старте
- `IExecuteSystem` — каждый кадр
- `ICleanupSystem` — после всех Execute
- `ReactiveSystem<T>` — реакция на изменения

### Features

```csharp
// Расположение: Features/[Name]/[Name]Feature.cs

public sealed class MyFeature : Feature
{
    public MyFeature(ISystemFactory systems)
    {
        Add(systems.Create<InitializeMySystem>());
        Add(systems.Create<ProcessMySystem>());
        Add(systems.Create<CleanupMySystem>());
    }
}
```

Подключить в `BattleFeature.cs`.

### Фабрики сущностей

```csharp
// Расположение: Features/[Name]/Factory/[Name]Factory.cs

public class MyFactory : IMyFactory
{
    private readonly IIdentifierService _identifiers;

    public MyFactory(IIdentifierService identifiers)
    {
        _identifiers = identifiers;
    }

    public GameEntity Create(Vector3 at)
    {
        return CreateEntity.Empty()
            .AddId(_identifiers.Next())
            .AddWorldPosition(at)
            .AddSpeed(5f)
            .AddViewPath("Gameplay/[Path]/prefab_name")
            .With(x => x.isMyFlag = true);
    }
}
```

Зарегистрировать в `BootstrapInstaller.BindGameplayFactories()`.

### View Binding

1. Добавь `ViewPath` при создании сущности
2. Создай префаб с `EntityBehaviour`
3. Добавь регистраторы (`TransformRegistrar`, `SpriteRendererRegistrar`, etc.)

---

## Чеклист: новая сущность

- [ ] Создать компоненты в `[Feature]Components.cs`
- [ ] Перегенерировать Entitas (Jenny)
- [ ] Создать фабрику + интерфейс
- [ ] Зарегистрировать фабрику в `BootstrapInstaller`
- [ ] Создать системы обработки
- [ ] Создать Feature, добавить системы
- [ ] Подключить Feature в `BattleFeature`
- [ ] Создать префаб с `EntityBehaviour` + регистраторы

---

## Антипаттерны

```csharp
// ❌ Логика в компонентах
public class Health : IComponent {
    public void TakeDamage(float amount) { } // НЕТ!
}

// ❌ Создание сущностей напрямую
_context.CreateEntity().AddSomething(); // Используй фабрики!

// ❌ Забыли .NoneOf(Destructed)
game.GetGroup(GameMatcher.Enemy); // Будет обрабатывать удалённых!

// ❌ Прямой доступ между системами
public class SystemA { private SystemB _b; } // Через компоненты!

// ❌ Логика в MonoBehaviour
public class Pedestrian : MonoBehaviour {
    void Update() { Move(); } // Только в ECS системах!
}

// ❌ Редактирование Generated/
// Это автогенерация — любые изменения будут перезаписаны!
```

---

## TODO

### P1 - Высокий приоритет

- [ ] NPC Анимации — интеграция с PedestrianFactory
- [ ] Camera Shake — триггер на HitEvent
- [ ] Улучшенные Hit Particles

### P2 - Средний приоритет

- [ ] UI Анимации (fade, scale bounce)
- [ ] Movement VFX (trail, dust)
- [ ] Audio Polish

### P3 - Низкий приоритет

- [ ] Slowmo на hit
- [ ] Screen flash на violation
- [ ] Combo visual feedback

### Технический долг

- [ ] Object pooling для ParticleSystem
- [ ] Кэширование процедурных мешей
- [ ] Unit тесты для экономики

---

## Правила для AI

### После КАЖДОГО изменения кода:

| Действие | Когда |
|----------|-------|
| Обновить `CHANGELOG.md` | **ВСЕГДА** |
| Обновить этот файл | При изменении архитектуры/структуры |

### Формат записи в CHANGELOG:

```markdown
## YYYY-MM-DD HH:MM - [Краткое описание]

**Файлы:**
- `path/to/file.cs` - что изменено

**Причина:** Зачем
**Детали:** Нюансы реализации
```

### При неопределённости:

1. Проверь этот файл и CHANGELOG
2. Изучи существующий код — найди похожие паттерны
3. Спроси пользователя
