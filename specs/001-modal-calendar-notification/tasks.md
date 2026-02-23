# Tasks: Modal Calendar Notification Application

**Feature**: Modal Calendar Notification Application  
**Branch**: `001-modal-calendar-notification`  
**Generated**: February 23, 2026  
**Based On**: plan.md v1.0, spec.md v1.0

---

## Overview

This document provides a dependency-ordered, independently testable task breakdown for implementing the Modal Calendar Notification Application. Tasks are organized by user story to enable incremental delivery and parallel execution where possible.

**Implementation Strategy**: 
- **MVP First**: User Story 1 (P1) delivers core notification value
- **Incremental Delivery**: Each user story is independently testable
- **Parallel Opportunities**: Tasks marked with `[P]` can be executed in parallel when working in different files

**Total Tasks**: 98  
**User Stories**: 4 (US1: P1, US2: P2, US3: P3, US5: P2)

---

## Phase 1: Setup & Project Initialization

**Goal**: Establish project structure, dependencies, and foundational infrastructure

**Tasks**:

- [X] T001 Create solution file `ModalCalendarNotification.sln` in `src/ModalCalendarNotification/`
- [X] T002 Create Directory.Packages.props with centralized NuGet package versions in `src/ModalCalendarNotification/Directory.Packages.props`
- [X] T003 Create main project `ModalCalendarNotification.csproj` in `src/ModalCalendarNotification/ModalCalendarNotification/` (WPF .NET 10 application)
- [X] T004 Create core library project `ModalCalendarNotification.Core.csproj` in `src/ModalCalendarNotification/ModalCalendarNotification.Core/`
- [X] T005 Create data library project `ModalCalendarNotification.Data.csproj` in `src/ModalCalendarNotification/ModalCalendarNotification.Data/`
- [X] T006 Create calendar providers library `ModalCalendarNotification.CalendarProviders.csproj` in `src/ModalCalendarNotification/ModalCalendarNotification.CalendarProviders/`
- [X] T007 Create UI library project `ModalCalendarNotification.UI.csproj` in `src/ModalCalendarNotification/ModalCalendarNotification.UI/`
- [X] T008 Create unit test project `ModalCalendarNotification.Tests.Unit.csproj` in `tests/ModalCalendarNotification.Tests.Unit/`
- [X] T009 Create E2E test project `ModalCalendarNotification.Tests.EndToEnd.csproj` in `tests/ModalCalendarNotification.Tests.EndToEnd/`
- [X] T010 Configure NuGet packages in Directory.Packages.props: WPF, CommunityToolkit.Mvvm, Microsoft.Data.Sqlite, Microsoft.Identity.Client, Polly, Serilog, xUnit, FakeItEasy, Shouldly, Kellerman.CompareNetObjects
- [X] T011 Create .editorconfig with Roslynator analyzers enabled in `src/ModalCalendarNotification/.editorconfig`
- [X] T012 Create appsettings.json configuration template in `src/ModalCalendarNotification/ModalCalendarNotification/appsettings.json`
- [X] T013 Create README.md with build and run instructions in `src/ModalCalendarNotification/README.md`

---

## Phase 2: Foundational Components (Blocking Prerequisites)

**Goal**: Build shared infrastructure required by all user stories

**Tasks**:

