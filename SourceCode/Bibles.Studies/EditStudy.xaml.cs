using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.Bookmarks;
using Bibles.Studies.Models;
using GeneralExtensions;
using System;
using System.Windows;
using ViSo.Dialogs.Controls;
using WPF.Tools.BaseClasses;

namespace Bibles.Studies
{
    /// <summary>
    /// Interaction logic for EditStudy.xaml
    /// </summary>
    public partial class EditStudy : UserControlBase
    {
        public EditStudy(StudyHeaderModel study)
        {
            this.InitializeComponent();

            if (study == null)
            {
                this.SubjectHeader = new StudyHeader();

                this.SubjectContent = new StudyContentModel();
            }
            else
            {
                this.SubjectHeader = study.CopyToObject(new StudyHeader()).To<StudyHeader>();

                this.SubjectContent = BiblesData.Database.GetStudyContent(this.SubjectHeader.StudyHeaderId);

                this.SubjectHeader.StudyCategoryName = BiblesData.Database.GetCategoryName(this.SubjectHeader.StudyCategoryId);
            }

            this.uxSubjectHeader.Items.Add(this.SubjectHeader);

            this.Loaded += this.EditStudy_Loaded;
        }

        public EditStudy() : this(null)
        {
        }

        public StudyHeader SubjectHeader { get; set; }

        public StudyContentModel SubjectContent { get; set; }

        public bool SaveStudy()
        {
            try
            {
                if (this.uxSubjectHeader.HasValidationError)
                {
                    return false;
                }

                this.SubjectHeader.StudyHeaderId = BiblesData.Database.InsertSubjectHeader(this.SubjectHeader.CopyToObject(new StudyHeaderModel()).To<StudyHeaderModel>());

                this.SubjectContent.StudyHeaderId = this.SubjectHeader.StudyHeaderId;

                this.SubjectContent.Content = this.uxContent.FlowDocumentText.ZipFile();

                BiblesData.Database.InsertStudyContent(this.SubjectContent);

                return true;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);

                return false;
            }
        }

        public void AddBookmark(ModelsBookmark bookmark)
        {
            this.uxStudyBookmarks.AddBookmark(bookmark);
        }

        private void EditStudy_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.WasFirstLoaded)
            {
                return;
            }

            try
            {    
                this.uxColumn0.Width = new GridLength(this.uxColumn0.Width.Value + 1, GridUnitType.Star);

                this.uxContent.FlowDocumentText = this.SubjectContent.Content.UnzipFile().ParseToString();

                this.uxStudyBookmarks.LoadBookmarks(this.SubjectHeader.StudyHeaderId);

                base.WasFirstLoaded = true;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void SubjectHeder_Browsed(object sender, string buttonKey)
        {
            try
            {
                switch(buttonKey)
                {
                    case "StudyCategoryBrowse":

                        StudyCategories category = new StudyCategories();

                        if (ControlDialog.ShowDialog("Categories", category, string.Empty, autoSize:false).IsFalse())
                        {
                            return;
                        }

                        StudyCategoryModel categoryModel = category.SelectedCategory;

                        this.SubjectHeader.StudyCategoryId = categoryModel.StudyCategoryId;

                        this.SubjectHeader.StudyCategoryName = categoryModel.CategoryName;

                        break;
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
