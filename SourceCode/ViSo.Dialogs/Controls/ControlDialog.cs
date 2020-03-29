using System;
using System.Windows;
using GeneralExtensions;
using WPF.Tools.BaseClasses;
using WPF.Tools.Specialized;

namespace ViSo.Dialogs.Controls
{
    public static class ControlDialog
    {
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

                return ControlDialog.window.ShowDialog();
            }
            catch (Exception err)
            {
                MessageDisplay.Show(err.InnerExceptionMessage());

                return false;
            }
            finally
            {
                ControlDialog.window = null;
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

                ControlDialog.window.Show();

            }
            catch (Exception err)
            {
                MessageDisplay.Show(err.InnerExceptionMessage());
            }
            finally
            {
                ControlDialog.window = null;
            }
        }
    }
}