- [X] T014 [P] Create CalendarEvent shared model in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Shared/Models/CalendarEvent.cs`
- [X] T015 [P] Create Notification shared model in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Shared/Models/Notification.cs`
- [X] T016 [P] Create ApplicationConfiguration shared model in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Shared/Models/ApplicationConfiguration.cs`
- [X] T017 [P] Create TimeProvider utility for clock abstraction in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Shared/Utilities/TimeProvider.cs`
- [X] T018 [P] Create CryptoHelper utility with Windows DPAPI wrapper in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Shared/Utilities/CryptoHelper.cs`
- [X] T019 [P] Create PollyPolicies utility for resilience policies in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Shared/Utilities/PollyPolicies.cs`
- [X] T020 [P] Create DisplayHelper utility for primary display detection in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Shared/Utilities/DisplayHelper.cs`
- [X] T021 Create AppDbContext for SQLite in `src/ModalCalendarNotification/ModalCalendarNotification.Data/AppDbContext.cs`
- [X] T022 Create initial database migration 001_InitialSchema in `src/ModalCalendarNotification/ModalCalendarNotification.Data/Migrations/001_InitialSchema.cs`
- [X] T023 Create database migration 002_AddAutoDismissConfig in `src/ModalCalendarNotification/ModalCalendarNotification.Data/Migrations/002_AddAutoDismissConfig.cs`
- [X] T024 Create App.xaml and App.xaml.cs for WPF bootstrap in `src/ModalCalendarNotification/ModalCalendarNotification/App.xaml` and `App.xaml.cs`
- [X] T025 Create Program.cs entry point with DI configuration in `src/ModalCalendarNotification/ModalCalendarNotification/Program.cs`
- [X] T026 Create ServiceConfiguration.cs for DI container setup in `src/ModalCalendarNotification/ModalCalendarNotification/ServiceConfiguration.cs`
- [X] T027 Create ApplicationLifecycleManager for startup/shutdown in `src/ModalCalendarNotification/ModalCalendarNotification/ApplicationLifecycleManager.cs`

**Unit Tests**:

- [X] T028 [P] Write unit tests for TimeProvider in `tests/ModalCalendarNotification.Tests.Unit/Shared/TimeProviderTests.cs`
- [X] T029 [P] Write unit tests for CryptoHelper (DPAPI) in `tests/ModalCalendarNotification.Tests.Unit/Shared/CryptoHelperTests.cs`
- [X] T030 [P] Write unit tests for PollyPolicies in `tests/ModalCalendarNotification.Tests.Unit/Shared/PollyPoliciesTests.cs`
- [X] T031 [P] Write unit tests for DisplayHelper in `tests/ModalCalendarNotification.Tests.Unit/Shared/DisplayHelperTests.cs`

---

## Phase 3: User Story 1 - Receive Intrusive Event Notifications (P1)

**Priority**: P1  
**Goal**: Users can receive modal notifications for upcoming calendar events at configured lead time  
**Independent Test Criteria**: Create calendar event in test provider → Configure 3-minute lead time → Verify modal appears on primary display at correct time with event name

**Tasks**:

### Authentication & Calendar Integration

- [X] T032 [P] [US1] Create IAuthenticationService interface in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/Authentication/IAuthenticationService.cs`
- [X] T033 [US1] Implement OAuthService with MSAL wrapper and Polly resilience in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/Authentication/OAuthService.cs`
- [X] T034 [P] [US1] Implement CredentialEncryption with Windows DPAPI in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/Authentication/CredentialEncryption.cs`
- [X] T035 [P] [US1] Implement TokenRefreshOrchestrator with retry logic in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/Authentication/TokenRefreshOrchestrator.cs`
- [X] T036 [P] [US1] Create ICalendarProvider interface in `src/ModalCalendarNotification/ModalCalendarNotification.CalendarProviders/ICalendarProvider.cs`
- [X] T037 [US1] Implement OutlookCalendarProvider with OAuth 2.0 via MSAL in `src/ModalCalendarNotification/ModalCalendarNotification.CalendarProviders/OutlookCalendarProvider.cs`
- [X] T038 [US1] Implement GoogleCalendarProvider with OAuth 2.0 via MSAL in `src/ModalCalendarNotification/ModalCalendarNotification.CalendarProviders/GoogleCalendarProvider.cs`
- [X] T039 [P] [US1] Create OutlookEventMapper for event normalization in `src/ModalCalendarNotification/ModalCalendarNotification.CalendarProviders/Mappers/OutlookEventMapper.cs`
- [X] T040 [P] [US1] Create GoogleEventMapper for event normalization in `src/ModalCalendarNotification/ModalCalendarNotification.CalendarProviders/Mappers/GoogleEventMapper.cs`
- [X] T041 [US1] Implement CalendarSyncService with Polly resilience in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/CalendarIntegration/CalendarSyncService.cs`

### Notification Engine

