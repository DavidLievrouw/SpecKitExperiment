using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using H.NotifyIcon;
using Serilog;

namespace ModalCalendarNotification.UI.Features.SystemTrayManagement;

public sealed class SystemTrayManager : IDisposable
{
    private readonly SystemTrayIcon _systemTrayIcon;
    private readonly ILogger _logger;
    private TaskbarIcon? _taskbarIcon;
    private string? _tempIconPath;

    public SystemTrayManager(SystemTrayIcon systemTrayIcon, ILogger logger)
    {
        _systemTrayIcon = systemTrayIcon;
        _logger = logger;
    }

    public void Initialize()
    {
        _logger.Information("Initializing system tray icon");

        try
        {
            var iconPath = GetApplicationIconPath();
            var iconSource = PathToBitmapImage(iconPath);

            _taskbarIcon = new TaskbarIcon
            {
                IconSource = iconSource,
                ToolTipText = "Modal Calendar Notification",
            };

            // Create context menu
            var contextMenu = new System.Windows.Controls.ContextMenu();

            // Add Settings menu item
            var settingsItem = new System.Windows.Controls.MenuItem { Header = "Settings" };
            settingsItem.Click += (sender, args) => HandleSettingsClick();
            contextMenu.Items.Add(settingsItem);

            // Add separator
            contextMenu.Items.Add(new System.Windows.Controls.Separator());

            // Add Exit menu item
            var exitItem = new System.Windows.Controls.MenuItem { Header = "Exit" };
            exitItem.Click += (sender, args) => HandleExitClick();
            contextMenu.Items.Add(exitItem);

            _taskbarIcon.ContextMenu = contextMenu;
            _taskbarIcon.TrayLeftMouseDown += (sender, args) => HandleSettingsClick();

            _logger.Information("System tray icon initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize system tray icon");
            throw;
        }
    }

    private BitmapImage PathToBitmapImage(string filePath)
    {
        try
        {
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.UriSource = new Uri(filePath, UriKind.Absolute);
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

            _logger.Information("Loaded icon from {FilePath}", filePath);
            return bitmapImage;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load icon from {FilePath}", filePath);
            throw;
        }
    }

