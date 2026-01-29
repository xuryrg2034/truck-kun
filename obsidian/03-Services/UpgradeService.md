---
tags: [service, di, upgrade]
aliases: [IUpgradeService]
---

# Upgrade Service

Система улучшений транспорта.

## Интерфейс

```csharp
public interface IUpgradeService
{
    IReadOnlyList<UpgradeInfo> GetAllUpgrades();
    bool PurchaseUpgrade(UpgradeType type);
    float GetUpgradeBonus(UpgradeType type);
}
```

## Типы улучшений

| Тип | Эффект |
|-----|--------|
| Speed | +% к максимальной скорости |
| Acceleration | +% к ускорению |
| Handling | +% к управляемости |
| Armor | +% к прочности |

## Используется в

- [[VehicleStats]] — применение бонусов
- UI гаража — покупка

## Зависимости

- [[MoneyService]] — оплата улучшений

## Путь в коде

`Assets/Code/Meta/Upgrades/UpgradeService.cs`