- [X] T042 [P] [US1] Create INotificationEngine interface in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/INotificationEngine.cs`
- [X] T043 [US1] Implement NotificationEngine with lead time calculation and event filtering in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/NotificationEngine.cs`
- [X] T044 [P] [US1] Implement SnoozeScheduler for snooze timer management in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/SnoozeScheduler.cs`
- [X] T045 [P] [US1] Implement AutoDismissHandler for timeout management in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/AutoDismissHandler.cs`
- [X] T046 [P] [US1] Create NotificationEventItem model for multi-event support in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/NotificationEventItem.cs`

### Modal UI

- [X] T047 [P] [US1] Create NotificationModalViewModel with MVVM pattern in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/NotificationManagement/NotificationModalViewModel.cs`
- [X] T048 [US1] Create NotificationModal.xaml with multi-event scrollable list in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/NotificationManagement/NotificationModal.xaml`
- [X] T049 [US1] Implement NotificationModal.xaml.cs code-behind with Topmost=true in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/NotificationManagement/NotificationModal.xaml.cs`

### Unit Tests

- [X] T050 [P] [US1] Write unit tests for OAuthService with mocked MSAL in `tests/ModalCalendarNotification.Tests.Unit/Features/Authentication/OAuthServiceTests.cs`
- [X] T051 [P] [US1] Write unit tests for TokenRefreshOrchestrator in `tests/ModalCalendarNotification.Tests.Unit/Features/Authentication/TokenRefreshOrchestratorTests.cs`
- [X] T052 [P] [US1] Write unit tests for CredentialEncryption in `tests/ModalCalendarNotification.Tests.Unit/Features/Authentication/CredentialEncryptionTests.cs`
- [X] T053 [P] [US1] Write unit tests for OutlookCalendarProvider with mocked OAuth in `tests/ModalCalendarNotification.Tests.Unit/Features/CalendarIntegration/OutlookCalendarProviderTests.cs`
- [X] T054 [P] [US1] Write unit tests for GoogleCalendarProvider with mocked OAuth in `tests/ModalCalendarNotification.Tests.Unit/Features/CalendarIntegration/GoogleCalendarProviderTests.cs`
- [X] T055 [P] [US1] Write property mapping tests for OutlookEventMapper using CompareNetObjects in `tests/ModalCalendarNotification.Tests.Unit/Features/CalendarIntegration/OutlookEventMapperTests.cs`
- [X] T056 [P] [US1] Write property mapping tests for GoogleEventMapper using CompareNetObjects in `tests/ModalCalendarNotification.Tests.Unit/Features/CalendarIntegration/GoogleEventMapperTests.cs`
- [X] T057 [P] [US1] Write unit tests for CalendarSyncService with Polly policies in `tests/ModalCalendarNotification.Tests.Unit/Features/CalendarIntegration/CalendarSyncServiceTests.cs`
- [X] T058 [P] [US1] Write unit tests for NotificationEngine (lead time, filtering, grouping) in `tests/ModalCalendarNotification.Tests.Unit/Features/NotificationManagement/NotificationEngineTests.cs`
- [X] T059 [P] [US1] Write unit tests for SnoozeScheduler in `tests/ModalCalendarNotification.Tests.Unit/Features/NotificationManagement/SnoozeSchedulerTests.cs`
- [X] T060 [P] [US1] Write unit tests for AutoDismissHandler in `tests/ModalCalendarNotification.Tests.Unit/Features/NotificationManagement/AutoDismissHandlerTests.cs`

### E2E Tests

