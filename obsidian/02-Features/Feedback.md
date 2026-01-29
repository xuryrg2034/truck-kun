---
tags: [feature, ecs, vfx]
aliases: [FeedbackFeature, эффекты, VFX]
related: [[Collision]], [[HitEffectService]]
---

# Feedback Feature

Визуальные и звуковые эффекты.

## Системы

| Система | Назначение |
|---------|------------|
| HitFeedbackSystem | Эффекты попаданий |
| FloatingTextSystem | Всплывающий текст |
| ParticleCleanupSystem | Очистка партиклов |

## Сервисы

- [[HitEffectService]] — эффекты попаданий
- [[FloatingTextService]] — всплывающий текст

## Типы эффектов

| Событие | Эффект |
|---------|--------|
| Hit Pedestrian | Частицы + текст с очками |
| Combo | Увеличенный текст |
| Speed Boost | Trail эффект |

## Зависимости

- **Получает от:** [[Collision]] — события попаданий
- **Использует:** Tween система для анимаций

## TODO

- [ ] Camera Shake на hit
- [ ] Screen flash на violation
- [ ] Slowmo эффект

## Путь в коде

`Assets/Code/Gameplay/Features/Feedback/`
