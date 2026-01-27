# Truck-kun Rising - Project Context

> Последнее обновление: 2026-01-27
> Обновляй при изменении структуры или добавлении модулей

---

## Описание проекта

**Truck-kun Rising** — аркадный раннер в стиле isekai, где игрок управляет грузовиком и "отправляет" пешеходов в другой мир. Тёмный юмор сочетается с системой квестов и экономической мета-игрой.

### Основная механика
- Автоматическое движение вперёд
- Игрок управляет только боковым перемещением
- Цель: сбивать "правильных" пешеходов (квесты)
- Избегать "защищённых" (штрафы)
- Зарабатывать деньги, покупать улучшения, выживать

---

## Технологический стек

| Технология | Версия/Детали | Назначение |
|------------|---------------|------------|
| Unity | 2022.3+ LTS | Игровой движок |
| Render Pipeline | URP | Графика |
| C# | .NET Standard 2.1 | Язык |
| ECS Framework | Entitas | Архитектура |
| DI Container | Zenject | Инъекция зависимостей |
| Tweens | Custom TweenSystem | Анимации (DOTween установлен, но не используется) |
| Audio | Procedural | Процедурная генерация звуков |
| Models | Procedural | Процедурная генерация моделей NPC |

---

## Структура директорий

```
Assets/
├── Code/                           # Весь исходный код
│   ├── Art/                        # Визуальные системы
│   │   ├── ProceduralMeshGenerator.cs   # Генерация мешей NPC
│   │   ├── ModelFactory.cs              # Фабрика моделей
│   │   └── VFX/                         # Визуальные эффекты
│   │       ├── TweenSystem.cs           # Система анимаций (кастомная)
│   │       ├── NPCAnimator.cs           # Анимации NPC (idle/walk)
│   │       └── [планируются другие VFX]
│   │
│   ├── Balance/                    # Конфигурация игры
│   │   ├── GameBalance.cs               # ВСЕ настройки игры (ScriptableObject)
│   │   └── Editor/
│   │       └── GameBalanceEditor.cs     # Кастомный инспектор
│   │
│   ├── Common/                     # Общие компоненты и сервисы
│   │   ├── CommonComponents.cs          # Базовые ECS компоненты
│   │   ├── EntityHelpers.cs             # Хелперы для сущностей
│   │   └── Services.cs                  # ITimeService, IIdentifierService
│   │
│   ├── Gameplay/                   # Игровая логика (ECS Features)
│   │   ├── BattleFeature.cs             # Главная Feature (порядок систем!)
│   │   ├── InputFeature.cs              # Обработка ввода
│   │   ├── HeroFeature.cs               # Логика грузовика
│   │   ├── PedestrianFeature.cs         # NPC: спавн, движение, типы
│   │   ├── CollisionFeature.cs          # Обнаружение столкновений
│   │   ├── FeedbackSystem.cs            # Частицы, звуки, текст
│   │   ├── MovementFeature.cs           # Применение движения
│   │   ├── QuestFeature.cs              # Система квестов
│   │   ├── EconomyFeature.cs            # Деньги, награды, штрафы
│   │   └── DaySession.cs                # Цикл дня
│   │
│   ├── Infrastructure/             # Инфраструктура
│   │   ├── EcsBootstrap.cs              # Инициализация ECS
│   │   ├── SystemFactory.cs             # Zenject фабрика систем
│   │   ├── CameraFollow.cs              # Следование камеры
│   │   ├── ViewSystems.cs               # Привязка View к Entity
│   │   ├── SaveSystem.cs                # Сохранение/загрузка
│   │   ├── SceneManagement.cs           # Переходы между сценами
│   │   └── EntityBehaviour.cs           # MonoBehaviour-обёртка Entity
│   │
│   ├── UI/                         # Пользовательский интерфейс
│   │   ├── MainMenu/
│   │   │   └── MainMenuUI.cs            # Главное меню
│   │   ├── HubUI/
│   │   │   ├── HubMainUI.cs             # UI хаба
│   │   │   ├── HubPanelBase.cs          # Базовый класс панелей
│   │   │   ├── UpgradePanel.cs          # Панель улучшений
│   │   │   ├── QuestBoardPanel.cs       # Доска квестов
│   │   │   └── FoodPanel.cs             # Покупка еды
│   │   ├── QuestUI/
│   │   │   ├── QuestUIController.cs     # Отображение квестов
│   │   │   └── QuestItemView.cs         # Элемент квеста
│   │   ├── EndDayScreen/
│   │   │   └── EndDayController.cs      # Экран конца дня
│   │   └── Settings/
│   │       ├── SettingsPanel.cs         # Настройки
│   │       └── PauseMenu.cs             # Меню паузы
│   │
│   ├── Meta/                       # Мета-игра
│   │   ├── UpgradeSystem.cs             # Система улучшений
│   │   └── DifficultySystem.cs          # Прогрессия сложности
│   │
│   ├── Hub/                        # Сцена хаба
│   │   ├── HubController.cs             # Контроллер хаба
│   │   ├── HubBootstrap.cs              # Инициализация хаба
│   │   ├── HubUIManager.cs              # Управление UI хаба
│   │   └── InteractableZone.cs          # Интерактивные зоны
│   │
│   ├── DevTools/                   # Инструменты разработки
│   │   ├── DebugService.cs              # Сервис отладки
│   │   ├── DebugPanel.cs                # UI отладки
│   │   └── DebugOverlay.cs              # Оверлей с информацией
│   │
│   ├── Editor/                     # Редактор Unity
│   │   ├── SceneCreator.cs              # Создание сцен
│   │   └── MeshSaver.cs                 # Сохранение мешей
│   │
│   └── Generated/                  # Entitas кодогенерация (НЕ РЕДАКТИРОВАТЬ!)
│       ├── Game/                        # GameContext компоненты
│       ├── Meta/                        # MetaContext компоненты
│       └── Input/                       # InputContext компоненты
│
├── Plugins/                        # Внешние плагины
│   ├── Demigiant/DOTween/              # DOTween (установлен, не используется)
│   └── Zenject/                        # DI контейнер
│
├── Prefabs/                        # Префабы
│   ├── HeroSpawner.prefab
│   ├── PlayerTruck.prefab
│   └── Pedestrians/
│
├── Materials/                      # Материалы
│   └── Pedestrians/
│
├── Resources/                      # Загружаемые ресурсы
│   ├── Configs/
│   │   └── GameBalance.asset           # Конфигурация игры
│   └── DOTweenSettings.asset
│
├── Entitas/                        # Entitas конфиги
│   └── Jenny/                          # Кодогенератор
│
└── Scenes/                         # Сцены
    ├── MainMenu.unity
    ├── Hub.unity
    └── Battle.unity
```

