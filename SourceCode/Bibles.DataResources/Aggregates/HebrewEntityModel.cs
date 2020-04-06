using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class HebrewEntityModel
    {
        [PrimaryKey]
        public string StrongsNumber { get; set; }

        public string Note { get; set; }

        public string Source { get; set; }

        public string Meaning { get; set; }

        public string Usage { get; set; }

        public string Pos { get; set; }

        public string Pron { get; set; }

        public string Xlit { get; set; }

        public string Language { get; set; }
    }
}
