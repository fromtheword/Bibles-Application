using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.Models.Preferences;
using GeneralExtensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bibles.Downloads
{
	internal class LoadTranslations
    {
        internal static bool LoadFile(string path)
        {
            try
            {
                string translationFileName = Path.GetFileNameWithoutExtension(path);

                string[] fileNameSplit = translationFileName.Split(new char[] { '_' });

                LanguageSetupModel language = BiblesData.Database.GetLanguage(fileNameSplit[0]);

                if (language == null)
                {
                    language = new LanguageSetupModel { Language = fileNameSplit[0] };

                    language.LanguageId = BiblesData.Database.InsertLanguage(language);
                }

                List<TranslationMappingModel> existingMappings = BiblesData.Database.GetTranslationMapping(language.LanguageId);

                List<string> fileValuesList = new List<string>();

                fileValuesList.AddRange(File.ReadAllLines(path));

                foreach(string item in fileValuesList)
                {
                    TranslationMapping mapping = JsonConvert.DeserializeObject(item, typeof(TranslationMapping)).To<TranslationMapping>();

                    if (!existingMappings.Any(m => m.EnglishLanguage.UnzipFile().ParseToString() == mapping.EnglishLanguage))
                    {
                        TranslationMappingModel mappingModel = new TranslationMappingModel
                        {
                            LanguageId = language.LanguageId,
                            TranslationMappingId = 0,
                            EnglishLanguage = mapping.EnglishLanguage.ZipFile(),
                            OtherLanguage = mapping.OtherLanguage.ZipFile()
                        };

                        BiblesData.Database.InsertTranslation(mappingModel);
                    }
                }
            }
            catch
            {
                throw;
            }

            return true;
        }
    }
}
