# Modal Calendar Notification - Quick Reference Guide

## Project Overview

**Application**: Windows system tray application that displays modal notifications for calendar events  
**Target**: .NET 10, Windows 10+  
**Architecture**: Vertical Slice (8 independent features)  
**Database**: SQLite (embedded, portable)  
**Deployment**: Single portable .exe (~100-150MB with bundled runtime)

---

## Key Features

| Feature | Purpose | Technology |
|---------|---------|-----------|
| **CalendarIntegration** | Fetch events from Outlook365 & Google Calendar | MSAL OAuth 2.0, Polly resilience |
| **CalendarSelection** | Let users choose which calendars to monitor | SQLite persistence, MVVM UI |
| **NotificationManagement** | Schedule & display modal notifications | Pure functions, snooze/dismiss actions |
| **ConfigurationManagement** | Settings dialog for user preferences | WPF XAML, appsettings.json |
| **DismissedEventsManagement** | Track dismissed event titles | SQLite repository |
| **StartupRecovery** | Detect missed events on app restart | 24-hour lookback window |
| **SystemTrayManagement** | System tray icon & context menu | NotifyIcon, WPF modals |
| **Authentication** | OAuth token management & encryption | MSAL, Windows DPAPI |

---

## Multi-Provider & Multi-Calendar Architecture

### Configuration Example

```
User Configures:
├── Outlook365
│   ├── Personal Calendar (selected ✓)
│   └── Work Calendar (selected ✓)
└── Google Calendar
    ├── Main Calendar (selected ✓)
    └── Archive Calendar (deselected ✗)

Result: 3 selected calendars → union of events → single notification stream
```

### Database Schema (Key Tables)

```sql
-- Provider credentials (encrypted)
ProviderCredentials
├── Id (PK)
├── ProviderType ('outlook' | 'google')
├── EncryptedAccessToken
├── EncryptedRefreshToken
└── ExpiresAt

-- Selected calendars per provider
SelectedCalendars
├── Id (PK)
├── ProviderCredentialsId (FK)
├── CalendarId
├── CalendarName
└── IsSelected (boolean)

-- Dismissed event titles (applies across all providers)
DismissedEventTitles
├── Id (PK)
├── Title
└── CreatedAt
```

---

## Technology Stack Summary

### Core Stack

| Component | Package | Version | Purpose |
|-----------|---------|---------|---------|
| Language | C# .NET | 10 | Latest LTS framework |
| UI Framework | WPF | Native | Windows system tray + modals |
| MVVM | CommunityToolkit.Mvvm | 8.3.2 | ViewModel/Command patterns |
| Database | SQLite | Embedded | Portable, no external setup |
| ORM | Entity Framework Core | 8.0.2 | Type-safe data access |

### Integration Stack

| Component | Package | Version | Purpose |
|-----------|---------|---------|---------|
| OAuth 2.0 | MSAL | 4.62.0 | Outlook365 & Google auth |
| Google API | Google.Apis.Calendar.v3 | 1.68.0 | Google Calendar integration |
| Resilience | Polly.Core | 8.2.0 | Retry, circuit breaker, timeout |
| Logging | Serilog | 3.2.0 | Structured logging |
| DI | Microsoft.Extensions.DependencyInjection | 8.0.0 | Service container |

### Testing Stack

| Component | Package | Version | Purpose |
|-----------|---------|---------|---------|
| Test Framework | xUnit | 2.7.1 | Unit & E2E tests |
| Mocking | FakeItEasy | 8.4.1 | Dependency mocking |
| Assertions | Shouldly | 4.1.0 | Fluent assertions |
| Property Testing | Kellerman.CompareNetObjects | 4.81.0 | Object comparison & mapping validation |

### Code Quality Stack

| Component | Package | Version | Purpose |
|-----------|---------|---------|---------|
| Analyzers | Roslynator | 4.12.6 | Code quality enforcement |
| Config | .editorconfig | - | Roslynator rules + style |
| Version Mgmt | Directory.Packages.props | - | Centralized NuGet versions |

---

## Directory Structure

