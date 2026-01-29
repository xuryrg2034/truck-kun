---
tags: [feature, ecs, quest]
aliases: [QuestFeature, квесты]
related: [[QuestService]], [[Pedestrian]]
---

# Quest Feature

Ежедневные квесты — цели на сбор определённых пешеходов.

## Системы

| Система | Назначение |
|---------|------------|
| QuestProgressSystem | Обновление прогресса |
| QuestCompletionSystem | Проверка завершения |
| QuestRewardSystem | Выдача наград |

## Компоненты (MetaContext)

- `DailyQuest` — данные квеста (тип цели, количество)
- `QuestProgress` — текущий прогресс
- `isActiveQuest` — флаг активности
- `isQuestCompleted` — флаг завершения

## Сервисы

- [[QuestService]] — управление квестами

## Зависимости

- **Получает от:** [[Collision]] — события попаданий
- **Использует:** [[Pedestrian]] — типы пешеходов
- **Триггерит:** [[Economy]] — награда за квест

## UI

- `QuestUIController` — панель квестов
- `QuestItemView` — элемент квеста

## Путь в коде

`Assets/Code/Gameplay/Features/Quest/`
