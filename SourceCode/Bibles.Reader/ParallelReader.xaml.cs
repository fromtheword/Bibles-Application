using Bibles.Common;
using Bibles.Data;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.AvailableBooks;
using Bibles.DataResources.Bookmarks;
using Bibles.DataResources.Models.Bookmarks;
using Bibles.Studies;
using Bibles.Studies.Models;
using GeneralExtensions;
using IconSet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using ViSo.Dialogs.Controls;
using ViSo.Dialogs.ModelViewer;
using ViSo.Dialogs.TextEditor;
using WPF.Tools.BaseClasses;
using WPF.Tools.ColoutPicker;
using WPF.Tools.Specialized;
using WPF.Tools.ToolModels;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for ParallelReader.xaml
    /// </summary>
    public partial class ParallelReader : UserControlBase
    {
        public delegate void BibleChangedEvent(object sender, ModelsBibleBook bible);

        public delegate void BookChangedEvent(object sender, string chapterkey);

        public delegate void SelectedVerseChangedEvent(object sender, BibleVerseModel verse);

        public event BibleChangedEvent BibleChanged;

        public event BookChangedEvent BookChanged;

        public event SelectedVerseChangedEvent SelectedVerseChanged;

        private int scrollOnLoadVerse = -1;

        private int selectedBibleId;

        private string selectedKey;

        private ParalelleSideEnum SelectedSide;

        private Dictionary<int, BibleVerseModel> versesDictionaryLeft;

        private Dictionary<int, BibleVerseModel> versesDictionaryRight;

        private Dictionary<int, HighlightRitchTextBox> loadedTextBoxDictionaryLeft = new Dictionary<int, HighlightRitchTextBox>();

        private Dictionary<int, HighlightRitchTextBox> loadedTextBoxDictionaryRight = new Dictionary<int, HighlightRitchTextBox>();

        private Dictionary<int, StackPanel> loadedVerseStackDictionaryLeft = new Dictionary<int, StackPanel>();

        private Dictionary<int, StackPanel> loadedVerseStackDictionaryRight = new Dictionary<int, StackPanel>();

        public ParallelReader()
        {
            this.InitializeComponent();

            this.ReaderKey = Guid.NewGuid();

            this.Loaded += this.Reader_Loaded;

            this.BibleLeft = new ModelsBibleBook();

            this.BibleRight = new ModelsBibleBook();

            this.BibleLeft.PropertyChanged += this.Bible_Changed;

            this.BibleRight.PropertyChanged += this.Bible_Changed;

            BibleLoader.LinkViewerClosed += this.RemoteLinkViewer_Closed;

            this.uxBibleLeft.Items.Add(this.BibleLeft);

            this.uxBibleRight.Items.Add(this.BibleRight);

            this.uxBibleRight[0].HideHeader(true);
        }

        ~ParallelReader()
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

        public Guid ReaderKey { get; private set; }

        public ModelsBibleBook BibleLeft { get; set; }

        public ModelsBibleBook BibleRight { get; set; }

        public void SetBible(int bibleId)
        {
            this.uxBibleLeft[0, 0].SetValue(bibleId);
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

            if (this.versesDictionaryLeft == null || this.versesDictionaryLeft.Count == 0)
            {
                this.LoadVerses();
            }

            if (Formatters.IsBiblesKey(key))
            {
                this.ScrollToVerse(Formatters.GetVerseFromKey(key));
            }
            else
            {
                this.ScrollToVerse(Formatters.GetVerseFromKey($"{this.BibleLeft.BibleId}||{key}"));
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
            switch (e.PropertyName)
            {
                case "BibleName":

                    this.BibleChanged?.Invoke(this, this.BibleLeft);

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
            if (this.SelectedSide == ParalelleSideEnum.None)
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

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
                    
                bookmark.SetVerse($"{this.selectedBibleId}||{Formatters.RemoveBibleId(this.selectedKey)}");
                
                if (studyHeaders.Count > 0)
                {
                    #region STUDY BOOKMARKS

                    StudyBookmarksModel studyMark = bookmark.CopyToObject(new StudyBookmarksModel()).To<StudyBookmarksModel>();

                    List<DataItemModel> studyOptions = new List<DataItemModel>();

                    studyOptions.Add(new DataItemModel { DisplayValue = $"<{(this.SelectedSide == ParalelleSideEnum.Left ? this.BibleLeft.BibleName : this.BibleRight.BibleName)}>", ItemKey = -1 });

                    foreach (KeyValuePair<int, StudyHeader> studyKey in studyHeaders)
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

                        bookmark = dbModel.CopyToObject(bookmark).To<ModelsBookmark>();

                        bookmark.SetVerse(bookmark.VerseKey);

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
                        this.loadedVerseStackDictionaryLeft[selectedVerse],
                        this.BibleLeft.BibleId,
                        this.versesDictionaryLeft[selectedVerse]
                    );

                BibleLoader.RefreshVerseNumberPanel
                    (
                        this.loadedVerseStackDictionaryRight[selectedVerse],
                        this.BibleRight.BibleId,
                        this.versesDictionaryRight[selectedVerse]
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

        private void Notes_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                string bibleVerseKey = this.SelectedVerseKey;

                if (!Formatters.IsBiblesKey(bibleVerseKey))
                {
                    bibleVerseKey = $"{this.selectedBibleId}||{bibleVerseKey}";
                }

                VerseNotesModel noteModel = BiblesData.Database.GetVerseNotes(bibleVerseKey);

                if (noteModel == null)
                {
                    noteModel = new VerseNotesModel { BibleVerseKey = bibleVerseKey, FootNote = new byte[] { } };
                }

                string footNotes = noteModel.FootNote.UnzipFile().ParseToString();

                if (TextEditing.ShowDialog(GlobalStaticData.Intance.GetKeyDescription(this.SelectedVerseKey), footNotes).IsFalse())
                {
                    return;
                }

                noteModel.FootNote = TextEditing.Text.ZipFile();

                BiblesData.Database.InsertVerseNote(noteModel);

                int selectedVerse = Formatters.GetVerseFromKey(this.selectedKey);

                BibleLoader.RefreshVerseNumberPanel
                   (
                       this.loadedVerseStackDictionaryLeft[selectedVerse],
                       this.BibleLeft.BibleId,
                       this.versesDictionaryLeft[selectedVerse]
                   );

                BibleLoader.RefreshVerseNumberPanel
                    (
                        this.loadedVerseStackDictionaryRight[selectedVerse],
                        this.BibleRight.BibleId,
                        this.versesDictionaryRight[selectedVerse]
                    );
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void LinkVerse_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedVerseKey.IsNullEmptyOrWhiteSpace()
                || Formatters.GetVerseFromKey(this.SelectedVerseKey) <= 0
                || this.SelectedSide == ParalelleSideEnum.None)
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                Type linkType = Type.GetType("Bibles.Link.LinkEditor,Bibles.Link");

                object[] args = new object[]
                {
                    this.selectedBibleId,
                    this.SelectedSide == ParalelleSideEnum.Left ?
                        this.versesDictionaryLeft[Formatters.GetVerseFromKey(this.SelectedVerseKey)]
                        :
                        this.versesDictionaryRight[Formatters.GetVerseFromKey(this.SelectedVerseKey)]
                };

                UserControlBase linkEditor = Activator.CreateInstance(linkType, args) as UserControlBase;

                string title = $"Link - {GlobalStaticData.Intance.GetKeyDescription(this.SelectedVerseKey)}";

                linkEditor.Height = this.Height;

                if (ControlDialog.ShowDialog(title, linkEditor, "AcceptLink", false).IsFalse())
                {
                    return;
                }

                int selectedVerse = Formatters.GetVerseFromKey(this.selectedKey);

                BibleLoader.RefreshVerseNumberPanel
                    (
                        this.loadedVerseStackDictionaryLeft[selectedVerse],
                        this.BibleLeft.BibleId,
                        this.versesDictionaryLeft[selectedVerse]
                    );

                BibleLoader.RefreshVerseNumberPanel
                    (
                        this.loadedVerseStackDictionaryRight[selectedVerse],
                        this.BibleRight.BibleId,
                        this.versesDictionaryRight[selectedVerse]
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
                        this.loadedVerseStackDictionaryLeft[selectedVerse],
                        this.BibleLeft.BibleId,
                        this.versesDictionaryLeft[selectedVerse]
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
                || Formatters.GetVerseFromKey(this.selectedKey) <= 0
                || this.SelectedSide == ParalelleSideEnum.None)
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

                HighlightRitchTextBox textBox = this.SelectedSide == ParalelleSideEnum.Left ? this.loadedTextBoxDictionaryLeft[verseNumber] : this.loadedTextBoxDictionaryRight[verseNumber];

                int start = textBox.GetSelectionStartIndex();

                int length = textBox.GetSelectedTextLength();

                textBox.HighlightText(start, length, picker.SelectedColour);

                string bibleVerseKey = $"{this.selectedBibleId}||{Formatters.RemoveBibleId(this.selectedKey)}";

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
                || Formatters.GetVerseFromKey(this.selectedKey) <= 0
                || this.SelectedSide == ParalelleSideEnum.None)
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                int verseNumber = Formatters.GetVerseFromKey(this.selectedKey);

                HighlightRitchTextBox textBox = this.SelectedSide == ParalelleSideEnum.Left ? this.loadedTextBoxDictionaryLeft[verseNumber] : this.loadedTextBoxDictionaryRight[verseNumber];

                int start = textBox.GetSelectionStartIndex();

                int length = textBox.GetSelectedTextLength();

                textBox.Text = this.SelectedSide == ParalelleSideEnum.Left ? this.versesDictionaryLeft[verseNumber].VerseText : this.versesDictionaryRight[verseNumber].VerseText;

                string bibleVerseKey = $"{this.selectedBibleId}||{Formatters.RemoveBibleId(this.selectedKey)}";

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

                if (this.versesDictionaryLeft.ContainsKey(Formatters.GetVerseFromKey(this.selectedKey)))
                {
                    this.SelectedVerseChanged?.Invoke(this, this.versesDictionaryLeft[Formatters.GetVerseFromKey(this.selectedKey)]);
                }
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

                if (this.versesDictionaryLeft.ContainsKey(Formatters.GetVerseFromKey(this.selectedKey)))
                {
                    this.SelectedVerseChanged?.Invoke(this, this.versesDictionaryLeft[Formatters.GetVerseFromKey(this.selectedKey)]);
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void VerseLeft_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SelectedSide = ParalelleSideEnum.Left;

                HighlightRitchTextBox box = (HighlightRitchTextBox)sender;

                string bibleVerseKey = ((BibleVerseModel)box.Tag).BibleVerseKey;

                this.selectedKey = Formatters.RemoveBibleId(bibleVerseKey);

                this.selectedBibleId = Formatters.GetBibleFromKey(bibleVerseKey);

                this.SetHeader();

                this.SelectedVerseChanged?.Invoke(this, this.versesDictionaryLeft[Formatters.GetVerseFromKey(this.selectedKey)]);
            }
            catch (Exception err)
            {
                this.SelectedSide = ParalelleSideEnum.None;

                ErrorLog.ShowError(err);
            }
        }

        private void VerseRight_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.SelectedSide = ParalelleSideEnum.Right;

                HighlightRitchTextBox box = (HighlightRitchTextBox)sender;

                string bibleVerseKey = ((BibleVerseModel)box.Tag).BibleVerseKey;

                this.selectedKey = Formatters.RemoveBibleId(bibleVerseKey);

                this.selectedBibleId = Formatters.GetBibleFromKey(bibleVerseKey);

                this.SetHeader();

                this.SelectedVerseChanged?.Invoke(this, this.versesDictionaryRight[Formatters.GetVerseFromKey(this.selectedKey)]);
            }
            catch (Exception err)
            {
                this.SelectedSide = ParalelleSideEnum.None;

                ErrorLog.ShowError(err);
            }
        }

        private void SetHeader()
        {
            this.uxBibleLeft[0].Header = GlobalStaticData.Intance.GetKeyDescription(this.selectedKey);
        }

        private void LoadVerses()
        {
            this.versesDictionaryLeft = null;

            this.versesDictionaryRight = null;

            string bibleKeyRemoved = Formatters.RemoveBibleId(this.selectedKey);

            this.versesDictionaryLeft = BiblesData.Database.GetVerses($"{this.BibleLeft.BibleId}||{bibleKeyRemoved}");

            this.versesDictionaryRight = BiblesData.Database.GetVerses($"{this.BibleRight.BibleId}||{bibleKeyRemoved}");

            this.ResetversSetup();
            
            #region LOAD LEFTSIDE

            for (int verse = 1; verse <= this.versesDictionaryLeft.Count; ++verse)
            {
                BibleVerseModel item = this.versesDictionaryLeft[verse];

                StackPanel panel = BibleLoader.GetVerseNumberPanel(this.BibleLeft.BibleId, item, 0);

                this.uxVerseGrid.Children.Add(panel);

                HighlightRitchTextBox textBox = BibleLoader.GetVerseAsTextBox(this.BibleLeft.BibleId, item, 1);

                textBox.GotFocus += this.VerseLeft_GotFocus;

                this.uxVerseGrid.Children.Add(textBox);

                this.loadedTextBoxDictionaryLeft.Add(verse, textBox);

                this.loadedVerseStackDictionaryLeft.Add(verse, panel);
            }

            #endregion

            #region LOAD RIGHT

            for (int verse = 1; verse <= this.versesDictionaryRight.Count; ++verse)
            {
                BibleVerseModel item = this.versesDictionaryRight[verse];

                StackPanel panel = BibleLoader.GetVerseNumberPanel(this.BibleRight.BibleId, item, 2);

                this.uxVerseGrid.Children.Add(panel);

                HighlightRitchTextBox textBox = BibleLoader.GetVerseAsTextBox(this.BibleLeft.BibleId, item, 3);

                textBox.GotFocus += this.VerseRight_GotFocus;

                this.uxVerseGrid.Children.Add(textBox);

                this.loadedTextBoxDictionaryRight.Add(verse, textBox);

                this.loadedVerseStackDictionaryRight.Add(verse, panel);
            }

            #endregion
        }

        private void ResetversSetup()
        {
            this.loadedTextBoxDictionaryLeft.Clear();

            this.loadedTextBoxDictionaryRight.Clear();

            this.loadedVerseStackDictionaryLeft.Clear();

            this.loadedVerseStackDictionaryRight.Clear();

            this.uxVerseGrid.Children.Clear();

            this.uxVerseGrid.RowDefinitions.Clear();

            this.uxBookmark.IsEnabled = this.versesDictionaryLeft.Count > 0;

            this.uxNotes.IsEnabled = this.versesDictionaryLeft.Count > 0;

            this.uxLink.IsEnabled = this.versesDictionaryLeft.Count > 0;

            int rowCount = this.versesDictionaryLeft.Count > this.versesDictionaryRight.Count ?
                this.versesDictionaryLeft.Count
                :
                this.versesDictionaryRight.Count;

            for (int x = 0; x < rowCount; ++x)
            {
                this.uxVerseGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26, GridUnitType.Auto) });
            }
        }

        private void ScrollToVerse(int verseNumber)
        {
            if (verseNumber <= 0 || !this.loadedTextBoxDictionaryLeft.ContainsKey(verseNumber))
            {
                return;
            }

            if (!this.IsLoaded)
            {
                this.scrollOnLoadVerse = verseNumber;
            }

            HighlightRitchTextBox verseBox = this.loadedTextBoxDictionaryLeft[verseNumber];

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
