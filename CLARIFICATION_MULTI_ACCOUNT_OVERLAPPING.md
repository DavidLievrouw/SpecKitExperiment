# ✅ CLARIFICATIONS COMPLETE - Multiple Accounts & Overlapping Events

## Summary of Changes

Following the speckit.clarify workflow instructions, the Modal Calendar Notification specification and implementation plan have been updated with:

1. **Multiple Accounts Per Provider Support** (e.g., multiple Google Calendar accounts)
2. **Overlapping Events Handling** (concurrent events in single modal)
3. **Updated Database Schema** to support multiple accounts per provider type

---

## Changes to spec.md

### New Clarification Session: Multiple Accounts & Overlapping Events

Added 5 new clarifications to address:

**Multiple Accounts Per Provider**:
- **Clarification 1**: Users MUST be able to configure multiple accounts for the same provider type (e.g., multiple Google Calendar accounts: personal, work, side project). Each account is treated as a separate provider instance with its own credentials and calendar selections.

- **Clarification 2**: When adding a provider, users MUST be able to provide a custom label/name for that account (e.g., "John's Work Google", "Personal Google"). This label is displayed in the configuration dialog and helps users distinguish between multiple accounts of the same provider type.

- **Clarification 3**: The database schema MUST NOT use ProviderType as a unique constraint. Instead, use a combination of ProviderType + UserLabel to allow multiple instances of the same provider type.

**Overlapping Events Handling**:
- **Clarification 4**: When multiple events occur at the same time (or within a 5-minute window), they MUST be displayed in a single modal dialog with a scrollable list. Each event in the list MUST have its own individual Snooze, Dismiss, and "Dismiss All Future" buttons. Users can interact with each event independently within the same modal.

- **Clarification 5**: When a user dismisses or snoozes one event from a multi-event modal, that specific event is removed from the modal. If other events remain, the modal stays open showing the remaining events. The modal only closes when all events have been actioned (snoozed, dismissed, or auto-dismissed).

### Updated User Story 1 - Overlapping Events Acceptance Scenarios

Added 5 new acceptance scenarios for overlapping events handling:

1. **Scenario 6**: "Given I have two events starting at the same time (or within 5 minutes of each other), When the notification lead time is reached, Then a single modal appears showing both events in a scrollable list"

2. **Scenario 7**: "Given a multi-event modal is displayed with 3 events, When I click 'Dismiss' on the second event, Then that event is removed from the list and the modal remains open showing the other 2 events"

3. **Scenario 8**: "Given a multi-event modal is displayed, When I click 'Snooze' on one event for 10 minutes, Then that event is removed from the current modal and will reappear in a new modal after 10 minutes"

4. **Scenario 9**: "Given a multi-event modal has 2 events remaining, When I dismiss both events, Then the modal closes completely"

5. **Scenario 10**: "Given overlapping events from different providers are scheduled, When the notification time arrives, Then all events appear in the same multi-event modal with provider information displayed for each"

### Updated User Story 5 - Multiple Accounts Acceptance Scenarios

Added 5 new acceptance scenarios for multiple accounts per provider:

1. **Scenario 11**: "Given I want to monitor calendars from multiple Google accounts, When I click 'Add Calendar Provider' and select 'Google Calendar', Then I am prompted to provide a custom label (e.g., 'Personal Google', 'Work Google') to identify this account"

2. **Scenario 12**: "Given I have provided a label for a Google Calendar account, When OAuth authentication completes, Then that account appears in the configuration dialog with the custom label I provided"

3. **Scenario 13**: "Given I have configured 'Personal Google' and want to add another Google account, When I click 'Add Calendar Provider' again and select 'Google Calendar', Then I can add a second Google account with a different label (e.g., 'Work Google')"

4. **Scenario 14**: "Given I have multiple Google Calendar accounts configured (Personal and Work), When events occur across all selected calendars from both Google accounts, Then notifications show events from all accounts with the account label displayed"

