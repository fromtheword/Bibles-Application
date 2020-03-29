using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class StudyHeaderModel
    {
        [PrimaryKey, AutoIncrement]
        public int StudyHeaderId { get; set; }

        public string StudyName { get; set; }

        public string Author { get; set; }

        public int StudyCategoryId { get; set; }
    }
}
