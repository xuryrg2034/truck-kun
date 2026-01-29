# ECS Structure Refactor — План

> **Цель:** Разнести монолитные Feature-файлы на подпапки по принципам ECS

---

## Текущие проблемы

| Файл | Строк | Содержимое |
|------|-------|------------|
| `PedestrianFeature.cs` | 751 | Enums, Components, Data, Config, Factory, Feature, 4 Systems, Extensions |
| `PhysicsFeature.cs` | 725 | Feature, 8 Systems, Static helper |
| `SurfaceFeature.cs` | 368 | Components, Feature, Systems, Factory |
| `FeedbackFeature.cs` | 362 | Components, Feature, Systems |
| `QuestFeature.cs` | 322 | Components, Feature, Systems |
| `CollisionFeature.cs` | 264 | Components, Feature, Systems |
| `EconomyFeature.cs` | 241 | Components, Feature, Systems |

**Антипаттерн:** Всё в одном файле — сложно навигировать, merge conflicts, нарушает SRP.

---

## Целевая структура (по ECS-паттерну)

```
Features/[FeatureName]/
├── Components/
│   └── [Feature]Components.cs     # Все компоненты фичи
├── Systems/
│   ├── [System1]System.cs         # Каждая система — отдельный файл
│   ├── [System2]System.cs
│   └── ...
├── [Feature]Feature.cs            # Только Feature-класс (композиция систем)
├── (опционально)
├── Factory/
│   └── [Feature]Factory.cs        # Фабрика сущностей
├── Data/
│   └── [Feature]Data.cs           # Data-классы, Enums
└── Extensions/
    └── [Feature]Extensions.cs     # Extension methods
```

---

## План рефакторинга

### Phase 1: Physics (725 строк → 10 файлов)

**Текущий:** `Features/Physics/PhysicsFeature.cs` + `PhysicsComponents.cs`

**Целевая структура:**
```
Features/Physics/
├── Components/
│   └── PhysicsComponents.cs           # ✅ Уже есть
├── Systems/
│   ├── DebugPhysicsEntitiesSystem.cs  # УДАЛИТЬ (debug)
│   ├── ReadInputForPhysicsSystem.cs
│   ├── CalculatePhysicsVelocitySystem.cs
│   ├── ApplySurfaceModifiersSystem.cs
│   ├── ClampPhysicsVelocitySystem.cs
│   ├── ApplyPhysicsVelocitySystem.cs
│   ├── SyncPhysicsPositionSystem.cs
│   └── UpdatePhysicsStateSystem.cs
├── PhysicsFeature.cs                  # Только Feature-класс
└── SurfaceZoneHandler.cs              # Static helper
```

**Задачи:**
1. Создать папку `Systems/`
2. Вынести каждую систему в отдельный файл
3. Удалить `DebugPhysicsEntitiesSystem` (debug код)
4. Оставить в `PhysicsFeature.cs` только Feature-класс
5. Вынести `SurfaceZoneHandler` в отдельный файл

---

### Phase 2: Pedestrian (751 строк → 12 файлов)

**Текущий:** `Features/Pedestrian/PedestrianFeature.cs`

**Целевая структура:**
```
Features/Pedestrian/
├── Components/
│   └── PedestrianComponents.cs        # Pedestrian, PedestrianType, CrossingPedestrian
├── Data/
│   ├── PedestrianEnums.cs             # PedestrianKind, PedestrianCategory
│   ├── PedestrianVisualData.cs        # PedestrianVisualData, PedestrianSpawnWeight
│   └── PedestrianConfig.cs            # ScriptableObject (→ переместить в Configs/)
├── Factory/
│   └── PedestrianFactory.cs           # IPedestrianFactory, PedestrianFactory
├── Systems/
│   ├── PedestrianSpawnSystem.cs
│   ├── PedestrianCrossingSystem.cs    # Не используется, но оставить
│   ├── PedestrianDespawnSystem.cs
│   └── DisableAnimationOnHitSystem.cs # Из Art/Animation?
├── Extensions/
│   └── PedestrianKindExtensions.cs
├── PedestrianFeature.cs               # Только Feature-класс
└── PedestrianSettings.cs              # ✅ Уже есть
```

