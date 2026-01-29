# Truck-kun Rising

> Аркадный раннер в стиле isekai. Entitas ECS + Zenject + Unity 6.

---

## Документация

📖 **Obsidian Knowledge Base**: `C:\Projects\home\unity\Truck-kun\obsidian\`

> Граф архитектуры, фич и сервисов. Открой как Obsidian vault.

📖 **Архитектура (legacy)**: [ECS_PATTERN.md](./ECS_PATTERN.md)

---

## Quick Reference

| Цель | Файл |
|------|------|
| Конфигурация уровня | `Assets/Code/Configs/LevelConfig.cs` |
| Конфигурация транспорта | `Assets/Code/Configs/VehicleConfig.cs` |
| Runtime статы транспорта | `Assets/Code/Gameplay/Features/Hero/VehicleStats.cs` |
| Точка входа ECS | `Assets/Code/Infrastructure/Bootstrap/EcsBootstrap.cs` |
| Порядок систем | `Assets/Code/Gameplay/BattleFeature.cs` |
| NPC логика | `Assets/Code/Gameplay/Features/Pedestrian/PedestrianFeature.cs` |
| Физика героя | `Assets/Code/Gameplay/Features/Physics/PhysicsFeature.cs` |
| Эффекты | `Assets/Code/Gameplay/Features/Feedback/FeedbackFeature.cs` |
| Анимации | `Assets/Code/Art/VFX/TweenSystem.cs` |

---

## TODO

### P1 - Высокий приоритет

- [ ] NPC Анимации — интеграция с PedestrianFactory
- [ ] Camera Shake — триггер на HitEvent
- [ ] Улучшенные Hit Particles

### P2 - Средний приоритет

- [ ] UI Анимации (fade, scale bounce)
- [ ] Movement VFX (trail, dust)
- [ ] Audio Polish

### P3 - Низкий приоритет

- [ ] Slowmo на hit
- [ ] Screen flash на violation
- [ ] Combo visual feedback

### Технический долг

- [ ] Object pooling для ParticleSystem
- [ ] Кэширование процедурных мешей
- [ ] Unit тесты для экономики

---

## Правила для AI

### После КАЖДОГО изменения кода:

| Действие | Когда |
|----------|-------|
| Обновить `CHANGELOG.md` | **ВСЕГДА** |
| Обновить этот файл | При изменении архитектуры/структуры |

### Формат записи в CHANGELOG:

```markdown
## YYYY-MM-DD HH:MM - [Краткое описание]

**Файлы:**
- `path/to/file.cs` - что изменено

**Причина:** Зачем
**Детали:** Нюансы реализации
```

### При неопределённости:

1. Проверь этот файл и CHANGELOG
2. Изучи существующий код — найди похожие паттерны
3. Проверь Obsidian vault для архитектурных вопросов
4. Спроси пользователя

---

## Obsidian Graph

При создании/изменении фич — обновляй Obsidian vault:

| Действие | Что обновить |
|----------|--------------|
| Новая Feature | `obsidian/02-Features/[Name].md` + связи |
| Новый Service | `obsidian/03-Services/[Name].md` |
| Изменение Flow | `obsidian/04-Game-Flow/*.md` |
| Новый Config | `obsidian/05-Configs/[Name].md` |

**Формат заметок**: см. `obsidian/00-Index/Claude-Instructions.md`
