using ModalCalendarNotification.Core.Shared.Models;

namespace ModalCalendarNotification.Tests.EndToEnd.Fixtures;

public sealed class OutlookCalendarMock
{
    public IReadOnlyList<CalendarEvent> Events { get; init; } = [];
}
