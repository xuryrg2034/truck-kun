---
tags: [service, di, quest]
aliases: [IQuestService]
---

# Quest Service

Управление ежедневными квестами.

## Интерфейс

```csharp
public interface IQuestService
{
    void GenerateDailyQuests();
    void OnPedestrianHit(PedestrianKind kind);
    IReadOnlyList<QuestData> GetActiveQuests();
}
```

## Используется в

- [[Quest]] — обновление прогресса
- [[Collision]] — события попаданий
- UI квестов

## Логика

1. При старте дня — генерация квестов
2. При попадании — проверка соответствия цели
3. При завершении — награда через [[MoneyService]]

## Путь в коде

`Assets/Code/Gameplay/Features/Quest/Services/QuestService.cs`
