# Feature Specification: Modal Calendar Notification Application

**Feature Branch**: `001-modal-calendar-notification`  
**Created**: February 23, 2026  
**Status**: Draft  
**Input**: User description: "Build a Windows application that integrates with Outlook365 calendar. A configurable amount of minutes before an event, a modal dialog should appear on the main display. It shows the name of the event, and allows for snoozing it for a selectable amount of minutes. You can also dismiss it. Third, you can dismiss it, and all following events with the same title. In the system tray, there is an icon which allows access to a configuration dialog. Here you can manage the integration with the calendar. You can also see the events for which all following events were dismissed, and cancel that dismissal. That dialog also shows version information. Make sure that the design allows for adding other calendar integrations, e.g. Google Calendar in the future. On start-up, it should check if any events were missed since the last modal dialog was shown, and if there are any, show a modal dialog that lists them."

## Clarifications

### Session 2026-02-23

- Q: Calendar provider support requirement for v1.0 → A: Version 1.0 MUST support both Microsoft Outlook365 AND Google Calendar (not Outlook365 initially with Google Calendar as future enhancement)
- Q: Provider selection workflow during configuration → A: When adding credentials, system must first display a list of available calendar providers for user selection before presenting provider-specific credential entry forms
- Q: Terminology standardization → A: Use "Microsoft Outlook365" and "Google Calendar" consistently throughout specification (not "Outlook365" standalone)
- Q: End-to-end testing approach → A: All E2E tests MUST be fully automated with no manual testing required; calendar provider APIs (Microsoft Outlook365 and Google Calendar) MUST be mocked/stubbed in E2E tests; all user interactions (button clicks, configuration entry) MUST be automated; tests MUST be repeatable and deterministic
- Q: Default notification lead time → A: 3 minutes
- Q: Snooze duration configuration granularity → A: 1 minute
- Q: Authentication mechanism for calendar providers → A: OAuth 2.0 authentication for both Microsoft Outlook365 and Google Calendar providers with credentials encrypted using Windows DPAPI

### Session 2026-02-23 (Continued)

- Q: Handling of past events in startup notifications → A: Missed event notifications are only shown once during application startup, within a 24-hour lookback window. Events in the past are never shown again after startup, even if not explicitly dismissed by the user. This 24-hour window applies only to application restarts, not to initial credential setup.
- Q: First-time credential configuration missed events behavior → A: When credentials are added for the first time during initial setup, the system MUST NOT display missed events from the previous 24 hours. Missed event startup notifications only apply to subsequent application restarts after credentials are already configured.
- Q: Auto-dismiss of untouched notifications → A: Notifications that remain in the modal state and are not touched by the user (no snooze, dismiss, or dismiss all future action taken) MUST be automatically dismissed after a configurable number of minutes following the event's start time (configurable by end users, default: 10 minutes post-event-start).

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Receive Intrusive Event Notifications (Priority: P1)

As a busy professional, I want to receive a highly visible modal notification before my calendar events start, so that I never miss important meetings or appointments that I would otherwise overlook in standard calendar applications.

**Why this priority**: This is the core value proposition of the application - solving the primary problem of missed calendar notifications. Without this, the application has no purpose.

**Independent Test**: Can be fully tested by creating a calendar event in Outlook365, configuring a lead time, and verifying that a modal dialog appears at the correct time on the primary display showing the event name and delivers immediate interruption value.

**Acceptance Scenarios**:

1. **Given** I have an upcoming calendar event in 3 minutes, **When** my configured notification lead time is 3 minutes, **Then** a modal dialog appears on my primary display showing the event name
2. **Given** a notification modal is displayed, **When** I click "Snooze" and select 5 minutes, **Then** the modal closes and reappears 5 minutes later
3. **Given** a notification modal is displayed, **When** I click "Dismiss", **Then** the modal closes and does not reappear for that specific event occurrence
4. **Given** I have multiple events at different times, **When** each event reaches its notification lead time, **Then** separate modal dialogs appear for each event
5. **Given** the application was closed during the time a notification should have appeared, **When** I restart the application, **Then** a startup modal displays all missed events since the last shown notification

---

### User Story 2 - Dismiss Recurring Event Notifications (Priority: P2)

As a user with regular recurring meetings that I no longer attend, I want to dismiss all future notifications for events with the same title, so that I am not repeatedly interrupted by notifications for events I don't need to be reminded about.

