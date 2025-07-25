using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;

namespace MintyToDos;

static class Program
{
    private static Settings _settings = null!;
    private static string _workingPath = null!;
    static void Main(string[] args)
    {
        InitWorkingFolder();
        LoadSettings();
        
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: \n\tMintyToDos help");
            Environment.Exit(-1);
        }

        ShowProgramInfo();
        
        switch (args[0].ToLower())
        {
            case "h":
            case "help":
                ShowHelp(args.Skip(1).ToImmutableList());
                break;
            
            case "c":
            case "create":
                if (!CreateTask(args.Skip(1).ToImmutableList()))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to create task");
                    Console.ResetColor();
                    Environment.Exit(-2);
                }
                break;
            
            case "l":
            case "list":
                ListTasks(args.Skip(1).ToImmutableList());
                break;
            
            case "d":
            case "delete":
                if (!DeleteTask(args.Skip(1).ToImmutableList()))
                {
                    Console.ResetColor();
                    Environment.Exit(-2);
                }
                break;
            
            case "ch":
            case "change":
                if (!ModifyTask(args.Skip(1).ToImmutableList()))
                {
                    Console.ResetColor();
                    Environment.Exit(-2);
                }
                break;
                
            
            default:
                Console.WriteLine($"Unrecognized command {args[0]}.");
                break;
        }
        
        SaveSettings();
    }

    private static bool ModifyTask(ImmutableList<string> args)
    {
        var tasks = GetTasks();
        (string, Task) task;

        var markAsDoneIndex = args.IndexOf("-D") == -1 ? args.IndexOf("--mark-as-done") : args.IndexOf("--D");
        var markAsCanceledIndex = args.IndexOf("-c") ==  -1 ? args.IndexOf("--mark-as-canceled") : args.IndexOf("-c");
        var setDescriptionIndex = args.IndexOf("-d") == -1 ? args.IndexOf("--description") : args.IndexOf("-d");
        var setNameIndex = args.IndexOf("-n") == -1 ? args.IndexOf("--new-name") : args.IndexOf("-n");
        
        try
        {
            task = tasks.First(p => p.Item2.Name == args[0]);
        }
        catch (InvalidOperationException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Invalid task name");
            return false;
        }

        if (markAsDoneIndex == -1 && markAsCanceledIndex == -1)
        {
            Console.WriteLine("Marking task as \"On Progress\"");
            task.Item2.IsBeingWorkedOn = true;
            task.Item2.IsCompleted = false;
            task.Item2.HasBeenCanceled = false;
        }
        else if (markAsDoneIndex != -1)
        {
            Console.WriteLine("Marking task as \"Done\"");
            task.Item2.IsBeingWorkedOn = false;
            task.Item2.IsCompleted = true;
            task.Item2.HasBeenCanceled = false;
        }
        else
        {
            Console.WriteLine("Marking task as \"Canceled\"");
            task.Item2.IsBeingWorkedOn = false;
            task.Item2.IsCompleted = false;
            task.Item2.HasBeenCanceled = true;    
        }

        if (setDescriptionIndex != -1)
        {
            try
            {
                task.Item2.Description = args[setDescriptionIndex + 1];
                if (string.IsNullOrWhiteSpace(task.Item2.Description))
                {
                    throw new InvalidOperationException();
                }
            }
            catch (InvalidOperationException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Description cannot be empty");
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please specify a description");
                return false;
            }
        }
        
        if (setNameIndex != -1)
        {
            try
            {
                task.Item2.Name = args[setNameIndex + 1];
                if (string.IsNullOrWhiteSpace(task.Item2.Name))
                {
                    throw new InvalidOperationException();
                }
            }
            catch (InvalidOperationException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Name cannot be empty");
                return false;
            }
            catch (IndexOutOfRangeException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Please specify a name");
                return false;
            }
        }
        
        try
        {
            File.WriteAllText(task.Item1, JsonSerializer.Serialize(task.Item2));
        }
        catch (FileNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Task not found.");
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Task not found.");
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Access denied.");
            return false;
        }
        catch (PathTooLongException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Path too long.");
            return false;
        }

        Console.ForegroundColor = ConsoleColor.Green;
        if (setNameIndex == -1) Console.WriteLine($"Successfully changed status of {task.Item2.Name}.");
        else Console.WriteLine($"Successfully changed status of {args[setNameIndex + 1]}.");

        return true;
    }

    private static bool DeleteTask(ImmutableList<string> args)
    {
        var tasks = GetTasks();

        if (tasks.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No tasks found.");
            return false;
        }

        try
        {
            var task = tasks.First(p => p.Item2.Name == args[0]);
            File.Delete(task.Item1);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Task {task.Item2.Name} deleted");
            Console.ResetColor();
        }
        catch (InvalidOperationException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Task not found.");
            return false;
        }
        catch (ArgumentException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Failed to delete task.");
            return false;
        }
        catch (FileNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Task not found.");
            return false;
        }
        catch (DirectoryNotFoundException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Task not found.");
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Access denied.");
            return false;
        }
        catch (PathTooLongException)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Path too long.");
            return false;
        }

        return true;
    }

    private static void ShowProgramInfo()
    {
        if (!File.Exists(Path.Combine(_workingPath, "motd.txt")) || _settings.ShowAboutInfoEveryTime)
        {
            ShowText();
        }
        else
        {
            var dt = DateTime.Parse(File.ReadAllText(Path.Combine(_workingPath, "motd.txt")));
            if ((dt.Minute - DateTime.Now.Minute) > 10)
            {
                ShowText();
            }
        }
        
        File.WriteAllText(Path.Combine(_workingPath, "motd.txt"), DateTime.Now.ToString(CultureInfo.CurrentCulture));
        
        return;
        
        // Helpers
        void ShowText()
        {
            Console.WriteLine("Minty ToDos CLI Version 1.0a");
        }
    }

    private static void LoadSettings()
    {
        if (!File.Exists(Path.Combine(_workingPath, "settings.json")))
        {
            _settings = new ()
            {
                ShowAboutInfoEveryTime = false
            };
            SaveSettings();
        }
        else
        {
            _settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(Path.Combine(_workingPath, "settings.json")))!;
        }
    }

    private static void SaveSettings()
    {
        var json = JsonSerializer.Serialize(_settings);
        File.WriteAllText(Path.Combine(_workingPath, "settings.json"), json);
    }

    private static void ListTasks(ImmutableList<string> args)
    {
        var taskPriorityIndex = args.IndexOf("-p") == -1 ? args.IndexOf("--task-priority") : args.IndexOf("-p");
	    var taskCompletedIndex = args.IndexOf("-d") ==  -1 ? args.IndexOf("--task-completed") : args.IndexOf("-d");
        var taskWorkInProgressIndex = args.IndexOf("-w") == -1 ?  args.IndexOf("--task-wip") : args.IndexOf("-w");
        var taskCanceledIndex = args.IndexOf("-c") == -1 ? args.IndexOf("--task-canceled") : args.IndexOf("-c");
        
        List<Task> tasks = new();
        
        GetTasks().Select(p => p.Item2).ToList().ForEach(tasks.Add);
        
        Comparison<Task> comparator;
        
        if (taskPriorityIndex != -1)
        {
            Console.WriteLine("Ordering by priority");
            comparator = (thisTask, nextTask) => thisTask.Priority < nextTask.Priority ? -1 : 1; 
        }
        else if (taskCompletedIndex != -1)
        {
            Console.WriteLine("Ordering by completion status");
            comparator = (thisTask, _) => thisTask.IsCompleted ? -1 : 1;
        }
        else if (taskWorkInProgressIndex != -1)
        {
            Console.WriteLine("Ordering by work-in-progress status");
            comparator = (thisTask, _) => thisTask.IsBeingWorkedOn ? -1 : 1;
        }
        else if (taskCanceledIndex != -1)
        {
            Console.WriteLine("Ordering by cancellation status");
            comparator = (thisTask, _) => thisTask.HasBeenCanceled ? -1 : 1;
        }
        else
        {
            Console.WriteLine("Ordering by creation date");
            comparator = (thisTask, nextTask) => thisTask.Created.CompareTo(nextTask.Created);
        }
        
        tasks.Sort(comparator);

        foreach (var task in tasks)
        {
            if (task.IsBeingWorkedOn)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($">>> {task.Name}\n\t{task.Description}");
                Console.WriteLine($"\tPriority: {task.Priority}");
            }
            else if (task.HasBeenCanceled)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"!!! {task.Name}\n\t{task.Description}");
                Console.WriteLine($"\tPriority: {task.Priority}");
            }
            else if (task.IsCompleted)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"^^^ {task.Name}\n\t{task.Description}");
                Console.WriteLine($"\tPriority: {task.Priority}");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"- {task.Name}\n\t{task.Description}");
                Console.WriteLine($"\tPriority: {task.Priority}");
            }
        }
        Console.ResetColor();
    }

    private static List<(string, Task)> GetTasks()
    {
        var files = Directory.GetFiles(_workingPath, "task-*.json");
        
        var list = new List<(string, Task)>();

        foreach (var file in files!)
        {
            if (file.StartsWith("settings.json")) continue;
            
            var task = JsonSerializer.Deserialize<Task>(File.ReadAllText(file));
            list.Add((file, task)!);
        }
        
        return list;
    }

    private static void InitWorkingFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        _workingPath = Path.Combine(appData, "MintyToDos");

        if (!Directory.Exists(_workingPath))
        {
            Directory.CreateDirectory(_workingPath);
        }
    }

    private static bool CreateTask(ImmutableList<string> args)
    {
        Console.ResetColor();
        
        if (args.Count == 0)
        {
            Console.WriteLine("Usage: create <TaskName> -p [Task Priority] -d [Description]");
            return false;
        }

        var taskPriorityIndex = args.IndexOf("-p") == -1 ? args.IndexOf("--set-priority") : args.IndexOf("-p");
        var taskDescriptionIndex = args.IndexOf("-d") == -1 ? args.IndexOf("--set-description") : args.IndexOf("-d");
        
        Console.WriteLine($"DEBUG: taskPriorityIndex={taskPriorityIndex}, taskDescriptionIndex={taskDescriptionIndex}");

        var task = new Task(args[0]);

        if (taskPriorityIndex != -1)
        {
            try
            {
                var priority = int.Parse(args[taskPriorityIndex + 1]);
                task.SetPriority(priority);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid task priority.");
                return false;
            }
            catch (OverflowException)
            {
                Console.WriteLine("The priority is too large or too small.");
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Please specify a priority.");
                return false;
            }
        }

        if (taskDescriptionIndex != -1)
        {
            var description = args[taskDescriptionIndex + 1];
            if (description.Length == 0 || string.IsNullOrWhiteSpace(description))
            {
                Console.WriteLine("Please specify a description.");
                return false;
            }
            task.SetDescription(description);
        }
        
        var json = JsonSerializer.Serialize(task);
        var fileName = Path.Combine(_workingPath, $"task-{Guid.CreateVersion7(DateTime.Now)}.json");
        File.WriteAllText(fileName, json);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Created task {args[0]}.");
        Console.ResetColor();
        
        Console.WriteLine($"DEBUG: File written to {fileName}.");

        return true;
    }

    private static void ShowHelp(ImmutableList<string> subcommands)
    {
        Console.WriteLine("There's no help available, for now.");
    }
}