using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class BibleVerseModel
    {
        [PrimaryKey]
        public string BibleVerseKey { get; set; }
        
        [MaxLength(1000)]
        public string VerseText { get; set; }
    }
}
