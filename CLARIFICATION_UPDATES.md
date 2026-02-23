# ✅ CLARIFICATION UPDATES COMPLETE

## Summary of Changes

Following the speckit.clarify workflow, the Modal Calendar Notification specification and implementation plan have been updated with:

1. **Calendar Selection User Acceptance Scenarios** (User Story 5)
2. **Auto-Dismiss Timeout Configuration Specifications**

---

## Changes to spec.md

### User Story 3 - Enhanced with Auto-Dismiss Timeout Configuration

Added 4 new acceptance scenarios (now 12 total) to User Story 3:

**New Scenarios Added**:
- **Scenario 7**: "Given the configuration dialog is open under 'Notification Settings', When I view the auto-dismiss timeout control, Then I see a spinner/input field labeled 'Auto-dismiss notification after (minutes)' with a default value of 10"
- **Scenario 8**: "Given the auto-dismiss timeout control is visible, When I change the value from 10 to 15 minutes and click 'Save', Then the new timeout is persisted and applied to all future notifications"
- **Scenario 9**: "Given an event notification is displayed, When I do not interact with the notification modal (no snooze, dismiss, or dismiss-all-future action) for the configured auto-dismiss duration after the event start time, Then the modal automatically closes without adding the event title to the dismissed list"
- **Scenario 10**: "Given auto-dismiss timeout is set to 5 minutes and an event starts, When 5 minutes pass after the event start time without user interaction, Then the notification modal closes automatically"

### User Story 5 - Complete Calendar Selection Acceptance Scenarios

Updated with 10 comprehensive acceptance scenarios for calendar selection workflow:

**Calendar Selection Scenarios** (all now in spec.md):
1. Provider dialog shows list of available providers
2. After OAuth, calendar selection dialog shows all available calendars
3. All calendars are checked/selected by default
4. Users can uncheck specific calendars
5. Selections are persisted when "Apply" is clicked
6. Multiple providers can be configured independently
7. Events from all selected calendars appear in unified notification stream
8. Calendar selections are preserved when reopening configuration
9. Users can re-enable deselected calendars without re-authenticating
10. When all calendars are deselected, no notifications are generated

---

## Changes to plan.md

### Updated Project Structure - ConfigurationManagement Feature

Enhanced ConfigurationManagement section in project structure to include:
- `CalendarListViewModel.cs` - MVVM ViewModel for calendar selection
- `CalendarListDialog.xaml/.cs` - Calendar selection UI with checkboxes
- Updated `ConfigurationDialogViewModel.cs` to include auto-dismiss timeout
- Updated `ConfigurationDialog.xaml/.cs` with notification behavior tab

### Added New Architectural Patterns (Patterns 5 & 6)

**Pattern 5: Calendar Selection Dialog Pattern**
- CalendarListViewModel with checkbox binding
- CalendarSelectionItem model
- CalendarListDialog WPF implementation
- Default all-selected behavior
- Selection persistence strategy

**Pattern 6: Auto-Dismiss Timeout Configuration Pattern**
- ConfigurationDialogViewModel with timeout property
- AutoDismissHandler scheduling logic
- XAML spinner control (min:1, max:120 minutes)
- Validation and persistence of timeout value
- AutoDismiss does NOT add to dismissed titles

### Updated Vertical Slice Architecture Diagram

Updated Feature 4 (ConfigurationManagement) in architecture diagram:
- Added `CalendarListDialog` (WPF UI for calendar selection)
- Updated `ConfigurationDialog` (WPF UI with auto-dismiss timeout control)

### Enhanced Database Schema

Added new table to support calendar selection:

```sql
-- Selected calendars per provider (user can select which calendars to monitor)
CREATE TABLE SelectedCalendars (
    Id INTEGER PRIMARY KEY,
    ProviderCredentialsId INTEGER NOT NULL,
    RemoteCalendarId TEXT NOT NULL,
    CalendarName TEXT NOT NULL,
    IsSelected BOOLEAN DEFAULT 1,
    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY(ProviderCredentialsId) REFERENCES ProviderCredentials(Id),
    UNIQUE(ProviderCredentialsId, RemoteCalendarId)
);
```

### Updated Architectural Patterns Numbering

Renumbered all patterns to reflect new additions:
- Pattern 5: Calendar Selection Dialog Pattern (NEW)
- Pattern 6: Auto-Dismiss Timeout Configuration Pattern (NEW)
- Pattern 7: Provider Abstraction Pattern (was 5)
- Pattern 8: Repository Pattern for Data Persistence (was 3)
- Pattern 9: Dependency Injection for Vendor Independence (was 4)
- Pattern 10: Functional Programming in Notification Engine (was 5)
- Pattern 11: System Tray Integration with Modal Windows (was 6)
- Pattern 12: Startup Missed Event Detection (was 7)
- Pattern 13: Auto-Dismiss After Configurable Timeout (was 8)

---

## Key Design Decisions

### Calendar Selection Flow

1. **User clicks "Add Provider"** → Provider selection dialog
2. **User selects provider** → OAuth authentication
3. **OAuth completes** → CalendarListDialog appears
4. **User sees calendar list** with all calendars checked by default
5. **User unselects unwanted calendars** → Updates IsSelected in UI
6. **User clicks "Apply"** → Selections persisted to SelectedCalendars table
7. **NotificationEngine filters** by selected calendars → Only generates notifications for selected calendars

