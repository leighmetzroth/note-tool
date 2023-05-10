# note-tool
A tool for making the setup of work logs faster and more efficient.

## Configuration

The idea here is that the settings that don't change (e.g. your folder location), are set in the `appsettings.json`.
The rest is done from the command line, or as selections in the CLI.

The folder structure is

```
C:\src\notes\       <- WeeklyFolderLocation
    .\2023\         <- YYYY
        .\05-01\    <- MM-DD
        .\05-08\    <- MM-DD
```

### Base Folder (WeeklyFolderLocation)

To change the base folder, just update the app setting for `WeeklyFolderLocation`.

### Stubbed Work Log Tasks

The `WorkLogTasks` app setting allows you to seed a base set of tasks that need to be done every day.

## To run the tool

### Stub a Weekly Work Log

#### Option 1 - Guided Mode

In a command line, run:
`dotnet run stub`

This will prompt you for a preset list of dates, or enter your own.

#### Option 2 - Most Recent Monday

In a command line, run:
`dotnet run stub -m`

This will stub out the weekly work log based on the most recent Monday (if run on a Monday, it will do it for that day).

#### Option 3 - Supply a Date

In a command line, run:
`dotnet run stub -w "2023-05-01"` (where the date is supplied in ISO format)

This will stub out the weekly work log based on the supplied date.
