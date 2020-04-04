using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class StrongsEntryModel
    {
        [PrimaryKey]
        public string StrongsNumber { get; set; }

        public string Entry { get; set; }
        
        public string GreekBeta { get; set; }

        public string GreekUnicode { get; set; }

        public string GreekTranslit { get; set; }

        public string Pronunciation { get; set; }

        public string Derivation { get; set; }

        public string StrongsDefinition { get; set; }

        public string KingJamesDefinition { get; set; }
    }
}
