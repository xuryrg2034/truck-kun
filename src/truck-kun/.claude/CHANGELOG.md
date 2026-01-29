# Changelog - Truck-kun Rising

> Журнал изменений проекта
> **ОБНОВЛЯЙ ПОСЛЕ КАЖДОГО ИЗМЕНЕНИЯ КОДА!**

---

## Формат записи

```markdown
## YYYY-MM-DD HH:MM - [Краткое описание]

**Файлы:**
- `путь/к/файлу.cs` - что изменено

**Причина:** Зачем это сделано
**Детали:** Важные нюансы реализации
```

---

## 2026-01-29 - Миграция документации в Obsidian

**Создан Obsidian vault для графовой документации проекта.**

**Структура:**
```
C:\Projects\home\unity\Truck-kun\obsidian\
├── 00-Index/
│   ├── Home.md              # Главная навигация
│   └── Claude-Instructions.md # Инструкции для AI
├── 01-Architecture/
│   ├── ECS-Pattern.md
│   ├── Contexts.md
│   └── View-Binding.md
├── 02-Features/
│   ├── Physics.md
│   ├── Pedestrian.md
│   ├── Collision.md
│   ├── Economy.md
│   ├── Quest.md
│   ├── Feedback.md
│   └── Surface.md
├── 03-Services/
│   ├── MoneyService.md
│   ├── QuestService.md
│   ├── HitEffectService.md
│   └── UpgradeService.md
├── 04-Game-Flow/
│   ├── Game-Loop.md
│   ├── Player-Progression.md
│   └── Economy-Flow.md
└── 05-Configs/
    ├── VehicleConfig.md
    ├── LevelConfig.md
    └── PedestrianConfig.md
```

**Причина:** Визуализация архитектуры через граф связей Obsidian
**Детали:**
- Все заметки связаны через [[wikilinks]]
- Инструкции для Claude по поддержке графа
- CLAUDE.md обновлён с ссылкой на vault

---

## 2026-01-29 - Рефакторинг всех Features (ECS-структура)

**Завершён полный рефакторинг Gameplay Features по стандартной ECS-структуре.**

### Surface (368 → 26 строк Feature)
```
Features/Surface/
├── Data/SurfaceSpawnSettings.cs
├── Factory/SurfaceFactory.cs
├── Systems/SurfaceSpawnSystem.cs
└── SurfaceFeature.cs
```

### Feedback (362 → 14 строк Feature)
```
Features/Feedback/
├── Services/
│   ├── HitEffectService.cs
│   └── FloatingTextService.cs
├── Systems/
│   ├── HitFeedbackSystem.cs
│   └── FloatingTextUpdateSystem.cs
└── FeedbackFeature.cs
```

### Quest (322 → 16 строк Feature)
```
Features/Quest/
├── Components/QuestComponents.cs
├── Services/QuestService.cs
├── Systems/QuestSystems.cs
└── QuestFeature.cs
```

### Collision (264 → 15 строк Feature)
```
Features/Collision/
├── Components/CollisionComponents.cs
├── Data/CollisionSettings.cs
├── Systems/CollisionSystems.cs
└── CollisionFeature.cs
```

### Economy (241 → 15 строк Feature)
```
Features/Economy/
├── Components/EconomyComponents.cs
├── Services/MoneyService.cs
├── Systems/EconomySystems.cs
└── EconomyFeature.cs
```

**Причина:** Приведение к стандартной ECS-структуре
**Детали:**
- Обновлён GameplaySceneInstaller с новыми namespaces
- Сервисы вынесены в подпапки Services/
- Компоненты в Components/, системы в Systems/

---

## 2026-01-29 - Рефакторинг PedestrianFeature (ECS-структура)

**Файлы:**

### Структура до:
```
Features/Pedestrian/
├── PedestrianFeature.cs   (751 строк - всё в одном файле)
├── PedestrianSettings.cs
├── PedestrianPhysicsSettings.cs
└── Systems/
    └── PedestrianForceMovementSystem.cs
```