- [X] T061 [P] [US1] Create test harness infrastructure in `tests/ModalCalendarNotification.Tests.EndToEnd/TestHarness/TestApplicationHost.cs`
- [X] T062 [P] [US1] Create MockCalendarEventGenerator in `tests/ModalCalendarNotification.Tests.EndToEnd/TestHarness/MockCalendarEventGenerator.cs`
- [X] T063 [P] [US1] Create ModalWindowSimulator in `tests/ModalCalendarNotification.Tests.EndToEnd/TestHarness/ModalWindowSimulator.cs`
- [X] T064 [P] [US1] Create TimeController for time mocking in `tests/ModalCalendarNotification.Tests.EndToEnd/TestHarness/TimeController.cs`
- [X] T065 [P] [US1] Create OutlookCalendarMock in `tests/ModalCalendarNotification.Tests.EndToEnd/Fixtures/OutlookCalendarMock.cs`
- [X] T066 [P] [US1] Create GoogleCalendarMock in `tests/ModalCalendarNotification.Tests.EndToEnd/Fixtures/GoogleCalendarMock.cs`
- [X] T067 [US1] Write E2E test: ReceiveNotificationWorkflow (add credentials → notification) in `tests/ModalCalendarNotification.Tests.EndToEnd/Workflows/ReceiveNotificationWorkflowTests.cs`
- [X] T068 [US1] Write E2E test: SnoozeWorkflow (snooze → re-notification timing) in `tests/ModalCalendarNotification.Tests.EndToEnd/Workflows/SnoozeWorkflowTests.cs`
- [X] T069 [US1] Write E2E test: AutoDismissWorkflow (untouched modal timeout) in `tests/ModalCalendarNotification.Tests.EndToEnd/Workflows/AutoDismissWorkflowTests.cs`

---

## Phase 4: User Story 2 - Dismiss Recurring Event Notifications (P2)

**Priority**: P2  
**Goal**: Users can dismiss all future notifications for events with the same title  
**Independent Test Criteria**: Create recurring event series → Display notification → Click "Dismiss All Future" → Verify subsequent occurrences don't trigger notifications while other events do  
**Dependencies**: Requires US1 (notification engine)

**Tasks**:

### Dismissed Events Management

