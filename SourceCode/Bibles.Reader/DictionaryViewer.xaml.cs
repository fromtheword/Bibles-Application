using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.DataEnums;
using Bibles.DataResources.Models.Dictionaries;
using Bibles.Reader.Models;
using GeneralExtensions;
using System;
using System.ComponentModel;
using System.Windows;
using WPF.Tools.BaseClasses;
using WPF.Tools.Dictionaries;
using WPF.Tools.Specialized;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for DictionaryViewer.xaml
    /// </summary>
    public partial class DictionaryViewer : UserControlBase
    {
        private DictionaryEntity selecteItem;

        private DictionaryListModel dictionaryModel = new DictionaryListModel();

        private DictionaryEntity[] dictionaryItemsPage;

        public DictionaryViewer()
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.dictionaryModel.PropertyChanged += this.SelectedDictionary_Changed;
               
            this.uxDictionary.Items.Add(this.dictionaryModel);

            this.Loaded += this.DictionaryViewer_Loaded;
        }

        public DictionaryEntity SelectedItem
        {
            get
            {
                return this.selecteItem;
            }

            set
            {
                this.selecteItem = value;

                base.OnPropertyChanged(() => this.SelectedItem);

                this.uxValue.Text = value == null ? string.Empty : value.ModelKey;

                this.uxContext.Text = value == null ? string.Empty : value.ModelValue.UnzipFile().ParseToString();
            }
        }

        public DictionaryEntity[] DictionaryItems
        {
            set
            {
                this.uxPager.ItemsSource.Clear();

                this.uxPager.ItemsSource.AddRange(value);
            }
        }

        public DictionaryEntity[] DictionaryItemsPage
        {
            get
            {
                return this.dictionaryItemsPage;
            }

            set
            {
                this.dictionaryItemsPage = value;

                base.OnPropertyChanged(() => this.DictionaryItemsPage);
            }
        }

        private void DictionaryViewer_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!base.WasFirstLoaded && this.dictionaryModel.GetDictionaries.Length == 0)
                {
                    MessageDisplay.Show(TranslationDictionary.Translate("No dictionaries were installed. From the Online Menu select Downloads and select a Dictionary to install."));
                }

                base.WasFirstLoaded = true;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Page_Changed(object sender, object[] selectedItems)
        {
            this.DictionaryItemsPage = selectedItems.TryCast<DictionaryEntity>();
        }
        
        private void SelectedDictionary_Changed(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                HaveInstalledEnum entity = (HaveInstalledEnum)this.dictionaryModel.Dictionary;

                this.DictionaryItems = BiblesData.Database.GetDictionary(entity).ToArray();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
