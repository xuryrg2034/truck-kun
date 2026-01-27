# Changelog - Truck-kun Rising

> Журнал изменений проекта
> **ОБНОВЛЯЙ ПОСЛЕ КАЖДОГО ИЗМЕНЕНИЯ КОДА!**

---

## Формат записи

```markdown
## YYYY-MM-DD HH:MM - [Краткое описание]

**Файлы:**
- `путь/к/файлу.cs` - что изменено

**Причина:** Зачем это сделано
**Детали:** Важные нюансы реализации
```

---

## 2026-01-27 17:15 - Создание физических компонентов Entitas

**Файлы:**
- `Assets/Code/Gameplay/PhysicsComponents.cs` - создан

**Причина:** Подготовка к миграции на гибридную физику с Rigidbody
**Детали:**
- `RigidbodyComponent` - ссылка на Unity Rigidbody
- `PhysicsVelocity` - целевая скорость
- `PhysicsBody` - флаг физического объекта
- `Acceleration` - параметры ускорения (forward, lateral, deceleration)
- `PhysicsDrag` - сопротивление (base, current)
- `SurfaceModifier` - модификаторы поверхности (friction, drag, type)
- `SurfaceZone` - триггер-зона поверхности
- `PhysicsConstraints` - ограничения скорости и границы дороги
- `PhysicsState` - текущее состояние (для отладки и эффектов)
- `PhysicsImpact` - данные столкновения
- `PhysicsSettings` - конфигурация для GameBalance
- `SurfaceType` enum - Normal, Oil, Grass, Ice, Puddle
- Extension methods для удобной работы

**Требуется:** Запустить Jenny для генерации кода Entitas

---

## 2026-01-27 16:30 - Создание системы контекста для Claude Code

**Файлы:**
- `AGENTS.md` - создан главный файл инструкций
- `.claude/CONTEXT.md` - обновлён с подробной структурой
- `.claude/ARCHITECTURE.md` - создана архитектурная документация
- `.claude/TODO.md` - обновлён с приоритетами
- `.claude/CHANGELOG.md` - обновлён формат
- `.claude/SESSION_NOTES.md` - создан файл заметок сессии

**Причина:** Создание системы автоматического восстановления контекста между сессиями
**Детали:** Claude Code теперь автоматически читает AGENTS.md и .claude/* при старте

---

## 2026-01-27 15:45 - Создание NPCAnimator

**Файлы:**
- `Assets/Code/Art/VFX/NPCAnimator.cs` - создан

**Причина:** Добавление процедурных анимаций для NPC (idle/walk)
**Детали:**
- `NPCAnimator` - компонент с idle sway и walk bob
- `NPCAnimationManager` - статический менеджер для управления
- `NPCAnimationSettings` - настраиваемые параметры
- Walk cycle: 2 сек loop с bob, sway, lean
- Idle: subtle sway + breathing effect

---

## 2026-01-27 15:00 - Начало работы над VFX системой

**Файлы:**
- Планирование VFX улучшений

**Причина:** Улучшение game feel
**Детали:**
- Запланированы: camera shake, улучшенные частицы, UI анимации
- DOTween установлен, но используется custom TweenSystem

---

## [Предыдущие изменения - сводка]

### Основные системы (реализованы ранее)

**ECS Архитектура:**
- `EcsBootstrap.cs` - инициализация Entitas
- `BattleFeature.cs` - главная Feature
- `SystemFactory.cs` - Zenject фабрика

**Геймплей:**
- `HeroFeature.cs` - движение грузовика
- `PedestrianFeature.cs` - NPC система
- `CollisionFeature.cs` - столкновения
- `FeedbackSystem.cs` - частицы, звуки, текст
- `QuestFeature.cs` - система квестов
- `EconomyFeature.cs` - экономика

**UI:**
- `MainMenuUI.cs` - главное меню
- `HubUI/*` - панели хаба
- `SettingsPanel.cs` - настройки

**Инфраструктура:**
- `SaveSystem.cs` - сохранение
- `SceneManagement.cs` - переходы
- `CameraFollow.cs` - камера

**Визуал:**
- `ProceduralMeshGenerator.cs` - генерация моделей NPC
- `TweenSystem.cs` - кастомные анимации

### Git коммиты (из истории)

- `facf32d` - Замена placeholder моделей
- `4349a48` - Debug UI и чит-коды
- `2041270` - Изменён тип камеры
- `2d98efa` - Скрипт создания сцен
- `6374f78` - Главное меню

---

## Архив (старые записи)

> Когда записей станет больше 30, перемести старые сюда

(пусто)
