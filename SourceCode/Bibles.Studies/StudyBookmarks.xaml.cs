using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.Bookmarks;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Windows;
using ViSo.Dialogs.ModelViewer;
using ViSo.Dialogs.TextEditor;
using WPF.Tools.BaseClasses;
using WPF.Tools.Dictionaries;
using WPF.Tools.Exstention;
using WPF.Tools.Specialized;

namespace Bibles.Studies
{
    /// <summary>
    /// Interaction logic for StudyBookmarks.xaml
    /// </summary>
    public partial class StudyBookmarks : UserControlBase
    {
        private ModelsBookmark selectedBookmark;

        private ModelsBookmark[] bookmarks;

        public StudyBookmarks()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        public int StudyId { get; private set; }

        public ModelsBookmark SelectedBookmark
        {
            get
            {
                return this.selectedBookmark;
            }

            set
            {
                this.selectedBookmark = value;

                base.OnPropertyChanged(() => this.SelectedBookmark);

                if (value == null)
                {
                    this.uxVerseText.Text = string.Empty;

                    this.uxVerseDescription.Text = string.Empty;
                }
                else
                {
                    this.uxVerseText.Text = value.VerseText;

                    this.uxVerseDescription.Text = value.Description;
                }
            }
        }

        public ModelsBookmark[] Bookmarks
        {
            get
            {
                return this.bookmarks;
            }

            set
            {
                this.bookmarks = value;

                base.OnPropertyChanged(() => this.Bookmarks);
            }
        }

        public void LoadBookmarks(int studyId)
        {
            this.StudyId = studyId;

            List<StudyBookmarkModel> studyBookmarks = BiblesData.Database.GetStudyBookmarks(studyId);

            List<ModelsBookmark> result = new List<ModelsBookmark>();

            foreach(StudyBookmarkModel bookmark in studyBookmarks)
            {
                ModelsBookmark model = bookmark.CopyToObject(new ModelsBookmark()).To<ModelsBookmark>();

                model.SetVerse(bookmark.VerseKey);

                result.Add(model);
            }

            this.Bookmarks = result.ToArray();
        }

        public void AddBookmark(ModelsBookmark bookmark)
        {
            this.Bookmarks = this.Bookmarks.Add(bookmark);
        }

        private void OpenVers_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedBookmark == null)
            {
                MessageDisplay.Show("Please select a Bookmark");

                return;
            }

            try
            {
                int bibleKey = Formatters.GetBibleFromKey(this.SelectedBookmark.VerseKey);

                object[] args = new object[]
                {
                    bibleKey,
                    this.SelectedBookmark.VerseKey
                };

                this.InvokeMethod(Application.Current.MainWindow, "LoadReader", args);

                Application.Current.MainWindow.Focus();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void EditVerse_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedBookmark == null)
            {
                MessageDisplay.Show("Please select a Bookmark");

                return;
            }

            try
            {
                ModelsBookmark bookmark = this.SelectedBookmark.CopyTo(new ModelsBookmark());

                ModelView.OnItemBrowse += this.BookmarkModel_Browse;

                if (ModelView.ShowDialog(this.GetParentWindow(), true, "Bookmark", bookmark).IsFalse())
                {
                    return;
                }

                this.SelectedBookmark = bookmark.CopyTo(this.SelectedBookmark);

                StudyBookmarkModel dbModel = this.SelectedBookmark.CopyToObject(new StudyBookmarkModel()).To<StudyBookmarkModel>();

                dbModel.StudyVerseKey = $"{this.StudyId}||{this.SelectedBookmark.VerseKey}";

                BiblesData.Database.InsertStudyBookmarkModel(dbModel);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void BookmarkModel_Browse(object sender, object model, string buttonKey)
        {
            try
            {
                ModelsBookmark bookmark = (ModelsBookmark)model;

                if (TextEditing.ShowDialog("Bookmark Description", bookmark.Description).IsFalse())
                {
                    return;
                }

                bookmark.Description = TextEditing.Text;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void DeleteVerse_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedBookmark == null)
            {
                MessageDisplay.Show("Please select a Bookmark");

                return;
            }

            try
            {
                string message = $"{TranslationDictionary.Translate("Are you sure you would like to delete?")} {this.SelectedBookmark.SelectedVerse}.";

                if (MessageDisplay.Show(message, "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                string studyVerseKey = $"{this.StudyId}||{this.SelectedBookmark.VerseKey}";

                BiblesData.Database.DeleteStudyBookmark(studyVerseKey);

                this.Bookmarks = this.Bookmarks.Remove(this.SelectedBookmark);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