### Структура после:
```
Features/Pedestrian/
├── Components/
│   └── PedestrianComponents.cs
├── Data/
│   ├── PedestrianEnums.cs
│   └── PedestrianVisualData.cs
├── Factory/
│   └── PedestrianFactory.cs
├── Systems/
│   ├── PedestrianSpawnSystem.cs
│   ├── PedestrianCrossingSystem.cs
│   ├── PedestrianDespawnSystem.cs
│   └── PedestrianForceMovementSystem.cs
├── Extensions/
│   └── PedestrianKindExtensions.cs
├── PedestrianFeature.cs   (28 строк)
├── PedestrianSettings.cs
└── PedestrianPhysicsSettings.cs

Configs/Pedestrian/
└── PedestrianConfig.cs    (перемещён из Feature)
```

**Причина:** Приведение к стандартной ECS-структуре
**Детали:**
- PedestrianConfig перемещён в Configs/Pedestrian/ (namespace: Code.Configs.Pedestrian)
- Обновлён GameplaySceneInstaller с новыми usings
- PedestrianCrossingSystem оставлен (не используется, но может понадобиться)

---

## 2026-01-29 - Рефакторинг PhysicsFeature (ECS-структура)

**Файлы:**

### Структура до:
```
Features/Physics/
├── PhysicsFeature.cs      (725 строк - Feature + 8 систем + helper)
└── PhysicsComponents.cs
```

### Структура после:
```
Features/Physics/
├── Components/
│   └── PhysicsComponents.cs
├── Systems/
│   ├── ReadInputForPhysicsSystem.cs
│   ├── CalculatePhysicsVelocitySystem.cs
│   ├── ApplySurfaceModifiersSystem.cs
│   ├── ClampPhysicsVelocitySystem.cs
│   ├── ApplyPhysicsVelocitySystem.cs
│   ├── SyncPhysicsPositionSystem.cs
│   └── UpdatePhysicsStateSystem.cs
├── PhysicsFeature.cs      (52 строки - только Feature)
└── SurfaceZoneHandler.cs
```

**Причина:** Приведение к стандартной ECS-структуре (каждая система — отдельный файл)
**Детали:**
- Удалён DebugPhysicsEntitiesSystem (debug код)
- Удалены Debug.Log вызовы из систем
- PhysicsFeature теперь содержит только композицию систем
- Namespace остался `Code.Gameplay.Features.Physics` для совместимости

---

## 2026-01-29 - Очистка Editor скриптов

**Файлы:**
- `Assets/Code/Editor/PhysicsBalanceValidator.cs` - удалён
- `Assets/Code/Editor/MeshSaver.cs` - удалён
- `Assets/Code/Editor/SceneCreator.cs` - удалён

**Причина:** Устаревшие/одноразовые утилиты
**Детали:**
- PhysicsBalanceValidator — хардкод значений, не соответствующих VehicleConfig
- MeshSaver — процедурные меши уже сохранены как ассеты
- SceneCreator — сцены уже созданы

---

## 2026-01-29 - VehicleConfig: единый конфиг транспорта

**Файлы:**

### Создано
- `Assets/Code/Configs/VehicleConfig.cs` - ScriptableObject с параметрами транспорта (масса, скорость, ускорение)
- `Assets/Code/Gameplay/Features/Hero/VehicleStats.cs` - immutable struct с применёнными множителями

### Изменено
- `Assets/Code/Configs/LevelConfig.cs` - добавлено поле VehicleConfig
- `Assets/Code/Gameplay/Features/Hero/HeroFeature.cs` - HeroFactory принимает VehicleStats
- `Assets/Code/Gameplay/Features/Physics/PhysicsFeature.cs` - CalculatePhysicsVelocitySystem использует ECS компоненты
- `Assets/Code/Infrastructure/Bootstrap/EcsBootstrap.cs` - создаёт VehicleStats с множителями
- `Assets/Code/Infrastructure/Installers/GameplaySceneInstaller.cs` - биндинги VehicleConfig
- `Assets/Code/Infrastructure/View/EntityBehaviour.cs` - конфигурирует Rigidbody из VehicleConfig
- `Assets/Code/Meta/UpgradeSystem.cs` - добавлены GetSpeedMultiplier/GetLateralMultiplier

