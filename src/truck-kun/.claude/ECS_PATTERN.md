# Архитектурный паттерн: Entitas ECS + Zenject + Unity

Универсальное руководство по построению игровой архитектуры на базе Entitas ECS с Dependency Injection.

---

## Рекомендуемый стек

| Слой | Технология | Назначение |
|------|------------|------------|
| ECS | Entitas | Данные и логика |
| DI | Zenject | Внедрение зависимостей |
| Async | UniTask | Асинхронные операции |
| View | Unity MonoBehaviour | Визуальное представление |

---

## Архитектурные слои

```
┌─────────────────────────────────────────────────┐
│                 Game States                      │
│         (StateMachine, IState, IUpdateable)      │
├─────────────────────────────────────────────────┤
│                   Features                       │
│            (композиция систем)                   │
├─────────────────────────────────────────────────┤
│                   Systems                        │
│    (IInitialize, IExecute, ICleanup, Reactive)   │
├─────────────────────────────────────────────────┤
│                 Components                       │
│           (данные без логики)                    │
├─────────────────────────────────────────────────┤
│                  Entities                        │
│          (контейнеры компонентов)                │
├─────────────────────────────────────────────────┤
│                  Contexts                        │
│     (Game, Input, Meta — или свои контексты)     │
└─────────────────────────────────────────────────┘
```

---

## 1. Контексты (Contexts)

Контексты разделяют сущности по доменам. Рекомендуемое разделение:

| Контекст | Назначение | Примеры сущностей |
|----------|------------|-------------------|
| **Game** | Игровые объекты | Персонажи, враги, снаряды, предметы |
| **Input** | События ввода | Клавиатура, мышь, тач, геймпад |
| **Meta** | Мета-прогресс | Валюта, достижения, статистика |

### Инициализация контекстов

```csharp
// В DI-инсталлере (один раз при старте)
public void BindContexts()
{
    var contexts = Contexts.sharedInstance;

    Container.BindInstance(contexts).AsSingle();
    Container.BindInstance(contexts.game).AsSingle();
    Container.BindInstance(contexts.input).AsSingle();
    Container.BindInstance(contexts.meta).AsSingle();
}
```

---

## 2. Компоненты (Components)

Компоненты — чистые данные без логики.

### Типы компонентов

```csharp
// Data-компонент: хранит значение
[Game]
public class WorldPosition : IComponent
{
    public Vector3 Value;
}

// Flag-компонент: маркер без данных
[Game]
public class Moving : IComponent { }

// Unique-компонент: только одна сущность в контексте
[Game, Unique]
public class GameSession : IComponent
{
    public float ElapsedTime;
}

// Индексируемый компонент: для быстрого поиска
[Game]
public class EntityId : IComponent
{
    [PrimaryEntityIndex]
    public int Value;
}
```

### Правила именования
- Компоненты — существительные: `Speed`, `Health`, `Direction`
- Флаги — прилагательные с `is`: `isMoving`, `isDead`, `isPlayer`

---

## 3. Системы (Systems)

Системы содержат всю логику. Типы систем по жизненному циклу:

| Интерфейс | Когда вызывается | Применение |
|-----------|------------------|------------|
| `IInitializeSystem` | Один раз при старте | Создание начальных сущностей |
| `IExecuteSystem` | Каждый кадр | Основная логика |
| `ICleanupSystem` | После всех Execute | Удаление временных компонентов |
| `ReactiveSystem<T>` | При изменении компонента | Реакция на события |

### Шаблон системы

```csharp
public class MovementSystem : IExecuteSystem
{
    private readonly IGroup<GameEntity> _movers;
    private readonly ITimeService _time;

    public MovementSystem(GameContext game, ITimeService time)
    {
        _time = time;
        _movers = game.GetGroup(GameMatcher
            .AllOf(
                GameMatcher.WorldPosition,
                GameMatcher.Direction,
                GameMatcher.Speed,
                GameMatcher.Moving)
            .NoneOf(
                GameMatcher.Destructed));
    }

    public void Execute()
    {
        foreach (var entity in _movers)
        {
            Vector3 delta = entity.Direction.Value * entity.Speed.Value * _time.DeltaTime;
            entity.ReplaceWorldPosition(entity.WorldPosition.Value + delta);
        }
    }
}
```

