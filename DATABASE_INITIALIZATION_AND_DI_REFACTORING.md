# Database Initialization and IServiceProvider Anti-Pattern Refactoring

**Date**: February 24, 2026  
**Build Status**: ✅ SUCCESS - All projects compile successfully

---

## Changes Implemented

### 1. ✅ Database Initialization Service

**File Created**: `DatabaseInitializationService.cs` in `ModalCalendarNotification.Data`

**Purpose**: Automatically initializes or migrates the database on application startup

**Features**:
- Checks for pending Entity Framework migrations
- Applies pending migrations if they exist
- Creates database if it doesn't exist
- Logs all database operations via Serilog
- Throws exceptions on database initialization failure

**Behavior**:
1. **On First Run**: Creates database with all schema
2. **On Update**: Detects and applies pending migrations
3. **On Healthy Run**: Confirms database is up to date

**Implementation Details**:
```csharp
public sealed class DatabaseInitializationService
{
    public async Task InitializeDatabaseAsync()
    {
        // 1. Check for pending migrations
        var pendingMigrations = await _dbContext.Database.GetPendingMigrationsAsync();
        
        // 2. If migrations pending: apply them
        if (pendingMigrations.Any())
            await _dbContext.Database.MigrateAsync();
        
        // 3. If no database exists: create it
        else if (!await _dbContext.Database.CanConnectAsync())
            await _dbContext.Database.EnsureCreatedAsync();
        
        // 4. Otherwise: database is healthy
    }
}
```

---

### 2. ✅ Removed IServiceProvider Anti-Pattern

**Problem**: `ApplicationLifecycleManager` was injecting `IServiceProvider` and using it as a service locator, which violates dependency injection principles.

**Solution**: Refactored to inject specific required services instead.

**Before** (Anti-pattern):
```csharp
public ApplicationLifecycleManager(
    SystemTrayManager systemTrayManager,
    IServiceProvider serviceProvider,  // ❌ Anti-pattern: service locator
    NotificationSchedulerService notificationScheduler
)
{
    var config = serviceProvider.GetService(typeof(IConfigurationService)) as IConfigurationService;
    var outlook = serviceProvider.GetService(typeof(OutlookCalendarProvider)) as OutlookCalendarProvider;
    // ... more service locator calls
}
```

**After** (Best practice):
```csharp
public ApplicationLifecycleManager(
    SystemTrayManager systemTrayManager,
    NotificationSchedulerService notificationScheduler,
    MissedEventRecoveryService missedEventRecoveryService,
    IConfigurationService configurationService,
    TimeProvider timeProvider,
    OutlookCalendarProvider outlookProvider,
    GoogleCalendarProvider googleProvider,
    Func<StartupMissedEventsViewModel> createStartupMissedEventsViewModel
)
{
    // ✅ All dependencies explicitly declared
    _systemTrayManager = systemTrayManager;
    _notificationScheduler = notificationScheduler;
    _missedEventRecoveryService = missedEventRecoveryService;
    // ... etc
}
```

**Benefits**:
- ✅ Explicit dependencies - easier to understand what the class needs
- ✅ Testable - can mock/stub dependencies easily
- ✅ Follows explicit dependencies principle
- ✅ IDE can detect circular dependencies
- ✅ Dependency graph is clear

---

### 3. ✅ Factory Function Pattern for UI Dialogs

**Problem**: `App.xaml.cs` was using `IServiceProvider` to get ConfigurationDialog, which is a composition root concern.

**Solution**: Used `Func<ConfigurationDialog>` factory pattern instead.

**Before**:
```csharp
var configDialog = _serviceProvider.GetRequiredService<ConfigurationDialog>();
```

**After**:
```csharp
private Func<ConfigurationDialog>? _createConfigurationDialog;

protected override void OnStartup(StartupEventArgs e)
{
    _createConfigurationDialog = _serviceProvider.GetRequiredService<Func<ConfigurationDialog>>();
}

private void OpenSettingsDialog()
{
    var configDialog = _createConfigurationDialog();  // Factory pattern
}
```

**Registration**:
```csharp
services.AddSingleton(sp => new Func<ConfigurationDialog>(() => 
    sp.GetRequiredService<ConfigurationDialog>()
));
```

**Benefits**:
- ✅ Cleaner composition
- ✅ Type-safe factory
- ✅ Still allows service locator in composition root only
- ✅ Easy to test

---

### 4. ✅ Database Initialization on App Startup

**Where**: `App.xaml.cs` - `OnStartup` method

**Execution Order**:
1. Create service provider (DI setup)
2. Initialize database ← **NEW**
3. Get ApplicationLifecycleManager
4. Get SystemTrayViewModel
5. Subscribe to events
6. Start notification scheduler

**Code**:
```csharp
protected override async void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);
    
    try
    {
        _serviceProvider = Program.CreateServiceProvider();
        
        // NEW: Initialize database before anything else
        var dbInitializer = _serviceProvider.GetRequiredService<DatabaseInitializationService>();
        await dbInitializer.InitializeDatabaseAsync();
        
        // Continue with normal startup
        _lifecycleManager = _serviceProvider.GetRequiredService<ApplicationLifecycleManager>();
        // ...
    }
}
```

---

## Service Registration Updates

**New Registration in ServiceConfiguration**:

