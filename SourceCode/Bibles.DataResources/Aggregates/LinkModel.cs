using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class LinkModel
    {
        [PrimaryKey, MaxLength(50)]
        public string LinkKeyId { get; set; }

        public string Comments { get; set; }
    }
}
