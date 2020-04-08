using System;
using System.ComponentModel;
using System.Windows;
using Bibles.Common;
using GeneralExtensions;
using WPF.Tools.BaseClasses;
using WPF.Tools.Specialized;

namespace ViSo.Dialogs.Controls
{
    public static class ControlDialog
    {
        public delegate void ControlDialogClosingEvent(object sender, CancelEventArgs e);

        public static event ControlDialogClosingEvent ControlDialogClosing;

        private static ControlWindow window;
        
        public static bool? ShowDialog(
            string windowTitle, 
            UserControlBase control, 
            string boolUpdateMethod, 
            bool autoSize = true,
            bool showOkButton = true,
            bool showCancelButton = true)
        {
            try
            {
                ControlDialog.window = new ControlWindow(windowTitle, control, boolUpdateMethod, true, autoSize, showOkButton, showCancelButton);

                ControlDialog.window.Closing += ControlDialog.ControlWindow_Closing;

                ControlDialog.window.Owner = Application.Current.MainWindow;

                return ControlDialog.window.ShowDialog();
            }
            catch (Exception err)
            {
                MessageDisplay.Show(err.InnerExceptionMessage());

                return false;
            }
        }
        
        public static void Show(string windowTitle, 
            UserControlBase control, 
            string boolUpdateMethod,
            Window owner = null, 
            bool isTopMost = false,
            bool autoSize = true,
            bool showOkButton = true,
            bool showCancelButton = true)
        {
            try
            {
                ControlDialog.window = new ControlWindow(windowTitle, control, boolUpdateMethod, false, autoSize, showOkButton, showCancelButton);

                if (owner != null)
                {
                    ControlDialog.window.Owner = owner;
                }

                ControlDialog.window.Topmost = isTopMost;

                ControlDialog.window.Closing += ControlDialog.ControlWindow_Closing;

                ControlDialog.window.Show();

            }
            catch (Exception err)
            {
                MessageDisplay.Show(err.InnerExceptionMessage());
            }
        }
        

        private static void ControlWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                ControlDialog.ControlDialogClosing?.Invoke(sender, e);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
            finally
            {
                ControlDialog.window = null;
            }
        }
    }
}