```
C:\_git\SpecKitExperiment\
├── specs/
│   └── 001-modal-calendar-notification/
│       ├── spec.md (410 lines, 66 requirements)
│       ├── plan.md (1,378 lines, detailed architecture)
│       └── checklists/
│           └── requirements.md
│
└── src/ModalCalendarNotification/
    ├── .editorconfig (Roslynator configuration)
    ├── Directory.Packages.props (centralized NuGet versions)
    ├── ModalCalendarNotification.sln (solution file)
    │
    ├── ModalCalendarNotification/ (WPF Application)
    │   ├── Features/ (8 vertical slices)
    │   │   ├── CalendarIntegration/ (OAuth, event fetching)
    │   │   ├── CalendarSelection/ (calendar checkboxes)
    │   │   ├── NotificationManagement/ (modals, snooze, auto-dismiss)
    │   │   ├── ConfigurationManagement/ (settings dialog)
    │   │   ├── DismissedEventsManagement/ (dismissed titles)
    │   │   ├── StartupRecovery/ (missed event detection)
    │   │   ├── SystemTrayManagement/ (tray icon)
    │   │   └── Authentication/ (OAuth token refresh, DPAPI)
    │   ├── Shared/ (common models, interfaces, utilities)
    │   ├── Data/ (DbContext, migrations)
    │   └── App.xaml/.cs (WPF entry point)
    │
    ├── ModalCalendarNotification.Tests.Unit/
    │   └── (feature-specific unit tests + property mapping tests)
    │
    └── ModalCalendarNotification.Tests.EndToEnd/
        └── (6 complete workflow tests with mocked APIs)
```

---

## Key Design Patterns

### 1. Vertical Slice Architecture
- Each feature owns UI + business logic + data access
- Features are independently deployable
- Clear feature boundaries, reduced coupling

### 2. Provider Abstraction
```csharp
public interface ICalendarProvider
{
    Task<IEnumerable<CalendarEvent>> GetEventsAsync(DateTime start, DateTime end);
    Task RefreshCredentialsAsync();
}

// Implementations:
public class OutlookCalendarProvider : ICalendarProvider { }
public class GoogleCalendarProvider : ICalendarProvider { }
```

### 3. Polly Resilience Policies
```csharp
var policy = Policy
    .Handle<HttpRequestException>()
    .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)))
    .WrapAsync(Policy.TimeoutAsync(TimeSpan.FromSeconds(10)));
```

### 4. Generated Mappers (No AutoMapper)
```csharp
public static class OutlookEventMapper
{
    public static CalendarEvent MapFromOutlookEvent(OutlookEventModel outlook)
    {
        return new CalendarEvent
        {
            Id = outlook.Id,
            Title = outlook.Subject,
            StartTime = outlook.Start.ToDateTime(),
            // ... all properties explicitly mapped
        };
    }
}

// Tested with CompareNetObjects for 100% property coverage
```

### 5. Repository Pattern
```csharp
public interface ICalendarSelectionRepository
{
    Task<IEnumerable<SelectedCalendar>> GetSelectedCalendarsAsync(int providerId);
    Task ToggleCalendarAsync(int providerId, string calendarId);
}
```

### 6. Functional Programming in Notification Engine
```csharp
// Pure functions for business logic
private static DateTime CalculateNotificationTime(CalendarEvent evt, int leadMinutes) 
    => evt.StartTime.AddMinutes(-leadMinutes);

private static IEnumerable<CalendarEvent> FilterBySelectedCalendars(
    IEnumerable<CalendarEvent> events,
    IEnumerable<SelectedCalendar> selected)
    => events.Where(e => selected.Any(s => s.CalendarId == e.CalendarId));
```

---

## Configuration & Deployment

### appsettings.json Structure

```json
{
  "Calendar": {
    "SyncIntervalSeconds": 300,
    "NotificationLeadTimeMinutes": 3,
    "AutoDismissMinutesAfterStart": 10,
    "MissedEventLookbackHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  },
  "OAuth": {
    "OutlookTenantId": "common",
    "GoogleClientId": "[configured]"
  }
}
```

### Database Location

- **Windows**: `%APPDATA%\ModalCalendarNotification\app.db`
- **Example**: `C:\Users\JohnDoe\AppData\Roaming\ModalCalendarNotification\app.db`
- **Auto-created** on first run
- **Auto-migrated** on version upgrades

### Deployment Artifact

```bash
# Portable executable (single file, self-contained)
ModalCalendarNotification.exe (~100-150 MB)

# Contains:
- .NET 10 runtime (bundled)
- All dependencies (NuGet packages)
- SQLite database engine

# No installation required:
- Drop on any Windows 10+ machine
- Run executable
- Database creates at %APPDATA%
- Optional: registry entry for auto-start
```

---

## Testing Strategy

