using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class HighlightVerseModel
    {
        [PrimaryKey]
        public string BibleVerseKeyId { get; set; }

        public string HexColour { get; set; }
    }
}