### Auto-Dismiss Timeout Configuration

- **UI Control**: Spinner with range 1-120 minutes
- **Default Value**: 10 minutes after event start time
- **Behavior**: 
  - Modal automatically closes if no user interaction
  - Does NOT mark event as "dismissed" (no title persistence)
  - Future occurrences will still trigger notifications
- **Distinction**:
  - Auto-dismiss ≠ "Dismiss All Future"
  - Auto-dismiss is temporary inactivity timeout
  - "Dismiss All Future" permanently suppresses event title

### NotificationEngine Updates

The NotificationEngine must be updated to:
1. **Load selected calendars** from SelectedCalendars table per provider
2. **Filter events** to only those from selected calendars
3. **Apply auto-dismiss timeout** from configuration (not event-specific)
4. **Distinguish** between auto-dismiss and explicit dismiss-all-future

---

## Specification Completeness Status

### User Stories
- ✅ User Story 1: Receive notifications (P1)
- ✅ User Story 2: Dismiss recurring events (P2)
- ✅ User Story 3: Configure calendar provider + notification lead time + **auto-dismiss timeout** (P3)
- ✅ User Story 4: System tray access (P4)
- ✅ User Story 5: **Multiple providers + calendar selection** (P2)

### Functional Requirements Additions

**New Requirements for Calendar Selection**:
- FR-007a: System MUST display calendar list after OAuth completion
- FR-007b: System MUST default all calendars to selected
- FR-007c: System MUST allow users to unselect specific calendars
- FR-007d: System MUST persist calendar selections per provider
- FR-007e: System MUST filter notifications by selected calendars

**New Requirements for Auto-Dismiss Timeout**:
- FR-028a: Configuration dialog MUST include auto-dismiss timeout control
- FR-028b: Control MUST accept values 1-120 minutes
- FR-028c: Default value MUST be 10 minutes
- FR-028d: User MUST be able to save and apply new timeout value
- FR-029a: Auto-dismissed modals MUST NOT persist as dismissed titles
- FR-030: Future event occurrences MUST trigger notifications despite auto-dismiss

### Success Criteria Alignment

- ✅ SC-004: "Users can select from available providers, configure their chosen calendar integration" → Now includes calendar selection
- ✅ SC-014: "Multiple providers" support documented with calendar selection scenarios
- ✅ SC-015: "Calendar selection and persistence" fully specified
- ✅ SC-018: "Calendar toggle without re-authentication" documented
- ✅ SC-020: "Auto-dismiss behavior clarification" provided

---

## Test Coverage Implications

### New Unit Tests Required

**CalendarListViewModelTests.cs**:
- LoadCalendars restores previous selections
- GetSelectedCalendars returns only checked items
- IsSelected binding works bidirectionally

**ConfigurationDialogViewModelTests.cs**:
- AutoDismissMinutesAfterStart range validation (1-120)
- SaveConfiguration persists timeout value
- Default value is 10

**NotificationEngineTests.cs** (updated):
- FilterBySelectedCalendars returns only selected calendar events
- Multiple calendars across providers handled correctly
- Auto-dismiss respects configured timeout
- Auto-dismiss does NOT add to dismissed titles

### New E2E Test Workflows

**CalendarSelectionWorkflow**:
1. Configure provider → OAuth completes
2. Calendar selection dialog appears
3. Uncheck specific calendars
4. Click Apply
5. Verify only selected calendars generate notifications

**AutoDismissTimeoutWorkflow**:
1. Open configuration dialog
2. Change auto-dismiss timeout to 5 minutes
3. Save configuration
4. Display notification
5. Wait 5 minutes without interaction
6. Verify modal closes automatically
7. Verify event is NOT in dismissed list
8. Verify next occurrence still triggers notification

---

## Compatibility Notes

### Backward Compatibility
- ✅ Existing single-provider users unaffected
- ✅ Single-calendar selection works the same
- ✅ Auto-dismiss defaults to 10 minutes (existing behavior)

### Migration Path
- ✅ SelectedCalendars table created on first run
- ✅ Existing ProviderCredentials entries continue working
- ✅ Configuration auto-applies new settings

---

## Documentation Status

| Document | Status | Changes |
|----------|--------|---------|
| spec.md | ✅ Updated | +4 auto-dismiss scenarios, +10 calendar selection scenarios |
| plan.md | ✅ Updated | +2 patterns, architecture diagram updated, database schema updated |
| Project Structure | ✅ Updated | CalendarListViewModel/Dialog added, ConfigurationDialogViewModel enhanced |
| Architectural Patterns | ✅ Added | Patterns 5 & 6 added with code examples |

---

## Ready for Implementation

The specification and plan are now complete and ready for Phase 2 implementation of:

1. **CalendarSelection feature** with full acceptance scenarios
2. **Auto-dismiss timeout configuration** with UI controls
3. **SelectedCalendars table** for persistence
4. **NotificationEngine filtering** by selected calendars

All clarifications have been resolved and integrated into both documents.

---

**Update Completed**: February 23, 2026  
**Changes**: Calendar selection scenarios + Auto-dismiss timeout configuration  
**Status**: ✅ Specification complete and ready for implementation


