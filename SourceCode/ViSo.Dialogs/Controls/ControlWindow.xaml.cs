using System;
using System.Windows;
using System.Windows.Controls;
using WPF.Tools.BaseClasses;
using GeneralExtensions;
using WPF.Tools.Specialized;

namespace ViSo.Dialogs.Controls
{
    /// <summary>
    /// Interaction logic for ControlWindow.xaml
    /// </summary>
    public partial class ControlWindow : WindowBase
    {
        private bool isAsDialog = false;

        private bool isAutoZize = false;

        private string boolUpdateMethodName;

        public ControlWindow(string windowTitle, 
            UserControlBase control, 
            string boolUpdateMethod, 
            bool isDialog, 
            bool autoSize,
            bool showOkButton,
            bool showCancelButton)
        {
            this.InitializeComponent();

            this.isAsDialog = isDialog;

            this.isAutoZize = autoSize;

            this.AutoSize = autoSize;

            this.Title = windowTitle;

            this.uxOk.Visibility = showOkButton ? Visibility.Visible : Visibility.Collapsed;

            this.uxCancel.Visibility = showCancelButton ? Visibility.Visible : Visibility.Collapsed;

            this.uxContent.Content = control;

            this.boolUpdateMethodName = boolUpdateMethod;

            this.Loaded += this.ControlWindow_Loaded;
        }

        public UserControlBase ControlContent
        {
            get
            {
                return this.uxContent.Content.To<UserControlBase>();
            }
        }

        private void ControlWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.AutoSize = this.isAutoZize;
                
                this.Top = this.Height / 5;
            }
            catch (Exception err)
            {
                MessageDisplay.Show(err.InnerExceptionMessage());
            }
        }

        private void OkButton_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!this.boolUpdateMethodName.IsNullEmptyOrWhiteSpace())
                {
                    bool updateResult = this.InvokeMethod(this.uxContent.Content, this.boolUpdateMethodName, new object[] { }).TryToBool();

                    if (!updateResult)
                    {
                        return;
                    }
                }

                if (this.isAsDialog)
                {
                    this.DialogResult = true;
                }
                else
                {
                    this.Close();
                }
            }
            catch (Exception err)
            {
                MessageDisplay.Show(err.InnerExceptionMessage());
            }
        }

        private void Cancel_Cliked(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
