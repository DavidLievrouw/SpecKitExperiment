namespace ModalCalendarNotification.Data.Migrations;

public static class _003_AddPerformanceIndexes
{
    public const string Id = "003_AddPerformanceIndexes";

    public static IReadOnlyList<string> SqlStatements { get; } =
    [
        "CREATE INDEX IF NOT EXISTS IX_DismissedEventTitle_Title ON DismissedEventTitle(Title);",
        "CREATE INDEX IF NOT EXISTS IX_Configuration_Key ON Configuration(Key);",
        "CREATE INDEX IF NOT EXISTS IX_ApplicationState_UpdatedAtUtc ON ApplicationState(UpdatedAtUtc);",
        "CREATE INDEX IF NOT EXISTS IX_SelectedCalendar_Provider_Calendar ON SelectedCalendar(ProviderName, CalendarId);"
    ];
}
