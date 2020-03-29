using Bibles.Data;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using System.Collections.Generic;
using WPF.Tools.BaseClasses;
using WPF.Tools.Dictionaries;
using WPF.Tools.ToolModels;

namespace Bibles
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : ApplicationBase
    {
        public App()
        {
            UserPreferenceModel preference = GlobalResources.UserPreferences;

            if (preference != null && preference.LanguageId > 0)
            {
                this.LoadTranslationsDictionary(preference.LanguageId);
            }
        }

        private void LoadTranslationsDictionary(int languageId)
        {
            List<TranslationMappingModel> translationMapping = BiblesData.Database
                .GetTranslationMapping(languageId);

            List<DataItemModel> translationItems = new List<DataItemModel>();

            foreach (TranslationMappingModel mapping in translationMapping)
            {
                translationItems.Add(new DataItemModel
                {
                    ItemKey = mapping.EnglishLanguage.UnzipFile().ParseToString(),
                    DisplayValue = mapping.OtherLanguage.UnzipFile().ParseToString()
                });
            }

            TranslationDictionary.LoadTransaltionFile(translationItems);
        }
    }
}
