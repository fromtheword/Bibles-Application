using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class DictionaryModel
    {
        [PrimaryKey]
        public string EntityModelKey { get; set; }

        public byte[] EntityValue { get; set; }
    }
}
