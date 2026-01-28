# AudioService - Руководство по использованию

## Быстрый старт

AudioService инициализируется автоматически при запуске игры через `AudioBootstrap`.

### Воспроизведение звуков

```csharp
using Code.Audio;

// Простое воспроизведение SFX
Audio.PlaySFX(SFXType.Hit);
Audio.PlaySFX(SFXType.UIClick);

// Воспроизведение в позиции (для 3D звуков)
Audio.PlaySFX(SFXType.Collision, transform.position);

// Специальные хелперы
Audio.UIClick();           // Клик по UI кнопке
Audio.Hit(0.8f);           // Удар с интенсивностью (0-1), автоматически выбирает Hit или HitStrong
```

### Воспроизведение музыки

```csharp
using Code.Audio;

// Запуск музыки
Audio.PlayMusic(MusicType.Gameplay);
Audio.PlayMusic(MusicType.MainMenu);

// Остановка музыки
Audio.StopMusic();
```

## Доступные типы звуков

### SFXType (звуковые эффекты)

| Тип | Описание |
|-----|----------|
| `Hit` | Обычный удар |
| `HitStrong` | Сильный удар |
| `Collision` | Столкновение |
| `Ragdoll` | Падение рэгдолла |
| `UIClick` | Клик по кнопке |
| `UIHover` | Наведение на кнопку |
| `UIBack` | Кнопка "Назад" |
| `Purchase` | Покупка |
| `CoinPickup` | Подбор монеты |
| `MoneyLoss` | Потеря денег |
| `QuestComplete` | Выполнение квеста |
| `QuestFailed` | Провал квеста |
| `QuestNew` | Новый квест |
| `Violation` | Нарушение (сбит защищённый NPC) |
| `Warning` | Предупреждение |
| `Success` | Успех |
| `EngineStart` | Запуск двигателя |
| `EngineLoop` | Работа двигателя |
| `Brake` | Торможение |
| `Skid` | Занос |

### MusicType (музыка)

| Тип | Описание |
|-----|----------|
| `None` | Без музыки |
| `MainMenu` | Главное меню |
| `Gameplay` | Геймплей |
| `Hub` | Хаб/магазин |
| `GameOver` | Проигрыш |
| `Victory` | Победа |

## Компоненты для сцен

### UIButtonSound

Автоматически воспроизводит звук при клике на кнопку.

```csharp
// Добавьте на GameObject с Button
[RequireComponent(typeof(Button))]
public class UIButtonSound : MonoBehaviour
```

В инспекторе:
- `Click Sound` — звук клика (по умолчанию UIClick)
- `Hover Sound` — звук наведения (по умолчанию UIHover)
- `Play Hover Sound` — включить звук наведения

### SceneMusic

Автоматически запускает музыку при загрузке сцены.

```csharp
// Добавьте на корневой объект сцены
public class SceneMusic : MonoBehaviour
```

В инспекторе:
- `Music Type` — тип музыки
- `Play On Start` — воспроизводить при старте
- `Start Delay` — задержка перед началом

### TriggerSound

Воспроизводит звук при входе в триггер.

```csharp
// Добавьте на объект с Collider (isTrigger = true)
[RequireComponent(typeof(Collider))]
public class TriggerSound : MonoBehaviour
```

В инспекторе:
- `Sound` — тип звука
- `Play Once` — воспроизвести только один раз
- `Trigger Tag` — тег объекта-триггера (по умолчанию "Player")

## Настройка AudioLibrary

1. Создайте AudioLibrary: **Assets → Create → Audio → Audio Library**
2. Разместите в `Assets/Resources/` для автозагрузки
3. Заполните массивы `Music Clips` и `SFX Clips`:
   - Выберите тип (MusicType/SFXType)
   - Назначьте AudioClip
   - Для SFX можно настроить `Volume Multiplier`

## Управление громкостью

```csharp
// Через AudioService напрямую
AudioService.Instance.SetMasterVolume(0.8f);
AudioService.Instance.SetMusicVolume(0.5f);
AudioService.Instance.SetSFXVolume(0.7f);

// Получение текущих значений
float music = AudioService.Instance.GetMusicVolume();
float sfx = AudioService.Instance.GetSFXVolume();
```

Громкость автоматически сохраняется в PlayerPrefs:
- `Audio_Master`
- `Audio_Music`
- `Audio_SFX`

## Архитектура

```
Code/Audio/
├── AudioService.cs      # Основной сервис (синглтон, DontDestroyOnLoad)
│   ├── MusicType        # Enum типов музыки
│   ├── SFXType          # Enum типов SFX
│   ├── AudioSettings    # Настройки громкости и пула
│   ├── IAudioService    # Интерфейс сервиса
│   ├── AudioService     # Реализация с пулингом
│   ├── AudioLibrary     # ScriptableObject с клипами
│   └── AudioBootstrap   # Автоинициализация
│
└── AudioHelpers.cs      # Статические хелперы и компоненты
    ├── Audio            # Статический класс для быстрого доступа
    ├── UIButtonSound    # Компонент для кнопок
    ├── SceneMusic       # Компонент для музыки сцены
    └── TriggerSound     # Компонент для триггеров
```

## Особенности

- **Пулинг SFX**: Создаётся пул AudioSource для эффективного воспроизведения
- **Кроссфейд музыки**: Плавный переход между треками
- **Лимит одновременных звуков**: Защита от перегрузки (по умолчанию 5)
- **Автоинициализация**: Не требует ручной настройки сцены
- **Сохранение настроек**: Громкость сохраняется между сессиями

## Пример интеграции в геймплей

```csharp
using Code.Audio;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        float force = collision.relativeVelocity.magnitude;

        // Автоматически выбирает Hit или HitStrong
        Audio.Hit(force / 10f);
    }

    void OnQuestComplete()
    {
        Audio.PlaySFX(SFXType.QuestComplete);
    }

    void OnPurchase()
    {
        Audio.PlaySFX(SFXType.Purchase);
    }
}
```