### Удалено
- `Assets/Code/Gameplay/Features/Hero/HeroSettings.cs` - legacy, дублировал настройки
- `Assets/Code/Art/VFX/VehicleEffects.cs` - неиспользуемый компонент
- `Assets/Code/Debug/DebugPhysicsUI.cs` - зависел от удалённого RunnerMovementSettings
- `Assets/Code/Editor/RunnerMovementSettingsEditor.cs` - legacy editor

**Причина:** Устранение дублирования настроек транспорта. Раньше настройки были в 3 местах.
**Детали:**
- VehicleConfig содержит базовые значения из ScriptableObject
- VehicleStats создаётся в EcsBootstrap с применёнными множителями (апгрейды, сложность)
- RunnerMovementSettings и IHeroSpawnPoint полностью удалены
- Границы дороги теперь физические объекты (centerX=0)
- PedestrianSpawnSystem больше не зависит от IHeroSpawnPoint
- Удалена папка Assets/Code/Debug/ (DebugService, DebugPhysicsController, DebugPhysicsVisualizer, DebugPhysicsUI)
- Удалены ссылки на debug классы из EconomyFeature и PhysicsCollisionHandler

---

## 2026-01-29 - Завершение миграции на модульные конфиги

**Файлы:**
- `.claude/CLAUDE.md` - исправлена ссылка на удалённый GameBalance.cs → LevelConfig.cs

**Причина:** GameBalance.cs удалён, конфигурация теперь через модульные ScriptableObjects
**Детали:**
- LevelConfig агрегирует: EconomyConfig, FeedbackConfig, DayConfig, PedestrianSpawnConfig, QuestPoolConfig
- Все конфиги обязательны, [InjectOptional] удалён

---

## 2026-01-28 13:35 - Рефакторинг структуры проекта

**Файлы:**
### Документация
- `.claude/CLAUDE.md` - упрощён, ссылка на ECS_PATTERN.md, обновлён Quick Reference
- `.claude/ECS_PATTERN.md` - восстановлен (универсальный шаблон архитектуры)

### Перемещено (Gameplay/Features/)
- `InputFeature.cs` → `Features/Input/`
- `HeroFeature.cs` → `Features/Hero/`
- `PedestrianFeature.cs` → `Features/Pedestrian/`
- `PhysicsFeature.cs`, `PhysicsComponents.cs` → `Features/Physics/`
- `CollisionFeature.cs` → `Features/Collision/`
- `RagdollFeature.cs` → `Features/Ragdoll/`
- `FeedbackSystem.cs` → `Features/Feedback/FeedbackFeature.cs` (переименован)
- `QuestFeature.cs` → `Features/Quest/`
- `EconomyFeature.cs`, `DaySession.cs` → `Features/Economy/`
- `MovementFeature.cs` → `Features/Movement/`
- `SurfaceFeature.cs`, `SurfaceTrigger.cs` → `Features/Surface/`

### Перемещено (Common/)
- `CommonComponents.cs` → `Common/Components/`
- `Services.cs` → `Common/Services/`
- `EntityHelpers.cs` → `Common/Extensions/`

### Перемещено (Infrastructure/)
- `EcsBootstrap.cs` → `Infrastructure/Bootstrap/`
- `SystemFactory.cs`, `Feature.cs` → `Infrastructure/Systems/`
- `EntityBehaviour.cs`, `ViewSystems.cs`, `PhysicsCollisionHandler.cs` → `Infrastructure/View/`
- `CameraFollow.cs` → `Art/VFX/`