5. **Scenario 15**: "Given I have multiple accounts of the same provider type, When I view the configuration dialog, Then each account is listed separately with its custom label (e.g., 'Personal Google', 'Work Google', 'Company Outlook365')"

---

## Changes to plan.md

### Updated Database Schema

**ProviderCredentials Table** - Changed to support multiple accounts per provider:

```sql
-- OLD (UNIQUE constraint on ProviderType)
CREATE TABLE ProviderCredentials (
    Id INTEGER PRIMARY KEY,
    ProviderType TEXT NOT NULL UNIQUE,  -- ❌ Only allows one account per type
    ...
);

-- NEW (UNIQUE constraint on ProviderType + AccountLabel)
CREATE TABLE ProviderCredentials (
    Id INTEGER PRIMARY KEY,
    ProviderType TEXT NOT NULL,         -- 'outlook' or 'google'
    AccountLabel TEXT NOT NULL,         -- ✅ User-provided: "Personal Google", "Work Google"
    EncryptedAccessToken TEXT NOT NULL,
    EncryptedRefreshToken TEXT,
    ExpiresAt DATETIME,
    LastSyncTime DATETIME,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(ProviderType, AccountLabel)  -- ✅ Allows multiple accounts per type
);
```

**SelectedCalendars Table** - Added CASCADE delete:

```sql
CREATE TABLE SelectedCalendars (
    Id INTEGER PRIMARY KEY,
    ProviderCredentialsId INTEGER NOT NULL,
    RemoteCalendarId TEXT NOT NULL,
    CalendarName TEXT NOT NULL,
    IsSelected BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(ProviderCredentialsId) REFERENCES ProviderCredentials(Id) ON DELETE CASCADE,
    UNIQUE(ProviderCredentialsId, RemoteCalendarId)
);
```

### Added Two New Architectural Patterns

**Pattern 13: Overlapping Events Handling Pattern**

Complete implementation with:
- `NotificationModalViewModel` supporting multiple concurrent events
- `NotificationEventItem` model for each event in multi-event modal
- `ObservableCollection<NotificationEventItem>` for dynamic event list
- Individual action methods: `SnoozeEventAsync`, `DismissEventAsync`, `DismissAllFutureAsync`
- `GroupConcurrentEvents` method with 5-minute window grouping
- XAML structure for scrollable multi-event modal with individual buttons per event

**Key Features**:
- Groups events within 5-minute window
- Scrollable list handles any number of concurrent events
- Each event has independent Snooze/Dismiss/Dismiss All Future buttons
- Modal remains open until all events actioned
- Displays provider/account information for each event

**Pattern 14: Multiple Accounts Per Provider Pattern**

Complete implementation with:
- `ProviderCredentials` model with `AccountLabel` field
- `ProviderSelectionViewModel` with account label input and validation
- `ProviderAccountItem` model for multi-account display in configuration dialog
- `ConfiguredAccounts` observable collection showing all accounts with labels
- Duplicate label validation

**Key Features**:
- Users provide custom labels when adding accounts
- Each account independently authenticated and configured
- Configuration dialog lists all accounts with labels
- Notifications display account label for clarity
- UNIQUE constraint prevents duplicate labels per provider type

### Updated Project Structure

**NotificationManagement Feature**:
- Enhanced `NotificationEngine.cs` to group concurrent events within 5-minute window
- Enhanced `NotificationModalViewModel.cs` to support multi-event display with `ObservableCollection<NotificationEventItem>`
- Added `NotificationEventItem.cs` model for individual events in modal
- Enhanced `NotificationModal.xaml/.cs` with scrollable list for concurrent events
- Updated tests to include concurrent event grouping scenarios

**ConfigurationManagement Feature**:
- Enhanced `ProviderSelectionViewModel.cs` with account label input field
- Enhanced `ProviderSelectionDialog.xaml/.cs` with account labeling UI
- Added `ProviderAccountItem.cs` model for displaying multiple accounts
- Enhanced `ConfigurationDialogViewModel.cs` to list all accounts with labels

