namespace ModalCalendarNotification.Core.Shared.Utilities;

public static class DisplayHelper
{
    public static DisplayInfo GetPrimaryDisplay()
    {
        return new DisplayInfo(1920, 1080, true);
    }

    public static DisplayPoint GetPrimaryDisplayCenter()
    {
        var display = GetPrimaryDisplay();
        return new DisplayPoint(display.Width / 2, display.Height / 2);
    }
}

public readonly record struct DisplayInfo(int Width, int Height, bool IsPrimary);

public readonly record struct DisplayPoint(int X, int Y);
