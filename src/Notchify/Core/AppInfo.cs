namespace Notchify.Core;

public static class AppInfo
{
    public const string Name = "Notch";
    public const string Version = "0.4.0";
    public const string Developer = "Adarsh Kumar";

    /// <summary>The app icon, for window title bars.</summary>
    public static System.Windows.Media.ImageSource? Icon
    {
        get
        {
            try { return System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Assets/Notch.ico")); }
            catch { return null; }
        }
    }
}
