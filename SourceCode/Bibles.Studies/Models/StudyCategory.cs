using WPF.Tools.Attributes;
using WPF.Tools.BaseClasses;

namespace Bibles.Studies.Models
{
    [ModelNameAttribute("Category")]
    public class StudyCategory : ModelsBase
    {
        private int studyCategoryId;

        private string categoryName;
        
        private int parentStudyCategoryId;

        public int StudyCategoryId 
        { 
            get
            {
                return this.studyCategoryId;
            }
            
            set
            {
                this.studyCategoryId = value;

                base.OnPropertyChanged(() => this.StudyCategoryId);
            }
        }

        [FieldInformation("Category Name", IsRequired = true)]
        public string CategoryName 
        { 
            get
            {
                return this.categoryName;
            }
            
            set
            {
                this.categoryName = value;

                base.OnPropertyChanged(() => this.CategoryName);
            }
        }

        public int ParentStudyCategoryId 
        {             
            get
            {
                return this.parentStudyCategoryId;
            }
            
            set
            {
                this.parentStudyCategoryId = value;

                base.OnPropertyChanged(() => this.ParentStudyCategoryId);
            }
        }
    }
}
