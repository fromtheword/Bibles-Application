using Bibles.DataResources.AvailableBooks;
using Bibles.DataResources.Bookmarks;
using Bibles.Common;
using Bibles.Data;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using IconSet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using ViSo.Dialogs.Controls;
using ViSo.Dialogs.ModelViewer;
using ViSo.Dialogs.TextEditor;
using WPF.Tools.BaseClasses;
using WPF.Tools.ColoutPicker;
using WPF.Tools.Exstention;
using WPF.Tools.Specialized;
using Bibles.Studies;
using Bibles.Studies.Models;
using Bibles.DataResources.Models.Bookmarks;
using WPF.Tools.ToolModels;
using System.Linq;
using WPF.Tools.TabControl;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for Reader.xaml
    /// </summary>
    public partial class Reader : UserControlBase
    {
        public delegate void BibleChangedEvent(object sender, ModelsBibleBook bible);

        public delegate void BookChangedEvent(object sender, string chapterkey);

        public delegate void SelectedVerseChangedEvent(object sender, BibleVerseModel verse);
        
        public event BibleChangedEvent BibleChanged;

        public event BookChangedEvent BookChanged;

        public event SelectedVerseChangedEvent SelectedVerseChanged;

        private int scrollOnLoadVerse = -1;

        private string selectedKey;

        private Strongs uxStrongs = new Strongs();

        private Dictionary<int, BibleVerseModel> versesDictionary;

        private Dictionary<int, HighlightRitchTextBox> loadedTextBoxDictionary = new Dictionary<int, HighlightRitchTextBox>();

        private Dictionary<int, StackPanel> loadedVerseStackDictionary = new Dictionary<int, StackPanel>();

        public Reader()
        {
            this.InitializeComponent();

            this.Loaded += this.Reader_Loaded;

            this.Bible = new ModelsBibleBook();

            this.Bible.PropertyChanged += this.Bible_Changed;

            BibleLoader.LinkViewerClosed += this.RemoteLinkViewer_Closed;

            this.uxBible.Items.Add(this.Bible);

            this.uxStrongsPin.Items.Add(this.uxStrongs);
        }

        ~Reader()
        {
            BibleLoader.LinkViewerClosed -= this.RemoteLinkViewer_Closed;
        }

        public string SelectedVerseKey
        {
            get
            {
                return this.selectedKey;
            }
        }
        
        public ModelsBibleBook Bible { get; set; }

        public void SetBible(int bibleId)
        {
            this.uxBible[0, 0].SetValue(bibleId);            
        }
        
        public void SetChapter(string key)
        {
            this.selectedKey = key;

            this.SetHeader();

            this.LoadVerses();

            this.SetCanPage(Formatters.GetChapterFromKey(key));
        }

        public void SetVerse(string key)
        {
            if (key.IsNullEmptyOrWhiteSpace())
            {
                return;
            }

            this.SetCanPage(Formatters.GetChapterFromKey(key));

            this.selectedKey = key;

            this.SetHeader();

            if (this.versesDictionary == null || this.versesDictionary.Count == 0)
            {
                this.LoadVerses();
            }

            if (Formatters.IsBiblesKey(key))
            {
                this.ScrollToVerse(Formatters.GetVerseFromKey(key));
            }
            else
            {
                this.ScrollToVerse(Formatters.GetVerseFromKey($"{this.Bible.BibleId}||{key}"));
            }
        }

        public void SelectVerse(string key)
        {
            if (key.IsNullEmptyOrWhiteSpace())
            {
                return;
            }

            this.SetCanPage(Formatters.GetChapterFromKey(key));

            this.selectedKey = key;

            this.SetHeader();

            // Causes a stack overflow
            //int verseNumber = Formatters.GetVerseFromKey(key);

            //if (verseNumber > 0 && this.loadedTextBoxDictionary.ContainsKey(verseNumber))
            //{
            //    HighlightRitchTextBox verseBox = this.loadedTextBoxDictionary[verseNumber];

            //    verseBox.Focus();
            //}
        }
        
        private void Reader_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.WasFirstLoaded)
            {
                return;
            }

            try
            {
                this.ScrollToVerse(this.scrollOnLoadVerse);

                this.scrollOnLoadVerse = -1;
                
                base.WasFirstLoaded = true;
                
                this.SetCanPage(Formatters.GetChapterFromKey(this.selectedKey));
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Bible_Changed(object sender, PropertyChangedEventArgs e)
        {
            switch(e.PropertyName)
            {
                case "BibleName":
                    
                    this.BibleChanged?.Invoke(this, this.Bible);

                    break;

                case "BibleId":

                    if (base.IsLoaded)
                    {
                        this.LoadVerses();
                    }

                    break;
            }
        }

        private void Bookmark_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                int selectedVerse = Formatters.GetVerseFromKey(this.selectedKey);

                if (selectedVerse <= 0)
                {
                    throw new ApplicationException("Please select a Verse.");
                }

                Dictionary<int, UserControlBase> openStudies = new Dictionary<int, UserControlBase>();

                Dictionary<int, StudyHeader> studyHeaders = new Dictionary<int, StudyHeader>();

                #region CHECK FOR OPEN STUDIES

                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() != typeof(ControlWindow))
                    {
                        continue;
                    }

                    UserControlBase controlBase = window.GetPropertyValue("ControlContent").To<UserControlBase>();

                    if (controlBase.GetType() != typeof(EditStudy))
                    {
                        continue;
                    }

                    StudyHeader studyHeader = controlBase.GetPropertyValue("SubjectHeader").To<StudyHeader>();

                    if (studyHeader.StudyHeaderId <= 0)
                    {
                        string studyName = studyHeader.StudyName.IsNullEmptyOrWhiteSpace() ? "Unknown" : studyHeader.StudyName;

                        string message = $"Study {studyName} have not been saved yet. This study will not be available for bookmarks.";

                        MessageDisplay.Show(message);

                        continue;
                    }

                    openStudies.Add(studyHeader.StudyHeaderId, controlBase);

                    studyHeaders.Add(studyHeader.StudyHeaderId, studyHeader);
                }

                #endregion

                ModelsBookmark bookmark = new ModelsBookmark();
                    
                ModelView.OnItemBrowse += this.BookmarkModel_Browse;

                if (Formatters.IsBiblesKey(this.selectedKey))
                {
                    bookmark.SetVerse(this.selectedKey);
                }
                else
                {
                    bookmark.SetVerse($"{this.Bible.BibleId}||{this.selectedKey}");
                }

                if (studyHeaders.Count > 0)
                {
                    #region STUDY BOOKMARKS

                    StudyBookmarksModel studyMark = bookmark.CopyToObject(new StudyBookmarksModel()).To<StudyBookmarksModel>();

                    List<DataItemModel> studyOptions = new List<DataItemModel>();

                    studyOptions.Add(new DataItemModel { DisplayValue = $"<{this.Bible.BibleName}>", ItemKey = -1 });

                    foreach(KeyValuePair<int, StudyHeader> studyKey in studyHeaders)
                    {
                        studyOptions.Add(new DataItemModel { DisplayValue = studyKey.Value.StudyName, ItemKey = studyKey.Key });
                    }

                    studyMark.AvailableStudies = studyOptions.ToArray();

                    if (ModelView.ShowDialog("Bookmark", studyMark).IsFalse())
                    {
                        return;
                    }

                    if (studyMark.Study <= 0)
                    {
                        BookmarkModel dbModel = studyMark.CopyToObject(new BookmarkModel()).To<BookmarkModel>();

                        BiblesData.Database.InsertBookmarkModel(dbModel);
                    }
                    else
                    {
                        StudyBookmarkModel dbModel = studyMark.CopyToObject(new StudyBookmarkModel()).To<StudyBookmarkModel>();

                        dbModel.StudyName = studyMark.AvailableStudies.First(st => st.ItemKey.ToInt32() == studyMark.Study).DisplayValue;

                        dbModel.StudyVerseKey = $"{studyMark.Study}||{dbModel.VerseKey}";

                        BiblesData.Database.InsertStudyBookmarkModel(dbModel);

                        this.InvokeMethod(openStudies[studyMark.Study], "AddBookmark", new object[] { bookmark });
                    }

                    #endregion
                }
                else
                {
                    #region NORMAL BOOKMARKS
                    
                    if (ModelView.ShowDialog("Bookmark", bookmark).IsFalse())
                    {
                        return;
                    }

                    BookmarkModel dbModel = bookmark.CopyToObject(new BookmarkModel()).To<BookmarkModel>();

                    BiblesData.Database.InsertBookmarkModel(dbModel);

                    #endregion
                }

                BibleLoader.RefreshVerseNumberPanel
                    (
                        this.loadedVerseStackDictionary[selectedVerse],
                        this.Bible.BibleId,
                        this.versesDictionary[selectedVerse]
                    );
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
            finally
            {
                ModelView.OnItemBrowse -= this.BookmarkModel_Browse;
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

        private void LinkVerse_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedVerseKey.IsNullEmptyOrWhiteSpace() ||
                Formatters.GetVerseFromKey(this.SelectedVerseKey) <= 0)
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                Type linkType = Type.GetType("Bibles.Link.LinkEditor,Bibles.Link");

                object[] args = new object[]
                {
                    this.Bible.BibleId,
                    this.versesDictionary[Formatters.GetVerseFromKey(this.SelectedVerseKey)]
                };

                UserControlBase linkEditor = Activator.CreateInstance(linkType, args) as UserControlBase;

                string title = $"Link - {GlobalStaticData.Intance.GetKeyDescription(this.SelectedVerseKey)}";

                linkEditor.Height = this.Height;

                if (ControlDialog.ShowDialog(title, linkEditor, "AcceptLink", false).IsFalse())
                {
                    return;
                }

                // TODO: Create list on viewer to refresh this page

                int selectedVerse = Formatters.GetVerseFromKey(this.selectedKey);

                BibleLoader.RefreshVerseNumberPanel
                    (
                        this.loadedVerseStackDictionary[selectedVerse],
                        this.Bible.BibleId,
                        this.versesDictionary[selectedVerse]
                    );
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void RemoteLinkViewer_Closed(object sender, string verseKey)
        {
            try
            {
                int selectedVerse = Formatters.GetVerseFromKey(verseKey);

                BibleLoader.RefreshVerseNumberPanel
                    (
                        this.loadedVerseStackDictionary[selectedVerse],
                        this.Bible.BibleId,
                        this.versesDictionary[selectedVerse]
                    );
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void BackColour_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.selectedKey.IsNullEmptyOrWhiteSpace() 
                || Formatters.GetVerseFromKey(this.selectedKey) <= 0)
            {
                MessageDisplay.Show("Please select a Verse");

                return;
            }

            try
            {
                ColourPicker picker = new ColourPicker();

                if (picker.ShowDialog().IsFalse())
                {
                    return;
                }

                int verseNumber = Formatters.GetVerseFromKey(this.selectedKey);

                HighlightRitchTextBox textBox = this.loadedTextBoxDictionary[verseNumber];

                int start = textBox.GetSelectionStartIndex();

                int length = textBox.GetSelectedTextLength();

                textBox.HighlightText(start, length, picker.SelectedColour);

                string bibleVerseKey = Formatters.IsBiblesKey(this.selectedKey) ?
                    this.selectedKey
                    :
                    $"{this.Bible.BibleId}||{this.selectedKey}";


                BiblesData.Database.InsertVerseColour(bibleVerseKey, start, length, ColourConverters.GetHexFromBrush(picker.SelectedColour));
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void ClearBackColour_Clicked(object sender, RoutedEventArgs e)
        {
            if (this.selectedKey.IsNullEmptyOrWhiteSpace()
                || Formatters.GetVerseFromKey(this.selectedKey) <= 0)
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                int verseNumber = Formatters.GetVerseFromKey(this.selectedKey);

                HighlightRitchTextBox textBox = this.loadedTextBoxDictionary[verseNumber];

                int start = textBox.GetSelectionStartIndex();

                int length = textBox.GetSelectedTextLength();

                textBox.Text = this.versesDictionary[verseNumber].VerseText;

                string bibleVerseKey = Formatters.IsBiblesKey(this.selectedKey) ?
                    this.selectedKey
                    :
                    $"{this.Bible.BibleId}||{this.selectedKey}";


                BiblesData.Database.DeleteVerseColours(bibleVerseKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Left_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                int chapter = Formatters.GetChapterFromKey(this.selectedKey);

                --chapter;

                this.PageToChapter(chapter);

                this.BookChanged?.Invoke(this, this.selectedKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Right_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                int chapter = Formatters.GetChapterFromKey(this.selectedKey);

                ++chapter;

                this.PageToChapter(chapter);

                this.BookChanged?.Invoke(this, this.selectedKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Verse_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                HighlightRitchTextBox box = (HighlightRitchTextBox)sender;

                this.selectedKey = Formatters.RemoveBibleId(((BibleVerseModel)box.Tag).BibleVerseKey);

                this.SetHeader();

                this.SelectedVerseChanged?.Invoke(this, this.versesDictionary[Formatters.GetVerseFromKey(this.selectedKey)]);

                this.uxStrongs.VerseKey = this.selectedKey;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void StronsPin_Changed(object sender, bool isPined)
        {
            TabControlVertical item = (TabControlVertical)sender;

            this.uxColumn2.Width = new GridLength(item.ActualWidth, GridUnitType.Auto);

            if (isPined)
            {
                this.uxColumn1.Width = new GridLength(3);
            }
            else
            {
                this.uxColumn1.Width = new GridLength(0);
            }
        }
               
        private void SetHeader()
        {
            this.uxBible[0].Header = GlobalStaticData.Intance.GetKeyDescription(this.selectedKey);
        }

        private void LoadVerses()
        {
            this.versesDictionary = null;

            this.versesDictionary = Formatters.IsBiblesKey(this.selectedKey) ?
                BiblesData.Database.GetVerses(this.selectedKey)
                :
                BiblesData.Database.GetVerses($"{this.Bible.BibleId}||{this.selectedKey}");

            this.ResetversSetup();

            for (int verse = 1; verse <= this.versesDictionary.Count; ++verse)
            {
                BibleVerseModel item = this.versesDictionary[verse];

                StackPanel panel = BibleLoader.GetVerseNumberPanel(this.Bible.BibleId, item, 0);

                this.uxVerseGrid.Children.Add(panel);

                HighlightRitchTextBox textBox = BibleLoader.GetVerseAsTextBox(this.Bible.BibleId, item, 1);

                textBox.GotFocus += this.Verse_GotFocus;

                this.uxVerseGrid.Children.Add(textBox);

                this.loadedTextBoxDictionary.Add(verse, textBox);

                this.loadedVerseStackDictionary.Add(verse, panel);
            }
        }

        private void ResetversSetup()
        {
            this.loadedTextBoxDictionary.Clear();

            this.loadedVerseStackDictionary.Clear();

            this.uxVerseGrid.Children.Clear();

            this.uxVerseGrid.RowDefinitions.Clear();

            this.uxBookmark.IsEnabled = this.versesDictionary.Count > 0;

            this.uxLink.IsEnabled = this.versesDictionary.Count > 0;

            for (int x = 0; x < this.versesDictionary.Count; ++x)
            {
                this.uxVerseGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26, GridUnitType.Auto) });
            }
        }

        private void ScrollToVerse(int verseNumber)
        {
            if (verseNumber <= 0 || !this.loadedTextBoxDictionary.ContainsKey(verseNumber))
            {
                return;
            }

            if (!this.IsLoaded)
            {
                this.scrollOnLoadVerse = verseNumber;
            }

            HighlightRitchTextBox verseBox = this.loadedTextBoxDictionary[verseNumber];

            Point versePoint = verseBox.TranslatePoint(new Point(), this.uxVerseGrid);

            this.uxVerseGridScroll.ScrollToVerticalOffset(versePoint.Y);

            verseBox.Focus();
        }

        private void SetCanPage(int chapter)
        {
            if (!base.WasFirstLoaded)
            {
                return;
            }

            this.uxLeftButton.IsEnabled = chapter > 1;

            this.uxRightButton.IsEnabled = chapter < GlobalStaticData.Intance.GetChaptersCount(this.selectedKey);
        }

        private void PageToChapter(int chapter)
        {
            this.SetCanPage(chapter);

            string next = Formatters.ChangeChapter(this.selectedKey, chapter);

            next = Formatters.ChangeVerse(next, 1);

            this.selectedKey = next;

            this.SetChapter(next);                     

            this.uxVerseGridScroll.ScrollToTop();
        }
    }
}