### Ключевые правила систем
1. **Зависимости через конструктор** — DI внедрит автоматически
2. **Фильтрация через Matcher** — `AllOf`, `AnyOf`, `NoneOf`
3. **Исключать удалённые** — `.NoneOf(Matcher.Destructed)`
4. **Не модифицировать коллекцию при итерации** — используй `.GetEntities()` для копии

---

## 4. Фичи (Features)

Feature — композиция систем, логически объединённых по домену.

```csharp
public sealed class MovementFeature : Feature
{
    public MovementFeature(ISystemFactory systems)
    {
        // Порядок добавления = порядок выполнения
        Add(systems.Create<CalculateDirectionSystem>());
        Add(systems.Create<ApplyMovementSystem>());
        Add(systems.Create<UpdateTransformSystem>());
        Add(systems.Create<CleanupMovementEventsSystem>());
    }
}
```

### Главная Feature (корневая композиция)

```csharp
public sealed class GameplayFeature : Feature
{
    public GameplayFeature(ISystemFactory systems)
    {
        // Инфраструктура
        Add(systems.Create<InputFeature>());
        Add(systems.Create<TimeFeature>());

        // Игровая логика
        Add(systems.Create<PlayerFeature>());
        Add(systems.Create<EnemyFeature>());
        Add(systems.Create<MovementFeature>());
        Add(systems.Create<CombatFeature>());

        // Визуализация
        Add(systems.Create<ViewBindingFeature>());

        // Очистка (всегда в конце)
        Add(systems.Create<DestructionFeature>());
    }
}
```

---

## 5. Фабрики сущностей (Entity Factories)

Фабрики инкапсулируют создание сущностей с нужным набором компонентов.

```csharp
public interface IEnemyFactory
{
    GameEntity Create(EnemyType type, Vector3 position);
}

public class EnemyFactory : IEnemyFactory
{
    private readonly IIdentifierService _ids;
    private readonly IStaticDataService _staticData;

    public EnemyFactory(IIdentifierService ids, IStaticDataService staticData)
    {
        _ids = ids;
        _staticData = staticData;
    }

    public GameEntity Create(EnemyType type, Vector3 position)
    {
        var config = _staticData.ForEnemy(type);

        return CreateEntity.Empty()
            .AddId(_ids.Next())
            .AddWorldPosition(position)
            .AddDirection(Vector3.zero)
            .AddSpeed(config.Speed)
            .AddHealth(config.MaxHealth)
            .AddMaxHealth(config.MaxHealth)
            .AddViewPath(config.PrefabPath)
            .With(x => x.isEnemy = true)
            .With(x => x.isMovementAvailable = true);
    }
}
```

### Хелперы создания сущностей

```csharp
public static class CreateEntity
{
    private static GameContext _game;

    public static void SetContext(GameContext game) => _game = game;

    public static GameEntity Empty() => _game.CreateEntity();
}
```

---

## 6. View Binding (связка ECS ↔ Unity)

Паттерн связывания ECS-сущностей с Unity-объектами.

### Архитектура View Binding

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
                               └────────────┬───────────┘
                                            │
                                            ▼
                               ┌────────────────────────┐
                               │     Registrars         │
                               │ (регистрация Unity-    │
                               │  компонентов в Entity) │
                               └────────────────────────┘
```

### EntityBehaviour (мост между мирами)

```csharp
public class EntityBehaviour : MonoBehaviour
{
    private GameEntity _entity;
    private IEntityComponentRegistrar[] _registrars;

    public GameEntity Entity => _entity;

    public void SetEntity(GameEntity entity)
    {
        _entity = entity;
        _entity.Retain(this);
        _entity.AddView(gameObject);

        _registrars = GetComponentsInChildren<IEntityComponentRegistrar>();
        foreach (var registrar in _registrars)
            registrar.RegisterComponents();
    }

