using WPF.Tools.Attributes;
using WPF.Tools.BaseClasses;

namespace Bibles.DataResources.Models.Preferences
{
    [ModelNameAttribute("Translation Mapping")]
    public class TranslationMapping : ModelsBase
    {
        private int translationMappingId;
        private int languageId;
        private string englishLanguage;
        private string otherLanguage;

        public int TranslationMappingId 
        { 
            get
            {
                return this.translationMappingId;
            }
            
            set
            {
                this.translationMappingId = value;

                base.OnPropertyChanged(() => this.TranslationMappingId);
            }
        }

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
            }
        }

        [FieldInformationAttribute("English", Sort = 1)]
        public string EnglishLanguage 
        { 
            get
            {
                return this.englishLanguage;
            }

            set
            {
                this.englishLanguage = value;

                base.OnPropertyChanged(() => this.EnglishLanguage);
            }
        }

        [FieldInformationAttribute("Other", Sort = 2)]
        public string OtherLanguage 
        { 
            get
            {
                return this.otherLanguage;
            }
            
            set
            {
                this.otherLanguage = value;

                base.OnPropertyChanged(() =>  this.OtherLanguage);
            }
        }
    }
}
