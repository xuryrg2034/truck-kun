---
tags: [config, scriptableobject]
aliases: [Vehicle Config, конфиг транспорта]
---

# Vehicle Config

Конфигурация транспорта игрока.

## Поля

| Поле | Тип | Описание |
|------|-----|----------|
| MaxSpeed | float | Максимальная скорость |
| Acceleration | float | Ускорение |
| Deceleration | float | Торможение |
| SteeringSpeed | float | Скорость поворота |
| Mass | float | Масса (Rigidbody) |
| Drag | float | Сопротивление |

## Используется в

- [[Physics]] — все системы движения
- [[VehicleStats]] — runtime статы с бонусами

## Путь

- **Скрипт:** `Assets/Code/Configs/VehicleConfig.cs`
- **Asset:** `Assets/Resources/Configs/VehicleConfig.asset`

## Runtime

Значения из конфига комбинируются с бонусами от [[UpgradeService]] в `VehicleStats`.