### Перемещено (Art/)
- `ModelFactory.cs`, `ProceduralMeshGenerator.cs` → `Art/Procedural/`

### Обновлённые namespaces
- `Code.Gameplay.Input` → `Code.Gameplay.Features.Input`
- `Code.Gameplay` (DaySession) → `Code.Gameplay.Features.Economy`
- `Code.Common` → разделён на `Code.Common.Services`, `Code.Common.Components`, `Code.Common.Extensions`
- `Code.Infrastructure` (EcsBootstrap) → `Code.Infrastructure.Bootstrap`
- `Code.Infrastructure.Systems` (Feature.cs) - добавлен namespace
- `Code.Art` → `Code.Art.Procedural`
- `Code.Infrastructure` (CameraFollow) → `Code.Art.VFX`

**Причина:** Приведение структуры проекта в соответствие с ECS_PATTERN.md
**Детали:**
- Feature-based организация кода для масштабируемости
- Каждая Feature в своей папке со всеми связанными файлами
- Чёткое разделение слоёв: Common, Infrastructure, Gameplay, Art

---

## 2026-01-28 11:40 - Реорганизация документации .claude/

**Файлы:**
- `.claude/CLAUDE.md` - полностью переписан, объединяет всю документацию
- `AGENTS.md` - удалён (содержимое перенесено в CLAUDE.md)
- `.claude/ARCHITECTURE.md` - удалён (содержимое в CLAUDE.md)
- `.claude/CONTEXT.md` - удалён (содержимое в CLAUDE.md)
- `.claude/CLAUDE_INSTRUCTIONS.md` - удалён (содержимое в CLAUDE.md)
- `.claude/ECS_PATTERN.md` - удалён (содержимое в CLAUDE.md)
- `.claude/SESSION_NOTES.md` - удалён (заменяется встроенной памятью Claude Code)
- `.claude/TODO.md` - удалён (TODO теперь секция в CLAUDE.md)

**Причина:** Устранение дублирования и упрощение структуры документации
**Детали:**
- Было 9 файлов → стало 2 (CLAUDE.md + CHANGELOG.md)
- CLAUDE.md теперь содержит: Quick Reference, Архитектуру, Структуру кода, Правила, Чеклисты, Антипаттерны, TODO
- Информация не потеряна, только консолидирована

---

## 2026-01-27 22:15 - Система поверхностей (Oil, Grass, Ice)

**Файлы:**
- `Assets/Code/Gameplay/SurfaceTrigger.cs` - создан
- `Assets/Code/Gameplay/SurfaceFeature.cs` - создан
- `Assets/Code/Gameplay/PhysicsComponents.cs` - добавлен OnSurface
- `Assets/Code/Gameplay/PhysicsFeature.cs` - улучшен ApplySurfaceModifiersSystem
- `Assets/Code/Gameplay/BattleFeature.cs` - добавлен SurfaceFeature
- `Assets/Code/Infrastructure/EcsBootstrap.cs` - добавлены SurfaceSpawnSettings

**Причина:** Разнообразие геймплея через опасные поверхности
**Детали:**

SurfaceTrigger.cs:
- MonoBehaviour с OnTriggerEnter/Exit
- Находит Hero через EntityBehaviour
- Применяет SurfaceModifier к entity
- Отслеживание текущей поверхности через OnSurface компонент
- Визуальные эффекты (particles) при входе

SurfaceFeature.cs:
- `SurfaceSpawnSystem` - спавн поверхностей впереди героя
- `SurfaceDespawnSystem` - удаление позади героя
- `SurfaceFactory` - создание визуала и коллайдеров
- Weighted random для типов: Oil, Grass, Puddle, Ice

Типы поверхностей:
| Type   | Friction | Drag | Effect                    |
|--------|----------|------|---------------------------|
| Normal | 1.0      | 1.0  | Стандартный               |
| Oil    | 0.3      | 0.5  | Скользко, скорость та же  |
| Grass  | 0.8      | 1.8  | Замедление                |
| Ice    | 0.15     | 0.3  | Очень скользко            |
| Puddle | 0.85     | 1.3  | Немного скользко          |

