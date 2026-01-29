# Config System Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development to implement this plan task-by-task.

**Goal:** Реализовать модульную систему конфигов согласно дизайну.

**Architecture:** Создать ScriptableObject конфиги + MonoBehaviour компоненты на префабах, затем мигрировать системы.

**Tech Stack:** Unity C#, ScriptableObjects, Zenject DI.

---

## Phase 1: Создание конфигов (ScriptableObjects)

### Task 1: Создать базовую структуру папок

**Files:**
- Create: `Assets/Code/Configs/` folder structure

**Action:** Создать папки:
```
Assets/Code/Configs/
├── Global/
└── Spawning/
```

---

### Task 2: Создать LevelConfig.cs

**Files:**
- Create: `Assets/Code/Configs/LevelConfig.cs`

**Code:**
```csharp
using UnityEngine;

namespace Code.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Truck-kun/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [Header("Level Info")]
        public string LevelId;
        public string LevelName;

        [Header("Global Configs")]
        public EconomyConfig Economy;
        public FeedbackConfig Feedback;

        [Header("Level-Specific Configs")]
        public DayConfig Day;
        public PedestrianSpawnConfig PedestrianSpawn;
        public QuestPoolConfig QuestPool;

        [Header("Optional Spawners")]
        public SurfaceSpawnConfig SurfaceSpawn;
        public ObstacleSpawnConfig ObstacleSpawn;
    }
}
```

---

### Task 3: Создать EconomyConfig.cs

**Files:**
- Create: `Assets/Code/Configs/Global/EconomyConfig.cs`

**Code:**
```csharp
using UnityEngine;

namespace Code.Configs
{
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
}
```

---

### Task 4: Создать FeedbackConfig.cs

**Files:**
- Create: `Assets/Code/Configs/Global/FeedbackConfig.cs`

**Code:**
```csharp
using UnityEngine;

namespace Code.Configs
{
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
}
```

---

### Task 5: Создать DayConfig.cs

**Files:**
- Create: `Assets/Code/Configs/Spawning/DayConfig.cs`

**Code:**
```csharp
using UnityEngine;

namespace Code.Configs
{
    [CreateAssetMenu(fileName = "DayConfig", menuName = "Truck-kun/Configs/Day")]
    public class DayConfig : ScriptableObject
    {
        [Header("Session Duration")]
        public float DurationSeconds = 60f;

        [Header("Difficulty Scaling")]
        [Tooltip("Spawn rate multiplier over day duration (0-1 normalized time)")]
        public AnimationCurve SpawnRateMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 1.5f);

        [Tooltip("Pedestrian speed multiplier over day duration")]
        public AnimationCurve PedestrianSpeedMultiplier = AnimationCurve.Linear(0f, 1f, 1f, 1.2f);

        [Header("Events")]
        public float RushHourStart = 30f;
        public float RushHourSpawnMultiplier = 2f;
    }
}
```

---

### Task 6: Создать PedestrianSpawnConfig.cs

**Files:**
- Create: `Assets/Code/Configs/Spawning/PedestrianSpawnConfig.cs`

**Code:**
```csharp
using System.Collections.Generic;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Configs
{
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
        public List<PedestrianSpawnEntry> SpawnPool = new();

        public GameObject GetRandomPrefab()
        {
            if (SpawnPool == null || SpawnPool.Count == 0)
                return null;

            float totalWeight = 0f;
            foreach (var entry in SpawnPool)
                totalWeight += entry.Weight;

            float random = Random.Range(0f, totalWeight);
            float cumulative = 0f;

            foreach (var entry in SpawnPool)
            {
                cumulative += entry.Weight;
                if (random <= cumulative)
                    return entry.Prefab;
            }

            return SpawnPool[0].Prefab;
        }
    }

    [System.Serializable]
    public class PedestrianSpawnEntry
    {
        public GameObject Prefab;
        [Range(0f, 10f)]
        public float Weight = 1f;
        public int MinDay = 1;
        public int MaxDay = 0;
    }
}
```

---

### Task 7: Создать QuestPoolConfig.cs

**Files:**
- Create: `Assets/Code/Configs/Spawning/QuestPoolConfig.cs`

**Code:**
```csharp
using System.Collections.Generic;
using Code.Gameplay.Features.Pedestrian;
using UnityEngine;

namespace Code.Configs
{
    [CreateAssetMenu(fileName = "QuestPoolConfig", menuName = "Truck-kun/Configs/QuestPool")]
    public class QuestPoolConfig : ScriptableObject
    {
        [Header("Generation")]
        public int MinQuestsPerDay = 2;
        public int MaxQuestsPerDay = 4;

        [Header("Quest Pool")]
        public List<QuestDefinition> AvailableQuests = new();
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
}
```

---