- [X] T070 [P] [US2] Create IDismissedEventTitleRepository interface in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/DismissedEventsManagement/IDismissedEventTitleRepository.cs`
- [X] T071 [US2] Implement DismissedEventTitleRepository with SQLite and case-insensitive matching in `src/ModalCalendarNotification/ModalCalendarNotification.Data/Features/DismissedEventsManagement/DismissedEventTitleRepository.cs`
- [X] T072 [P] [US2] Create DismissedEventTitle model in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/DismissedEventsManagement/DismissedEventTitle.cs`
- [X] T073 [US2] Update NotificationEngine to filter dismissed event titles in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/NotificationEngine.cs`
- [X] T074 [US2] Add "Dismiss All Future" button to NotificationModal.xaml in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/NotificationManagement/NotificationModal.xaml`
- [X] T075 [US2] Implement DismissAllFuture command in NotificationModalViewModel in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/NotificationManagement/NotificationModalViewModel.cs`

### Unit Tests

- [X] T076 [P] [US2] Write unit tests for DismissedEventTitleRepository (CRUD, case-insensitive) in `tests/ModalCalendarNotification.Tests.Unit/Features/DismissedEventsManagement/DismissedEventTitleRepositoryTests.cs`
- [X] T077 [P] [US2] Write unit tests for NotificationEngine dismissed title filtering in `tests/ModalCalendarNotification.Tests.Unit/Features/NotificationManagement/NotificationEngineTests.cs` (extend existing)

### E2E Tests

- [X] T078 [US2] Write E2E test: DismissAllFutureWorkflow (title filtering) in `tests/ModalCalendarNotification.Tests.EndToEnd/Workflows/DismissAllFutureWorkflowTests.cs`

---

## Phase 5: User Story 3 - Configure Calendar Integration and Notification Settings (P3)

**Priority**: P3  
**Goal**: Users can configure calendar providers, notification lead time, and auto-dismiss timeout  
**Independent Test Criteria**: Access system tray → Open settings → Select provider → Enter credentials → Set lead time → Verify notifications use new settings  
**Dependencies**: Requires US1 (notification engine)

**Tasks**:

### System Tray Integration

- [X] T079 [P] [US3] Create SystemTrayViewModel with MVVM pattern in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/SystemTrayManagement/SystemTrayViewModel.cs`
- [X] T080 [US3] Implement SystemTrayIcon with context menu in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/SystemTrayManagement/SystemTrayIcon.cs`

### Configuration Management

- [X] T081 [P] [US3] Create IConfigurationService interface in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/ConfigurationManagement/IConfigurationService.cs`
- [X] T082 [US3] Implement ConfigurationService with settings persistence in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/ConfigurationManagement/ConfigurationService.cs`
- [X] T083 [P] [US3] Create ProviderAccountItem model for multi-account display in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/ConfigurationManagement/ProviderAccountItem.cs`
- [X] T084 [P] [US3] Create ProviderSelectionViewModel with account label input in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ProviderSelectionViewModel.cs`
- [X] T085 [US3] Create ProviderSelectionDialog.xaml with provider list UI in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ProviderSelectionDialog.xaml`
- [X] T086 [US3] Implement ProviderSelectionDialog.xaml.cs code-behind in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ProviderSelectionDialog.xaml.cs`
- [X] T087 [P] [US3] Create ConfigurationDialogViewModel with auto-dismiss timeout in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ConfigurationDialogViewModel.cs`
- [X] T088 [US3] Create ConfigurationDialog.xaml with tabbed settings UI in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ConfigurationDialog.xaml`
- [X] T089 [US3] Implement ConfigurationDialog.xaml.cs code-behind in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ConfigurationDialog.xaml.cs`
- [X] T090 [US3] Add Dismissed Events tab to ConfigurationDialog with restore functionality in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ConfigurationDialog.xaml`

### Unit Tests

- [X] T091 [P] [US3] Write unit tests for SystemTrayViewModel in `tests/ModalCalendarNotification.Tests.Unit/Features/SystemTrayManagement/SystemTrayViewModelTests.cs`
- [X] T092 [P] [US3] Write unit tests for ConfigurationService in `tests/ModalCalendarNotification.Tests.Unit/Features/ConfigurationManagement/ConfigurationServiceTests.cs`

### E2E Tests

- [X] T093 [US3] Write E2E test: ConfigurationPersistenceWorkflow (settings survive restart) in `tests/ModalCalendarNotification.Tests.EndToEnd/Workflows/ConfigurationPersistenceWorkflowTests.cs`

---

## Phase 6: User Story 5 - Configure Multiple Calendar Providers and Select Calendars (P2)

**Priority**: P2  
**Goal**: Users can configure multiple calendar providers and select specific calendars per provider  
**Independent Test Criteria**: Configure Office365 → Select calendars → Configure Google Calendar → Select calendars → Verify notifications from both providers  
**Dependencies**: Requires US1 (notification engine), US3 (configuration UI)

**Tasks**:

### Calendar Selection Management

- [X] T094 [P] [US5] Create ICalendarSelectionRepository interface in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/CalendarSelection/ICalendarSelectionRepository.cs`
- [X] T095 [US5] Implement CalendarSelectionRepository with SQLite in `src/ModalCalendarNotification/ModalCalendarNotification.Data/Features/CalendarSelection/CalendarSelectionRepository.cs`
- [X] T096 [P] [US5] Create SelectedCalendar model in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/CalendarSelection/SelectedCalendar.cs`
- [X] T097 [P] [US5] Create CalendarListViewModel with checkbox selection in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/CalendarSelection/CalendarListViewModel.cs`
- [X] T098 [US5] Create CalendarListDialog.xaml with calendar checkboxes in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/CalendarSelection/CalendarListDialog.xaml`
- [X] T099 [US5] Implement CalendarListDialog.xaml.cs code-behind in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/CalendarSelection/CalendarListDialog.xaml.cs`
- [X] T100 [US5] Update NotificationEngine to respect calendar selections in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/NotificationEngine.cs`
- [X] T101 [US5] Update ConfigurationDialog to integrate calendar selection workflow in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ConfigurationDialog.xaml`

### Startup Recovery (Missed Events)