    public void Release()
    {
        foreach (var registrar in _registrars)
            registrar.UnregisterComponents();

        _entity.Release(this);
        _entity = null;
    }
}
```

### Регистратор компонентов

```csharp
public interface IEntityComponentRegistrar
{
    void RegisterComponents();
    void UnregisterComponents();
}

public class TransformRegistrar : MonoBehaviour, IEntityComponentRegistrar
{
    public void RegisterComponents()
    {
        GetComponent<EntityBehaviour>().Entity
            .AddTransformRef(transform);
    }

    public void UnregisterComponents()
    {
        var entity = GetComponent<EntityBehaviour>().Entity;
        if (entity.hasTransformRef)
            entity.RemoveTransformRef();
    }
}
```

---

## 7. Game State Machine

Стейт-машина управляет глобальным состоянием игры.

### Интерфейсы состояний

```csharp
public interface IState
{
    void Enter();
    void Exit();
}

public interface IUpdateableState : IState
{
    void Update();
}

public interface IPayloadState<TPayload> : IState
{
    void Enter(TPayload payload);
}
```

### Типичный flow состояний

```
Bootstrap → LoadProgress → MainMenu → LoadLevel → GameplayLoop → GameOver
                              ↑                         │
                              └─────────────────────────┘
```

### Состояние игрового цикла

```csharp
public class GameplayLoopState : IUpdateableState
{
    private readonly ISystemFactory _systems;
    private Feature _gameplayFeature;

    public GameplayLoopState(ISystemFactory systems)
    {
        _systems = systems;
    }

    public void Enter()
    {
        _gameplayFeature = _systems.Create<GameplayFeature>();
        _gameplayFeature.Initialize();
    }

    public void Update()
    {
        _gameplayFeature.Execute();
        _gameplayFeature.Cleanup();
    }

    public void Exit()
    {
        _gameplayFeature.TearDown();
        _gameplayFeature = null;
    }
}
```

---

## 8. Dependency Injection (Zenject)

### Структура инсталлера

```csharp
public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        BindContexts();
        BindInfrastructure();
        BindServices();
        BindFactories();
        BindStateMachine();
    }

    private void BindContexts()
    {
        var contexts = Contexts.sharedInstance;
        Container.BindInstance(contexts).AsSingle();
        Container.BindInstance(contexts.game).AsSingle();
        Container.BindInstance(contexts.input).AsSingle();
    }

    private void BindServices()
    {
        Container.Bind<ITimeService>().To<TimeService>().AsSingle();
        Container.Bind<IInputService>().To<InputService>().AsSingle();
        Container.Bind<IIdentifierService>().To<IdentifierService>().AsSingle();
    }

    private void BindFactories()
    {
        Container.Bind<ISystemFactory>().To<SystemFactory>().AsSingle();
        Container.Bind<IEntityViewFactory>().To<EntityViewFactory>().AsSingle();
        Container.Bind<IPlayerFactory>().To<PlayerFactory>().AsSingle();
        Container.Bind<IEnemyFactory>().To<EnemyFactory>().AsSingle();
    }
}
```

### SystemFactory

```csharp
public interface ISystemFactory
{
    T Create<T>() where T : ISystem;
}

public class SystemFactory : ISystemFactory
{
    private readonly DiContainer _container;

    public SystemFactory(DiContainer container)
    {
        _container = container;
    }

    public T Create<T>() where T : ISystem
    {
        return _container.Instantiate<T>();
    }
}
```

---

## 9. Индексы для быстрого поиска

Entitas поддерживает индексы для O(1) поиска сущностей.

```csharp
// Определение индекса в компоненте
[Game]
public class EntityId : IComponent
{
    [PrimaryEntityIndex]  // Уникальный ключ
    public int Value;
}

[Game]
public class ParentId : IComponent
{
    [EntityIndex]  // Неуникальный ключ (один-ко-многим)
    public int Value;
}

