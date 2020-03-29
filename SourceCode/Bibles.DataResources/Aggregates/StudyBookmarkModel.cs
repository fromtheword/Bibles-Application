using SQLite;
using System;

namespace Bibles.DataResources.Aggregates
{
    public class StudyBookmarkModel
    {
        [PrimaryKey]
        public string StudyVerseKey { get; set; }

        public string StudyName { get; set; }

        public string VerseKey { get; set; }

        public string BookMarkName { get; set; }

        public string Description { get; set; }

        public int VerseRangeEnd { get; set; }

        public DateTime BookmarkDate { get; set; }
    }
}