    private string GetApplicationIconPath()
    {
        try
        {
            // Load the SystemTrayIconDrawing from the XAML resource
            var resourceDict = new ResourceDictionary
            {
                Source = new Uri(
                    "pack://application:,,,/ModalCalendarNotification;component/Resources/SystemTrayIcon.xaml",
                    UriKind.Absolute
                ),
            };

            if (resourceDict.Contains("SystemTrayIconDrawing"))
            {
                var drawing = resourceDict["SystemTrayIconDrawing"] as DrawingImage;
                if (drawing != null)
                {
                    _logger.Information("Loaded custom calendar icon from resources");
                    return DrawingImageToIcoFile(drawing, 32, 32);
                }
            }

            _logger.Warning("SystemTrayIconDrawing not found in resources, using fallback");
            return CreateDefaultIconFile();
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "Could not load custom icon, using fallback");
            return CreateDefaultIconFile();
        }
    }

    private string DrawingImageToIcoFile(DrawingImage drawingImage, int width, int height)
    {
        try
        {
            // Create a DrawingVisual to render the drawing
            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawDrawing(drawingImage.Drawing);
            }

            // Create a RenderTargetBitmap to render the visual
            var renderTargetBitmap = new RenderTargetBitmap(
                width,
                height,
                96,
                96,
                PixelFormats.Pbgra32
            );
            renderTargetBitmap.Render(drawingVisual);
            renderTargetBitmap.Freeze();

            return BitmapToIcoFile(renderTargetBitmap);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to convert DrawingImage to ICO file");
            return CreateDefaultIconFile();
        }
    }

    private string BitmapToIcoFile(BitmapSource bitmap)
    {
        try
        {
            // Get temp directory
            var tempDir = Path.Combine(Path.GetTempPath(), "ModalCalendarNotification");
            Directory.CreateDirectory(tempDir);

            // Create unique ICO file path
            _tempIconPath = Path.Combine(tempDir, $"icon_{Guid.NewGuid()}.ico");

            // Convert bitmap to ICO format using pure managed code
            var icoData = BitmapToIcoBytes(bitmap);
            File.WriteAllBytes(_tempIconPath, icoData);

            _logger.Information("Icon saved to {IconPath}", _tempIconPath);
            return _tempIconPath;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to save bitmap to ICO file");
            throw;
        }
    }

    private byte[] BitmapToIcoBytes(BitmapSource bitmap)
    {
        // Create a PNG-encoded version first
        var pngEncoder = new PngBitmapEncoder();
        pngEncoder.Frames.Add(BitmapFrame.Create(bitmap));

        byte[] pngBytes;
        using (var memoryStream = new MemoryStream())
        {
            pngEncoder.Save(memoryStream);
            pngBytes = memoryStream.ToArray();
        }

        // Create a simple ICO file with PNG data
        // ICO format: Header (6 bytes) + IconDir entries + image data
        // Image data: BMP bytes (without BMP header, starting from pixel data)

        using (var icoStream = new MemoryStream())
        {
            // ICO Header (6 bytes)
            icoStream.WriteByte(0); // Reserved
            icoStream.WriteByte(0); // Reserved
            icoStream.WriteByte(1); // Type (1 = ICO)
            icoStream.WriteByte(0);
            icoStream.WriteByte(1); // Number of images
            icoStream.WriteByte(0);

            // Icon Directory Entry (16 bytes)
            icoStream.WriteByte(32); // Width
            icoStream.WriteByte(32); // Height
            icoStream.WriteByte(0); // Color palette (0 = no palette)
            icoStream.WriteByte(0); // Reserved
            icoStream.WriteByte(1); // Color planes
            icoStream.WriteByte(0);
            icoStream.WriteByte(32); // Bits per pixel
            icoStream.WriteByte(0);

            // Image data size (as 32-bit little-endian)
            byte[] sizeBytes = BitConverter.GetBytes(pngBytes.Length);
            icoStream.Write(sizeBytes, 0, 4);

            // Image data offset (6 + 16 = 22 bytes for header + 1 entry)
            byte[] offsetBytes = BitConverter.GetBytes(22);
            icoStream.Write(offsetBytes, 0, 4);

            // Write PNG data
            icoStream.Write(pngBytes, 0, pngBytes.Length);

            return icoStream.ToArray();
        }
    }

    private string CreateDefaultIconFile()
    {
        try
        {
            // Create a simple default icon - white square with blue header
            var drawingGroup = new DrawingGroup();

            // White background
            var backgroundBrush = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
            var borderPen = new Pen(new SolidColorBrush(Color.FromArgb(255, 31, 41, 55)), 0.5);

            var backgroundGeometry = Geometry.Parse("M2,2 L30,2 L30,30 L2,30 Z");
            drawingGroup.Children.Add(
                new GeometryDrawing
                {
                    Brush = backgroundBrush,
                    Pen = borderPen,
                    Geometry = backgroundGeometry,
                }
            );

            // Blue header
            var headerBrush = new SolidColorBrush(Color.FromArgb(255, 59, 130, 246));
            var headerGeometry = Geometry.Parse("M2,2 L30,2 L30,8 L2,8 Z");
            drawingGroup.Children.Add(
                new GeometryDrawing { Brush = headerBrush, Geometry = headerGeometry }
            );

            // Create DrawingImage and convert to ICO file
            var drawingImage = new DrawingImage(drawingGroup);
            return DrawingImageToIcoFile(drawingImage, 32, 32);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create default icon file");
            return CreateMinimalFallbackIconFile();
        }
    }

    private string CreateMinimalFallbackIconFile()
    {
        try
        {
            // Create a minimal 32x32 bitmap as last resort
            var writeableBitmap = new WriteableBitmap(32, 32, 96, 96, PixelFormats.Pbgra32, null);

            // Fill with blue color
            var pixels = new uint[32 * 32];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = 0xFF3B82F6; // Blue
            }

            writeableBitmap.WritePixels(new Int32Rect(0, 0, 32, 32), pixels, 32 * 4, 0);
            writeableBitmap.Freeze();

            return BitmapToIcoFile(writeableBitmap);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create minimal fallback icon file");
            throw;
        }
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

    public void Dispose()
    {
        _logger.Information("Disposing system tray icon");

        if (_taskbarIcon != null)
        {
            _taskbarIcon.Dispose();
            _taskbarIcon = null;
        }

        // Clean up temp icon file
        if (!string.IsNullOrEmpty(_tempIconPath) && File.Exists(_tempIconPath))
        {
            try
            {
                File.Delete(_tempIconPath);
                _logger.Information("Deleted temporary icon file: {IconPath}", _tempIconPath);
            }
            catch (Exception ex)
            {
                _logger.Warning(
                    ex,
                    "Failed to delete temporary icon file: {IconPath}",
                    _tempIconPath
                );
            }
        }
    }
}
