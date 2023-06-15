using System.ComponentModel;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Text;
using NoteTool.Exceptions;
using Spectre.Console;
using Spectre.Console.Cli;

namespace NoteTool.Stub;

public class StubWeeklyFolderCommand : AsyncCommand<StubWeeklyFolderCommand.Settings>
{
    private readonly IAnsiConsole ansiConsole;
    private readonly IClock clock;
    private readonly WorkLogOptions workLogOptions;

    public StubWeeklyFolderCommand(IAnsiConsole ansiConsole, IClock clock, IOptions<WorkLogOptions> workLogOptions)
    {
        this.ansiConsole = ansiConsole;
        this.clock = clock;
        this.workLogOptions = workLogOptions.Value;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            ansiConsole.MarkupLine("[blue]Starting stub command[/]");

            var weekStarting = GetWeekStartingDate(settings);

            ansiConsole.MarkupLine($"Stubbing notes for [green]{weekStarting}[/]");

            var baseFolder = workLogOptions.WeeklyFolderLocation ?? ".\\";
            var yearFolder = Path.Combine(baseFolder, weekStarting.ToString("yyyy", CultureInfo.InvariantCulture));

            if (!Directory.Exists(yearFolder))
            {
                ansiConsole.MarkupLine("[orange3]Year folder doesn't exist, creating...[/]");
                Directory.CreateDirectory(yearFolder);
            }

            var weekFolder = Path.Combine(yearFolder, weekStarting.ToString("MM-dd", CultureInfo.InvariantCulture));

            if (!Directory.Exists(weekFolder))
            {
                ansiConsole.MarkupLine("[orange3]Week folder doesn't exist, creating...[/]");
                Directory.CreateDirectory(weekFolder);
            }

            var workLogFile = Path.Combine(weekFolder, "WorkLog.md");

            if (!File.Exists(workLogFile))
            {
                ansiConsole.MarkupLine("[orange3]WorkLog.md file doesn't exist, creating...[/]");

                var workLogBuilder = new StringBuilder();
                workLogBuilder.AppendLine($"# {weekStarting.ToString("yyyy", CultureInfo.InvariantCulture)} - Week Starting {weekStarting.ToString("d MMM", CultureInfo.InvariantCulture)}");

                for (var i = 0; i <= 4; i++)
                {
                    var day = weekStarting.PlusDays(i);
                    workLogBuilder.AppendLine($"## {day}");

                    var dailyWorkLogs = GetDailyWorkLogTasks(day).Concat(workLogOptions.WorkLogTasks);

                    foreach (var workLogTask in dailyWorkLogs)
                    {
                        workLogBuilder.AppendLine($"- [ ] {workLogTask}");
                    }

                    workLogBuilder.AppendLine("- [ ] ");
                    workLogBuilder.AppendLine();
                }

                await WriteFile(workLogFile, workLogBuilder.ToString());
            }

            ansiConsole.MarkupLine("[blue]Stubbing of weekly folder complete[/]");
        }
        catch (InvalidSettingException e)
        {
            ansiConsole.MarkupLine($"[red]{e.Message}[/]");
            return 1;
        }
        catch (InvalidUserEntryException e)
        {
            ansiConsole.MarkupLine($"[red]{e.Message}[/]");
            return 1;
        }

