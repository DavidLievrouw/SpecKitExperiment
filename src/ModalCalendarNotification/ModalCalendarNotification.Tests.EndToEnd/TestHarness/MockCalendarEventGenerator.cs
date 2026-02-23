using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Tests.EndToEnd.TestHarness;

public static class MockCalendarEventGenerator
{
    public static CalendarEvent CreateUpcoming(string id, string title, int minutesFromNow)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        return new CalendarEvent
        {
            Id = id,
            Title = title,
            StartUtc = now.AddMinutes(minutesFromNow),
            EndUtc = now.AddMinutes(minutesFromNow + 30),
            Provider = "Test",
            CalendarId = "TestCalendar",
        };
    }
}
