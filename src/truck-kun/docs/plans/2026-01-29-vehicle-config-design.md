# Vehicle Config System — Design Document

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Создать единый VehicleConfig (ScriptableObject) для параметров транспорта, убрать дублирование настроек, интегрировать с апгрейдами.

**Architecture:** VehicleConfig → VehicleStats (runtime с множителями) → HeroFactory → ECS компоненты

**Tech Stack:** Unity ScriptableObject, Entitas ECS, Zenject DI

---

## Обзор изменений

### Проблема
Настройки транспорта дублируются в 3 местах:
- `RunnerMovementSettings` класс в HeroFeature.cs
- `HeroSettings` MonoBehaviour (не используется)
- Хардкод в GameplaySceneInstaller.cs

### Решение
Единый `VehicleConfig` (ScriptableObject) + `VehicleStats` (runtime структура с применёнными множителями).

---

## Новые файлы

### VehicleConfig.cs
```csharp
// Assets/Code/Configs/VehicleConfig.cs
[CreateAssetMenu(fileName = "VehicleConfig", menuName = "Truck-kun/Vehicle Config")]
public class VehicleConfig : ScriptableObject
{
    [Header("Identity")]
    public string VehicleId = "truck_default";

    [Header("Physics Body")]
    public float Mass = 1000f;
    public float AngularDrag = 0.05f;
    public bool UseGravity = true;
    public bool UseContinuousCollision = true;

    [Header("Speed Limits")]
    public float BaseForwardSpeed = 15f;
    public float MinForwardSpeed = 9f;
    public float MaxForwardSpeed = 24f;
    public float MaxLateralSpeed = 8f;

    [Header("Acceleration")]
    public float ForwardAcceleration = 10f;
    public float LateralAcceleration = 15f;
    public float Deceleration = 8f;

    [Header("Resistance")]
    public float BaseDrag = 0.5f;

    [Header("Visuals")]
    public EntityBehaviour Prefab;
}
```

### VehicleStats.cs
```csharp
// Assets/Code/Gameplay/Features/Hero/VehicleStats.cs
public readonly struct VehicleStats
{
    public readonly float Mass;
    public readonly float AngularDrag;
    public readonly bool UseGravity;
    public readonly bool UseContinuousCollision;

    public readonly float ForwardSpeed;
    public readonly float MinForwardSpeed;
    public readonly float MaxForwardSpeed;
    public readonly float MaxLateralSpeed;

    public readonly float ForwardAcceleration;
    public readonly float LateralAcceleration;
    public readonly float Deceleration;

    public readonly float BaseDrag;

    public static VehicleStats Create(
        VehicleConfig config,
        float speedMultiplier,
        float lateralMultiplier,
        float difficultyMultiplier)
    {
        // Применяет множители к базовым значениям
    }
}
```

---

## Файлы для удаления

| Файл | Причина |
|------|---------|
| `Assets/Code/Gameplay/Features/Hero/HeroSettings.cs` | Не используется |
| `Assets/Code/Art/VFX/VehicleEffects.cs` | Удаляем эффекты |

### Классы для удаления из HeroFeature.cs:
- `RunnerMovementSettings`
- `IHeroSpawnPoint`
- `HeroSpawnPoint`
- `HeroRigidbodySetup`

---

## Файлы для изменения

| Файл | Изменения |
|------|-----------|
| `LevelConfig.cs` | +VehicleConfig Vehicle |
| `GameplaySceneInstaller.cs` | Убрать хардкод, биндить VehicleConfig |
| `EcsBootstrap.cs` | Создавать VehicleStats, убрать мутацию |
| `HeroFactory.cs` | Принимать VehicleStats |
| `HeroFeature.cs` | Удалить легаси классы |
| `EntityBehaviour.cs` | Конфигурировать Rigidbody из VehicleConfig |
| `PhysicsFeature.cs` | Убрать RunnerMovementSettings |
| `UpgradeSystem.cs` | Убрать ApplyUpgradesToSettings() |

---

## Префаб PlayerTruck

Финальные компоненты:
- `BoxCollider` — size: (1.8, 1.6, 4.5), center: (0, 0.8, 0.2)
- `Rigidbody` — дефолтные значения (конфигурируется в runtime)
- `EntityBehaviour` — мост ECS ↔ Unity

Удалить:
- `VehicleEffects` компонент

---

## Поток данных

```
VehicleConfig (SO)
       ↓
UpgradeService.GetBonus() + DifficultyService
       ↓
VehicleStats.Create(config, multipliers)
       ↓
HeroFactory.CreateHero(stats) → ECS компоненты
       ↓
EntityBehaviour → конфигурирует Rigidbody
```

---

## План реализации

### Task 1: Создать VehicleConfig.cs
- Create: `Assets/Code/Configs/VehicleConfig.cs`

### Task 2: Создать VehicleStats.cs
- Create: `Assets/Code/Gameplay/Features/Hero/VehicleStats.cs`

### Task 3: Обновить LevelConfig
- Modify: `Assets/Code/Configs/LevelConfig.cs`
- Добавить поле VehicleConfig

### Task 4: Удалить legacy файлы
- Delete: `Assets/Code/Gameplay/Features/Hero/HeroSettings.cs`
- Delete: `Assets/Code/Art/VFX/VehicleEffects.cs`

### Task 5: Рефакторинг HeroFeature.cs
- Удалить: RunnerMovementSettings, HeroSpawnPoint, HeroRigidbodySetup
- Оставить: Hero компонент, HeroFeature, HeroFactory (обновить)

### Task 6: Рефакторинг HeroFactory
- Изменить сигнатуру CreateHero(VehicleStats)
- Убрать зависимости от legacy классов

### Task 7: Рефакторинг GameplaySceneInstaller
- Убрать хардкод RunnerMovementSettings
- Биндить VehicleConfig из LevelConfig

### Task 8: Рефакторинг EcsBootstrap
- Убрать ApplyUpgradesToSettings()
- Создавать VehicleStats с множителями
- Передавать в HeroFactory

### Task 9: Рефакторинг EntityBehaviour
- Получать VehicleConfig для конфигурации Rigidbody
- Убрать зависимость от RunnerMovementSettings

### Task 10: Рефакторинг PhysicsFeature
- Убрать зависимость от RunnerMovementSettings
- Использовать значения из ECS компонентов

### Task 11: Рефакторинг UpgradeSystem
- Убрать ApplyUpgradesToSettings()
- Оставить GetBonus() для получения множителей

### Task 12: Создать VehicleConfig.asset
- Создать ScriptableObject в Unity
- Настроить значения
- Привязать к LevelConfig

### Task 13: Обновить префаб PlayerTruck
- Удалить VehicleEffects компонент
- Проверить Rigidbody и Collider
