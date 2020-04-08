using Bibles.Common;
using GeneralExtensions;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPF.Tools.BaseClasses;
using WPF.Tools.CommonControls;
using WPF.Tools.ToolModels;

namespace Bibles.Downloads
{
    /// <summary>
    /// Interaction logic for OnlineResources.xaml
    /// </summary>
    public partial class OnlineResources : UserControlBase
    {
        public OnlineResources()
        {
            this.InitializeComponent();

            this.InitializeLinks();
        }

        private void LinkItem_Cliked(object sender, MouseButtonEventArgs e)
        {
            try
            {
                TextBlock linkBox = (TextBlock)sender;

                Process.Start(linkBox.Tag.ParseToString());
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void LinkItem_Leave(object sender, MouseEventArgs e)
        {
            TextBlock linkBox = (TextBlock)sender;

            linkBox.Foreground = Brushes.DarkBlue;
        }

        private void LinkItem_Enter(object sender, MouseEventArgs e)
        {
            TextBlock linkBox = (TextBlock)sender;

            linkBox.Foreground = Brushes.Blue;
        }

        private void InitializeLinks()
        {
            foreach(DataItemModel item in this.GetLinks().OrderBy(d => d.DisplayValue))
            {
                TextBlock linkItem = new TextBlock
                { 
                    Text = item.DisplayValue, 
                    Tag = item.ItemKey, 
                    Foreground = Brushes.DarkBlue, 
                    TextDecorations = TextDecorations.Underline                   
                };

                linkItem.PreviewMouseLeftButtonUp += this.LinkItem_Cliked;

                linkItem.MouseEnter += this.LinkItem_Enter;

                linkItem.MouseLeave += this.LinkItem_Leave;

                this.uxLinks.Children.Add(linkItem);
            }
        }

        private DataItemModel[] GetLinks()
        {
            return new DataItemModel[]
            {
                new DataItemModel { DisplayValue = "The Third Angel Message", ItemKey = "https://thethirdangelsmessage.com/bible-study-series.php" },
                new DataItemModel { DisplayValue = "The Third Angel Message (YouTube)", ItemKey = "https://www.youtube.com/channel/UC-VWzjQ7kbnZRipdBzCvnEg" },
                new DataItemModel { DisplayValue = "Amazing Discoveries", ItemKey = "https://amazingdiscoveries.co.za/"},
                new DataItemModel { DisplayValue = "Amazing Discoveries (YouTube)", ItemKey = "https://www.youtube.com/channel/UC1oOoBASMrTyUBSltYcx0jA"},
                new DataItemModel { DisplayValue = "Saved To Serve – Prophesy Again", ItemKey = "https://www.prophesyagain.org/" },
                new DataItemModel { DisplayValue = "Saved To Serve – Prophesy Again (YouTube)", ItemKey = "https://www.youtube.com/playlist?list=PLp1HL89V3I5jmZjv-hKdlJzYI_Zp2xk-c" }
            };
        }
    }
}
