---
tags: [moc, physics]
aliases: [Физика MOC, Physics Map]
---

# Physics — Карта связей

> Открой **Local Graph** (Ctrl+Shift+G) чтобы увидеть только связи физики.

## Core

- [[Physics]] — главная Feature

## Связанные Features

- [[Collision]] — столкновения на базе физики
- [[Surface]] — модификаторы поверхности

## Конфиги

- [[VehicleConfig]] — параметры транспорта

## Архитектура

- [[View-Binding]] — связь с Rigidbody
- [[Contexts]] — GameContext для Hero

## Пайплайн

```
Input → SteeringSystem → AccelerationSystem → VelocityApplySystem → Rigidbody
         ↓                                                            ↓
    SurfaceModifier ←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←←← CollisionHandler
```

## Системы

| Система | Роль |
|---------|------|
| VelocityApplySystem | Применение скорости |
| AccelerationSystem | Расчёт ускорения |
| SteeringSystem | Поворот |
| SurfaceModifierSystem | Эффекты поверхности |
| DragSystem | Сопротивление |
| SpeedLimitSystem | Ограничение скорости |
