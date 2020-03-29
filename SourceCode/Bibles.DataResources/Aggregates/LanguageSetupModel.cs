using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class LanguageSetupModel
    {
        [PrimaryKey, AutoIncrement]
        public int LanguageId { get; set; }

        public string Language { get; set; }
    }
}