### Unit Tests (90%+ Coverage Target)

| Feature | Test Focus | Tools |
|---------|-----------|-------|
| Mappers | Property coverage | Kellerman.CompareNetObjects |
| Notification Engine | Lead time, snooze, filtering | Pure function tests |
| Repositories | CRUD operations | In-memory SQLite |
| Providers | OAuth mocking | FakeItEasy |

### E2E Tests (6 Complete Workflows)

1. **Initial Setup**: Add provider → select calendars → see notification
2. **Multi-Provider**: Configure Outlook + Google → unified stream
3. **Calendar Selection**: Add calendars → deselect some → verify filtering
4. **Startup Recovery**: Missed event detection within 24-hour window
5. **Snooze**: User snoozes → re-notification at exact time
6. **Dismiss All**: Dismiss all "Daily Standup" → future ones don't notify

---

## Security Model

### Credential Encryption

- **Mechanism**: Windows DPAPI (Data Protection API)
- **Scope**: Per-user, per-machine
- **Protected Fields**: OAuth access tokens, refresh tokens
- **Key Management**: Handled by Windows (no manual key management)

### OAuth 2.0 Flow

1. User clicks "Configure [Provider]"
2. Browser opens OAuth provider login
3. MSAL handles auth code exchange
4. Tokens stored encrypted in SQLite
5. Automatic token refresh before expiry
6. Failed refresh prompts user to re-authenticate

### Data at Rest

- **SQLite Database**: Stored in user's AppData (OS-protected)
- **Credentials**: Encrypted with DPAPI
- **Configuration**: Plain JSON in appsettings.json
- **Logs**: File sink with sensitive data filtering

---

## Performance Targets

| Metric | Target | Strategy |
|--------|--------|----------|
| Notification Appearance | < 5 seconds after lead time | Background sync, pre-calculated times |
| Configuration Dialog | < 2 seconds to open | Async calendar list loading |
| Calendar Sync | Complete within 30 seconds | Efficient Graph API queries |
| Memory Footprint | < 50 MB at rest | Lazy loading, event filtering |
| Idle CPU Usage | < 2% | Timer-based polling, async operations |
| Snooze Accuracy | ±5 seconds | Precise DateTime calculations |
| Database Query | < 100ms | Indexed searches on CalendarId, Title |

---

## Development Workflow

### Setting Up Your Environment

```bash
# Clone repository
git clone [repository]
cd SpecKitExperiment/src/ModalCalendarNotification

# Install dependencies (from Directory.Packages.props)
dotnet restore

# Verify Roslynator is enabled
dotnet build

# Run tests
dotnet test

# Run application
dotnet run --project ModalCalendarNotification/ModalCalendarNotification.csproj
```

### Code Style Enforcement

- Roslynator analyzers run on every build (warnings)
- Fix warnings to maintain clean code
- `.editorconfig` auto-formats on save (in IDE)

### Adding New Features

```
1. Create feature folder in Features/
2. Add [FeatureName]/Tests/ for tests
3. Implement interfaces in feature folder
4. Register services in Program.cs DI
5. Write unit tests with complete coverage
6. Add E2E test if user-facing
7. Update appsettings.json if new config needed
```

---

## Important Files

| File | Purpose | Size |
|------|---------|------|
| `spec.md` | Feature specification | 410 lines |
| `plan.md` | Implementation plan | 1,378 lines |
| `.editorconfig` | Code style & Roslynator config | 230 lines |
| `Directory.Packages.props` | Centralized NuGet versions | 85 lines |
| `Program.cs` | DI container setup | TBD |
| `AppDbContext.cs` | EF Core DbContext | TBD |

---

## Quick Links

- **Specification**: `specs/001-modal-calendar-notification/spec.md`
- **Plan**: `specs/001-modal-calendar-notification/plan.md`
- **Implementation Summary**: `IMPLEMENTATION_SUMMARY.md`
- **Completion Report**: `COMPLETION_REPORT.md`
- **This Guide**: `QUICK_REFERENCE.md`

---

## Contact

Questions about:
- **Architecture**: See plan.md → Architectural Patterns
- **Specification**: See spec.md → Clarifications & Requirements
- **Testing**: See plan.md → Testing Strategy
- **Deployment**: See plan.md → Deployment Strategy

---

**Version**: 1.0 (February 23, 2026)  
**Status**: Planning Complete, Ready for Task Generation  
**Next Phase**: speckit.tasks → Generate implementation tasks with dependencies

