using GeneralExtensions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using WPF.Tools.Attributes;
using WPF.Tools.BaseClasses;
using WPF.Tools.ModelViewer;
using WPF.Tools.ToolModels;

namespace Bibles.DataResources.Models.Preferences
{
    [ModelNameAttribute("Bibles")]
    public class UserPreference : ModelsBase
    {
        private string font;
        private int defaultBible;
        private int language;
        private bool synchronizzeTabs;
        private int fontSize;

        public int UserId { get; set; }

        [FieldInformationAttribute("Default Bible", Sort = 1)]
        [ItemTypeAttribute(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        [ValuesSourceAttribute("ListedBibles")]
        public int DefaultBible 
        { 
            get
            {
                return this.defaultBible;
            }
            
            set
            {
                this.defaultBible = value;

                base.OnPropertyChanged(() => this.DefaultBible);
            }
        }

        [FieldInformationAttribute("Language", Sort = 2)]
        [ItemTypeAttribute(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        [ValuesSourceAttribute("Languages")]
        public int LanguageId
        { 
            get
            {
                return this.language;
            }
            
            set
            {
                this.language = value;

                base.OnPropertyChanged(() => this.LanguageId);
            }
        }

        [FieldInformationAttribute("Synchronize Tabs", Sort = 3)]
        public bool SynchronizzeTabs 
        { 
            get
            {
                return this.synchronizzeTabs;
            }
            
            set
            {
                this.synchronizzeTabs = value;

                base.OnPropertyChanged(() => this.SynchronizzeTabs);
            }
        }

        [FieldInformationAttribute("Font", Sort = 4)]
        [ItemTypeAttribute(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        [ValuesSourceAttribute("FontFamilies")]
        public string Font 
        { 
            get
            {
                return this.font;
            }
            
            set
            {
                this.font = value;
                                

                base.OnPropertyChanged(() => this.Font);
            }
        }

        [FieldInformationAttribute("Font Size", Sort = 5)]
        [ItemTypeAttribute(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        [ValuesSourceAttribute("FontSizes")]
        public int FontSize 
        { 
            get
            {
                return this.fontSize;
            }
            
            set
            {
                this.fontSize = value;

                base.OnPropertyChanged(() => this.FontSize);
            }
        }

        public string LastReadVerse { get; set; }

        public DataItemModel[] ListedBibles
        {
            get
            {
                return this
                    .InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "ListedBibles", new object[] { })
                    .To<DataItemModel[]>();
            }
        }

        public DataItemModel[] FontFamilies
        {
            get
            {
                List<DataItemModel> result = new List<DataItemModel>();

                foreach (FontFamily item in Fonts.SystemFontFamilies)
                {
                    string fontName = item.ParseToString();

                    result.Add(new DataItemModel { DisplayValue = fontName, ItemKey = fontName });
                }

                return result.ToArray();
            }
        }

        public DataItemModel[] FontSizes
        {
            get
            {
                double[] defaultSizes = new double[] { 8, 9, 10, 12, 14, 16, 24, 18 };

                return new DataItemModel[]
                {
                    new DataItemModel { DisplayValue = "8", ItemKey = 8},
                    new DataItemModel { DisplayValue = "9", ItemKey = 9},
                    new DataItemModel { DisplayValue = "10", ItemKey = 10},
                    new DataItemModel { DisplayValue = "12", ItemKey = 12},
                    new DataItemModel { DisplayValue = "14", ItemKey = 14},
                    new DataItemModel { DisplayValue = "16", ItemKey = 15},
                    new DataItemModel { DisplayValue = "18", ItemKey = 18},
                    new DataItemModel { DisplayValue = "24", ItemKey = 24},
                };
            }
        }

        public DataItemModel[] Languages
        {
            get
            {
                List<DataItemModel> result = new List<DataItemModel>();

                result.Add(new DataItemModel { DisplayValue = "<Default>", ItemKey = 0 });
                
                result.AddRange(BiblesData.Database
                                .GetLanguages()
                                .Select(l => new DataItemModel { DisplayValue = l.Language, ItemKey = l.LanguageId }));

                return result.ToArray();
            }
        }
    }
}
