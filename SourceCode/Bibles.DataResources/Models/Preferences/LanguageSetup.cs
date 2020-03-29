using Bibles.DataResources.Aggregates;
using System.Linq;
using WPF.Tools.Attributes;
using WPF.Tools.BaseClasses;
using WPF.Tools.ModelViewer;
using WPF.Tools.ToolModels;

namespace Bibles.DataResources.Models.Preferences
{
    [ModelNameAttribute("Language")]
    public class LanguageSetup : ModelsBase
    {
        private int languageId;

        private string language;

        [FieldInformationAttribute("Selected Language", Sort = 1)]
        [ItemTypeAttribute(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        [ValuesSourceAttribute("Languages")]
        [BrowseButton("LanguageAddRequest", "Add Language", "Add")]
        public int LanguageId 
        { 
            get
            {
                return this.languageId;
            }
            
            set
            {
                this.languageId = value;

                base.OnPropertyChanged(() => this.LanguageId);

                LanguageSetupModel agrigate = BiblesData.Database.GetLanguage(value);

                this.Language = agrigate == null ? "Other Language" : agrigate.Language;
            }
        }

        public string Language 
        { 
            get
            {
                return this.language;
            }
            
            set
            {
                this.language = value;
            }
        }    
    
        public DataItemModel[] Languages
        {
            get
            {
                return BiblesData.Database
                                .GetLanguages()
                                .Select(l => new DataItemModel { DisplayValue = l.Language, ItemKey = l.LanguageId })
                                .ToArray();
            }
        }
    }
}
