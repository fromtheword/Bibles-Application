using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using WPF.Tools.BaseClasses;

namespace WPF.Tools.Exstention
{
    public static class ScreenExstentions
    {
        public static Screen CurrentScreen(this WindowBase window)
        {
            //Screen[] screens = Screen.AllScreens;

            //Screen result = Screen.FromHandle(new WindowInteropHelper(window).Handle);

            return Screen.FromHandle(new WindowInteropHelper(window).Handle);
        }

        public static int ScreenWidth(this WindowBase window)
        {
            return window.CurrentScreen().Bounds.Width;
        }

        public static int ScreenHeight(this WindowBase window)
        {
            return window.CurrentScreen().Bounds.Height;
        }

    }
}
