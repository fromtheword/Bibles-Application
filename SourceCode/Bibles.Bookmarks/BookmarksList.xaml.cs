using Bibles.DataResources.Bookmarks;
using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Windows;
using ViSo.Dialogs.ModelViewer;
using ViSo.Dialogs.TextEditor;
using WPF.Tools.BaseClasses;
using WPF.Tools.Exstention;
using WPF.Tools.Specialized;
using WPF.Tools.Dictionaries;

namespace Bibles.Bookmarks
{
    /// <summary>
    /// Interaction logic for BookmarksList.xaml
    /// </summary>
    public partial class BookmarksList : UserControlBase
    {
        public delegate void BookmarkReaderRequestEvent(object sender, ModelsBookmark bookmark);

        public event BookmarkReaderRequestEvent BookmarkReaderRequest;

        private ModelsBookmark selectedBookmark;

        private ModelsBookmark[] bookmarks;

        public BookmarksList()
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.InitializeData();
        }

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

                if (value != null)
                {
                    this.uxBibleVerse.Text = value.VerseText;

                    this.uxBookmarkDescription.Text = value.Description;
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

        private void OpenVers_Cliked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedBookmark == null)
            {
                MessageDisplay.Show("Please select a Bookmark");

                return;
            }

            try
            {
                this.BookmarkReaderRequest?.Invoke(this, this.SelectedBookmark);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void EditVerse_Cliked(object sender, System.Windows.RoutedEventArgs e)
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

                BookmarkModel dbModel = this.SelectedBookmark.CopyToObject(new BookmarkModel()).To<BookmarkModel>();

                BiblesData.Database.InsertBookmarkModel(dbModel);
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
        
        private void DeleteVerse_Cliked(object sender, System.Windows.RoutedEventArgs e)
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

                BiblesData.Database.DeleteBookmark(this.SelectedBookmark.VerseKey);

                this.Bookmarks = this.Bookmarks.Remove(this.SelectedBookmark);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    
        private void InitializeData()
        {
            List<BookmarkModel> bookmarkList = BiblesData.Database.GetBookmarks();

            List<ModelsBookmark> result = new List<ModelsBookmark>();

            foreach (BookmarkModel bookmark in bookmarkList)
            {
                ModelsBookmark resultModel = bookmark.CopyToObject(new ModelsBookmark()).To<ModelsBookmark>();

                resultModel.SetVerse(bookmark.VerseKey);

                result.Add(resultModel);
            }

            this.Bookmarks = result.ToArray();
        }
    }
}
