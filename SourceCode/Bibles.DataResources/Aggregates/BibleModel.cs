using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class BibleModel
    {
        [PrimaryKey, AutoIncrement]
        public int BiblesId { get; set; }

        public string BibleName { get; set; }
    }
}
