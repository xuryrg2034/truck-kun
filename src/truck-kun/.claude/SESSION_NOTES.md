# Session Notes - Truck-kun Rising

> Заметки текущей сессии
> Обновляй в конце сессии или при важных решениях

---

## Текущая сессия: 2026-01-27

### Контекст задачи

**Цель:** Миграция движения героя на гибридную физику с Rigidbody

**Запрошено пользователем:**
1. Анализ текущей системы движения (RunnerHeroMoveSystem)
2. Создание Entitas компонентов для физики
3. Подготовка к интеграции Rigidbody

### Что сделано

1. ✅ **Анализ движения героя:**
   - `RunnerHeroMoveSystem` - кинематическое движение через WorldPosition
   - Forward: автоматически вперёд (Vector3.forward * speed * dt)
   - Lateral: по input.x с Clamp по границам дороги
   - `UpdateTransformPositionSystem` - синхронизация Transform.position

2. ✅ **Создан `PhysicsComponents.cs`:**
   - `RigidbodyComponent` - ссылка на Unity Rigidbody
   - `PhysicsVelocity` - целевая скорость
   - `PhysicsBody` - флаг физического объекта
   - `Acceleration` - forward, lateral, deceleration
   - `PhysicsDrag` - base, current
   - `SurfaceModifier` - friction, drag, surfaceType
   - `SurfaceZone` - триггер для поверхностей
   - `PhysicsConstraints` - min/max speed, road bounds
   - `PhysicsState` - текущее состояние (отладка)
   - `PhysicsImpact` - данные столкновения
   - `PhysicsSettings` - конфигурация
   - `SurfaceType` enum
   - Extension methods

3. ✅ Создана система контекста `.claude/`

4. ✅ Создан `NPCAnimator.cs` (VFX - приостановлено)

### Что осталось (Physics)

- [ ] Запустить Jenny для генерации кода
- [ ] Модифицировать `EntityBehaviour.cs` для привязки Rigidbody
- [ ] Создать `PhysicsHeroMoveSystem` (замена RunnerHeroMoveSystem)
- [ ] Модифицировать `UpdateTransformPositionSystem` для физики
- [ ] Настроить Rigidbody на префабе PlayerTruck
- [ ] Создать `PhysicsFeature` для группировки систем

### Что осталось (VFX - приостановлено)

- [ ] Camera shake
- [ ] Улучшенные hit particles
- [ ] UI анимации
- [ ] Trail + dust effects

### Важные решения

1. **Гибридная физика**: Rigidbody.velocity для героя, кинематика для NPC
2. **Границы дороги**: Clamp на velocity.x, не на позицию
3. **Surface модификаторы**: Отдельные компоненты для friction/drag
4. **PhysicsSettings**: Вынесено в отдельный класс для GameBalance

### План интеграции физики

```
Этап 1: ✅ Компоненты созданы
Этап 2: Генерация Jenny
Этап 3: EntityBehaviour + Rigidbody привязка
Этап 4: PhysicsHeroMoveSystem
Этап 5: UpdateTransformPositionSystem модификация
Этап 6: Префаб настройка
Этап 7: Тестирование
```

### Открытые вопросы

1. Использовать `rb.velocity =` или `rb.MovePosition()`?
2. Нужна ли интерполяция Rigidbody (Interpolate)?
3. Continuous или Discrete collision detection?

---

## Архив сессий

### Шаблон записи

```markdown
## Сессия: YYYY-MM-DD

### Контекст
- Что делали

### Решения
- Важные решения

### Для следующей сессии
- Что нужно помнить
```

---

(предыдущие сессии не задокументированы)