- [X] T102 [P] [US5] Create IApplicationStateRepository interface in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/StartupRecovery/IApplicationStateRepository.cs`
- [X] T103 [US5] Implement ApplicationStateRepository with timestamp persistence in `src/ModalCalendarNotification/ModalCalendarNotification.Data/Features/StartupRecovery/ApplicationStateRepository.cs`
- [X] T104 [P] [US5] Implement MissedEventDetector with 24-hour lookback in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/StartupRecovery/MissedEventDetector.cs`
- [X] T105 [US5] Implement MissedEventRecoveryService orchestrator in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/StartupRecovery/MissedEventRecoveryService.cs`
- [X] T106 [P] [US5] Create StartupMissedEventsViewModel in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/StartupRecovery/StartupMissedEventsViewModel.cs`
- [X] T107 [US5] Create StartupMissedEventsModal.xaml in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/StartupRecovery/StartupMissedEventsModal.xaml`
- [X] T108 [US5] Implement StartupMissedEventsModal.xaml.cs code-behind in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/StartupRecovery/StartupMissedEventsModal.xaml.cs`

### Unit Tests

- [X] T109 [P] [US5] Write unit tests for CalendarSelectionRepository in `tests/ModalCalendarNotification.Tests.Unit/Features/CalendarSelection/CalendarSelectionRepositoryTests.cs`
- [X] T110 [P] [US5] Write unit tests for ApplicationStateRepository in `tests/ModalCalendarNotification.Tests.Unit/Features/StartupRecovery/ApplicationStateRepositoryTests.cs`
- [X] T111 [P] [US5] Write unit tests for MissedEventDetector (24-hour window) in `tests/ModalCalendarNotification.Tests.Unit/Features/StartupRecovery/MissedEventDetectorTests.cs`
- [X] T112 [P] [US5] Write unit tests for MissedEventRecoveryService in `tests/ModalCalendarNotification.Tests.Unit/Features/StartupRecovery/MissedEventRecoveryServiceTests.cs`

### E2E Tests

- [X] T113 [US5] Write E2E test: MissedEventDetectionWorkflow (startup detection) in `tests/ModalCalendarNotification.Tests.EndToEnd/Workflows/MissedEventDetectionWorkflowTests.cs`

---

## Phase 7: Polish & Cross-Cutting Concerns

**Goal**: Finalize cross-cutting features, performance optimization, and production readiness

**Tasks**:

- [X] T114 [P] Configure Serilog structured logging in `src/ModalCalendarNotification/ModalCalendarNotification/Program.cs`
- [X] T115 [P] Implement multi-monitor display change handling in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/NotificationManagement/DisplayMonitor.cs`
- [X] T116 [P] Add error handling and graceful degradation to CalendarSyncService in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/CalendarIntegration/CalendarSyncService.cs`
- [X] T117 [P] Add performance logging for notification latency in `src/ModalCalendarNotification/ModalCalendarNotification.Core/Features/NotificationManagement/NotificationEngine.cs`
- [X] T118 [P] Implement database indexes for performance in `src/ModalCalendarNotification/ModalCalendarNotification.Data/Migrations/003_AddPerformanceIndexes.cs`
- [X] T119 Create FakeCalendarProvider for testing in `tests/ModalCalendarNotification.Tests.Unit/Fakes/FakeCalendarProvider.cs`
- [X] T120 Create FakeDismissedEventTitleRepository for testing in `tests/ModalCalendarNotification.Tests.Unit/Fakes/FakeDismissedEventTitleRepository.cs`
- [X] T121 Create FakeTimeProvider for testing in `tests/ModalCalendarNotification.Tests.Unit/Fakes/FakeTimeProvider.cs`
- [X] T122 Add application icon and system tray icon resources in `src/ModalCalendarNotification/ModalCalendarNotification/Resources/`
- [X] T123 Implement single-instance enforcement (prevent multiple app instances) in `src/ModalCalendarNotification/ModalCalendarNotification/Program.cs`
- [X] T124 Add version information display in ConfigurationDialog About tab in `src/ModalCalendarNotification/ModalCalendarNotification.UI/Features/ConfigurationManagement/ConfigurationDialog.xaml`
- [X] T125 Final E2E test run and validation across all workflows

---

## Dependencies & Execution Order

### Story Completion Order

```
Phase 1 (Setup)
  ↓
Phase 2 (Foundational)
  ↓
Phase 3 (US1 - P1) ← MUST complete first (core value)
  ↓
  ├─→ Phase 4 (US2 - P2) ← Can start after US1
  └─→ Phase 5 (US3 - P3) ← Can start after US1
        ↓
        Phase 6 (US5 - P2) ← Requires US1 + US3
          ↓
          Phase 7 (Polish)
```

