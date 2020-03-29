using GeneralExtensions;
using SQLite;

namespace Bibles.DataResources.Aggregates
{
    public class StudyContentModel
    {
        private byte[] content;

        [PrimaryKey]
        public int StudyHeaderId { get; set; }

        public byte[] Content
        {
            get
            {
                if (this.content == null)
                {
                    return string.Empty.ZipFile();
                }

                return this.content;
            }

            set
            {
                if (value == null)
                {
                    this.content = string.Empty.ZipFile();
                }
                else
                {
                    this.content = value;
                }
            }
        }
    }
}
