# Config System Redesign

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:writing-plans to create implementation plan from this design.

**Goal:** Заменить разрозненные конфиги модульной системой с разделением "параметры на префабе / правила в конфиге".

**Architecture:** LevelConfig как точка входа собирает глобальные и level-specific конфиги. Параметры сущностей живут на префабах как MonoBehaviour компоненты.

**Tech Stack:** Unity ScriptableObjects, Zenject DI, Entitas ECS.

---

## Принцип разделения

| Тип данных | Где хранить | Пример |
|------------|-------------|--------|
| Правила игры | Config (ScriptableObject) | "Пешеходы спавнятся каждые 2 сек" |
| Параметры сущности | Компонент на префабе | "Этот пешеход весит 70 кг" |
| Контент/пул | Config | "Доступные типы квестов" |
| Визуал конкретного объекта | Префаб | "Цвет, модель, размер" |

---

## Структура файлов

```
Assets/
├── Prefabs/
│   ├── Hero/
│   │   └── Truck.prefab              ← HeroSettings компонент
│   ├── Pedestrians/
│   │   ├── StudentNerd.prefab        ← PedestrianSettings
│   │   ├── Grandma.prefab            ← PedestrianSettings
│   │   └── Salaryman.prefab          ← PedestrianSettings
│   ├── Surfaces/
│   │   ├── OilSpill.prefab           ← SurfaceSettings
│   │   └── IcePatch.prefab           ← SurfaceSettings
│   └── Obstacles/
│       ├── Ramp.prefab               ← ObstacleSettings
│       └── Barrier.prefab            ← ObstacleSettings
│
└── Configs/
    ├── Global/                        ← Общие для всех уровней
    │   ├── EconomyConfig.asset
    │   └── FeedbackConfig.asset
    │
    └── Levels/
        ├── Level1/
        │   ├── LevelConfig.asset      ← Точка входа
        │   ├── DayConfig.asset
        │   ├── PedestrianSpawnConfig.asset
        │   ├── QuestPoolConfig.asset
        │   ├── SurfaceSpawnConfig.asset
        │   └── ObstacleSpawnConfig.asset
        │
        └── Level2/
            └── ...
```

---

## Код структура

```
Assets/Code/
├── Configs/                          ← НОВАЯ ПАПКА
│   ├── LevelConfig.cs
│   ├── Global/
│   │   ├── EconomyConfig.cs
│   │   └── FeedbackConfig.cs
│   └── Spawning/
│       ├── DayConfig.cs
│       ├── PedestrianSpawnConfig.cs
│       ├── QuestPoolConfig.cs
│       ├── SurfaceSpawnConfig.cs
│       └── ObstacleSpawnConfig.cs
│
├── Gameplay/Features/
│   ├── Hero/
│   │   └── HeroSettings.cs           ← MonoBehaviour
│   ├── Pedestrian/
│   │   └── PedestrianSettings.cs     ← MonoBehaviour
│   ├── Surface/
│   │   └── SurfaceSettings.cs        ← MonoBehaviour
│   └── Obstacle/
│       └── ObstacleSettings.cs       ← MonoBehaviour
```

---

## Конфиги (ScriptableObjects)

### LevelConfig — точка входа

```csharp
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Truck-kun/Level Config")]
public class LevelConfig : ScriptableObject
{
    [Header("Level Info")]
    public string LevelId;
    public string LevelName;

    [Header("Global Configs (shared)")]
    public EconomyConfig Economy;
    public FeedbackConfig Feedback;

    [Header("Level-Specific Configs")]
    public DayConfig Day;
    public PedestrianSpawnConfig PedestrianSpawn;
    public QuestPoolConfig QuestPool;

    [Header("Optional Spawners")]
    public SurfaceSpawnConfig SurfaceSpawn;      // null = нет поверхностей
    public ObstacleSpawnConfig ObstacleSpawn;    // null = нет препятствий
}
```

### EconomyConfig

```csharp
[CreateAssetMenu(fileName = "EconomyConfig", menuName = "Truck-kun/Configs/Economy")]
public class EconomyConfig : ScriptableObject
{
    [Header("Start")]
    public int StartingMoney = 1000;

    [Header("Penalties")]
    public int ViolationPenalty = 100;

    [Header("Daily Costs")]
    public int DailyCost = 200;

    [Header("Combo System")]
    public float ComboMultiplierStep = 0.1f;
    public float MaxComboMultiplier = 2.0f;
    public float ComboResetTime = 3f;
}
```

### FeedbackConfig

