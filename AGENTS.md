# Codex Agent Rules

These rules apply to all Codex CLI sessions in this repository.

## General

* Do not change public APIs unless explicitly requested.
* Do not modify configuration files or CI/CD unless explicitly approved.
* If something is unclear or ambiguous, stop and ask before proceeding.

## Workflow

* Always start by proposing a plan.
* List all files that will be read and modified.
* Do not apply any changes until explicit confirmation is given.
* Always show a diff before applying changes.

## Code Changes

* Preserve existing behavior unless stated otherwise.
* Do not introduce new dependencies unless explicitly requested.
* Keep changes minimal and focused on the requested task.
* Do not refactor unrelated code.

## Commands

* Before running any shell commands, list them first.
* Do not run commands without explicit approval.

## Tests

* Use the existing testing framework.
* Add or update tests only when explicitly requested.
* Prefer testing public interfaces over internal implementation details.

## Superpowers Integration

This repository uses **superpowers** as command-style prompt templates.

### Directory layout (current)

Superpowers are stored in a global location:

* `C:\Users\xuryr\.codex\superpowers\commands\*.md`
* `C:\Users\xuryr\.codex\superpowers\skills\*\SKILL.md` (may exist, but commands are primary)

Example:

* `C:\Users\xuryr\.codex\superpowers\commands\brainstorm.md`
* `C:\Users\xuryr\.codex\superpowers\commands\write-plan.md`
* `C:\Users\xuryr\.codex\superpowers\commands\execute-plan.md`

### Command invocation syntax

The user may invoke a superpower command by placing **one command tag on its own line**:

```
#<command-name>
```

Examples:

* `#brainstorm`
* `#write-plan`
* `#execute-plan`

### Resolution rules

When a command tag is present:

1. Read `C:\Users\xuryr\.codex\superpowers\commands\<command-name>.md`
2. Treat its contents as **mandatory system instructions** for this request
3. Apply the command before any other reasoning or actions

If multiple command tags are present, apply them **in the order they appear**.

### Priority order

Instructions must be applied in the following strict order:

1. Explicit user instructions in the current message
2. Active superpowers command files (`commands/*.md`)
3. This `AGENTS.md`

If there is a conflict, higher priority always wins.

### Safety rules

* Do not guess or infer a command
* Do not auto-activate commands
* Do not combine commands unless explicitly requested

---

## Windows Environment Notes

This project is used on **Windows**.

* Assume PowerShell as the default shell.
* Use Windows-compatible commands only.
* Do not rely on Unix-only tools (`sed`, `awk`, `grep`, `xargs`, etc.) unless explicitly confirmed.
* Prefer:

  * `Get-ChildItem` instead of `ls`
  * `Select-String` instead of `grep`
  * `npm`, `pnpm`, `dotnet`, or project-specific scripts

If a command may behave differently on Windows, explain it before execution.