**Why this priority**: This addresses a common pain point with recurring events and reduces notification fatigue, but the core notification functionality (P1) must exist first.

**Independent Test**: Can be fully tested by creating a recurring event series, displaying a notification, clicking "Dismiss All Future", and verifying that subsequent occurrences of that event title do not trigger notifications while other events still do.

**Acceptance Scenarios**:

1. **Given** a notification modal is displayed for an event, **When** I click "Dismiss All Future", **Then** the modal closes and no future events with the exact same title trigger notifications
2. **Given** I have dismissed all future notifications for "Daily Standup", **When** an event titled "Weekly Review" occurs, **Then** I still receive a notification for "Weekly Review"
3. **Given** I dismissed all future notifications for an event title, **When** I access the configuration dialog, **Then** I can see that event title listed in the dismissed events section
4. **Given** an event title is in my dismissed list, **When** I select it and click "Restore Notifications", **Then** future events with that title will trigger notifications again

---

### User Story 3 - Configure Calendar Integration and Notification Settings (Priority: P3)

As a user, I want to configure which calendar provider I'm using and how far in advance I receive notifications, so that the application works with my calendar system and notification preferences.

**Why this priority**: Configuration is essential but requires the core notification mechanism (P1) to exist first. Users need default settings to work initially.

**Independent Test**: Can be fully tested by accessing the system tray icon, opening the configuration dialog, selecting a provider from the list, entering calendar credentials, setting notification lead time, and verifying that notifications now appear based on those settings.

**Acceptance Scenarios**:

1. **Given** the application is running, **When** I click the system tray icon and select "Settings", **Then** a configuration dialog appears
2. **Given** the configuration dialog is open and I choose to add calendar credentials, **When** the provider selection appears, **Then** I see a list of available calendar providers (Microsoft Outlook365, Google Calendar)
3. **Given** the provider selection list is displayed, **When** I select "Microsoft Outlook365", **Then** the application initiates OAuth 2.0 authentication flow for Outlook365
4. **Given** the provider selection list is displayed, **When** I select "Google Calendar", **Then** the application initiates OAuth 2.0 authentication flow for Google Calendar
5. **Given** the configuration dialog is open, **When** I complete the OAuth 2.0 authentication flow for my selected calendar provider, **Then** the application successfully connects and retrieves my calendar events
6. **Given** the configuration dialog is open, **When** I set the notification lead time to 10 minutes, **Then** all future notifications appear 10 minutes before events start
7. **Given** the configuration dialog is open, **When** I view the "About" section, **Then** I see the current version number of the application
8. **Given** I have not yet configured a calendar integration, **When** I start the application for the first time, **Then** I am prompted to configure calendar access before notifications can begin

---

### User Story 4 - System Tray Presence and Quick Access (Priority: P4)

As a user, I want the application to run unobtrusively in the system tray with quick access to settings, so that it doesn't clutter my taskbar but remains easily accessible when I need to configure it.

**Why this priority**: This is a user experience enhancement that makes the application feel polished and professional, but the core notification functionality must exist first.

**Independent Test**: Can be fully tested by starting the application, verifying it appears only in the system tray (not taskbar), right-clicking the icon, and accessing the configuration dialog and exit options.

**Acceptance Scenarios**:

1. **Given** the application is running, **When** I look at the system tray, **Then** I see the application icon
2. **Given** the application is running, **When** I look at the taskbar, **Then** I do not see an application window (only modal notifications appear as needed)
3. **Given** the system tray icon is visible, **When** I right-click it, **Then** I see a context menu with "Settings", "Exit", and other relevant options
4. **Given** the system tray icon is visible, **When** I left-click it, **Then** the configuration dialog opens
5. **Given** I select "Exit" from the system tray menu, **When** the application closes, **Then** no further notifications appear until I restart the application

---

### Edge Cases

- What happens when the application loses connection to the calendar service (network outage, credential expiration)?
  - Application should attempt to reconnect automatically at regular intervals
  - User should be notified via system tray icon tooltip or status indicator in settings
  - Cached events should continue to trigger notifications if they were already retrieved
  
- How does the system handle multiple simultaneous events starting at the same time?
  - All events should be shown in a single modal with a list of concurrent events
  - Each event should have its own snooze/dismiss controls
  