```csharp
[CreateAssetMenu(fileName = "FeedbackConfig", menuName = "Truck-kun/Configs/Feedback")]
public class FeedbackConfig : ScriptableObject
{
    [Header("Floating Text")]
    public float TextRiseSpeed = 2f;
    public float TextDuration = 1.5f;
    public int FontSize = 32;

    [Header("Colors")]
    public Color RewardColor = new Color(0.2f, 0.8f, 0.2f);
    public Color PenaltyColor = new Color(0.9f, 0.2f, 0.2f);
    public Color ComboColor = new Color(1f, 0.8f, 0f);

    [Header("Hit Particles")]
    public int ParticleBurstCount = 15;
    public float ParticleLifetime = 1f;
    public float ParticleSpeed = 5f;

    [Header("Camera Shake")]
    public float ShakeIntensity = 0.3f;
    public float ShakeDuration = 0.15f;
    public AnimationCurve ShakeFalloff;
}
```

### DayConfig

```csharp
[CreateAssetMenu(fileName = "DayConfig", menuName = "Truck-kun/Configs/Day")]
public class DayConfig : ScriptableObject
{
    [Header("Session Duration")]
    public float DurationSeconds = 60f;

    [Header("Difficulty Scaling")]
    public AnimationCurve SpawnRateMultiplier;
    public AnimationCurve PedestrianSpeedMultiplier;

    [Header("Events")]
    public float RushHourStart = 30f;
    public float RushHourSpawnMultiplier = 2f;
}
```

### PedestrianSpawnConfig

```csharp
[CreateAssetMenu(fileName = "PedestrianSpawnConfig", menuName = "Truck-kun/Configs/PedestrianSpawn")]
public class PedestrianSpawnConfig : ScriptableObject
{
    [Header("Spawn Timing")]
    public float MinSpawnInterval = 1f;
    public float MaxSpawnInterval = 3f;

    [Header("Behavior")]
    [Range(0f, 1f)]
    public float CrossingChance = 0.7f;
    public float DespawnDistance = 25f;

    [Header("Road Bounds")]
    public float SpawnDistanceAhead = 20f;
    public float RoadWidth = 8f;

    [Header("Spawn Pool")]
    public List<PedestrianSpawnEntry> SpawnPool;
}

[System.Serializable]
public class PedestrianSpawnEntry
{
    public GameObject Prefab;
    [Range(0f, 10f)]
    public float Weight = 1f;
    public int MinDay = 1;
    public int MaxDay = 0;  // 0 = без ограничения
}
```

### QuestPoolConfig

```csharp
[CreateAssetMenu(fileName = "QuestPoolConfig", menuName = "Truck-kun/Configs/QuestPool")]
public class QuestPoolConfig : ScriptableObject
{
    [Header("Generation")]
    public int MinQuestsPerDay = 2;
    public int MaxQuestsPerDay = 4;

    [Header("Quest Pool")]
    public List<QuestDefinition> AvailableQuests;
}

[System.Serializable]
public class QuestDefinition
{
    public string QuestId;
    public string NameRu;

    [Header("Target")]
    public QuestType Type;
    public PedestrianKind TargetKind;

    [Header("Requirements")]
    public int MinTarget = 3;
    public int MaxTarget = 10;

    [Header("Reward")]
    public int BaseReward = 200;
    public int BonusPerExtra = 20;
}

public enum QuestType
{
    HitCount,
    HitSpecificType,
    ComboChain,
    SpeedHit,
    NoPenalty
}
```

### SurfaceSpawnConfig

```csharp
[CreateAssetMenu(fileName = "SurfaceSpawnConfig", menuName = "Truck-kun/Configs/SurfaceSpawn")]
public class SurfaceSpawnConfig : ScriptableObject
{
    [Header("Enable")]
    public bool Enabled = true;

    [Header("Spawn Timing")]
    public float MinInterval = 5f;
    public float MaxInterval = 15f;

    [Header("Placement")]
    public float SpawnDistanceAhead = 30f;
    public float LateralMargin = 1f;

    [Header("Spawn Pool")]
    public List<SurfaceSpawnEntry> SpawnPool;
}

[System.Serializable]
public class SurfaceSpawnEntry
{
    public GameObject Prefab;
    [Range(0f, 10f)]
    public float Weight = 1f;
    public Vector2 LengthRange = new Vector2(3f, 6f);
    public Vector2 WidthRange = new Vector2(2f, 4f);
}
```

### ObstacleSpawnConfig

```csharp
[CreateAssetMenu(fileName = "ObstacleSpawnConfig", menuName = "Truck-kun/Configs/ObstacleSpawn")]
public class ObstacleSpawnConfig : ScriptableObject
{
    [Header("Enable")]
    public bool Enabled = true;

    [Header("Spawn Timing")]
    public float MinInterval = 8f;
    public float MaxInterval = 20f;

    [Header("Placement")]
    public float SpawnDistanceAhead = 35f;

    [Header("Spawn Pool")]
    public List<ObstacleSpawnEntry> SpawnPool;
}

[System.Serializable]
public class ObstacleSpawnEntry
{
    public GameObject Prefab;
    [Range(0f, 10f)]
    public float Weight = 1f;
    public ObstaclePlacement Placement;
}

public enum ObstaclePlacement
{
    Road,
    Roadside,
    CrossingPath
}
```

---

