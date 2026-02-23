using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using H.NotifyIcon;
using Serilog;

namespace ModalCalendarNotification.UI.Features.SystemTrayManagement;

public sealed class SystemTrayManager : IDisposable
{
    private readonly ILogger _logger;
    private readonly SystemTrayIcon _systemTrayIcon;
    private TaskbarIcon? _taskbarIcon;
    private Window? _hiddenWindow;

    public SystemTrayManager(SystemTrayIcon systemTrayIcon, ILogger logger)
    {
        _systemTrayIcon = systemTrayIcon;
        _logger = logger;
    }

    public void Dispose()
    {
        _logger.Information("Disposing system tray icon");

        if (_taskbarIcon != null)
        {
            _taskbarIcon.Dispose();
            _taskbarIcon = null;
        }

        if (_hiddenWindow != null)
        {
            _hiddenWindow.Close();
            _hiddenWindow = null;
        }
    }

    public void Initialize()
    {
        _logger.Information("Initializing system tray icon");

        try
        {
            ImageSource iconSource = GetApplicationIcon();

            _taskbarIcon = new TaskbarIcon
            {
                IconSource = iconSource,
                ToolTipText = "Modal Calendar Notification",
            };

            // Create context menu
            var contextMenu = new ContextMenu();

            // Add Settings menu item
            var settingsItem = new MenuItem { Header = "Settings" };
            settingsItem.Click += (sender, args) => HandleSettingsClick();
            contextMenu.Items.Add(settingsItem);

            // Add separator
            contextMenu.Items.Add(new Separator());

            // Add Exit menu item
            var exitItem = new MenuItem { Header = "Exit" };
            exitItem.Click += (sender, args) => HandleExitClick();
            contextMenu.Items.Add(exitItem);

            _taskbarIcon.ContextMenu = contextMenu;
            _taskbarIcon.TrayLeftMouseDown += (sender, args) => HandleSettingsClick();

            // Add TaskbarIcon to a window - create a hidden window if needed
            var window = Application.Current?.MainWindow;
            if (window != null)
            {
                AddTaskbarIconToWindow(window);
            }
            else
            {
                // Create a hidden window to host the TaskbarIcon
                _hiddenWindow = new Window
                {
                    Width = 0,
                    Height = 0,
                    WindowStyle = WindowStyle.None,
                    AllowsTransparency = true,
                    ShowInTaskbar = false,
                    Visibility = Visibility.Hidden,
                };

                var grid = new Grid();
                grid.Children.Add(_taskbarIcon);
                _hiddenWindow.Content = grid;
                _hiddenWindow.Show();

                _logger.Information("Created hidden window for TaskbarIcon");
            }

            _logger.Information("System tray icon initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize system tray icon");
            throw;
        }
    }

    private void AddTaskbarIconToWindow(Window window)
    {
        if (window.Content is Grid grid)
        {
            grid.Children.Add(_taskbarIcon);
        }
        else
        {
            // If window content is not a Grid, wrap it
            var wrappedContent = window.Content;
            var newGrid = new Grid();
            window.Content = newGrid;
            if (wrappedContent is UIElement element)
            {
                newGrid.Children.Add(element);
            }
            newGrid.Children.Add(_taskbarIcon);
        }
    }

    private ImageSource GetApplicationIcon()
    {
        // Create a simple 16x16 white bitmap and save as .ico file
        var tempPath = Path.Combine(Path.GetTempPath(), "tray_icon.ico");

        var bitmap = new WriteableBitmap(16, 16, 96, 96, PixelFormats.Bgra32, null);
        var pixels = new byte[16 * 16 * 4];
        for (int i = 0; i < pixels.Length; i += 4)
        {
            pixels[i] = 255; // B
            pixels[i + 1] = 255; // G
            pixels[i + 2] = 255; // R
            pixels[i + 3] = 255; // A
        }
        bitmap.WritePixels(new Int32Rect(0, 0, 16, 16), pixels, 16 * 4, 0);

        // Convert to ICO format
        using (var fileStream = new FileStream(tempPath, FileMode.Create))
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var pngStream = new MemoryStream())
            {
                encoder.Save(pngStream);
                var pngData = pngStream.ToArray();

                // Write ICO file format
                // ICO header
                fileStream.Write(new byte[] { 0, 0 }, 0, 2); // Reserved
                fileStream.Write(new byte[] { 1, 0 }, 0, 2); // Type: 1 = ICO
                fileStream.Write(new byte[] { 1, 0 }, 0, 2); // Number of images

                // Image directory
                fileStream.WriteByte(16); // Width
                fileStream.WriteByte(16); // Height
                fileStream.WriteByte(0); // Color palette
                fileStream.WriteByte(0); // Reserved
                fileStream.Write(new byte[] { 1, 0 }, 0, 2); // Color planes
                fileStream.Write(new byte[] { 32, 0 }, 0, 2); // Bits per pixel
                fileStream.Write(BitConverter.GetBytes(pngData.Length), 0, 4); // Image size
                fileStream.Write(BitConverter.GetBytes(22), 0, 4); // Image offset

                // Image data
                fileStream.Write(pngData, 0, pngData.Length);
            }
        }

        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.UriSource = new Uri(tempPath, UriKind.Absolute);
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.EndInit();
        bitmapImage.Freeze();

        return bitmapImage;
    }

    private void HandleSettingsClick()
    {
        _logger.Information("Settings clicked from system tray");
        _systemTrayIcon.TriggerSettings();
    }

    private void HandleExitClick()
    {
        _logger.Information("Exit clicked from system tray");
        _systemTrayIcon.TriggerExit();
    }
}
