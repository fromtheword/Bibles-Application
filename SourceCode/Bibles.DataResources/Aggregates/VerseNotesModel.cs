using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class VerseNotesModel
    {
        [PrimaryKey]
        public string BibleVerseKey { get; set; }

        public byte[] FootNote { get; set; }
    }
}
