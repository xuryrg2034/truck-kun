---
tags: [service, di, economy]
aliases: [IMoneyService]
---

# Money Service

Управление балансом игрока.

## Интерфейс

```csharp
public interface IMoneyService
{
    int Balance { get; }
    void Add(int amount);
    bool TrySpend(int amount);
    event Action<int> OnBalanceChanged;
}
```

## Используется в

- [[Economy]] — начисление наград
- [[UpgradeService]] — покупка улучшений
- UI панели баланса

## Реализация

Хранит баланс в `MetaContext` через компонент `Money`.

## Путь в коде

`Assets/Code/Gameplay/Features/Economy/Services/MoneyService.cs`
