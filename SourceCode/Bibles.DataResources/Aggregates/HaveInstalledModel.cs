using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class HaveInstalledModel
    {
        [PrimaryKey]
        public int EntityModel { get; set; }
    }
}
