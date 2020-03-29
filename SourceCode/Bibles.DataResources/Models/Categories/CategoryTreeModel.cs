using System.Collections.Generic;

namespace Bibles.DataResources.Models.Categories
{
    public class CategoryTreeModel
    {
        public CategoryTreeModel()
        {
            this.ChildCategories = new List<CategoryTreeModel>();
        }

        public int StudyCategoryId { get; set; }

        public string CategoryName { get; set; }

        public int ParentStudyCategoryId { get; set; }

        public List<CategoryTreeModel> ChildCategories;
    }
}
