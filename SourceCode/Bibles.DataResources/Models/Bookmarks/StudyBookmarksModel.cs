using Bibles.DataResources.Bookmarks;
using WPF.Tools.Attributes;
using WPF.Tools.ModelViewer;
using WPF.Tools.ToolModels;

namespace Bibles.DataResources.Models.Bookmarks
{
    [ModelNameAttribute("Bookmark")]
    public class StudyBookmarksModel : ModelsBookmark
    {
        private int study;

        [FieldInformationAttribute("Study", Sort = 0, IsRequired = true)]
        [ValuesSourceAttribute("AvailableStudies")]
        [ItemType(ModelItemTypeEnum.ComboBox, isComboboxEdit:false)]
        public int Study
        {
            get
            {
                return this.study;
            }

            set
            {
                this.study = value;

                base.OnPropertyChanged(() => this.Study);
            }
        }

        public DataItemModel[] AvailableStudies
        {
            get;

            set;
        }

    }
}
