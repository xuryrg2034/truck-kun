---
tags: [meta, claude, instructions]
aliases: [AI Instructions, Инструкции]
---

# Инструкции для Claude

Этот документ описывает правила обновления Obsidian-графа при работе с кодом проекта.

## Когда обновлять граф

### При создании новой Feature

1. Создать заметку `02-Features/[FeatureName].md`
2. Использовать шаблон ниже
3. Добавить wikilinks к зависимым Features и Services
4. Обновить таблицу в [[Home]]
5. **Если фича крупная** — создать `[FeatureName]-MOC.md` (Map of Content)

### При создании нового Service

1. Создать заметку `03-Services/[ServiceName].md`
2. Указать какие Features используют сервис
3. Добавить в таблицу [[Home]]

### При изменении Game Flow

1. Обновить соответствующую заметку в `04-Game-Flow/`
2. Проверить актуальность wikilinks

### При добавлении конфига

1. Создать заметку `05-Configs/[ConfigName].md`
2. Указать какие системы читают конфиг

---

## Шаблон Feature

```markdown
---
tags: [feature, ecs]
aliases: [FeatureName]
related: [[Dependency1]], [[Dependency2]]
---

# Feature Name

Краткое описание назначения.

## Системы

| Система | Назначение |
|---------|------------|
| [[System1]] | Описание |
| [[System2]] | Описание |

## Компоненты

- `ComponentName` — описание

## Зависимости

- **Использует:** [[Service1]], [[Config1]]
- **Триггерит:** [[Feature2]]
- **Зависит от:** [[Feature3]]

## Путь в коде

`Assets/Code/Gameplay/Features/[Name]/`
```

---

## Шаблон Service

```markdown
---
tags: [service, di]
aliases: [IServiceName]
---

# Service Name

Краткое описание.

## Интерфейс

```csharp
public interface IServiceName
{
    void Method1();
    int Property { get; }
}
```

## Используется в

- [[Feature1]]
- [[Feature2]]

## Путь в коде

`Assets/Code/Gameplay/Features/[Feature]/Services/`
```

---

## Шаблон Config

```markdown
---
tags: [config, scriptableobject]
---

# Config Name

Описание настроек.

## Поля

| Поле | Тип | Описание |
|------|-----|----------|
| Field1 | float | Описание |

## Используется в

- [[System1]]
- [[Service1]]

## Путь

`Assets/Resources/Configs/[Name].asset`
```

---

## Правила wikilinks

- Всегда используй `[[ИмяЗаметки]]` для связей
- Для систем внутри Feature можно не создавать отдельные заметки
- Для ключевых сервисов — обязательно отдельная заметка
- Aliases в frontmatter помогают находить заметки

---

## Шаблон MOC (Map of Content)

MOC — карта связей для крупной фичи. Позволяет открыть Local Graph (`Ctrl+Shift+G`) и увидеть только связанные заметки.

```markdown
---
tags: [moc, featurename]
aliases: [FeatureName MOC, FeatureName Map]
---

# FeatureName — Карта связей

> Открой **Local Graph** (Ctrl+Shift+G) чтобы увидеть только связи этой фичи.

## Core

- [[FeatureName]] — главная Feature

## Связанные Features

- [[RelatedFeature1]]
- [[RelatedFeature2]]

## Сервисы

- [[RelatedService]]

## Конфиги

- [[RelatedConfig]]

## Flow

[ASCII-диаграмма или описание потока данных]
```

**Когда создавать MOC:**
- Фича имеет 3+ связей с другими фичами
- Фича является центральной для механики (Physics, Economy, Pedestrian)

---

## Не забудь

После изменений в Obsidian — обновить `.claude/CHANGELOG.md`
