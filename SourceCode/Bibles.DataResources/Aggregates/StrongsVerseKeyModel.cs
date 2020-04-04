using System;
using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class StrongsVerseKeyModel
    {
        [PrimaryKey, AutoIncrement]
        public int StrongsVerseKeyId { get; set; }

        public string VerseKey { get; set; }

        public string StrongsReference { get; set; }

        public string ReferencedText { get; set; }
    }
}
