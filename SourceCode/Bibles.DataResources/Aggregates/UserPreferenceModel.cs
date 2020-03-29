using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class UserPreferenceModel
    {
        [PrimaryKey, AutoIncrement]
        public int UserId { get; set; }

        public int DefaultBible { get; set; }

        public int LanguageId { get; set; }

        public bool SynchronizzeTabs { get; set; }

        public string Font { get; set; }

        public int FontSize { get; set; }

        public string LastReadVerse { get; set; }
    }
}
