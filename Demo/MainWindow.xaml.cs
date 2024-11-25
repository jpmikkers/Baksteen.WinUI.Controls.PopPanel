// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace HeightAnimationTest;
using Microsoft.UI.Xaml;
using Windows.Graphics;
using Microsoft.UI.Windowing;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();
        SetWindowSize(700, 500);
    }

    private void SetWindowSize(int width, int height)
    {
        // Get the AppWindow of the current window
        var appWindow = GetAppWindowForCurrentWindow();
        if(appWindow != null)
        {
            // Set the window size
            appWindow.Resize(new SizeInt32(width, height));
        }
    }

    private AppWindow GetAppWindowForCurrentWindow()
    {
        // Get the HWND for the current window
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        return appWindow;
    }
}