**Задачи:**
1. Создать подпапки `Components/`, `Data/`, `Factory/`, `Systems/`, `Extensions/`
2. Вынести компоненты в `Components/PedestrianComponents.cs`
3. Вынести Enums и Data-классы в `Data/`
4. Переместить `PedestrianConfig` в `Configs/Pedestrian/` (лучше вместе с другими конфигами)
5. Вынести фабрику в `Factory/`
6. Вынести системы по файлам
7. Вынести extensions
8. Оставить в `PedestrianFeature.cs` только Feature-класс

---

### Phase 3: Остальные Features

#### Surface (368 строк)
```
Features/Surface/
├── Components/
│   └── SurfaceComponents.cs
├── Systems/
│   ├── SurfaceSpawnSystem.cs
│   ├── SurfaceDespawnSystem.cs
│   └── SurfaceFactory.cs              # Или в Factory/
├── SurfaceFeature.cs
└── SurfaceTrigger.cs                  # ✅ Уже отдельно
```

#### Feedback (362 строк)
```
Features/Feedback/
├── Components/
│   └── FeedbackComponents.cs
├── Systems/
│   ├── HitFeedbackSystem.cs
│   ├── ParticleSpawnSystem.cs
│   └── ...
└── FeedbackFeature.cs
```

#### Quest (322 строк)
```
Features/Quest/
├── Components/
│   └── QuestComponents.cs
├── Systems/
│   ├── QuestTrackingSystem.cs
│   ├── QuestCompletionSystem.cs
│   └── ...
└── QuestFeature.cs
```

#### Collision (264 строк)
```
Features/Collision/
├── Components/
│   └── CollisionComponents.cs
├── Systems/
│   └── ...
└── CollisionFeature.cs
```

#### Economy (241 строк)
```
Features/Economy/
├── Components/
│   └── EconomyComponents.cs
├── Systems/
│   └── ...
├── EconomyFeature.cs
└── DaySession.cs                      # ✅ Уже отдельно
```

---

## Configs — отдельная структура

Все ScriptableObjects переместить в единое место:

```
Configs/
├── Global/
│   ├── EconomyConfig.cs               # ✅ Уже есть
│   └── FeedbackConfig.cs              # ✅ Уже есть
├── Spawning/
│   ├── PedestrianSpawnConfig.cs       # ✅ Уже есть
│   ├── SurfaceSpawnConfig.cs          # ✅ Уже есть
│   └── ObstacleSpawnConfig.cs         # ✅ Уже есть
├── Pedestrian/
│   └── PedestrianConfig.cs            # ← Переместить из PedestrianFeature.cs
├── VehicleConfig.cs                   # ✅ Уже есть
└── LevelConfig.cs                     # ✅ Уже есть
```

---

## Порядок выполнения

| # | Фаза | Приоритет | Файлов затронуто |
|---|------|-----------|------------------|
| 1 | Physics | Высокий | 9 новых файлов |
| 2 | Pedestrian | Высокий | 12 новых файлов |
| 3 | Surface | Средний | 5 новых файлов |
| 4 | Feedback | Средний | 4+ новых файлов |
| 5 | Quest | Низкий | 4+ новых файлов |
| 6 | Collision | Низкий | 3+ новых файлов |
| 7 | Economy | Низкий | 3+ новых файлов |

---

## Namespace конвенция

```csharp
// Components
namespace Code.Gameplay.Features.Physics.Components

// Systems
namespace Code.Gameplay.Features.Physics.Systems

// Feature
namespace Code.Gameplay.Features.Physics

// Data/Enums
namespace Code.Gameplay.Features.Pedestrian.Data

// Factory
namespace Code.Gameplay.Features.Pedestrian.Factory
```

---

## Checklist после рефакторинга

- [ ] Все системы в отдельных файлах
- [ ] Компоненты в `Components/` подпапке
- [ ] Feature-класс содержит только композицию систем
- [ ] Namespaces соответствуют структуре папок
- [ ] Удалены debug-системы
- [ ] Unity компилируется без ошибок
- [ ] Jenny перегенерирован (если менялись компоненты)

---

## Примечания

- **НЕ менять логику** — только структуру файлов
- **Сохранять git history** — использовать `git mv` где возможно
- **Обновлять CHANGELOG** после каждой фазы
