using KellermanSoftware.CompareNetObjects;
using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarIntegration;

public sealed class GoogleEventMapperTests
{
    [Fact]
    public void Map_MapsAllExpectedProperties()
    {
        var source = new GoogleEventModel(
            "id-1",
            "Planning",
            "weekly planning",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1),
            "Room B",
            "team",
            false
        );

        var mapped = GoogleEventMapper.Map(source);
        var expected = new CalendarEvent
        {
            Id = source.Id,
            Title = source.Summary,
            Description = source.Description,
            StartUtc = source.StartUtc,
            EndUtc = source.EndUtc,
            Location = source.Location,
            Provider = "GoogleCalendar",
            CalendarId = source.CalendarId,
            IsAllDay = source.IsAllDay,
        };

        var comparison = new CompareLogic().Compare(expected, mapped);
        comparison.AreEqual.ShouldBeTrue();
    }
}
