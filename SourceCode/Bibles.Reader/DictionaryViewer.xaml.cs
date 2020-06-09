using Bibles.DataResources;
using Bibles.DataResources.DataEnums;
using Bibles.DataResources.Models.Dictionaries;
using WPF.Tools.BaseClasses;
using GeneralExtensions;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for DictionaryViewer.xaml
    /// </summary>
    public partial class DictionaryViewer : UserControlBase
    {
        private DictionaryEntity selecteItem;

        private DictionaryEntity[] dictionaryItemsPage;

        public DictionaryViewer(HaveInstalledEnum entity)
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.DictionaryItems = BiblesData.Database.GetDictionary(entity).ToArray();
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

        private void Page_Changed(object sender, object[] selectedItems)
        {
            this.DictionaryItemsPage = selectedItems.TryCast<DictionaryEntity>();
        }
    }
}
