﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using ModalCalendarNotification.CalendarProviders;
using ModalCalendarNotification.Core.Features.CalendarIntegration;
using ModalCalendarNotification.Core.Features.ConfigurationManagement;
using ModalCalendarNotification.Core.Features.NotificationManagement;
using ModalCalendarNotification.Core.Shared.Models;
using ModalCalendarNotification.Core.Shared.Utilities;
using TimeProvider = ModalCalendarNotification.Core.Shared.Utilities.TimeProvider;

namespace ModalCalendarNotification.Features.NotificationManagement;

/// <summary>
/// Orchestrates calendar event monitoring, notification scheduling, and modal display.
/// Periodically fetches events from configured providers and shows modals when notifications are due.
/// Uses cached events when connection fails for offline support.
/// </summary>
public sealed class NotificationSchedulerService : IDisposable
{
    private readonly ICalendarProvider[] _providers;
    private readonly IConfigurationService _configurationService;
    private readonly INotificationEngine _notificationEngine;
    private readonly TimeProvider _timeProvider;
    private readonly ILogger<NotificationSchedulerService> _logger;
    private readonly ICachedEventsRepository _cachedEventsRepository;
    private readonly ISyncStatusService _syncStatusService;
    private Timer? _syncTimer;
    private Timer? _notificationCheckTimer;
    private readonly ConcurrentDictionary<string, NotificationState> _pendingNotifications =
        new();
    private readonly Dictionary<string, string> _eventAccountLabels = new();
    private bool _disposed;

    public NotificationSchedulerService(
        ICalendarProvider[] providers,
        IConfigurationService configurationService,
        INotificationEngine notificationEngine,
        TimeProvider timeProvider,
        ILogger<NotificationSchedulerService> logger,
        ICachedEventsRepository cachedEventsRepository,
        ISyncStatusService syncStatusService
    )
    {
        _providers = providers ?? [];
        _configurationService = configurationService;
        _notificationEngine = notificationEngine;
        _timeProvider = timeProvider;
        _logger = logger;
        _cachedEventsRepository = cachedEventsRepository;
        _syncStatusService = syncStatusService;
    }

    public async Task StartSchedulerAsync()
    {
        try
        {
            _logger.LogInformation("Starting notification scheduler");

            // Start periodic sync with calendar providers (every 5 minutes)
            _syncTimer = new Timer(
                async _ => await SyncCalendarEventsAsync(),
                state: null,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromMinutes(5)
            );

            // Check for due notifications every 30 seconds
            _notificationCheckTimer = new Timer(
                async _ => await CheckAndDisplayDueNotificationsAsync(),
                state: null,
                dueTime: TimeSpan.Zero,
                period: TimeSpan.FromSeconds(30)
            );

            _logger.LogInformation("Notification scheduler started");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start notification scheduler");
        }
    }

    public void StopScheduler()
    {
        try
        {
            _logger.LogInformation("Stopping notification scheduler");
            _syncTimer?.Dispose();
            _notificationCheckTimer?.Dispose();
            _logger.LogInformation("Notification scheduler stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop notification scheduler");
        }
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        StopScheduler();
        _disposed = true;
    }