## Компоненты на префабах (MonoBehaviour)

### HeroSettings

```csharp
public class HeroSettings : MonoBehaviour
{
    [Header("Physics Body")]
    public float Mass = 1500f;
    public float AngularDrag = 5f;
    public bool UseGravity = true;
    public bool UseContinuousCollision = true;

    [Header("Speed Limits")]
    public float MaxForwardSpeed = 15f;
    public float MinForwardSpeed = 3f;
    public float MaxLateralSpeed = 8f;

    [Header("Acceleration")]
    public float ForwardAcceleration = 10f;
    public float LateralAcceleration = 15f;
    public float Deceleration = 8f;

    [Header("Drag")]
    public float BaseDrag = 0.5f;
}
```

### PedestrianSettings

```csharp
public class PedestrianSettings : MonoBehaviour
{
    [Header("Identity")]
    public PedestrianKind Kind;
    public PedestrianCategory Category;

    [Header("Rewards")]
    public int BaseReward = 100;
    public int PenaltyIfProtected = 150;

    [Header("Physics")]
    public float Mass = 70f;
    public float Drag = 2f;
    public float AngularDrag = 1f;

    [Header("Movement")]
    public float WalkSpeed = 2f;
    public float MovementForce = 500f;

    [Header("Visual")]
    public float TiltAngle = 15f;
}
```

### SurfaceSettings

```csharp
public class SurfaceSettings : MonoBehaviour
{
    public SurfaceType Type;

    [Header("Physics Modifiers")]
    [Range(0f, 2f)]
    public float FrictionMultiplier = 1f;
    [Range(0f, 5f)]
    public float DragMultiplier = 1f;

    [Header("Effect")]
    public float EffectDuration = 0f;
    public bool AffectsPedestrians = false;
}
```

### ObstacleSettings

```csharp
public class ObstacleSettings : MonoBehaviour
{
    public ObstacleType Type;

    [Header("Ramp")]
    public float LaunchAngle = 30f;
    public float BoostForce = 15f;

    [Header("Barrier")]
    public float ImpactSpeedLoss = 0.5f;
    public float KnockbackForce = 5f;

    [Header("SpeedBump")]
    public float BumpImpulse = 3f;
    public float SpeedPenalty = 0.2f;

    [Header("Hole")]
    public float DownForce = 20f;
    public float StuckDuration = 0.5f;
}
```

---

## Что удалить после миграции

- `Assets/Code/Balance/GameBalance.cs` — весь файл
- `Assets/Resources/Configs/GameBalance.asset`
- `Assets/Resources/Configs/Balance/GameBalance.asset`
- Все отдельные `*Settings` классы внутри Feature файлов:
  - `EconomySettings` в EconomyFeature.cs
  - `FeedbackSettings` в FeedbackFeature.cs
  - `CollisionSettings` в CollisionFeature.cs
  - `RagdollSettings` в RagdollFeature.cs
  - `SurfaceSpawnSettings` в SurfaceFeature.cs
  - `PedestrianSpawnSettings` в PedestrianFeature.cs
  - `PedestrianVisualData` в PedestrianFeature.cs
  - `QuestSettings`, `QuestDefinition` в QuestFeature.cs
  - `DaySessionSettings` в DaySession.cs
  - `RunnerMovementSettings` в HeroFeature.cs
- `Assets/Code/Gameplay/Features/Pedestrian/PedestrianPhysicsSettings.cs`
- `BalanceProvider` и `BalanceAdapters` классы

---

## Интеграция с Zenject

В `GameplaySceneInstaller`:

```csharp
[SerializeField] private LevelConfig _levelConfig;

public override void InstallBindings()
{
    // Bind all configs from LevelConfig
    Container.BindInstance(_levelConfig).AsSingle();
    Container.BindInstance(_levelConfig.Economy).AsSingle();
    Container.BindInstance(_levelConfig.Feedback).AsSingle();
    Container.BindInstance(_levelConfig.Day).AsSingle();
    Container.BindInstance(_levelConfig.PedestrianSpawn).AsSingle();
    Container.BindInstance(_levelConfig.QuestPool).AsSingle();

    // Optional configs
    if (_levelConfig.SurfaceSpawn != null)
        Container.BindInstance(_levelConfig.SurfaceSpawn).AsSingle();

    if (_levelConfig.ObstacleSpawn != null)
        Container.BindInstance(_levelConfig.ObstacleSpawn).AsSingle();
}
```

---

## Verification Checklist

После реализации:
- [ ] Все новые конфиги созданы как ScriptableObjects
- [ ] Все компоненты на префабах созданы
- [ ] LevelConfig собирает все конфиги уровня
- [ ] GameplaySceneInstaller биндит конфиги через Zenject
- [ ] Системы получают конфиги через DI (не через статики)
- [ ] Старый GameBalance удалён
- [ ] Все старые Settings классы удалены
- [ ] Unity компилируется без ошибок
- [ ] Игра запускается и работает