### Task 8: Создать SurfaceSpawnConfig.cs

**Files:**
- Create: `Assets/Code/Configs/Spawning/SurfaceSpawnConfig.cs`

**Code:**
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Code.Configs
{
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
        public List<SurfaceSpawnEntry> SpawnPool = new();
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
}
```

---

### Task 9: Создать ObstacleSpawnConfig.cs

**Files:**
- Create: `Assets/Code/Configs/Spawning/ObstacleSpawnConfig.cs`

**Code:**
```csharp
using System.Collections.Generic;
using UnityEngine;

namespace Code.Configs
{
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
        public List<ObstacleSpawnEntry> SpawnPool = new();
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
}
```

---

## Phase 2: Компоненты на префабах (MonoBehaviour)

### Task 10: Создать HeroSettings.cs

**Files:**
- Create: `Assets/Code/Gameplay/Features/Hero/HeroSettings.cs`

**Code:**
```csharp
using UnityEngine;

namespace Code.Gameplay.Features.Hero
{
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
}
```

---

### Task 11: Создать PedestrianSettings.cs

**Files:**
- Create: `Assets/Code/Gameplay/Features/Pedestrian/PedestrianSettings.cs`

**Code:**
```csharp
using UnityEngine;

namespace Code.Gameplay.Features.Pedestrian
{
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

    public enum PedestrianCategory
    {
        Normal,
        Protected,
        Special
    }
}
```

---

### Task 12: Обновить SurfaceSettings.cs (уже существует как SurfaceTrigger)

**Files:**
- Read: `Assets/Code/Gameplay/Features/Surface/SurfaceTrigger.cs`
- Modify or Create: `Assets/Code/Gameplay/Features/Surface/SurfaceSettings.cs`

**Action:** Проверить существующий код и либо обновить, либо создать новый компонент.

---

### Task 13: Обновить ObstacleSettings.cs (проверить существующий)

**Files:**
- Read: `Assets/Code/Gameplay/Features/Obstacle/ObstacleComponents.cs`
- Modify if needed: `Assets/Code/Gameplay/Features/Obstacle/ObstacleBehaviour.cs`

**Action:** Проверить существующий код, возможно уже есть подходящий компонент.

---

## Phase 3: Интеграция с Zenject

### Task 14: Обновить GameplaySceneInstaller

**Files:**
- Modify: `Assets/Code/Infrastructure/Installers/GameplaySceneInstaller.cs`

**Action:** Добавить биндинг LevelConfig и всех вложенных конфигов.

---

## Phase 4: Миграция систем

### Task 15: Мигрировать PedestrianSpawnSystem

**Files:**
- Modify: `Assets/Code/Gameplay/Features/Pedestrian/PedestrianFeature.cs`

**Action:** Заменить использование старых Settings на PedestrianSpawnConfig.

---

### Task 16: Мигрировать EconomyFeature

**Files:**
- Modify: `Assets/Code/Gameplay/Features/Economy/EconomyFeature.cs`

**Action:** Заменить использование старых Settings на EconomyConfig.

---

### Task 17: Мигрировать FeedbackSystem

**Files:**
- Modify: `Assets/Code/Gameplay/Features/Feedback/FeedbackFeature.cs`

**Action:** Заменить использование старых Settings на FeedbackConfig.

---

### Task 18: Мигрировать QuestFeature

**Files:**
- Modify: `Assets/Code/Gameplay/Features/Quest/QuestFeature.cs`

**Action:** Заменить использование старых Settings на QuestPoolConfig.

---

## Phase 5: Очистка

### Task 19: Удалить старый GameBalance

**Files:**
- Delete: `Assets/Code/Balance/GameBalance.cs`
- Delete: `Assets/Resources/Configs/GameBalance.asset` (через Unity)
- Delete: `Assets/Resources/Configs/Balance/` folder (если есть)

**Action:** Удалить только после успешной миграции всех систем.

---

### Task 20: Удалить неиспользуемые Settings классы

**Files:**
- Clean up embedded Settings classes in Feature files

**Action:** Удалить старые `*Settings` классы, которые больше не используются.

---

## Phase 6: Создание ассетов

### Task 21: Создать GlobalEconomyConfig.asset (в Unity)

**Action:** Пользователь создаёт через Unity меню: Truck-kun/Configs/Economy

---

### Task 22: Создать Level1 конфиги (в Unity)

**Action:** Пользователь создаёт набор конфигов для первого уровня.

---

## Verification Checklist

- [ ] Все конфиг классы созданы
- [ ] Все MonoBehaviour компоненты созданы
- [ ] GameplaySceneInstaller обновлён
- [ ] Системы мигрированы на новые конфиги
- [ ] Старый GameBalance удалён
- [ ] Unity компилируется
- [ ] Игра запускается
