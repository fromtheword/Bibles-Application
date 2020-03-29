using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class TranslationMappingModel
    {
        [PrimaryKey, AutoIncrement]
        public int TranslationMappingId { get; set; }

        public int LanguageId { get; set; }

        public byte[] EnglishLanguage { get; set; }

        public byte[] OtherLanguage { get; set; }
    }
}
