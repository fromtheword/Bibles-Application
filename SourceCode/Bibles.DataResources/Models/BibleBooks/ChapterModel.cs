using System.Collections.Generic;

namespace Bibles.DataResources.BibleBooks
{
    public class ChapterModel
    {
        public ChapterModel()
        {
            this.Verses = new Dictionary<int, VerseModel>();
        }

        public int ChapterNumber { get; set; }

        public string ChapterKey { get; set; }

        public Dictionary<int, VerseModel> Verses;
    }
}
