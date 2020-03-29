using GeneralExtensions;
using System.Linq;
using WPF.Tools.Attributes;
using WPF.Tools.BaseClasses;
using WPF.Tools.ModelViewer;
using WPF.Tools.ToolModels;

namespace Bibles.DataResources.AvailableBooks
{
    [ModelNameAttribute("Bibles")]
    public class ModelsBibleBook : ModelsBase
    {
        private int bibleId;

        private string bibleName;

        [FieldInformationAttribute("Bible")]
        [ItemTypeAttribute(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        [ValuesSourceAttribute("ListedBibles")]
        public int BibleId
        {
            get
            {
                return this.bibleId;
            }

            set
            {
                this.bibleId = value;

                DataItemModel nameModel = this.ListedBibles.FirstOrDefault(id => id.ItemKey.ToInt32() == value);

                this.BibleName = nameModel == null ? string.Empty : nameModel.DisplayValue;

                base.OnPropertyChanged("BibleId");

                base.OnPropertyChanged("BibleName");
            }
        }
        
        public string BibleName 
        {
            get
            {
                return this.bibleName;
            }

            set
            {
                this.bibleName = value;
            }
        }

        public DataItemModel[] ListedBibles
        {
            get
            {
                return this
                    .InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "ListedBibles", new object[] { })
                    .To<DataItemModel[]>();
            }
        }
    }
}
