using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class StudyCategoryModel
    {
        [PrimaryKey, AutoIncrement]
        public int StudyCategoryId { get; set; }

        public string CategoryName { get; set; }

        public int ParentStudyCategoryId { get; set; }
    }
}