// Использование
GameEntity entity = gameContext.GetEntityWithEntityId(42);
HashSet<GameEntity> children = gameContext.GetEntitiesWithParentId(parentId);
```

---

## 10. Структура папок проекта

```
Assets/Code/
├── Common/
│   ├── Components/           # Общие компоненты (Id, Destructed, View)
│   ├── Extensions/           # Extension methods
│   └── Helpers/              # CreateEntity хелперы
│
├── Infrastructure/
│   ├── Installers/           # Zenject инсталлеры
│   ├── States/               # Game states и StateMachine
│   ├── Systems/              # SystemFactory
│   ├── Services/             # Time, Input, Identifiers
│   └── View/                 # EntityBehaviour, ViewFactory, Registrars
│
├── Gameplay/
│   ├── RootFeature.cs        # Главная Feature
│   └── Features/
│       └── [FeatureName]/
│           ├── Components/   # [Feature]Components.cs
│           ├── Systems/      # Системы фичи
│           ├── Factory/      # Фабрики сущностей
│           ├── Configs/      # ScriptableObject конфиги
│           └── [Feature]Feature.cs
│
├── Meta/                     # Мета-прогресс, сохранения
│
├── StaticData/               # ScriptableObject конфиги
│
└── Generated/                # Автогенерация Entitas (НЕ РЕДАКТИРОВАТЬ)
```

---

## 11. Чеклист: добавление новой игровой сущности

- [ ] **Компоненты**: создать в `Features/[Name]/Components/`
- [ ] **Regenerate**: перегенерировать код Entitas
- [ ] **Фабрика**: создать интерфейс и реализацию
- [ ] **DI**: зарегистрировать фабрику в инсталлере
- [ ] **Системы**: создать системы обработки
- [ ] **Feature**: создать или дополнить Feature
- [ ] **Root Feature**: подключить в корневую Feature
- [ ] **Префаб**: создать Unity префаб с EntityBehaviour
- [ ] **Registrars**: добавить нужные регистраторы на префаб
- [ ] **Конфиг**: создать ScriptableObject если нужны настройки

---

## 12. Типичные пайплайны

### Пайплайн ввода
```
InputService → EmitInputSystem → ProcessInputSystem → SetDirectionSystem
                (читает Unity Input)  (создаёт Input-entity)  (применяет к игроку)
```

### Пайплайн движения
```
DirectionSystem → MovementSystem → UpdateTransformSystem
  (расчёт направления)  (изменение WorldPosition)  (синхронизация Transform)
```

### Пайплайн уничтожения
```
DeathSystem → MarkDestructedSystem → CleanupViewSystem → DestroyEntitySystem
  (HP <= 0)    (добавляет Destructed)   (Release view)    (entity.Destroy())
```

---

## 13. Лучшие практики

### DO (делай)
- Разделяй данные (компоненты) и логику (системы)
- Используй фабрики для создания сущностей
- Фильтруй `.NoneOf(Destructed)` в системах
- Получай зависимости через конструктор
- Используй индексы для связей между сущностями

### DON'T (не делай)
- Не пиши логику в компонентах
- Не создавай сущности напрямую через `context.CreateEntity()`
- Не храни ссылки на сущности — используй Id
- Не модифицируй коллекцию во время итерации
- Не пиши бизнес-логику в MonoBehaviour
- Не редактируй папку Generated/

---

## 14. Адаптация для 2D/3D

| Аспект | 2D | 3D |
|--------|----|----|
| Position | Vector2 или Vector3 (x, y, 0) | Vector3 (x, y, z) |
| Rotation | float angle или Quaternion | Quaternion |
| Colliders | Collider2D | Collider |
| Physics | Physics2D | Physics |
| Renderer | SpriteRenderer | MeshRenderer / SkinnedMeshRenderer |
| Camera | Orthographic | Perspective |

При переходе между 2D и 3D:
1. Контексты/фичи/циклы остаются без изменений
2. Обновить компоненты позиции/ориентации
3. Заменить коллайдеры и физику
4. Обновить регистраторы под новые типы рендереров
5. Адаптировать системы камеры и спавна
