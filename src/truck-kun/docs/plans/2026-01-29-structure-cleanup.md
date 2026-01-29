# Structure Cleanup Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Fix namespace inconsistencies and consolidate debug-related files.

**Architecture:** Move files to correct locations and update namespaces to match folder structure.

**Tech Stack:** Unity C#, no external dependencies.

---

## Summary

Current issues:
1. `Debug/` files use wrong namespace `Code.DebugTools` (should be `Code.Debug`)
2. `DevTools/DebugService.cs` should be in `Debug/` folder
3. After cleanup, delete empty `DevTools/` folder

---

### Task 1: Read DebugPhysicsController.cs

**Files:**
- Read: `Assets/Code/Debug/DebugPhysicsController.cs`

**Action:** Read the file to see current namespace.

---

### Task 2: Fix DebugPhysicsController namespace

**Files:**
- Modify: `Assets/Code/Debug/DebugPhysicsController.cs`

**Action:** Change `namespace Code.DebugTools` to `namespace Code.Debug`

---

### Task 3: Read DebugPhysicsVisualizer.cs

**Files:**
- Read: `Assets/Code/Debug/DebugPhysicsVisualizer.cs`

**Action:** Read the file to see current namespace.

---

### Task 4: Fix DebugPhysicsVisualizer namespace

**Files:**
- Modify: `Assets/Code/Debug/DebugPhysicsVisualizer.cs`

**Action:** Change `namespace Code.DebugTools` to `namespace Code.Debug`

---

### Task 5: Read DebugPhysicsUI.cs

**Files:**
- Read: `Assets/Code/Debug/DebugPhysicsUI.cs`

**Action:** Read the file to see current namespace.

---

### Task 6: Fix DebugPhysicsUI namespace

**Files:**
- Modify: `Assets/Code/Debug/DebugPhysicsUI.cs`

**Action:** Change `namespace Code.DebugTools` to `namespace Code.Debug`

---

### Task 7: Read DebugService.cs

**Files:**
- Read: `Assets/Code/DevTools/DebugService.cs`

**Action:** Read the file to see current content and namespace.

---

### Task 8: Move DebugService.cs to Debug folder

**Files:**
- Move: `Assets/Code/DevTools/DebugService.cs` -> `Assets/Code/Debug/DebugService.cs`

**Action:** Use git mv command:
```bash
git mv "Assets/Code/DevTools/DebugService.cs" "Assets/Code/Debug/DebugService.cs"
git mv "Assets/Code/DevTools/DebugService.cs.meta" "Assets/Code/Debug/DebugService.cs.meta"
```

---

### Task 9: Fix DebugService namespace

**Files:**
- Modify: `Assets/Code/Debug/DebugService.cs`

**Action:** Change `namespace Code.DevTools` to `namespace Code.Debug`

---

### Task 10: Find files using Code.DevTools

**Action:** Search for `using Code.DevTools` in all .cs files.

```bash
grep -r "using Code.DevTools" Assets/Code/
```

---

### Task 11: Update usings from Code.DevTools to Code.Debug

**Files:**
- Modify: Any files found in Task 10

**Action:** Replace `using Code.DevTools;` with `using Code.Debug;`

---

### Task 12: Find files using Code.DebugTools

**Action:** Search for `using Code.DebugTools` in all .cs files.

```bash
grep -r "using Code.DebugTools" Assets/Code/
```

---

### Task 13: Update usings from Code.DebugTools to Code.Debug

**Files:**
- Modify: Any files found in Task 12

**Action:** Replace `using Code.DebugTools;` with `using Code.Debug;`

---

### Task 14: Delete empty DevTools folder

**Action:**
```bash
rm -rf "Assets/Code/DevTools"
rm -f "Assets/Code/DevTools.meta"
```

---

### Task 15: Verify changes with git status

**Action:** Run `git status` to see all changes.

---

### Task 16: Commit all changes

**Action:**
```bash
git add -A
git commit -m "refactor: consolidate debug files and fix namespaces

- Move DebugService from DevTools to Debug folder
- Update all Debug files to use Code.Debug namespace
- Remove empty DevTools folder"
```

---

## Verification Checklist

- [ ] All files in `Debug/` folder use `Code.Debug` namespace
- [ ] `DevTools/` folder deleted
- [ ] No references to `Code.DevTools` or `Code.DebugTools` remain
- [ ] Git commit created
