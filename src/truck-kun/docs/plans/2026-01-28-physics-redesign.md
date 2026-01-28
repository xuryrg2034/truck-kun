# Physics System Redesign

> Полная физика для пешеходов и дорожных препятствий.

---

## Обзор

Переход от гибридной физики (kinematic + velocity) к полноценной Unity физике:
- Пешеходы: dynamic Rigidbody, force-based движение
- Препятствия: статичные объекты на уровне
- Столкновения: реалистичная передача импульса

---

## Архитектура

### Слои коллизий

| Layer | Номер | Взаимодействует с |
|-------|-------|-------------------|
| Hero | 8 | Pedestrian, Obstacle, Ground |
| Pedestrian | 9 | Hero, Ground |
| Obstacle | 10 | Hero, Pedestrian |
| Ground | 11 | Hero, Pedestrian, Obstacle |

### Data Flow

```
ECS Direction → AddForce → Physics Step → Transform Update
                              ↓
                     OnCollisionEnter
                              ↓
                    HitEvent + Ragdoll
```

---

## Пешеходы: Force-based движение

### Принцип

```csharp
// Каждый FixedUpdate:
Vector3 desiredVelocity = direction * maxSpeed;
Vector3 velocityDiff = desiredVelocity - rb.linearVelocity;
Vector3 force = velocityDiff * moveForce;
rb.AddForce(force, ForceMode.Force);
```

### Массы по типам

| Тип | Масса (кг) | Обоснование |
|-----|------------|-------------|
| StudentNerd | 60 | Худощавый студент |
| Salaryman | 75 | Взрослый мужчина в костюме |
| Grandma | 50 | Пожилая женщина |
| OldMan | 65 | Пожилой мужчина |
| Teenager | 55 | Подросток |

### Настройки Rigidbody

```csharp
rb.mass = GetMassForType(pedestrianType);
rb.linearDamping = settings.Drag;      // ~2.0
rb.angularDamping = 0.5f;
rb.useGravity = true;
rb.isKinematic = false;
rb.constraints = RigidbodyConstraints.FreezeRotationX |
                 RigidbodyConstraints.FreezeRotationZ;
```

---

## Столкновения и отброс

### Формула силы удара

```csharp
float impactSpeed = collision.relativeVelocity.magnitude;
float mass = pedestrianRb.mass;

// Горизонтальная сила (направление удара)
Vector3 hitDirection = (pedestrian.position - hero.position).normalized;
float horizontalForce = impactSpeed * mass * ForceMultiplier;

// Вертикальная сила (подброс)
float liftForce = 0f;
if (impactSpeed >= MinSpeedForLift)
{
    float liftFactor = Mathf.InverseLerp(MinSpeedForLift, MaxLiftSpeed, impactSpeed);
    liftForce = horizontalForce * LiftMultiplier * liftFactor;
}

Vector3 totalForce = hitDirection * horizontalForce + Vector3.up * liftForce;
pedestrianRb.AddForce(totalForce, ForceMode.Impulse);
```

### Настройки

```csharp
public class CollisionSettings
{
    public float ForceMultiplier = 15f;     // Множитель силы удара
    public float MinSpeedForLift = 5f;      // Мин. скорость для подброса
    public float MaxLiftSpeed = 20f;        // Макс. скорость для расчёта lift
    public float LiftMultiplier = 0.3f;     // Доля вертикальной силы
}
```

---

## Дорожные препятствия

### Типы

| Тип | Физика | Эффект на Hero |
|-----|--------|----------------|
| **Ramp** | Static collider с наклоном | Подбрасывает вверх (физика) |
| **Barrier** | Dynamic Rigidbody ~200kg | Отталкивается, замедляет Hero |
| **SpeedBump** | Static collider | Небольшой подброс + потеря скорости |
| **Hole** | Trigger zone | Проваливание + сильная потеря скорости |

### Компоненты

```csharp
[Game] public class Obstacle : IComponent { }
[Game] public class ObstacleType : IComponent
{
    public ObstacleKind Value;
}

public enum ObstacleKind { Ramp, Barrier, SpeedBump, Hole }
```

### Размещение

Препятствия размещаются вручную на уровне. Не требуется система спавна.

---

## Ragdoll и Despawn

### Жизненный цикл

```
Hit → RagdollActivated → Timer → FadeOut → Destructed
```

### Компоненты

```csharp
[Game] public class RagdollTimer : IComponent
{
    public float TimeRemaining;
}

[Game] public class FadingOut : IComponent { }
```

### Системы

| Система | Ответственность |
|---------|-----------------|
| `RagdollActivationSystem` | Hit → отключить Animator, включить Ragdoll |
| `RagdollTimerSystem` | Отсчёт DespawnAfterHitDelay |
| `RagdollFadeSystem` | Плавное исчезновение (scale/alpha) |
| `RagdollCleanupSystem` | Удаление entity после fade |

### Настройки

```csharp
public class RagdollSettings
{
    public float DespawnAfterHitDelay = 3f;  // Секунд до начала fade
    public float FadeDuration = 0.5f;        // Длительность исчезновения
    public bool EnableFadeOut = true;
}
```

### Логика активации

```csharp
// При получении HitEvent:
1. Найти все Rigidbody в дочерних объектах
2. Переключить isKinematic = false
3. Отключить Animator
4. Применить силу удара (из CollisionImpact)
5. Добавить RagdollTimer
```

---

## Конфигурация

### PedestrianPhysicsSettings

```csharp
public class PedestrianPhysicsSettings
{
    // Движение
    public float MoveForce = 50f;
    public float MaxSpeed = 3f;
    public float Drag = 2f;

    // Массы по типам
    public float StudentMass = 60f;
    public float SalarymanMass = 75f;
    public float GrandmaMass = 50f;
    public float OldManMass = 65f;
    public float TeenagerMass = 55f;
}
```

---

## План реализации

| Шаг | Задача | Зависимости |
|-----|--------|-------------|
| 1 | Настроить Collision Layers в Unity | — |
| 2 | Создать `PedestrianPhysicsSettings` | — |
| 3 | Переделать `PedestrianFactory` — dynamic Rigidbody | Шаг 2 |
| 4 | Создать `PedestrianForceMovementSystem` | Шаг 3 |
| 5 | Обновить `CollisionSettings` | — |
| 6 | Переделать `PhysicsCollisionHandler` — новая формула | Шаг 5 |
| 7 | Обновить `RagdollSettings` | — |
| 8 | Рефакторинг `RagdollFeature` — timer + fade | Шаг 7 |
| 9 | Создать `ObstacleComponents.cs` | — |
| 10 | Создать префабы препятствий | Шаг 9 |

### Файлы

**Новые:**
- `Gameplay/Features/Pedestrian/PedestrianPhysicsSettings.cs`
- `Gameplay/Features/Pedestrian/Systems/PedestrianForceMovementSystem.cs`
- `Gameplay/Features/Obstacle/ObstacleComponents.cs`
- `Gameplay/Features/Obstacle/ObstacleFeature.cs`

**Изменить:**
- `Gameplay/Features/Pedestrian/PedestrianFactory.cs`
- `Gameplay/Features/Collision/CollisionSettings.cs`
- `Gameplay/Features/Ragdoll/RagdollSettings.cs`
- `Gameplay/Features/Ragdoll/RagdollFeature.cs`
- `Infrastructure/View/PhysicsCollisionHandler.cs`
- `Infrastructure/Installers/GameplaySceneInstaller.cs`