- What happens when an event is modified or cancelled in the calendar after the notification has been scheduled?
  - Application should sync with calendar at regular intervals (configurable, default: every 5 minutes)
  - Cancelled events should not trigger notifications
  - Modified event times should update notification schedules
  
- How does the system handle events that start in the past (user sets up calendar integration after events have passed)?
  - On application startup (after initial setup), show startup modal with missed events from the last 24 hours only
  - Events in the past are shown ONLY on application startup within the 24-hour lookback window and are never shown again, even if not explicitly dismissed
  - When credentials are added for the first time during initial setup, do NOT display missed events from the previous 24 hours
  - Provide option in settings to configure the missed event lookback window duration
  
- What happens when a user dismisses "all future" for an event title, but then a new event with that exact title is created later?
  - The dismissal applies to all events with that title, regardless of when they were created
  - User must manually restore notifications for that title if they want to be notified
  
- How does the system handle very long event titles that don't fit in the modal dialog?
  - Event titles should wrap to multiple lines if needed
  - Modal should have a maximum width but expand vertically as needed
  - Extremely long titles (>500 characters) should be truncated with ellipsis and show full text on hover
  
- What happens when the user's system time zone changes?
  - Application should detect time zone changes and recalculate notification times
  - Events should be displayed in the current local time zone
  
- How does the system handle snooze when the event start time is reached or passed?
  - If snoozed time extends beyond event start time, notification should still appear
  - Notification should indicate that the event has already started
  
- What happens when the primary display changes (laptop connected to/disconnected from external monitor)?
  - Application should detect display configuration changes
  - Modal should always appear on the current primary display
  - If primary display is disconnected, modal should appear on an available display

- What happens to a notification modal that the user doesn't interact with?
  - If a notification modal remains open and the user takes no action (no snooze, dismiss, or dismiss all future) for a configurable duration after the event start time, the modal MUST automatically dismiss itself
  - Default auto-dismiss timeout: 10 minutes after event start time
  - Auto-dismiss timeout is configurable by end users through the configuration dialog
  - Auto-dismissed notifications do not persist as dismissed event titles and will trigger notifications for future occurrences

## Requirements *(mandatory)*

### Functional Requirements

**Calendar Integration**

- **FR-001**: System MUST support integration with both Microsoft Outlook365 and Google Calendar providers in version 1.0
- **FR-002**: System MUST be architecturally designed to support additional calendar providers through extensible provider interfaces
- **FR-003**: System MUST authenticate users with their selected calendar provider using OAuth 2.0 protocol
- **FR-004**: System MUST synchronize calendar events at configurable intervals (default: every 5 minutes)
- **FR-005**: System MUST handle calendar API connection failures gracefully and attempt automatic reconnection
- **FR-006**: System MUST retrieve event attributes including: title, start time, end time, and unique event identifier

**Notification Display**

- **FR-007**: System MUST display a modal dialog on the primary display at a configurable time before each event starts (default: 3 minutes)
- **FR-008**: Modal dialog MUST show the event title
- **FR-009**: Modal dialog MUST appear on top of all other windows and require user interaction to dismiss
- **FR-010**: Modal dialog MUST remain visible until the user takes action (snooze, dismiss, or dismiss all future)
- **FR-011**: System MUST support displaying multiple concurrent events in a single modal if they occur simultaneously

**Notification Actions**

- **FR-012**: Users MUST be able to snooze a notification for a user-selectable duration
- **FR-013**: System MUST provide preset snooze duration options (1 minute, 3 minutes, 5 minutes, 10 minutes) and allow custom duration input with 1-minute granularity
- **FR-014**: Users MUST be able to dismiss a single event notification
- **FR-015**: Users MUST be able to dismiss all future notifications for events with the same title
- **FR-016**: System MUST maintain a persistent list of event titles for which all future notifications have been dismissed
- **FR-017**: System MUST prevent notifications for events matching dismissed titles

**Startup Behavior**

- **FR-018**: On application startup, system MUST check for events that were missed since the last notification was shown
- **FR-019**: If missed events exist, system MUST display a startup modal listing all missed events within the last 24 hours
- **FR-020**: Startup modal MUST allow users to review missed events and acknowledge them
- **FR-021**: System MUST persist the timestamp of the last shown notification to disk to survive application restarts
- **FR-022**: When credentials are added for the first time during initial setup, system MUST NOT display missed events from the previous 24 hours
- **FR-023**: Events that are displayed in the startup modal are shown only once and never again, even if not explicitly dismissed by the user
- **FR-024**: System MUST NOT display past events again after the startup phase, except on subsequent application restarts within the 24-hour missed event window

