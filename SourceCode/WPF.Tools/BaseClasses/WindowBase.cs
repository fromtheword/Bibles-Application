using GeneralExtensions;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Windows;
using System.Windows.Input;
using WPF.Tools.CommonControls;
using WPF.Tools.Dictionaries;
using WPF.Tools.Exstention;
using WPF.Tools.Specialized;

namespace WPF.Tools.BaseClasses
{
    public abstract class WindowBase : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        private bool autoSize;

        //private bool loadSizeChanged = false;

        public WindowBase()
        {
            //this.Initialize();

            this.UseLayoutRounding = true;

            this.SnapsToDevicePixels = true;

            this.VisualBitmapScalingMode = System.Windows.Media.BitmapScalingMode.HighQuality;

            this.VisualClearTypeHint = System.Windows.Media.ClearTypeHint.Enabled;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            this.Loaded += this.WindowBase_Loaded;

            this.Closed += this.WindowBase_Closed;

            //this.SizeChanged += this.WindowBase_SizeChanged;
        }

        public bool AutoSize
        {
            get
            {
                return this.autoSize;
            }

            set
            {
                this.autoSize = value;

                this.SizeToContent = value ? SizeToContent.WidthAndHeight : SizeToContent.Manual;

                this.CenterWindow();
            }
        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.FontFamily = Application.Current.MainWindow.FontFamily;

                this.FontSize = Application.Current.MainWindow.FontSize;

                if (TranslationDictionary.TranslationLoaded)
                {
                    this.Translate();
                }

                if (this.AutoSize)
                {
                    this.CenterWindow();
                }
            }
            catch
            {
                // DO NOTHING
            }
        }

        private void WindowBase_Closed(object sender, EventArgs e)
        {
            try
            {
                if (Application.Current.MainWindow == null ||
                    Application.Current.MainWindow == this)
                {
                    Dispatcher.InvokeShutdown();
                }
            }
            catch
            {
                // DO NOTHING
            }
        }
        
        protected void OnPropertyChanged() // All properties changed
        {
            OnPropertyChanged(null);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (expression == null) throw new ArgumentNullException(nameof(expression));
            OnPropertyChanged(Propertyname.GetProperty(expression).Name);
        }

        protected override void OnPreviewKeyUp(KeyEventArgs e)
        {
            base.OnPreviewKeyUp(e);

            switch (e.Key)
            {
                case Key.Escape:

                    if (this == Application.Current.MainWindow)
                    {
                        return;
                    }

                    this.Close();

                    break;
            }
        }

        //private void Initialize()
        //{
        //    this.Style = (Style) this.FindResource("TreeViewItemStyle");

        //    ResourceDictionary resource = new ResourceDictionary();

        //    Uri resourcePath = new Uri("/WPF.Tools;component/Styles/WindowStyle.xaml", UriKind.Relative);

        //    resource.Source = resourcePath;

        //    this.Resources.MergedDictionaries.Add(resource);
        //}

        private void Translate()
        {
            this.Title = TranslationDictionary.Translate(this.Title);

            foreach (LableItem item in this.FindVisualControls(typeof(LableItem)))
            {
                string itemContent = item.Content.ParseToString();

                item.Content = TranslationDictionary.Translate(itemContent);
            }

            foreach (ActionButton item in this.FindVisualControls(typeof(ActionButton)))
            {
                string itemContent = item.Content.ParseToString();

                item.Content = TranslationDictionary.Translate(itemContent);
            }

            foreach (WatermarkTextBox item in this.FindVisualControls(typeof(WatermarkTextBox)))
            {
                item.WatermarkText = TranslationDictionary.Translate(item.WatermarkText);
            }
        }
    
        private void CenterWindow()
        {
            try
            {
                if (!this.IsLoaded)
                {
                    return;
                }

                double height = SystemParameters.PrimaryScreenHeight;

                double width = SystemParameters.PrimaryScreenWidth;

                this.Top = (height - this.ActualHeight) / 3;

                this.Left = (width - this.ActualWidth) / 3;
            }
            catch
            {

            }
        }
    }
}
