namespace MintyToDos;

class Task(string name)
{

    /// <summary>
    /// Indicates how important the <see cref="Task"/> is compared with others
    /// </summary>
    public int Priority { get; set; } = 0;
    
    public string Name { get; set; } = name;
    public string Description { get; set; } = "No description given to this task.";
    public bool HasBeenCanceled { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public bool IsBeingWorkedOn { get; set; } = false;
    
    public DateTime Created { get; set; } = DateTime.Now;

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
}