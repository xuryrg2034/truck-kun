# Truck-kun Rising - Architecture

> Архитектурные решения и паттерны проекта
> Обновляй при изменении архитектуры или добавлении слоёв

---

## Визуальная схема слоёв

```
┌─────────────────────────────────────────────────────────────────┐
│                         UNITY LAYER                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │   Scenes    │  │   Prefabs   │  │     MonoBehaviours      │  │
│  │  MainMenu   │  │ PlayerTruck │  │  CameraFollow           │  │
│  │    Hub      │  │ Pedestrians │  │  EntityBehaviour        │  │
│  │   Battle    │  │             │  │  EcsBootstrap           │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                      INFRASTRUCTURE                              │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │                    Zenject Container                     │    │
│  │  ┌───────────┐  ┌───────────┐  ┌───────────────────┐   │    │
│  │  │ Services  │  │ Factories │  │    Settings       │   │    │
│  │  │ Time      │  │ System    │  │ GameBalance       │   │    │
│  │  │ Identifier│  │ Pedestrian│  │ MovementSettings  │   │    │
│  │  │ Audio     │  │           │  │ SpawnSettings     │   │    │
│  │  └───────────┘  └───────────┘  └───────────────────┘   │    │
│  └─────────────────────────────────────────────────────────┘    │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
┌─────────────────────────────────────────────────────────────────┐
│                         ECS LAYER (Entitas)                      │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                      CONTEXTS                               │ │
│  │  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │ │
│  │  │ GameContext  │  │ InputContext │  │ MetaContext  │     │ │
│  │  │  - Hero      │  │  - MoveInput │  │  - Upgrades  │     │ │
│  │  │  - Pedestrian│  │              │  │  - Progress  │     │ │
│  │  │  - Position  │  │              │  │              │     │ │
│  │  │  - Hit       │  │              │  │              │     │ │
│  │  └──────────────┘  └──────────────┘  └──────────────┘     │ │
│  └────────────────────────────────────────────────────────────┘ │
│                                                                  │
│  ┌────────────────────────────────────────────────────────────┐ │
│  │                  FEATURES (System Groups)                   │ │
│  │                                                              │ │
│  │  BattleFeature (главная Feature, определяет порядок)        │ │
│  │  ┌────────────────────────────────────────────────────┐    │ │
│  │  │ 1. InputFeature      - чтение ввода               │    │ │
│  │  │ 2. HeroFeature       - движение героя             │    │ │
│  │  │ 3. PedestrianFeature - NPC логика                 │    │ │
│  │  │ 4. CollisionFeature  - обнаружение столкновений   │    │ │
│  │  │ 5. FeedbackFeature   - визуальный feedback        │    │ │
│  │  │ 6. QuestFeature      - прогресс квестов           │    │ │
│  │  │ 7. EconomyFeature    - экономика                  │    │ │
│  │  │ 8. BindViewFeature   - привязка View              │    │ │
│  │  │ 9. MovementFeature   - применение движения        │    │ │
│  │  └────────────────────────────────────────────────────┘    │ │
│  └────────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

---

## Data Flow (Поток данных)

### Основной игровой цикл

```
┌──────────────┐
│ Unity Update │
└──────┬───────┘
       │
       ▼
┌──────────────────────────────────────────────────────────────┐
│                    InputFeature                               │
│  Input.GetAxis() → InputContext.moveInput = new Vector2()    │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                    HeroFeature                                │
│  MoveInput → вычисление направления → MoveDirection          │
│  MoveDirection + Speed → WorldPosition                        │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                  PedestrianFeature                            │
│  Timer → Spawn NPC → WorldPosition + PedestrianType          │
│  CrossingPedestrian → обновление X позиции                    │
│  Distance check → Despawn далёких NPC                         │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                  CollisionFeature                             │
│  Hero.WorldPosition vs Pedestrians.WorldPosition              │
│  Distance < HitRadius → entity.isHit = true                   │
│  → CreateEntity().AddHitEvent(type, id)                       │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                  FeedbackFeature                              │
│  HitEvent (ReactiveSystem) → SpawnParticles()                 │
│                            → PlaySound()                       │
│                            → SpawnFloatingText()               │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│            QuestFeature + EconomyFeature                      │
│  HitEvent → UpdateQuestProgress() if matching type            │
│  HitEvent → ApplyPenalty() if protected type                  │
│  HitEvent → CompleteQuest() → AddReward()                     │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                  BindViewFeature                              │
│  Entity + ViewPrefab → Instantiate → AddView                  │
│  WorldPosition → Transform.position sync                       │
└──────────────────────────────┬───────────────────────────────┘
                               │
                               ▼