        return 0;
    }

    private static async Task WriteFile(string file, string content)
    {
        await using var outputFile = new StreamWriter(file);
        await outputFile.WriteAsync(content);
    }

    private static LocalDate GetMostRecentMonday(LocalDate date)
    {
        if (date.DayOfWeek == IsoDayOfWeek.Monday)
        {
            return date;
        }

        var dateToCheck = date.PlusDays(-1);

        while (dateToCheck.DayOfWeek != IsoDayOfWeek.Monday)
        {
            dateToCheck = dateToCheck.PlusDays(-1);
        }

        return dateToCheck;
    }

    private static string BuildDateSelection(LocalDate date, string descriptor) =>
        $"{date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture)} ({descriptor})";

    private string[] GetDailyWorkLogTasks(LocalDate date) =>
        date.DayOfWeek switch
        {
            IsoDayOfWeek.Monday => workLogOptions.DailyWorkLogTasks?.Monday ?? Array.Empty<string>(),
            IsoDayOfWeek.Tuesday => workLogOptions.DailyWorkLogTasks?.Tuesday ?? Array.Empty<string>(),
            IsoDayOfWeek.Wednesday => workLogOptions.DailyWorkLogTasks?.Wednesday ?? Array.Empty<string>(),
            IsoDayOfWeek.Thursday => workLogOptions.DailyWorkLogTasks?.Thursday ?? Array.Empty<string>(),
            IsoDayOfWeek.Friday => workLogOptions.DailyWorkLogTasks?.Friday ?? Array.Empty<string>(),
            IsoDayOfWeek.Saturday => workLogOptions.DailyWorkLogTasks?.Saturday ?? Array.Empty<string>(),
            IsoDayOfWeek.Sunday => workLogOptions.DailyWorkLogTasks?.Sunday ?? Array.Empty<string>(),
            _ => Array.Empty<string>(),
        };

    private LocalDate GetWeekStartingDate(Settings settings)
    {
        var (weekStarting, mostRecentMonday) = settings;

        if (string.IsNullOrWhiteSpace(weekStarting))
        {
            return PromptUserForWeekStarting(mostRecentMonday);
        }

        var parseResult = LocalDatePattern.Iso.Parse(weekStarting);

        if (parseResult.Success)
        {
            return parseResult.Value;
        }

        throw new InvalidSettingException(nameof(Settings.WeekStarting));
    }

    private LocalDate PromptUserForWeekStarting(bool automaticallyReturnMostRecentMonday)
    {
        var now = clock.GetCurrentInstant();
        var systemTimezone = DateTimeZoneProviders.Tzdb.GetSystemDefault();
        var today = now.InZone(systemTimezone).LocalDateTime.Date;

        var mostRecentMonday = GetMostRecentMonday(today);

        if (automaticallyReturnMostRecentMonday)
        {
            return mostRecentMonday;
        }

        var nextMonday = mostRecentMonday.PlusWeeks(1);
        var followingMonday = mostRecentMonday.PlusWeeks(2);

        var value = ansiConsole.Prompt(
            new SelectionPrompt<WeekStartingPromptOptions>()
                .Title("What date do you want to want to generate the stub for?")
                .UseConverter(option =>
                {
                    return option switch
                    {
                        WeekStartingPromptOptions.MostRecentMonday => BuildDateSelection(mostRecentMonday, mostRecentMonday == today ? "Today" : "Most Recent Monday"),
                        WeekStartingPromptOptions.NextMonday => BuildDateSelection(nextMonday, "Next Monday"),
                        WeekStartingPromptOptions.FollowingMonday => BuildDateSelection(followingMonday, "Following Monday"),
                        WeekStartingPromptOptions.UserEntry => "Enter a date of your choosing",
                        _ => option.ToString(),
                    };
                })
                .AddChoices(
                    WeekStartingPromptOptions.MostRecentMonday,
                    WeekStartingPromptOptions.NextMonday,
                    WeekStartingPromptOptions.FollowingMonday,
                    WeekStartingPromptOptions.UserEntry));

        return value switch
        {
            WeekStartingPromptOptions.MostRecentMonday => mostRecentMonday,
            WeekStartingPromptOptions.NextMonday => nextMonday,
            WeekStartingPromptOptions.FollowingMonday => followingMonday,
            _ => PromptForUserEntry(),
        };

        LocalDate PromptForUserEntry()
        {
            var userEntry = ansiConsole.Ask<string>("Enter the date (yyyy-mm-dd):");

            var parseResult = LocalDatePattern.Iso.Parse(userEntry);

            if (parseResult.Success)
            {
                return parseResult.Value;
            }

            throw new InvalidUserEntryException("The date entered is not valid");
        }
    }

    public class Settings : CommandSettings
    {
        [CommandOption("-w|--week-starting")]
        [Description("Generates folder for week starting this date")]
        public string? WeekStarting { get; set; }

        [CommandOption("-m|--most-recent-monday")]
        [Description("Automatically select the most recent monday")]
        public bool MostRecentMonday { get; set; }

        public override ValidationResult Validate()
        {
            return !string.IsNullOrWhiteSpace(WeekStarting) && MostRecentMonday
                ? ValidationResult.Error("Cannot set a week starting and automatically pick the most recent monday")
                : ValidationResult.Success();
        }

        public void Deconstruct(out string? weekStarting, out bool mostRecentMonday)
        {
            weekStarting = WeekStarting;
            mostRecentMonday = MostRecentMonday;
        }
    }
}