using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.Models.Categories;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using ViSo.Dialogs.Input;
using WPF.Tools.BaseClasses;
using WPF.Tools.CommonControls;
using WPF.Tools.Dictionaries;
using WPF.Tools.Specialized;

namespace Bibles.Studies
{
    /// <summary>
    /// Interaction logic for StudyCategories.xaml
    /// </summary>
    public partial class StudyCategories : UserControlBase
    {
        public delegate void SelectedCategoryChangedEvent(object sender, RoutedPropertyChangedEventArgs<object> e);

        public event SelectedCategoryChangedEvent SelectedCategoryChanged;

        public StudyCategories()
        {
            this.InitializeComponent();

            this.Initialize();
        }

        public StudyCategoryModel SelectedCategory
        {
            get
            {
                if (this.uxCategoryTree.SelectedItem == null)
                {
                    return null;
                }

                CategoryTreeModel treeItem = ((TreeViewItemTool)this.uxCategoryTree.SelectedItem).Tag.To<CategoryTreeModel>();

                return treeItem.CopyToObject(new StudyCategoryModel()).To<StudyCategoryModel>();
            }
        }

        private void CategoryTree_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InputBox.ShowDialog("Category", "Name").IsFalse())
                {
                    return;
                }

                int studyCategoryId = BiblesData.Database.InsertCategory(InputBox.Result, 0);

                CategoryTreeModel treeModel = new CategoryTreeModel
                {
                    CategoryName = InputBox.Result,
                    ParentStudyCategoryId = 0,
                    StudyCategoryId = studyCategoryId
                };

                this.uxCategoryTree.Items.Add(this.CreateTreeViewItem(treeModel));
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void CategoryItem_Add(object sender, RoutedEventArgs e)
        {
            try
            {
                if (InputBox.ShowDialog("Category", "Name").IsFalse())
                {
                    return;
                };

                int studyCategoryId = BiblesData.Database.InsertCategory(InputBox.Result, this.SelectedCategory.StudyCategoryId);

                CategoryTreeModel treeModel = new CategoryTreeModel
                {
                    CategoryName = InputBox.Result,
                    ParentStudyCategoryId = this.SelectedCategory.StudyCategoryId,
                    StudyCategoryId = studyCategoryId
                };

                TreeViewItemTool tool = this.CreateTreeViewItem(treeModel);

                ((TreeViewItemTool)this.uxCategoryTree.SelectedItem).Items.Add(tool);

                ((TreeViewItemTool)this.uxCategoryTree.SelectedItem).IsExpanded = true;

                tool.IsSelected = true;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void CategoryItem_Delete(object sender, RoutedEventArgs e)
        {
            try
            {
                if (MessageDisplay.Show($"{TranslationDictionary.Translate("Are you sure you would like to delete?")} {this.SelectedCategory.CategoryName}", "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                BiblesData.Database.DeleteCategory(this.SelectedCategory.StudyCategoryId);

                TreeViewItemTool category = this.uxCategoryTree.SelectedItem.To<TreeViewItemTool>();

                if (category.Parent.GetType() == typeof(TreeViewItemTool))
                {
                    TreeViewItemTool parent = category.Parent.To<TreeViewItemTool>();

                    parent.Items.Remove(category);
                }
                else
                {
                    this.uxCategoryTree.Items.Remove(category);
                }

            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void TreeViewItem_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                e.Handled = true;

                this.SelectedCategoryChanged?.Invoke(this, e);
            }
            catch(Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        public void SelectCategory(int studyCategoryId)
        {
            foreach(TreeViewItemTool item in this.uxCategoryTree.Items)
            {
                item.IsExpanded = this.SelectCategoryItem(item, studyCategoryId);
            }
        }

        public void Initialize()
        {
            this.uxCategoryTree.ContextMenu = null;

            this.uxCategoryTree.Items.Clear();

            ContextMenu categoryTreeContextMenu = new ContextMenu();

            MenuItem treeAdd = new MenuItem { Header = "Add Parent Category" };

            treeAdd.Click += this.CategoryTree_Add;

            categoryTreeContextMenu.Items.Add(treeAdd);

            this.uxCategoryTree.ContextMenu = categoryTreeContextMenu;
            
            List<CategoryTreeModel> categoriesList = BiblesData.Database.GetCategoryTree();

            foreach(CategoryTreeModel category in categoriesList)
            {
                TreeViewItemTool categoryTreeItem = this.CreateTreeViewItem(category);

                this.LoadCategoryChildren(category, categoryTreeItem);

                this.uxCategoryTree.Items.Add(categoryTreeItem);
            }
        }

        private void LoadCategoryChildren(CategoryTreeModel category, TreeViewItemTool categoryTreeItem)
        {
            foreach(CategoryTreeModel childCategory in category.ChildCategories)
            {
                TreeViewItemTool childTreeItem = this.CreateTreeViewItem(childCategory);

                this.LoadCategoryChildren(childCategory, childTreeItem);

                categoryTreeItem.Items.Add(childTreeItem);
            }
        }

        private TreeViewItemTool CreateTreeViewItem(CategoryTreeModel category)
        {
            TreeViewItemTool result = new TreeViewItemTool
            {
                Header = category.CategoryName,
                Tag = category,
                ContextMenu = new ContextMenu()
            };

            MenuItem itemAdd = new MenuItem { Header = "Add Child Category" };

            MenuItem itemDelete = new MenuItem { Header = "Delete Category" };

            itemAdd.Click += this.CategoryItem_Add;

            itemDelete.Click += this.CategoryItem_Delete;

            result.ContextMenu.Items.Add(itemAdd);

            result.ContextMenu.Items.Add(itemDelete);

            return result;
        }
    
        private bool SelectCategoryItem(TreeViewItemTool parent, int studyCategoryId)
        {
            if (parent.Tag != null)
            {
                CategoryTreeModel tagValue = parent.Tag.To<CategoryTreeModel>();

                if (tagValue.StudyCategoryId == studyCategoryId)
                {
                    parent.IsSelected = true;

                    return true;
                }
            }

            if (parent.Items == null)
            {
                return false;
            }

            foreach(TreeViewItemTool child in parent.Items)
            {
                child.IsExpanded = this.SelectCategoryItem(child, studyCategoryId);

                if (child.IsExpanded)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