ApplySurfaceModifiersSystem:
- Sliding эффект для низкого friction
- Drag влияет на forward speed
- Random drift на очень скользких поверхностях

**Требуется:** Запустить Jenny для генерации OnSurface

---

## 2026-01-27 21:30 - Переход на физические коллизии

**Файлы:**
- `Assets/Code/Infrastructure/PhysicsCollisionHandler.cs` - создан
- `Assets/Code/Gameplay/CollisionFeature.cs` - переработан
- `Assets/Code/Infrastructure/EntityBehaviour.cs` - добавлен SetupCollisionHandler
- `Assets/Code/Gameplay/PedestrianFeature.cs` - добавлен kinematic Rigidbody
- `Assets/Code/Infrastructure/EcsBootstrap.cs` - настройки физических коллизий

**Причина:** Переход с distance check на Unity OnCollisionEnter
**Детали:**

PhysicsCollisionHandler.cs:
- MonoBehaviour для обработки OnCollisionEnter
- Находит GameEntity пешехода через EntityBehaviour
- Создаёт HitEvent с CollisionImpact данными
- Автоматически добавляется на Hero в EntityBehaviour

CollisionFeature.cs:
- Новый компонент `CollisionImpact { Force, Point, Normal }`
- `FallbackCollisionDetectionSystem` - только для non-physics heroes
- `CollisionSettings.UsePhysicsCollision` - флаг переключения
- Extension methods для VFX scaling

PedestrianFeature.cs:
- Пешеходы получают kinematic Rigidbody
- Explicit `isTrigger = false` на коллайдере

**Требуется:** Запустить Jenny для генерации CollisionImpact

---

## 2026-01-27 20:15 - Интеграция PhysicsFeature в BattleFeature

**Файлы:**
- `Assets/Code/Gameplay/BattleFeature.cs` - полностью переписан
- `Assets/Code/Gameplay/MovementFeature.cs` - обновлён DirectionalDeltaMoveSystem
- `Assets/Code/Infrastructure/EcsBootstrap.cs` - упрощён, убран отдельный _physicsFeature

**Причина:** Интеграция физической системы в основную архитектуру
**Детали:**

BattleFeature.cs:
- Добавлен `_physicsFeature` как внутренний член
- `Initialize()` - инициализирует и physics
- `Execute()` - выполняет Update-системы (логика)
- `FixedExecute()` - выполняет FixedUpdate-системы (физика)
- `Cleanup()` - очищает обе системы
- `TearDown()` - корректно уничтожает обе системы

MovementFeature.cs:
- `DirectionalDeltaMoveSystem` теперь исключает:
  - `Hero` (у героя своя система движения)
  - `Rigidbody` (физика управляется PhysicsFeature)
- Система теперь только для NPC (пешеходы и т.д.)

EcsBootstrap.cs:
- Удалён отдельный `_physicsFeature`
- `FixedUpdate()` вызывает `_battleFeature.FixedExecute()`
- Упрощён `OnDestroy()`

Порядок выполнения:
```
Update:      Input → Hero → Pedestrian → Collision → Feedback → Quest → Economy → BindView → Movement
FixedUpdate: Physics (ReadInput → CalcVelocity → Surface → Clamp → Apply → Sync → State)
```

---

## 2026-01-27 19:30 - Проверка и подтверждение PhysicsFeature

**Файлы:**
- `Assets/Code/Gameplay/PhysicsFeature.cs` - проверен, код корректен
- `Assets/Code/Gameplay/PhysicsComponents.cs` - проверен, все компоненты правильные
- `Assets/Code/Generated/Game/Components/` - Jenny сгенерировал все 9 физических компонентов

