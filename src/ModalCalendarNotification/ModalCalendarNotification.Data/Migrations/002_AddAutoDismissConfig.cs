namespace ModalCalendarNotification.Data.Migrations;

public static class _002_AddAutoDismissConfig
{
    public const string Id = "002_AddAutoDismissConfig";

    public static IReadOnlyList<string> SqlStatements { get; } =
    [
        "INSERT OR IGNORE INTO Configuration (Key, Value) VALUES ('Notification.AutoDismissTimeoutSeconds', '60');",
    ];
}
