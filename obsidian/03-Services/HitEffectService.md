---
tags: [service, di, vfx]
aliases: [IHitEffectService]
---

# Hit Effect Service

Визуальные эффекты попаданий.

## Интерфейс

```csharp
public interface IHitEffectService
{
    void PlayHitEffect(Vector3 position, PedestrianKind kind);
    void PlayComboEffect(Vector3 position, int comboCount);
}
```

## Используется в

- [[Feedback]] — система эффектов
- [[Collision]] — при попадании

## Эффекты

| Метод | Эффект |
|-------|--------|
| PlayHitEffect | Партиклы + звук |
| PlayComboEffect | Увеличенные частицы |

## Путь в коде

`Assets/Code/Gameplay/Features/Feedback/Services/HitEffectService.cs`