┌──────────────────────────────────────────────────────────────┐
│                  CleanupSystems                               │
│  Destroy hit entities with views                              │
│  Remove HitEvent entities                                     │
└──────────────────────────────────────────────────────────────┘
```

---

## Описание слоёв

### Unity Layer
- **Сцены**: MainMenu, Hub, Battle - точки входа
- **Префабы**: визуальные представления сущностей
- **MonoBehaviours**: мосты между Unity и ECS

### Infrastructure Layer
- **Zenject**: управление зависимостями
- **Services**: stateless утилиты (время, ID, аудио)
- **Factories**: создание сложных объектов
- **Settings**: конфигурация из ScriptableObject

### ECS Layer
- **Contexts**: хранилища сущностей по доменам
- **Components**: чистые данные без логики
- **Systems**: логика обработки данных
- **Features**: группировка связанных систем

---

## Правила взаимодействия

### Между слоями

```
MonoBehaviour → ECS:
  - Только через EcsBootstrap.Initialize()
  - Или через Services (ITimeService.DeltaTime)

ECS → MonoBehaviour:
  - Через View компонент (IEntityView интерфейс)
  - EntityBehaviour как обёртка

ECS Systems между собой:
  - Через Components (данные)
  - Через Event Entities (HitEvent, ViolationEvent)
  - Через Groups и Collectors
```

### Внутри ECS

```
Feature A → Feature B:
  - Через shared components
  - Через event entities
  - НИКОГДА напрямую между системами

System → System (в одной Feature):
  - Через порядок в Feature
  - Предыдущая система изменяет данные
  - Следующая читает изменённые
```

---

## Архитектурные решения

### Почему Entitas, а не Unity ECS (DOTS)?

| Критерий | Entitas | Unity DOTS |
|----------|---------|------------|
| Простота | Высокая | Низкая |
| Документация | Хорошая | Меняется |
| Гибкость | Высокая | Ограничена |
| Производительность | Достаточная | Максимальная |

**Решение**: Entitas для простоты разработки. Проект не требует максимальной производительности.

### Почему Custom TweenSystem, а не DOTween?

| Критерий | Custom | DOTween |
|----------|--------|---------|
| Размер | ~400 строк | Большой |
| Контроль | Полный | Частичный |
| Зависимости | Нет | Есть |
| Функционал | Базовый | Полный |

**Решение**: Custom для простых анимаций. DOTween установлен как fallback для сложных случаев.

### Почему процедурная генерация моделей?

- Нет необходимости в художнике
- Быстрое прототипирование
- Легко менять визуал через код
- Позже можно заменить на готовые модели

---

## Антипаттерны (что НЕ делать)

### ❌ Прямой доступ между системами

```csharp
// ПЛОХО
public class SystemA : IExecuteSystem
{
    private SystemB _systemB; // НЕТ!
}

// ХОРОШО - через компоненты
entity.AddSomeFlag(); // SystemB реагирует через ReactiveSystem
```

### ❌ Логика в компонентах

```csharp
// ПЛОХО
[Game] public class Health : IComponent
{
    public float Value;
    public void TakeDamage(float amount) { Value -= amount; } // НЕТ!
}

// ХОРОШО - логика в системах
public class DamageSystem : IExecuteSystem
{
    public void Execute()
    {
        entity.ReplaceHealth(entity.health.Value - damage);
    }
}
```

### ❌ MonoBehaviour логика вместо ECS

```csharp
// ПЛОХО
public class Pedestrian : MonoBehaviour
{
    void Update() { Move(); CheckCollision(); } // НЕТ!
}

// ХОРОШО - MonoBehaviour только как View
public class PedestrianView : MonoBehaviour, IEntityView
{
    public GameEntity Entity { get; private set; }
    // Только визуал, никакой логики
}
```

### ❌ Создание сущностей в Update без контроля

```csharp
// ПЛОХО
public void Execute()
{
    _context.CreateEntity().AddSomething(); // Каждый кадр!
}

// ХОРОШО - с таймером или условием
if (_cooldown <= 0 && _entities.count < _maxCount)
{
    _context.CreateEntity().AddSomething();
    _cooldown = _interval;
}
```

### ❌ Забывать очищать event-сущности

```csharp
// ПЛОХО - события накапливаются
// (нет CleanupSystem)

// ХОРОШО - всегда CleanupSystem для событий
public class CleanupHitEventsSystem : ICleanupSystem
{
    public void Cleanup()
    {
        foreach (var entity in _hitEvents.GetEntities())
            entity.Destroy();
    }
}
```

---

## Расширение архитектуры

### Добавление новой Feature

1. Создать файл `NewFeature.cs` в `Gameplay/`
2. Определить компоненты в `#region Components`
3. Создать системы в `#region Systems`
4. Объединить в `public sealed class NewFeature : Feature`
5. Добавить в `BattleFeature` в правильном порядке
6. Зарегистрировать зависимости в Zenject Installer

### Добавление нового типа пешехода

1. Добавить в `PedestrianKind` enum
2. Добавить визуальные данные в `PedestrianVisualData.Default()`
3. Добавить вес спавна в `PedestrianConfig`
4. Добавить баланс в `GameBalance.Pedestrians.TypeBalances`
5. Опционально: добавить процедурную модель в `ProceduralMeshGenerator`

### Добавление нового VFX

1. Создать класс в `Art/VFX/`
2. Если нужен сервис - добавить интерфейс и реализацию
3. Зарегистрировать в Zenject
4. Вызывать из соответствующей системы (обычно FeedbackSystem)
