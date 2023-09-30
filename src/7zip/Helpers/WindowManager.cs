using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using _7zip.Core.Enums;
using _7zip.Windows;

namespace _7zip.Helpers
{
    public class WindowManager
    {
        static public Window CreateWindow(WindowType windowType)
        {
            if (windowType == WindowType.Main)
            {
                Window newWindow = new MainWindow();
                TrackWindow(newWindow);
                return newWindow;
            }
            else if(windowType == WindowType.Extraction)
            {
                Window newWindow = new OperationWindow();
                TrackWindow(newWindow);
                return newWindow;
            }
            else if(windowType == WindowType.Compression)
            {
                Window newWindow = new NewCompressionWindow();
                TrackWindow(newWindow);
                return newWindow;
            }
            else if(windowType == WindowType.Blank)
            {
                Window newWindow = new Window();
                TrackWindow(newWindow);
                return newWindow;
            }
            else
            {
                Window newWindow = new Window();
                TrackWindow(newWindow);
                return newWindow;
            }
        }

        static public void TrackWindow(Window window)
        {
            window.Closed += (sender, args) =>
            {
                _activeWindows.Remove(window);
            };
            _activeWindows.Add(window);
        }

        static public Window? GetWindowForElement(UIElement element)
        {
            if (element.XamlRoot != null)
            {
                foreach (Window window in _activeWindows)
                {
                    if (element.XamlRoot == window.Content.XamlRoot)
                    {
                        return window;
                    }
                }
            }
            return null;
        }

        static public List<Window> ActiveWindows { get { return _activeWindows; } }
        static public Window Current => MainWindow.Current;

        static private List<Window> _activeWindows = new List<Window>();
    }
}