### Critical Path

1. **Setup (T001-T013)**: Must complete before any implementation
2. **Foundational (T014-T031)**: Must complete before user stories
3. **US1 Core (T032-T049)**: Blocking for all other user stories
4. **US2 (T070-T078)**: Independent after US1
5. **US3 (T079-T093)**: Independent after US1
6. **US5 (T094-T113)**: Requires US1 + US3 complete
7. **Polish (T114-T125)**: Final cleanup after all stories

### Parallel Execution Opportunities

**Phase 2 - Foundational** (can run in parallel):
- T014-T020 (Shared models and utilities)
- T028-T031 (Unit tests for utilities)

**Phase 3 - US1** (can run in parallel after dependencies):
- T032-T035 (Authentication components)
- T036-T040 (Calendar provider implementations)
- T042-T046 (Notification engine components)
- T047-T049 (Modal UI)
- T050-T060 (Unit tests)
- T061-T066 (E2E test infrastructure)

**Phase 4 + Phase 5** (can run in parallel after US1):
- US2 tasks (T070-T078)
- US3 tasks (T079-T093)

**Phase 7 - Polish** (can run in parallel):
- T114-T118 (Cross-cutting features)
- T119-T121 (Test fakes)

---

## Testing Strategy

### Unit Test Coverage Targets

- **Notification Engine**: 90%+ (pure business logic)
- **Calendar Providers**: 85%+ (OAuth mocking)
- **Repositories**: 90%+ (database abstraction)
- **Mappers**: 100% (property coverage with CompareNetObjects)
- **State Management**: 95%+ (critical logic)

### E2E Test Workflows

All E2E tests use:
- **Mocked calendar APIs** (Outlook365, Google Calendar)
- **Automated UI interactions** (no manual testing)
- **Time mocking** (via TimeController)
- **In-memory SQLite** (deterministic database)

**Key Workflows**:
1. ReceiveNotificationWorkflow (T067)
2. SnoozeWorkflow (T068)
3. AutoDismissWorkflow (T069)
4. DismissAllFutureWorkflow (T078)
5. ConfigurationPersistenceWorkflow (T093)
6. MissedEventDetectionWorkflow (T113)

---

## MVP Scope

**Recommended MVP** (delivers core value):
- **Phase 1**: Setup (T001-T013)
- **Phase 2**: Foundational (T014-T031)
- **Phase 3**: User Story 1 (T032-T069) - Complete notification functionality

**Total MVP Tasks**: 69  
**Estimated Effort**: ~2-3 weeks for single developer

This MVP delivers:
✅ Modal notifications at configured lead time  
✅ Snooze functionality  
✅ Auto-dismiss timeout  
✅ Multi-event support  
✅ Both Outlook365 and Google Calendar integration  
✅ Comprehensive unit and E2E tests  

---

## Format Validation

✅ All tasks follow checklist format: `- [ ] [TaskID] [P?] [Story?] Description with file path`  
✅ Sequential task IDs (T001-T125)  
✅ `[P]` marker for parallelizable tasks  
✅ `[US1]`, `[US2]`, `[US3]`, `[US5]` labels for user story phases  
✅ Exact file paths specified for each task  
✅ Dependencies clearly documented  
✅ Independent test criteria per user story  

---

## Summary

- **Total Tasks**: 125
- **User Stories**: 4 (US1: 38 tasks, US2: 9 tasks, US3: 15 tasks, US5: 20 tasks)
- **Parallel Opportunities**: 52 tasks marked with `[P]`
- **MVP Tasks**: 69 (Setup + Foundational + US1)
- **Independent Test Criteria**: Defined for each user story
- **Dependency Graph**: Linear story completion with parallel task execution within stories

**Next Steps**:
1. Begin Phase 1 (Setup) tasks T001-T013
2. Complete Phase 2 (Foundational) tasks T014-T031
3. Implement MVP (US1) tasks T032-T069
4. Incrementally deliver US2, US3, US5
5. Finalize with Phase 7 (Polish)

Each task is immediately executable with specific file paths and clear acceptance criteria.

