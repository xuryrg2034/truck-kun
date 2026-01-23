# Truck-kun (MVP)

Mid-core decision-based runner. Сессии — короткие «дни». Фокус игрока — быстро принимать решения под давлением, а не показывать рефлексы.

## Быстрый старт
- Открой Unity‑проект.
- Открой сцену `Assets/Scenes/GameScene.unity`.
- Нажми Play.
- Ввод использует действие `Player/Move` из InputActionAsset на `EcsBootstrap`.

## Поток выполнения (код)
- `EcsBootstrap` поднимает Entitas контексты, бинды Zenject и фичи, затем тикает их каждый кадр.
- Порядок `BattleFeature`: Input -> Hero -> BindView -> Movement.

## Сессия дня
- `DaySessionService` держит состояние Running/Finished и таймер.
- `EcsBootstrap.Update` тикает таймер и прекращает выполнение систем при завершении.
- По окончании создаётся Canvas‑оверлей с текстом «DAY FINISHED».
- Длительность настраивается в `DaySessionSettings.DurationSeconds` на `EcsBootstrap`.

## Ввод
- `InputSystemService` читает `Player/Move` каждый кадр и эмитит `MoveInput`‑сущности.
- `InputFeature` создаёт и удаляет input‑сущности каждый кадр.

## Движение героя (runner)
- `RunnerHeroMoveSystem` постоянно двигает героя вперёд и применяет только lateral‑ввод.
- Боковое движение ограничено `RoadWidth` вокруг X спавна.
- Настройка: `RunnerMovementSettings` на `EcsBootstrap`:
  - `ForwardSpeed`, `LateralSpeed`, `RoadWidth`.

## View и трансформы
- `EntityBehaviour` связывает Unity `Transform` с Entitas‑сущностью и хранит ссылку view.
- `BindEntityViewFromPrefabSystem` инстансит префаб из `ViewPrefab` и привязывает его к сущности.
- `UpdateTransformPositionSystem` пушит `WorldPosition` в Unity‑трансформ.

## Камера
- `CameraFollow` сам находит view героя и следует за ним со сглаживанием.
- Настройки: `_followOffset`, `_useLocalOffset`, `_smoothTime`.

## Сценовая привязка
- `EcsBootstrap` в `GameScene` ссылается на:
  - InputActionAsset
  - Transform точки спавна героя
  - Префаб героя
  - Настройки runner‑движения и длительности дня

## Ключевые ассеты
- Сцена: `Assets/Scenes/GameScene.unity`
- Префабы: `Assets/Prefabs/Hero.prefab`, `Assets/Prefabs/Road.prefab`
- Материал: `Assets/Prefabs/Road.mat`

## Ключевые файлы кода
- `Assets/Code/Infrastructure/EcsBootstrap.cs` — входная точка, бинды, апдейт.
- `Assets/Code/Gameplay/BattleFeature.cs` — порядок фич.
- `Assets/Code/Gameplay/HeroFeature.cs` — спавн героя и runner‑движение.
- `Assets/Code/Gameplay/MovementFeature.cs` — общее движение + синк трансформа.
- `Assets/Code/Gameplay/InputFeature.cs` — ввод.
- `Assets/Code/Gameplay/DaySession.cs` — состояние/таймер дня.
- `Assets/Code/Infrastructure/ViewSystems.cs` — биндинг view.
- `Assets/Code/Infrastructure/CameraFollow.cs` — следование камеры.

## Ограничения MVP
- Предполагаются только Target и Forbidden NPC.
- Одна цель дня и один модификатор‑последствие (ещё не реализовано).
- Рост сложности через давление (плотность/время), а не новые правила.
