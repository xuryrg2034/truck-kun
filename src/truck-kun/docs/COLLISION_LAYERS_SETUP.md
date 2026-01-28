# Настройка Collision Layers в Unity

## Слои

Настройте следующие слои в **Edit → Project Settings → Tags and Layers**:

| Layer | Номер | Описание |
|-------|-------|----------|
| Hero | 8 | Грузовик игрока |
| Pedestrian | 9 | Пешеходы |
| Obstacle | 10 | Дорожные препятствия |
| Ground | 11 | Земля/дорога |

## Layer Collision Matrix

Настройте матрицу в **Edit → Project Settings → Physics → Layer Collision Matrix**:

```
           Hero  Pedestrian  Obstacle  Ground
Hero        -       ✓          ✓         ✓
Pedestrian  ✓       -          ✓         ✓
Obstacle    ✓       ✓          -         ✓
Ground      ✓       ✓          ✓         -
```

### Расшифровка:
- **Hero ↔ Pedestrian**: ДА (столкновения с пешеходами)
- **Hero ↔ Obstacle**: ДА (столкновения с препятствиями)
- **Hero ↔ Ground**: ДА (грузовик на земле)
- **Pedestrian ↔ Pedestrian**: НЕТ (пешеходы не сталкиваются друг с другом)
- **Pedestrian ↔ Obstacle**: ДА (пешеходы обходят/взаимодействуют с препятствиями)
- **Pedestrian ↔ Ground**: ДА (пешеходы ходят по земле)
- **Obstacle ↔ Obstacle**: НЕТ
- **Obstacle ↔ Ground**: ДА (препятствия на земле)

## Присвоение слоёв объектам

### Hero (Грузовик)
```
Prefab: Assets/Prefabs/Hero/Truck.prefab
Layer: Hero (8)
```

### Pedestrians (Пешеходы)
В `PedestrianFactory.CreateFromPrefab()` уже установлен слой.
```csharp
pedestrian.layer = LayerMask.NameToLayer("Pedestrian");
```

### Obstacles (Препятствия)
При создании префабов препятствий:
```
Layer: Obstacle (10)
```

### Ground (Земля)
```
Объекты дороги: Layer: Ground (11)
```
