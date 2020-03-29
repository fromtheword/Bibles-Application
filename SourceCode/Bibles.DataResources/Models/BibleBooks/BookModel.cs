using System.Collections.Generic;

namespace Bibles.DataResources.BibleBooks
{
    public class BookModel
    {
        public BookModel()
        {
            this.Chapters = new Dictionary<int, ChapterModel>();
        }

        public string BookName { get; set; }

        public string BookKey { get; set; }

        public Dictionary<int, ChapterModel> Chapters;
    }
}