```csharp
// Database initialization (added first)
services.AddScoped<DatabaseInitializationService>();

// Factory functions for UI dialogs
services.AddSingleton(sp => new Func<ConfigurationDialog>(() => 
    sp.GetRequiredService<ConfigurationDialog>()
));

services.AddTransient(sp => new Func<StartupMissedEventsViewModel>(() =>
    sp.GetRequiredService<StartupMissedEventsViewModel>()
));

// ApplicationLifecycleManager (updated with all dependencies)
services.AddSingleton(sp =>
    new ApplicationLifecycleManager(
        sp.GetRequiredService<SystemTrayManager>(),
        sp.GetRequiredService<NotificationSchedulerService>(),
        sp.GetRequiredService<MissedEventRecoveryService>(),
        sp.GetRequiredService<IConfigurationService>(),
        sp.GetRequiredService<AppTimeProvider>(),
        sp.GetRequiredService<OutlookCalendarProvider>(),
        sp.GetRequiredService<GoogleCalendarProvider>(),
        sp.GetRequiredService<Func<StartupMissedEventsViewModel>>()
    )
);
```

---

## Dependency Injection Pattern Improvements

### Anti-Patterns Eliminated

1. ❌ **Service Locator Pattern** - `IServiceProvider` injected into services
   - Removed from `ApplicationLifecycleManager`
   - Kept in `App.xaml.cs` composition root (acceptable)

2. ❌ **Implicit Dependencies** - Services getting dependencies via reflection
   - All dependencies now explicit in constructors

3. ❌ **Hidden Dependencies** - Hard to see what a class needs
   - Constructor signature shows all needs

### Best Practices Applied

✅ **Constructor Injection** - All services injected via constructor  
✅ **Explicit Dependencies** - No hidden service locator calls  
✅ **Composition Root** - Service provider used only in `App.xaml.cs` and `Program.cs`  
✅ **Factory Functions** - Used for transient/dialog creation  
✅ **Testability** - Can easily mock/stub any dependency  

---

## Startup Sequence

**New Initialization Order**:

```
Application Start
    ↓
OnStartup() called
    ↓
Build Service Provider (DI setup)
    ↓
Initialize Database ← NEW
    ├─ Check for pending migrations
    ├─ Apply migrations if needed
    ├─ Create database if needed
    └─ Log all operations
    ↓
Get ApplicationLifecycleManager
    ├─ All dependencies injected explicitly
    └─ No service locator calls
    ↓
Initialize SystemTray
    ↓
Start NotificationScheduler
    ├─ Begin monitoring calendar providers
    ├─ Sync events every 5 minutes
    └─ Check for notifications every 30 seconds
    ↓
Check for Missed Events
    ├─ Fetch from all configured providers
    ├─ Detect events from last 24 hours
    └─ Show modal if any missed
    ↓
Application Ready
```

---

## Build Verification

```
✅ Build Status: SUCCESS
✅ Errors: 0
✅ Warnings: ~9 (code style only - non-critical)
✅ Build Time: ~1.6 seconds

All 7 Projects:
  ✅ ModalCalendarNotification.Core
  ✅ ModalCalendarNotification.Data (with new DatabaseInitializationService)
  ✅ ModalCalendarNotification.UI
  ✅ ModalCalendarNotification.CalendarProviders
  ✅ ModalCalendarNotification (Main - refactored ApplicationLifecycleManager)
  ✅ ModalCalendarNotification.Tests.Unit
  ✅ ModalCalendarNotification.Tests.EndToEnd
```

---

## Database Initialization Logging

Example log output on application startup:

```
[INF] Initializing application database
[INF] Found 3 pending migrations
[INF]   - 20260224_AddCachedEvents
[INF]   - 20260224_AddSyncStatus
[INF]   - 20260224_AddSnoozeControl
[INF] Applied pending migrations successfully
[INF] Database initialization completed successfully
```

Or on subsequent runs:

```
[INF] Initializing application database
[INF] Database is up to date
[INF] Database initialization completed successfully
```

---

## Code Quality Improvements

### Explicit Dependencies
- All services explicitly declared in constructors
- IDE can identify missing dependencies at compile time
- Circular dependencies detected immediately

### Testability
- Can mock/stub any dependency
- No need to set up complex service provider mocks
- Clearer test setup

### Maintainability
- Constructor shows exactly what a service needs
- Changes to dependencies visible in signature
- Easier to understand data flow

### Type Safety
- `Func<T>` factories are type-safe
- Compiler prevents wrong types from being injected
- No casting required

---

## Migration Path for Future Updates

When database schema changes:

1. **Create new EF Core migration**:
   ```bash
   dotnet ef migrations add YourMigrationName
   ```

2. **On next app startup**:
   - `DatabaseInitializationService` detects pending migrations
   - Automatically applies them
   - No manual DBA work needed

3. **Logging** shows what was updated:
   ```
   [INF] Found 1 pending migrations
   [INF]   - 20260224_YourMigrationName
   [INF] Applied pending migrations successfully
   ```

---

## Summary

✅ **Database Initialization**: Automatic on application startup  
✅ **IServiceProvider Anti-Pattern**: Removed from business logic  
✅ **Explicit Dependencies**: All constructor-injected  
✅ **Factory Pattern**: Used for UI dialog creation  
✅ **Build Successful**: Zero compilation errors  
✅ **Best Practices**: SOLID principles followed  

The application now follows best practices for dependency injection, has automatic database initialization, and no longer uses the service locator anti-pattern in business logic.


