namespace ModalCalendarNotification.Data.Migrations;

public static class _001_InitialSchema
{
    public const string Id = "001_InitialSchema";

    public static IReadOnlyList<string> SqlStatements { get; } =
    [
        "CREATE TABLE IF NOT EXISTS Configuration (Key TEXT PRIMARY KEY, Value TEXT NOT NULL);",
        "CREATE TABLE IF NOT EXISTS DismissedEventTitle (Id INTEGER PRIMARY KEY AUTOINCREMENT, Title TEXT NOT NULL, DismissedAtUtc TEXT NOT NULL);",
        "CREATE TABLE IF NOT EXISTS ApplicationState (Key TEXT PRIMARY KEY, Value TEXT NULL, UpdatedAtUtc TEXT NOT NULL);",
    ];
}