**Причина:** Проверка после проблем с генерацией Jenny
**Детали:**
- Все компоненты успешно сгенерированы Jenny
- Используется правильный API: `GameMatcher.Rigidbody` (не `RigidbodyComponent`)
- Используется `rb.linearVelocity` (Unity 6+ API)
- PhysicsFeature выполняется в `FixedUpdate()` для стабильной физики

---

## 2026-01-27 16:02 - Удалено дублирующее присваивание группы героев

**Файлы:**
- `Assets/Code/Gameplay/HeroFeature.cs` - удалено повторное создание `_heroes` в `RunnerHeroMoveSystem`

**Причина:** Устранение дублирующей логики.
**Детали:** Оставлено единственное корректное создание группы с `.NoneOf(GameMatcher.Rigidbody)`.

---

## 2026-01-27 15:58 - Исправление matcher для Rigidbody

**Файлы:**
- `Assets/Code/Gameplay/HeroFeature.cs` - заменён `GameMatcher.RigidbodyComponent` на `GameMatcher.Rigidbody`

**Причина:** Исправить ошибку компиляции CS0117 из-за неверного имени matcher.
**Детали:** Entitas отбрасывает суффикс `Component` в имени matcher.

---

## 2026-01-27 18:15 - Создание PhysicsFeature с системами движения

**Файлы:**
- `Assets/Code/Gameplay/PhysicsFeature.cs` - создан
- `Assets/Code/Common/Services.cs` - обновлён (добавлен FixedDeltaTime)
- `Assets/Code/Infrastructure/EcsBootstrap.cs` - обновлён (FixedUpdate для физики)

**Причина:** Реализация гибридной физической системы движения
**Детали:**

PhysicsFeature.cs содержит 7 систем:
1. `ReadInputForPhysicsSystem` - чтение lateral input
2. `CalculatePhysicsVelocitySystem` - расчёт velocity с ускорением
3. `ApplySurfaceModifiersSystem` - модификаторы поверхности (friction, drag)
4. `ClampPhysicsVelocitySystem` - ограничения скорости и границ дороги
5. `ApplyPhysicsVelocitySystem` - применение velocity к Rigidbody
6. `SyncPhysicsPositionSystem` - синхронизация WorldPosition ← Rigidbody
7. `UpdatePhysicsStateSystem` - обновление состояния для отладки

Services.cs:
- Добавлен `FixedDeltaTime` для физики
- Добавлен `UnscaledDeltaTime` для UI
- Добавлен `TimeScale` для slow-mo
- Добавлен `Time` для общего времени

EcsBootstrap.cs:
- Добавлен `_physicsFeature`
- Добавлен `FixedUpdate()` для выполнения PhysicsFeature
- Обновлён `CreateSettingsFromBalance()` с физическими параметрами

---

## 2026-01-27 17:45 - Обновление HeroFactory и EntityBehaviour для физики

**Файлы:**
- `Assets/Code/Gameplay/HeroFeature.cs` - обновлён
- `Assets/Code/Infrastructure/EntityBehaviour.cs` - обновлён

**Причина:** Интеграция физических компонентов в создание героя
**Детали:**

HeroFeature.cs:
- `RunnerMovementSettings` расширен: Min/MaxForwardSpeed, MaxLateralSpeed, ForwardAcceleration, LateralAcceleration, Deceleration, BaseDrag, Mass, AngularDrag, UseContinuousCollision
- `HeroFactory.CreateHero()` добавляет все физические компоненты
- `HeroRigidbodySetup` - статический хелпер для настройки Rigidbody
- `RunnerHeroMoveSystem` теперь пропускает сущности с RigidbodyComponent (`.NoneOf()`)

EntityBehaviour.cs:
- Добавлен `[Inject] Construct(RunnerMovementSettings)` для настроек
- `BindRigidbody()` - автоматическая привязка/создание Rigidbody
- `OnCollisionEnter()` - обработка столкновений через PhysicsImpact

---

## 2026-01-27 17:15 - Создание физических компонентов Entitas

**Файлы:**
- `Assets/Code/Gameplay/PhysicsComponents.cs` - создан

