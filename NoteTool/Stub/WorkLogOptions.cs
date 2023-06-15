namespace NoteTool.Stub;

public class WorkLogOptions
{
    public string? WeeklyFolderLocation { get; set; }

    public string[] WorkLogTasks { get; set; } = Array.Empty<string>();

    public DailyWorkLogTasksOptions? DailyWorkLogTasks { get; set; }

    public class DailyWorkLogTasksOptions
    {
        public string[] Monday { get; set; } = Array.Empty<string>();

        public string[] Tuesday { get; set; } = Array.Empty<string>();

        public string[] Wednesday { get; set; } = Array.Empty<string>();

        public string[] Thursday { get; set; } = Array.Empty<string>();

        public string[] Friday { get; set; } = Array.Empty<string>();

        public string[] Saturday { get; set; } = Array.Empty<string>();

        public string[] Sunday { get; set; } = Array.Empty<string>();
    }
}