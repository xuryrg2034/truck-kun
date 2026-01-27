# Session Notes - Truck-kun Rising

> Заметки текущей сессии
> Обновляй в конце сессии или при важных решениях

---

## Текущая сессия: 2026-01-27

### Контекст задачи

**Цель:** Добавить анимации и VFX эффекты для улучшения game feel

**Запрошено пользователем:**
1. NPC анимации (walk cycle 2s, idle sway)
2. Улучшенные hit particles (искры, обломки)
3. Trail за грузовиком + dust clouds
4. UI анимации (fade, scale bounce) с DOTween
5. Camera shake при столкновении

### Что сделано

1. ✅ Создан `NPCAnimator.cs`:
   - Idle анимация: sway + breathing
   - Walk анимация: bob + sway + lean (2 sec cycle)
   - `NPCAnimationManager` для управления
   - `NPCAnimationSettings` для конфигурации

2. ✅ Создана система контекста `.claude/`:
   - `AGENTS.md` - главные инструкции
   - `CONTEXT.md` - структура проекта
   - `ARCHITECTURE.md` - архитектура
   - `TODO.md` - задачи
   - `CHANGELOG.md` - история
   - `SESSION_NOTES.md` - этот файл

### Что осталось

- [ ] Camera shake
- [ ] Улучшенные hit particles
- [ ] UI анимации
- [ ] Trail + dust effects
- [ ] Интеграция NPCAnimator с PedestrianFactory

### Важные решения

1. **TweenSystem vs DOTween**: Проект использует custom TweenSystem. DOTween установлен как fallback. Решение: продолжать с custom для простых анимаций.

2. **NPCAnimator подход**: Процедурные анимации через код вместо Unity Animator. Причина: все модели процедурные, нет Animator Controllers.

3. **Структура VFX**: Все VFX в `Assets/Code/Art/VFX/`. Конфигурация в `GameBalance.FeedbackBalance`.

### Заметки для следующей сессии

- NPCAnimator создан, но не интегрирован в PedestrianFactory
- Нужно добавить вызов `NPCAnimationManager.AttachAnimator()` при создании NPC
- CrossingPedestrian должен автоматически включать walking state
- Camera shake - следующий приоритет (максимальный импакт при минимальных усилиях)

### Открытые вопросы

1. Какой визуальный стиль trail предпочтительнее - tire marks или energy trail?
2. Нужна ли настройка VFX через GameBalance или хардкод достаточно?
3. Cinemachine для camera shake или custom implementation?

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