### Updated Vertical Slice Architecture Diagram

**Feature 3: NotificationManagement** now includes:
- NotificationEngine (groups concurrent events within 5-min window)
- NotificationModalViewModel (supports multi-event display)
- NotificationEventItem (model for each event in multi-event modal)
- NotificationModal (scrollable list for concurrent events)
- Tests (includes concurrent event grouping tests)

**Feature 4: ConfigurationManagement** now includes:
- ProviderSelectionViewModel (includes account label input)
- ProviderSelectionDialog (provider list UI with account labeling)
- ProviderAccountItem (model for multi-account display in config UI)

---

## Key Design Decisions

### Multiple Accounts Per Provider

**Database Strategy**:
- UNIQUE constraint: (ProviderType, AccountLabel)
- Allows unlimited accounts per provider type
- User-provided labels ensure clarity

**User Experience**:
- When adding provider → prompt for custom label
- Default label: "{ProviderType} Account {timestamp}" if user doesn't provide
- Configuration dialog shows: "Personal Google (google)", "Work Google (google)"
- Notifications display: "Team Meeting (Work Google - Team Calendar)"

**Example Configuration**:
```
Personal Google (google)
  ├─ Personal Calendar ✓
  └─ Family Calendar ✓
Work Google (google)
  ├─ Work Calendar ✓
  └─ Team Calendar ✓
Company Outlook365 (outlook)
  ├─ Calendar ✓
  └─ Shared Calendars ✗
```

### Overlapping Events Handling

**5-Minute Window Grouping**:
- Events starting within 5 minutes of each other are grouped
- Single modal displays all grouped events
- Prevents multiple overlapping modals

**Individual Event Actions**:
- Each event has its own Snooze/Dismiss/Dismiss All Future buttons
- Actioning one event removes it from the modal
- Modal closes only when all events are actioned

**Scrollable List**:
- Handles unlimited concurrent events
- Each event shows: Title, Start Time, Provider/Account, Action buttons
- Selected event can be highlighted for keyboard navigation

**Example Multi-Event Modal**:
```
┌──────────────────────────────────────────┐
│  You have 3 upcoming events              │
├──────────────────────────────────────────┤
│ ┌──────────────────────────────────────┐ │
│ │ Team Standup                         │ │
│ │ 10:00 AM - Work Google              │ │
│ │ [Snooze] [Dismiss] [Dismiss All]    │ │
│ └──────────────────────────────────────┘ │
│ ┌──────────────────────────────────────┐ │
│ │ Design Review                        │ │
│ │ 10:00 AM - Company Outlook365       │ │
│ │ [Snooze] [Dismiss] [Dismiss All]    │ │
│ └──────────────────────────────────────┘ │
│ ┌──────────────────────────────────────┐ │
│ │ Client Call                          │ │
│ │ 10:05 AM - Personal Google          │ │
│ │ [Snooze] [Dismiss] [Dismiss All]    │ │
│ └──────────────────────────────────────┘ │
└──────────────────────────────────────────┘
```

---

## Implementation Readiness

### Components Ready for Development

| Component | Status | Acceptance Scenarios | Code Examples |
|-----------|--------|----------------------|---------------|
| Multiple Accounts Per Provider | ✅ Ready | 5 scenarios (11-15) | Pattern 14 in plan.md |
| Overlapping Events Handling | ✅ Ready | 5 scenarios (6-10) | Pattern 13 in plan.md |
| Database Schema Updates | ✅ Ready | AccountLabel column, UNIQUE constraint | SQL in plan.md |
| Multi-Event Modal | ✅ Ready | NotificationModalViewModel with ObservableCollection | Code patterns in plan.md |
| Account Labeling UI | ✅ Ready | ProviderSelectionViewModel with label input | Code patterns in plan.md |

### Test Coverage Required

