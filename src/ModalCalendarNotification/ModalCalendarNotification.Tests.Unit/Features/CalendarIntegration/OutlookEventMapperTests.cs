using KellermanSoftware.CompareNetObjects;
using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Shared.Models;
using Shouldly;
using Xunit;

namespace ModalCalendarNotification.Tests.Unit.Features.CalendarIntegration;

public sealed class OutlookEventMapperTests
{
    [Fact]
    public void Map_MapsAllExpectedProperties()
    {
        var source = new OutlookEventModel(
            "id-1",
            "Daily Standup",
            "sync",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30),
            "Room A",
            "primary",
            false);

        var mapped = OutlookEventMapper.Map(source);
        var expected = new CalendarEvent
        {
            Id = source.Id,
            Title = source.Subject,
            Description = source.Body,
            StartUtc = source.StartUtc,
            EndUtc = source.EndUtc,
            Location = source.Location,
            Provider = "Outlook365",
            CalendarId = source.CalendarId,
            IsAllDay = source.IsAllDay
        };

        var comparison = new CompareLogic().Compare(expected, mapped);
        comparison.AreEqual.ShouldBeTrue();
    }
}