**Причина:** Подготовка к миграции на гибридную физику с Rigidbody
**Детали:**
- `RigidbodyComponent` - ссылка на Unity Rigidbody
- `PhysicsVelocity` - целевая скорость
- `PhysicsBody` - флаг физического объекта
- `Acceleration` - параметры ускорения (forward, lateral, deceleration)
- `PhysicsDrag` - сопротивление (base, current)
- `SurfaceModifier` - модификаторы поверхности (friction, drag, type)
- `SurfaceZone` - триггер-зона поверхности
- `PhysicsConstraints` - ограничения скорости и границы дороги
- `PhysicsState` - текущее состояние (для отладки и эффектов)
- `PhysicsImpact` - данные столкновения
- `PhysicsSettings` - конфигурация для GameBalance
- `SurfaceType` enum - Normal, Oil, Grass, Ice, Puddle
- Extension methods для удобной работы

**Требуется:** Запустить Jenny для генерации кода Entitas

---

## 2026-01-27 16:30 - Создание системы контекста для Claude Code

**Файлы:**
- `AGENTS.md` - создан главный файл инструкций
- `.claude/CONTEXT.md` - обновлён с подробной структурой
- `.claude/ARCHITECTURE.md` - создана архитектурная документация
- `.claude/TODO.md` - обновлён с приоритетами
- `.claude/CHANGELOG.md` - обновлён формат
- `.claude/SESSION_NOTES.md` - создан файл заметок сессии

**Причина:** Создание системы автоматического восстановления контекста между сессиями
**Детали:** Claude Code теперь автоматически читает AGENTS.md и .claude/* при старте

---

## 2026-01-27 15:45 - Создание NPCAnimator

**Файлы:**
- `Assets/Code/Art/VFX/NPCAnimator.cs` - создан

**Причина:** Добавление процедурных анимаций для NPC (idle/walk)
**Детали:**
- `NPCAnimator` - компонент с idle sway и walk bob
- `NPCAnimationManager` - статический менеджер для управления
- `NPCAnimationSettings` - настраиваемые параметры
- Walk cycle: 2 сек loop с bob, sway, lean
- Idle: subtle sway + breathing effect

---

## 2026-01-27 15:00 - Начало работы над VFX системой

**Файлы:**
- Планирование VFX улучшений

**Причина:** Улучшение game feel
**Детали:**
- Запланированы: camera shake, улучшенные частицы, UI анимации
- DOTween установлен, но используется custom TweenSystem

---

## [Предыдущие изменения - сводка]

### Основные системы (реализованы ранее)

**ECS Архитектура:**
- `EcsBootstrap.cs` - инициализация Entitas
- `BattleFeature.cs` - главная Feature
- `SystemFactory.cs` - Zenject фабрика

**Геймплей:**
- `HeroFeature.cs` - движение грузовика
- `PedestrianFeature.cs` - NPC система
- `CollisionFeature.cs` - столкновения
- `FeedbackSystem.cs` - частицы, звуки, текст
- `QuestFeature.cs` - система квестов
- `EconomyFeature.cs` - экономика

**UI:**
- `MainMenuUI.cs` - главное меню
- `HubUI/*` - панели хаба
- `SettingsPanel.cs` - настройки

**Инфраструктура:**
- `SaveSystem.cs` - сохранение
- `SceneManagement.cs` - переходы
- `CameraFollow.cs` - камера

**Визуал:**
- `ProceduralMeshGenerator.cs` - генерация моделей NPC
- `TweenSystem.cs` - кастомные анимации

### Git коммиты (из истории)

- `facf32d` - Замена placeholder моделей
- `4349a48` - Debug UI и чит-коды
- `2041270` - Изменён тип камеры
- `2d98efa` - Скрипт создания сцен
- `6374f78` - Главное меню

---

## Архив (старые записи)

> Когда записей станет больше 30, перемести старые сюда

(пусто)