**Unit Tests**:
- `NotificationEngineTests.GroupConcurrentEvents_Groups5MinuteWindow`
- `NotificationModalViewModelTests.DismissEvent_RemovesFromList`
- `NotificationModalViewModelTests.SnoozeEvent_RemovesAndReschedules`
- `ProviderSelectionViewModelTests.AddProvider_ValidatesUniqueLabel`
- `ConfigurationDialogViewModelTests.LoadAccounts_ShowsAllWithLabels`

**E2E Tests**:
- `OverlappingEventsWorkflow` (3 events within 5 minutes → single modal → dismiss each independently)
- `MultipleAccountsWorkflow` (add "Personal Google" + "Work Google" → configure calendars → verify unified notifications)
- `MultiAccountOverlappingEventsWorkflow` (events from multiple accounts overlapping → single modal with account labels)

---

## Specification Completeness

### User Stories
- ✅ Story 1: Receive notifications + **overlapping events handling** ← UPDATED
- ✅ Story 2: Dismiss recurring events
- ✅ Story 3: Configure provider + lead time + auto-dismiss timeout
- ✅ Story 4: System tray access
- ✅ Story 5: Multiple providers + calendar selection + **multiple accounts per provider** ← UPDATED

### Clarification Sessions
- ✅ Session 1: Provider support, terminology, testing, defaults
- ✅ Session 2: Startup notifications, first-time setup, auto-dismiss
- ✅ Session 3: Multi-provider, multi-calendar, calendar selection UI
- ✅ Session 4: **Multiple accounts, overlapping events** ← NEW

### Database Tables
- ✅ `ProviderCredentials` - **Updated with AccountLabel and new UNIQUE constraint**
- ✅ `SelectedCalendars` - **Updated with CASCADE delete**
- ✅ `DismissedEventTitles`
- ✅ `ApplicationState`

### Architectural Patterns
- ✅ Patterns 1-12: (existing patterns)
- ✅ **Pattern 13: Overlapping Events Handling** ← NEW
- ✅ **Pattern 14: Multiple Accounts Per Provider** ← NEW

---

## Migration Considerations

### Database Migration Path

For existing installations, migration script needed:

```sql
-- Migration: Add AccountLabel support to ProviderCredentials
ALTER TABLE ProviderCredentials ADD COLUMN AccountLabel TEXT;
ALTER TABLE ProviderCredentials ADD COLUMN CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP;

-- Set default labels for existing providers
UPDATE ProviderCredentials 
SET AccountLabel = ProviderType || ' Account' 
WHERE AccountLabel IS NULL;

-- Make AccountLabel NOT NULL after defaults set
-- (requires recreation of table in SQLite)

-- Drop old UNIQUE constraint on ProviderType
-- Add new UNIQUE constraint on (ProviderType, AccountLabel)
-- (requires table recreation in SQLite)

-- Add CASCADE delete to SelectedCalendars
-- (requires table recreation in SQLite)
```

### User Impact
- Existing single-account users: No change in behavior
- Users can now add multiple accounts per provider
- Existing configurations automatically labeled

---

## Files Updated

| File | Changes | Lines Added |
|------|---------|-------------|
| `specs/001-modal-calendar-notification/spec.md` | +5 clarifications, +10 acceptance scenarios | ~50 |
| `specs/001-modal-calendar-notification/plan.md` | +2 patterns, updated schema, updated project structure | ~250 |

---

## Status

✅ **CLARIFICATIONS COMPLETE AND INTEGRATED**

All acceptance scenarios for:
- Multiple accounts per provider type (with custom labels)
- Overlapping events handling (multi-event modal)
- Updated database schema supporting multiple accounts

...have been added to spec.md and plan.md with full architectural support.

The specification is now **ready for Phase 2 implementation** of these features.

---

**Date**: February 23, 2026  
**Workflow**: speckit.clarify  
**Status**: ✅ Complete  
**Next Phase**: Implementation (speckit.tasks or speckit.implement)