**System Tray Integration**

- **FR-025**: Application MUST run as a system tray application (no taskbar window)
- **FR-026**: System tray icon MUST provide a context menu with access to settings and exit options
- **FR-027**: System tray icon MUST indicate application status (connected, disconnected, error) through visual changes or tooltip

**Notification Auto-Dismiss**

- **FR-028**: If a notification modal remains open and the user takes no action (no snooze, dismiss, or dismiss all future) for a configurable duration after the event start time, the modal MUST automatically dismiss itself
- **FR-029**: Default auto-dismiss timeout is 10 minutes after event start time (configurable by end users through the configuration dialog)
- **FR-030**: Auto-dismissed notifications MUST NOT persist as dismissed event titles and MUST trigger notifications for future occurrences of the same event

**Configuration Management**

- **FR-031**: System MUST provide a configuration dialog accessible from the system tray
- **FR-032**: Configuration dialog MUST display a list of supported calendar providers (Microsoft Outlook365, Google Calendar) when adding credentials
- **FR-033**: Configuration dialog MUST allow users to select a calendar provider before entering provider-specific credentials
- **FR-034**: Configuration dialog MUST allow users to set up and manage calendar provider credentials for their selected provider
- **FR-035**: Configuration dialog MUST allow users to configure notification lead time (in minutes)
- **FR-036**: Configuration dialog MUST allow users to configure auto-dismiss timeout (in minutes)
- **FR-037**: Configuration dialog MUST allow users to configure calendar synchronization interval
- **FR-038**: Configuration dialog MUST display a list of event titles for which all future notifications have been dismissed
- **FR-039**: Configuration dialog MUST allow users to restore notifications for previously dismissed event titles
- **FR-040**: Configuration dialog MUST display application version information
- **FR-041**: System MUST persist all configuration settings to survive application restarts

**Data Persistence**

- **FR-042**: System MUST persist user configuration (calendar credentials, notification lead time, auto-dismiss timeout, sync interval)
- **FR-043**: System MUST persist the list of dismissed event titles
- **FR-044**: System MUST persist the timestamp of the last shown notification
- **FR-045**: System MUST encrypt sensitive data (OAuth 2.0 tokens and calendar credentials) at rest using Windows DPAPI (Data Protection API)

**Error Handling**

- **FR-046**: System MUST provide clear error messages when calendar authentication fails
- **FR-047**: System MUST notify users when calendar synchronization fails
- **FR-048**: System MUST continue operating with cached event data when temporary connection issues occur
- **FR-049**: System MUST log errors for troubleshooting purposes

**Testing Requirements (Constitutional Principle IV)**

- **FR-050**: All end-to-end tests MUST be fully automated, executing complete user workflows from application startup through notification display without manual intervention
- **FR-051**: Calendar provider APIs (Microsoft Outlook365 and Google Calendar) MUST be mocked or stubbed in E2E tests, not hitting real external services
- **FR-052**: All user interactions in E2E tests (button clicks, configuration entry, notification dismissal) MUST be automated through test code
- **FR-053**: E2E test suite MUST be repeatable and deterministic, producing identical results across multiple executions
- **FR-054**: E2E tests MUST exercise all internal components in real configuration with only external third-party dependencies mocked

### Key Entities

- **Calendar Event**: Represents a scheduled event from the user's calendar. Key attributes include unique identifier, title (text), start time, end time, and source calendar. Events are retrieved from external calendar providers and cached locally for notification scheduling.

- **Notification**: Represents a scheduled notification for a calendar event. Key attributes include associated event identifier, scheduled notification time (calculated from event start time minus lead time), notification status (pending, shown, snoozed, dismissed), and snooze duration if applicable. Notifications track the lifecycle of user interactions with event alerts.

- **Dismissed Event Title**: Represents an event title for which all future notifications have been dismissed. Key attributes include the exact event title text and the timestamp when the dismissal was created. This entity enables filtering of future notifications.

