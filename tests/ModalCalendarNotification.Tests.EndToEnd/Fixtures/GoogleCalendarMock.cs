using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Tests.EndToEnd.Fixtures;

public sealed class GoogleCalendarMock
{
    public IReadOnlyList<CalendarEvent> Events { get; init; } = [];
}