    private async Task SyncCalendarEventsAsync()
    {
        try
        {
            var config = await _configurationService.LoadAsync();
            var now = _timeProvider.UtcNow;
            var lookAheadWindow = now.AddDays(7); // Look ahead 7 days for events

            var allEvents = new List<CalendarEvent>();
            var syncSucceeded = true;

            // Fetch events from all configured providers
            foreach (var account in config.ProviderAccounts)
            {
                try
                {
                    var provider = _providers.FirstOrDefault(p => p.ProviderName == account.ProviderName);
                    if (provider == null)
                        continue;

                    var events = await provider.GetEventsAsync(now, lookAheadWindow);

                    // Store account label mapping for later display
                    foreach (var evt in events)
                    {
                        _eventAccountLabels[evt.Id] = account.AccountLabel;
                    }

                    allEvents.AddRange(events);
                }
                catch (Exception ex)
                {
                    syncSucceeded = false;
                    _logger.LogWarning(
                        ex,
                        "Failed to sync events from provider {Provider}, will use cached events",
                        account.ProviderName
                    );
                }
            }

            // If sync failed or returned no events, use cached events
            if (!syncSucceeded || allEvents.Count == 0)
            {
                _logger.LogInformation("Using cached events due to sync failure or empty results");
                var cachedEvents = await _cachedEventsRepository.GetCachedEventsAsync(now, lookAheadWindow);
                allEvents.AddRange(cachedEvents);
                _syncStatusService.MarkSyncFailure("Failed to fetch fresh events from providers");
            }
            else
            {
                // Sync succeeded - cache the events for offline use
                await _cachedEventsRepository.CacheEventsAsync(allEvents);
                _syncStatusService.MarkSyncSuccess();
            }

            // Build notifications from collected events
            var configLeadTime = config.NotificationLeadTimeMinutes;
            var notifications = _notificationEngine.BuildNotifications(allEvents, now, configLeadTime);

            // Store pending notifications
            foreach (var notification in notifications)
            {
                _pendingNotifications.AddOrUpdate(
                    notification.Id,
                    new NotificationState
                    {
                        Notification = notification,
                        CalendarEvent = allEvents.FirstOrDefault(e => e.Id == notification.EventId),
                    },
                    (_, _) => new NotificationState
                    {
                        Notification = notification,
                        CalendarEvent = allEvents.FirstOrDefault(e => e.Id == notification.EventId),
                    }
                );
            }

            _logger.LogInformation(
                "Synced {EventCount} events, {NotificationCount} pending notifications",
                allEvents.Count,
                notifications.Count
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during calendar sync");
        }
    }

    private async Task CheckAndDisplayDueNotificationsAsync()
    {
        try
        {
            var now = _timeProvider.UtcNow;
            var dueNotifications = _pendingNotifications
                .Where(kvp => kvp.Value.Notification.TriggerAtUtc <= now)
                .ToList();

            if (dueNotifications.Count == 0)
                return;

            // Group notifications by their trigger time window (within 5 minutes)
            var groupedNotifications = GroupNotificationsByTimeWindow(
                dueNotifications.ConvertAll(kvp => kvp.Value),
                timeWindowMinutes: 5
            );

            // Display each group as a modal
            foreach (var group in groupedNotifications)
            {
                await DisplayNotificationModalAsync(group);
            }

            // Remove displayed notifications
            foreach (var (id, _) in dueNotifications)
            {
                _pendingNotifications.TryRemove(id, out _);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for due notifications");
        }
    }

    private List<List<NotificationState>> GroupNotificationsByTimeWindow(
        List<NotificationState> notifications,
        int timeWindowMinutes
    )
    {
        var sorted = notifications
            .OrderBy(n => n.Notification.TriggerAtUtc)
            .ToList();

        var groups = new List<List<NotificationState>>();
        var currentGroup = new List<NotificationState> { sorted[0] };

        for (int i = 1; i < sorted.Count; i++)
        {
            var timeDiff = sorted[i].Notification.TriggerAtUtc - sorted[i - 1].Notification.TriggerAtUtc;
            if (timeDiff.TotalMinutes <= timeWindowMinutes)
            {
                currentGroup.Add(sorted[i]);
            }
            else
            {
                groups.Add(currentGroup);
                currentGroup = new List<NotificationState> { sorted[i] };
            }
        }

        groups.Add(currentGroup);
        return groups;
    }

    private async Task DisplayNotificationModalAsync(
        List<NotificationState> notifications
    )
    {
        try
        {
            // Create NotificationEventItem objects with provider info
            var eventItems = notifications
                .ConvertAll(state => new Core.Features.NotificationManagement.NotificationEventItem
                {
                    EventId = state.Notification.EventId,
                    Title = state.Notification.Title,
                    StartUtc = state.CalendarEvent?.StartUtc ?? state.Notification.TriggerAtUtc,
                    Provider = state.CalendarEvent?.Provider ?? "Unknown",
                    ProviderAccountLabel = _eventAccountLabels.ContainsKey(state.Notification.EventId)
                        ? _eventAccountLabels[state.Notification.EventId]
                        : "Unknown",
                })
;

            _logger.LogInformation(
                "Displaying notification modal with {EventCount} events",
                eventItems.Count
            );

            // Note: This would be called from UI thread context
            // For now, this logs the intention to display
            // In a complete implementation, this would be hooked to actually show the modal
            // via an event or callback mechanism
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error displaying notification modal");
        }
    }

    private sealed class NotificationState
    {
        public required Notification Notification { get; init; }
        public CalendarEvent? CalendarEvent { get; init; }
    }
}



