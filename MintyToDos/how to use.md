
# MintyToDos CLI 

**MintyToDos** is a simple and customizable command-line task manager written in C#.

## Usage

```bash
  MintyToDos <command> [options]
```

If no arguments are provided, it displays usage instructions.

---

## Commands

### `help` / `h`
Displays general help or command-specific help.

```bash
  MintyToDos help
  MintyToDos help <command>
```

Examples:
```bash
  MintyToDos help list
```

---

### `create` / `c`
Creates a new task with a name, priority and description.

```bash
  MintyToDos create <TaskName> -p <Priority> -d <Description>
```

#### Options:
- `-p`, `--set-priority`: Sets the task priority (integer).
- `-d`, `--set-description`: Sets the task description.

#### Example:
```bash
  MintyToDos create "Play Undertale" -p 2 -d "Play the entire game in true-pacifist route"
```

---

###  `list` / `l`
Lists all stored tasks. You can sort by different criteria.

```bash
  MintyToDos list [options]
```

#### Options:
- `-p`, `--task-priority`: Sort by priority.
- `-d`, `--task-completed`: Show completed tasks first.
- `-w`, `--task-wip`: Show in-progress tasks first.
- `-c`, `--task-canceled`: Show canceled tasks first.

#### Example:
```bash
  MintyToDos list -p
```

---

### `delete` / `d`
Deletes an existing task by name.

```bash
  MintyToDos delete <TaskName>
```

#### Example:
```bash
  MintyToDos delete "Play Undertale"
```

---

### ️`change` / `ch`
Modifies a task’s name, description, or status.

```bash
  MintyToDos change <TaskName> [options]
```

#### Options:
- `-D`, `--mark-as-done`: Marks the task as completed ✅
- `-c`, `--mark-as-canceled`: Marks the task as canceled ❌
- *(No status flag)*: Sets the task as in-progress 🔄
- `-d`, `--description <text>`: Updates the task description.
- `-n`, `--new-name <name>`: Renames the task.

#### Examples:
```bash
  MintyToDos change "Play Undertale" -D
  MintyToDos change "Play Undertale" -n "Play Deltarune" -d "Chapter 4 only"
```

---

## Data Location

All data is stored under:

```
    %AppData%\MintyToDos\
```

Includes:
- `settings.json`: app settings.
- `task-*.json`: each task is stored as a separate file[^1].

---

## Example Session

```bash
    MintyToDos create "Feed Cat" -p 1 -d "He is hungry!!"
    MintyToDos list
    MintyToDos change "Feed Cat" -D
    MintyToDos delete "Feed Cat"
```

---


**MintyToDos CLI v1.0a**  
###### _Made with 💜 by Minty_
[^1]: Each file is named with the format task-{GUIDV7}.json
