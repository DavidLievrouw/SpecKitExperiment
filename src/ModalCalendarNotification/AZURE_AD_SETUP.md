# Azure AD / Office 365 Configuration Guide

## Important: Configure Azure AD App Registration

Before the Office 365 calendar provider can work, you need to configure an Azure AD app registration.

### Steps to Register an Azure AD Application

1. **Navigate to Azure Portal**
   - Go to https://portal.azure.com
   - Sign in with your Microsoft account

2. **Register a New Application**
   - Navigate to "Azure Active Directory" → "App registrations"
   - Click "New registration"
   - Fill in the details:
     - **Name**: Modal Calendar Notification
     - **Supported account types**: "Accounts in any organizational directory and personal Microsoft accounts"
     - **Redirect URI**: Select "Public client/native (mobile & desktop)" and enter: `http://localhost`
   - Click "Register"

3. **Copy the Application (client) ID**
   - After registration, you'll see the "Application (client) ID" on the overview page
   - Copy this value

4. **Configure API Permissions**
   - Click on "API permissions" in the left menu
   - Click "Add a permission"
   - Select "Microsoft Graph"
   - Select "Delegated permissions"
   - Search for and add: `Calendars.Read`
   - Click "Add permissions"
   - (Optional) Click "Grant admin consent" if you have admin rights

5. **Update the Configuration**
   - Open `ModalCalendarNotification.Core/Features/Authentication/MsalTokenClient.cs`
   - Replace `YOUR_OUTLOOK_CLIENT_ID` with the Application (client) ID you copied
   - Or better yet, add it to `appsettings.json`:

```json
{
  "Authentication": {
    "Outlook": {
      "ClientId": "YOUR_APPLICATION_CLIENT_ID_HERE",
      "TenantId": "common"
    }
  }
}
```

Then update `MsalTokenClient.cs` to read from configuration instead of hardcoded values.

### Troubleshooting

If you encounter authentication errors:

1. **"AADSTS700016: Application not found"**
   - The Client ID is incorrect or the app hasn't been registered yet

2. **"AADSTS65001: The user or administrator has not consented"**
   - You need to grant consent for the Calendars.Read permission
   - Either grant admin consent in the Azure Portal, or the user will be prompted on first login

3. **"The application does not have a valid client secret"**
   - For public client applications (desktop apps), you don't need a client secret
   - Make sure you selected "Public client/native" as the platform

### Future Enhancement: Google Calendar

Google Calendar authentication is not yet implemented. To add it:

1. Create a Google Cloud project
2. Enable the Google Calendar API
3. Create OAuth 2.0 credentials (Desktop app)
4. Implement Google OAuth flow in a separate authentication provider
   - Note: MSAL only works with Microsoft services
   - You'll need to use Google's authentication library instead

## Current Implementation Status

### ✅ Implemented
- Authentication service interface (`IAuthenticationService`)
- MSAL token client for Office 365 authentication
- Provider selection dialog with authentication flow
- Proper window lifecycle management (creating new dialog instances)
- Microsoft.Extensions.Logging integration with Serilog
- Office 365 calendar provider skeleton
- Google Calendar provider skeleton

### ⚠️ Requires Configuration
- **Azure AD Application Client ID** - Must be configured before Office 365 authentication will work
- Calendar selection UI - Currently just sets the provider, doesn't show individual calendars
- Token caching - MSAL handles this automatically, but consider using MSAL Extensions for secure storage

### ❌ Not Yet Implemented
- Google Calendar authentication (requires different OAuth library)
- Calendar list fetching (showing which calendars are available)
- Calendar selection persistence (saving which calendars to monitor)
- Actual calendar event monitoring
- Notification display when events are approaching

