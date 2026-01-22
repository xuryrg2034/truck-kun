# Резюме работ по Truck-kun (ECS)

## Что сделано
- Собран минимальный ECS‑каркас: загрузка контекстов, DI, запуск фич и систем.
- Добавлены общие компоненты и сервисы (id, позиция, время).
- Добавлен пайплайн привязки вьюхи к сущности через префаб.
- Добавлены игровые фичи: Input, Hero, Movement, Battle.
- Пересоздан asset Input System с действием Player/Move.
- Добавлены префабы героя и спавнера.

## Поток выполнения (runtime)
1) `EcsBootstrap` создаёт локальный `DiContainer`, биндует контексты/сервисы/фабрики.
2) Через `SystemFactory` создаётся `BattleFeature`, вызывается `Initialize()`.
3) В `Update` каждый кадр идут `Execute()` и `Cleanup()`.
4) В `OnDestroy` — `TearDown()` и `Dispose()` для input.

## Контексты и сущности
Game‑контекст:
- `Id` (также Meta, primary index)
- `WorldPosition`
- `TransformComponent`
- `View`
- `ViewPrefab`
- `MoveDirection`
- `MoveSpeed`
- `Hero`

Input‑контекст:
- `MoveInput`

Hero‑сущность:
- `Hero`, `Id`, `WorldPosition`, `MoveDirection`, `MoveSpeed`
- опционально `ViewPrefab` для визуального объекта

Input‑сущности:
- каждый кадр создаётся одна `InputEntity` с `MoveInput`, затем удаляется в cleanup

## Фичи и системы
InputFeature:
- `InitializeInputSystem`: включает Input System
- `EmitInputSystem`: создаёт `MoveInput` каждый кадр
- `CleanupInputSystem`: чистит input‑сущности

HeroFeature:
- `InitializeHeroSystem`: спавнит героя при отсутствии
- `SetHeroDirectionByInputSystem`: переносит `MoveInput` в `MoveDirection`

MovementFeature:
- `DirectionalDeltaMoveSystem`: обновляет `WorldPosition` = dir * speed * dt
- `RotateAlongDirectionSystem`: поворачивает `Transform` по направлению
- `UpdateTransformPositionSystem`: синхронизирует `Transform.position`

BindViewFeature:
- `BindEntityViewFromPrefabSystem`: инстанцирует префаб и привязывает к сущности

## Вью и привязка
- `EntityBehaviour` связывает Unity‑объект с `GameEntity`, добавляет `View` и `TransformComponent`, и освобождает их при уничтожении.
- `EntityViewFactory` использует `IInstantiator` для создания префаба и присваивает сущность.

## Ассеты и сцена
- Input asset: `Assets/InputSystem_Actions.inputactions` (действие `Player/Move`).
- Сцена: `Assets/Scenes/GameScene.unity` с `EcsBootstrap`:
  - `_inputActions` → `InputSystem_Actions`
  - `_heroSpawn` → объект‑маркер
  - `_heroViewPrefab` → `Hero.prefab`
- Префабы:
  - `Assets/Prefabs/Hero.prefab` (куб + `EntityBehaviour`)
  - `Assets/Prefabs/HeroSpawner.prefab` (пустой маркер)

## Где что лежит и почему
- `Assets/Code/Common`: общие компоненты/сервисы для разных фич.
- `Assets/Code/Infrastructure`: bootstrap, DI, view‑binding.
- `Assets/Code/Gameplay`: игровые фичи и системы.
- `Assets/Code/Generated`: автогенерация Entitas.
- `Assets/Prefabs`: готовые к назначению объекты.

## Примечания
- Движение 3D по плоскости XZ (Y = 0 для направления).
- `InputSystemService` читает `Player/Move`; имена должны совпадать.
- После добавления компонентов нужно запускать Jenny (Entitas).
