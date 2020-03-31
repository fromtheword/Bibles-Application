using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeneralExtensions;
using IconSet;

namespace WPF.Tools.CommonControls
{
    public class TreeViewItemTool : TreeViewItem
    {
        public delegate void OnCheckChangedEvent(object sender, bool? checkState);

        public event OnCheckChangedEvent OnCheck_Changed;
        
        private bool isCheckBox;

        private bool isChecked;

        private CheckBoxItem checkBox;

        private LableItem labelItem;

        private Image image;

        private StackPanel itemsStack;

        private object itemContent;

        private Brush labelForeGround = Brushes.Black;

        private string resourceImageName;

        public TreeViewItemTool()
        {
            this.InitializeLayout();
        }
        
        public bool IsCheckBox
        {
            get => this.IsChecked ? this.IsChecked : this.isCheckBox;

            set
            {
                this.isCheckBox = value;

                if (value)
                {
                    this.checkBox = new CheckBoxItem { Margin = new Thickness(0, 5, 0, 0) };

                    this.checkBox.Visibility = this.IsCheckBox ? Visibility.Visible : Visibility.Collapsed;

                    this.checkBox.IsChecked = this.IsChecked;

                    this.checkBox.Checked += this.isChecked_Changeds;

                    this.checkBox.Unchecked += this.isChecked_Changeds;

                    this.itemsStack.Children.Insert(0, this.checkBox);
                }
                else if (this.checkBox != null)
                {
                    this.itemsStack.Children.RemoveAt(0);

                    this.checkBox = null;
                }

                if (this.checkBox != null)
                {
                    this.checkBox.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        public bool IsChecked
        {
            get
            {
                if (this.checkBox == null)
                {
                    return this.isChecked;
                }

                return this.checkBox.IsChecked.IsTrue();
            }

            set
            {
                this.isChecked = value;

                if (this.checkBox != null)
                {
                    this.checkBox.IsChecked = value;
                }
            }
        }

        public string ResourceImageName 
        { 
            get
            {
                return this.resourceImageName;
            }
            
            set
            {
                this.resourceImageName = value;

                if (!value.IsNullEmptyOrWhiteSpace())
                {
                    this.image = new Image
                    {
                        Source = IconSets.ResourceImageSource(value, 16),
                    };

                    this.itemsStack.Children.Insert(0, this.image);
                }
                else if (this.image != null)
                {
                    this.itemsStack.Children.RemoveAt(0);

                    this.image = null;
                }
            }
        }

        new public object Header
        {
            get
            {
                if (this.labelItem == null)
                {
                    return this.itemContent;
                }

                return this.labelItem.Content;
            }

            set
            {
                this.itemContent = value;

                if (this.labelItem != null)
                {
                    this.labelItem.Content = value;
                }
            }
        }

        new public Brush Foreground
        {
            get
            {
                return this.labelForeGround;
            }

            set
            {
                this.labelForeGround = value;

                if (this.labelItem != null)
                {
                    this.labelItem.Foreground = value;
                }
            }
        }

        private void isChecked_Changeds(object sender, RoutedEventArgs e)
        {
            OnCheck_Changed?.Invoke(this, this.checkBox.IsChecked);
        }
        
        private void InitializeLayout()
        {
            this.itemsStack = new StackPanel();

            this.itemsStack.Orientation = Orientation.Horizontal;

            this.labelItem = new LableItem { Content = this.Header, VerticalContentAlignment = VerticalAlignment.Top };

            this.itemsStack.Children.Add(this.labelItem);

            base.Header = this.itemsStack;
        }
    }
}