---

## Ключевые файлы и их роль

| Файл | Роль | Когда изменять |
|------|------|----------------|
| `GameBalance.cs` | ВСЕ игровые параметры | Баланс, настройки |
| `EcsBootstrap.cs` | Создание ECS контекста | Инициализация |
| `BattleFeature.cs` | Порядок выполнения систем | Добавление систем |
| `PedestrianFeature.cs` | Вся логика NPC | NPC механики |
| `CollisionFeature.cs` | Столкновения | Hit detection |
| `FeedbackSystem.cs` | Визуальный/аудио feedback | VFX, звуки |
| `TweenSystem.cs` | Кастомные анимации | Новые типы твинов |

---

## Стандарты кодирования

### Именование

```csharp
// Компоненты - без суффикса, атрибут контекста
[Game] public class Pedestrian : IComponent { }
[Game] public class PedestrianType : IComponent { public PedestrianKind Value; }

// Системы - суффикс System
public class PedestrianSpawnSystem : IExecuteSystem { }
public class HitFeedbackSystem : ReactiveSystem<GameEntity> { }

// Features - суффикс Feature, sealed
public sealed class PedestrianFeature : Feature { }

// Сервисы - интерфейс с I, класс с суффиксом Service
public interface ITimeService { }
public class TimeService : ITimeService { }

// Settings - суффикс Settings (классы конфигурации)
public class PedestrianSpawnSettings { }
```

### Структура файлов

```csharp
// Порядок в файле:
// 1. #region Enums
// 2. #region Components
// 3. #region Settings
// 4. #region Factory/Service
// 5. #region Feature
// 6. #region Systems
// 7. #region Extensions
```

### Паттерны ECS

```csharp
// ReactiveSystem - для событий
protected override ICollector<GameEntity> GetTrigger(IContext<GameEntity> context)
    => context.CreateCollector(GameMatcher.SomeEvent.Added());

// IExecuteSystem - для per-frame логики
public void Execute() { /* каждый кадр */ }

// IInitializeSystem - для инициализации
public void Initialize() { /* один раз при старте */ }

// ICleanupSystem - для очистки в конце кадра
public void Cleanup() { /* удаление событий */ }
```

---

## Важные зависимости

```
InputFeature
    ↓ (создаёт MoveInput)
HeroFeature
    ↓ (читает MoveInput, двигает Hero)
PedestrianFeature
    ↓ (спавнит NPC, двигает crossing)
CollisionFeature
    ↓ (проверяет Hit, создаёт HitEvent)
FeedbackFeature
    ↓ (реагирует на HitEvent → частицы, звук)
QuestFeature + EconomyFeature
    ↓ (обновляют прогресс и деньги)
BindViewFeature + MovementFeature
    ↓ (синхронизируют Transform с WorldPosition)
```

---

## Типы пешеходов

| Тип | Категория | Скорость | Масштаб | Цвет | Награда |
|-----|-----------|----------|---------|------|---------|
| StudentNerd | Normal | 2.0 | 0.85 | Голубой | Квест |
| Salaryman | Normal | 1.8 | 1.0 | Серый | Квест |
| Teenager | Normal | 2.2 | 0.95 | Зелёный | Квест |
| Grandma | **Protected** | 0.8 | 0.8 | Розовый | -100¥ |
| OldMan | **Protected** | 0.9 | 0.9 | Коричневый | -100¥ |

---

## Экономика

| Параметр | Значение |
|----------|----------|
| Стартовые деньги | 1000¥ |
| Стоимость еды/день | 100¥ |
| Штраф за protected | 100¥ |
| Базовая награда квеста | 50¥ |
| Награда за цель | 10¥ |
