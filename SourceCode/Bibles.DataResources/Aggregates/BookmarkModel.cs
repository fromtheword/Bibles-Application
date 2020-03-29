using SQLite;
using System;

namespace Bibles.DataResources.Aggregates
{
    public class BookmarkModel
    {
        [PrimaryKey]
        public string VerseKey { get; set; }

        public string BookMarkName { get; set; }

        public string Description { get; set; }

        public int VerseRangeEnd { get; set; }

        public DateTime BookmarkDate { get; set; }
    }
}
