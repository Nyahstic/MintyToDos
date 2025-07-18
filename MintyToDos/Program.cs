using System.Collections.Immutable;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MintyToDos;

class Program
{
    private static string WorkingPath;
    static void Main(string[] args)
    {
        InitWorkingFolder();
        
        if (args.Length == 0)
        {
            Console.WriteLine("Usage: \n\tMintyToDos help");
            Environment.Exit(-1);
        }
        
        Console.WriteLine("Minty ToDos CLI Version 1.0a");
        
        switch (args[0].ToLower())
        {
            case "h":
            case "help":
                ShowHelp(args.Skip(1).ToImmutableList());
                break;
            
            case "create":
                if (!CreateTask(args.Skip(1).ToImmutableList()))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed to create task");
                    Console.ResetColor();
                }
                break;
            
            default:
                Console.WriteLine($"Unrecognized command {args[0]}.");
                break;
        }
    }

    private static void InitWorkingFolder()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        WorkingPath = Path.Combine(appData, "MintyToDos");

        if (!Directory.Exists(WorkingPath))
        {
            Directory.CreateDirectory(WorkingPath);
        }
    }

    private static bool CreateTask(ImmutableList<string> args)
    {
        Console.ResetColor();
        
        if (args.Count == 0)
        {
            Console.WriteLine("Usage: create <TaskName> -p [Task Priority] -d [Description] -D [Deadline]");
            return false;
        }

        var taskPriorityIndex = args.IndexOf("-p");
        var taskDescriptionIndex = args.IndexOf("-d");
        var deadlineIndex = args.IndexOf("-D");
        
        Console.WriteLine($"DEBUG: taskPriorityIndex={taskPriorityIndex}, taskDescriptionIndex={taskDescriptionIndex},  deadlineIndex={deadlineIndex}");

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
        }

        if (deadlineIndex != -1)
        {
            try
            {
                var deadLine = TimeSpan.Parse(args[deadlineIndex]);
                task.SetDeadline(deadLine);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Invalid deadline.");
                return false;
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid deadline.");
                return false;
            }
            catch (OverflowException)
            {
                Console.WriteLine("The deadline is too large.");
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                Console.WriteLine("Please specify a deadline.");
                return false;
            }
        }
        
        var json = JsonSerializer.Serialize(task);
        var fileName = Path.Combine(WorkingPath, $"{Guid.CreateVersion7(DateTime.Now)}.json");
        File.WriteAllText(fileName, json);
        
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Created task {args[0]}.");
        Console.ResetColor();
        
        Console.WriteLine($"DEBUG: File written to {fileName}.");

        return true;
    }

    private static void ShowHelp(ImmutableList<string> subcommands)
    {
        foreach (var subcommand in subcommands)
        {
            Console.WriteLine($"{subcommand}");
        }
    }
}

class Task(string name)
{
    public string Name { get; set; } = name;
    public string Description { get; set; } = "No description given to this task.";

    /// <summary>
    /// Indicates how important the <see cref="Task"/> is compared with others
    /// </summary>
    public int Priority { get; set; } = 0;

    public TimeSpan? Deadline { get; set; } = null;
    public bool HasDeadline { get; set; } = false;
    public bool HasBeenCanceled { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public bool IsBeingWorkedOn { get; set; } = false;

    public void SetDescription(string description)
    {
        if (!string.IsNullOrWhiteSpace(description))
            Description = description;
        else 
            Description = "No description given to this task.";
    }

    public void SetPriority(int priority)
    {
        Priority = priority;
    }

    public void SetDeadline(TimeSpan deadline)
    {
        Deadline = deadline;
    }

    public void SetState(bool isBeingWorkedOn, bool hasBeenCanceled, bool isCompleted)
    {
        HasBeenCanceled = isBeingWorkedOn;
        HasBeenCanceled = isCompleted;
        IsCompleted = isCompleted;
    }
}