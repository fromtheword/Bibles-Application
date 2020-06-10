using Bibles.DataResources;
using Bibles.DataResources.DataEnums;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;
using WPF.Tools.Attributes;
using WPF.Tools.BaseClasses;
using WPF.Tools.ModelViewer;
using WPF.Tools.ToolModels;

namespace Bibles.Reader.Models
{
    [ModelNameAttribute("Dictionaries")]
    public class DictionaryListModel : ModelsBase
    {
        private int dictionary;

        private HaveInstalledEnum[] excludedOptions = new HaveInstalledEnum[] { HaveInstalledEnum.GreekEntryModel, HaveInstalledEnum.HebrewEntityModel, HaveInstalledEnum.StrongsEntryModel };

        [FieldInformationAttribute("Dictionary")]
        [ItemTypeAttribute(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        [ValuesSourceAttribute("GetDictionaries")]
        public int Dictionary
        {
            get
            {
                return this.dictionary;
            }

            set
            {
                this.dictionary = value;

                base.OnPropertyChanged(() => this.Dictionary);
            }
        }

        public DataItemModel[] GetDictionaries
        {
            get
            {
                List<DataItemModel> result = new List<DataItemModel>();

                foreach(HaveInstalledEnum item in Enum.GetValues(typeof(HaveInstalledEnum)))
                {
                    if (this.excludedOptions.Contains(item))
                    {
                        continue;
                    }

                    if (!BiblesData.Database.IsInstalled(item))
                    {
                        continue;
                    }

                    result.Add(new DataItemModel { DisplayValue = item.GetDescriptionAttribute(), ItemKey = (int)item });
                }

                return result.ToArray();
            }
        }
    }
}