- **Calendar Provider Configuration**: Represents the connection details for a calendar service. Key attributes include provider type (Microsoft Outlook365 or Google Calendar in v1.0, extensible for additional providers), OAuth 2.0 authentication tokens, connection status, and last successful sync timestamp. This entity is designed to support multiple provider types through a common interface.

- **Application Configuration**: Represents user preferences and settings. Key attributes include notification lead time (minutes), calendar sync interval (minutes), primary calendar provider selection, and startup behavior preferences. This entity persists user customizations.

- **Application State**: Represents runtime state that must persist across restarts. Key attributes include last notification shown timestamp, currently snoozed notifications with their wake times, and calendar provider connection health status.

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can see modal notifications for upcoming calendar events at their configured lead time with 100% accuracy (no missed notifications when application is running)

- **SC-002**: Users receive startup notifications for all missed events within 2 seconds of application launch (only on restart after initial setup; first-time credential setup does not display missed events)

- **SC-003**: Modal notifications appear on the primary display and capture user attention without fail (modal remains on top until user interaction)

- **SC-004**: Users can select from available providers (Microsoft Outlook365 or Google Calendar), configure their chosen calendar integration, and begin receiving notifications within 3 minutes of first application launch

- **SC-005**: Application successfully syncs with calendar provider and updates event list every 5 minutes (configurable) with 99% success rate under normal network conditions

- **SC-006**: Users can snooze notifications and receive re-notification at exactly the selected snooze time (within 5 seconds accuracy)

- **SC-007**: Users can dismiss all future notifications for a specific event title and receive zero notifications for that title going forward

- **SC-008**: Users can restore notifications for previously dismissed event titles within 30 seconds of accessing the configuration dialog

- **SC-009**: Application runs continuously in the system tray with less than 50MB memory footprint and negligible CPU usage when idle

- **SC-010**: Application persists all configuration and state data correctly through unexpected shutdowns and restarts (100% data recovery)

- **SC-011**: Application handles temporary network outages and reconnects to calendar provider within 1 minute of network restoration without user intervention

- **SC-012**: Users can access configuration dialog from system tray within 2 clicks

- **SC-013**: Application architecture supports adding new calendar provider integrations with changes isolated to provider-specific modules (vendor independence verification)

- **SC-014**: All end-to-end tests execute fully automated from application startup through notification display with no manual intervention required, using mocked calendar provider APIs (Microsoft Outlook365 and Google Calendar), automated user interactions (button clicks, configuration entry), and deterministic, repeatable results aligning with Constitutional Principle IV (Functional End-to-End Testing with third-party dependencies mocked)

- **SC-015**: Untouched notification modals automatically dismiss after the configured timeout period (default: 10 minutes post-event-start) and do not persist as dismissed event titles, allowing future occurrences to trigger notifications

## Assumptions *(optional)*

- Users have active accounts with supported calendar providers (Microsoft Outlook365 or Google Calendar) with calendar access
- Version 1.0 MUST support both Microsoft Outlook365 and Google Calendar providers
- Users run Windows 10 or later operating systems
- Users have network connectivity to access cloud calendar services
- Users want notifications for all calendar events (no event-type filtering needed initially)
- Default notification lead time of 3 minutes is suitable for most users
- Events with identical titles are considered part of the same recurring series for "dismiss all future" purposes
- Users understand that dismissing "all future" applies to event title text matching, not calendar event IDs
- Application has permission to run on startup (users can configure this via Windows settings)
- Primary display detection uses Windows API for multi-monitor setups
- Calendar providers support standard event modification timestamps for detecting changes
- Missed event window of 24 hours applies only to application restarts after initial credential setup; first-time credential configuration does NOT show missed events from the previous 24 hours
- Events in the past are shown only once during the first application startup within the 24-hour lookback window and are never displayed again after that
- Auto-dismiss timeout is configurable by end users through the configuration dialog; default is 10 minutes after event start time
- Auto-dismissed notifications do not persist as dismissed event titles and will trigger notifications for future occurrences
- End-to-end tests use mocked/stubbed calendar provider APIs (Microsoft Outlook365 and Google Calendar) rather than hitting real external services, ensuring tests are repeatable, deterministic, and align with Constitutional Principle IV (Functional End-to-End Testing with only third-party dependencies mocked)
- All testing (unit and E2E) is fully automated with no manual testing required for validation

## Open Questions *(optional)*

None - all reasonable defaults have been established. The specification is complete and ready for planning.

