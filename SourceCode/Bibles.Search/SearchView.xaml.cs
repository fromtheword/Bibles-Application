using Bibles.Common;
using Bibles.Data;
using Bibles.DataResources;
using Bibles.DataResources.DataEnums;
using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WPF.Tools.BaseClasses;
using WPF.Tools.CommonControls;
using WPF.Tools.Exstention;
using WPF.Tools.Specialized;
using WPF.Tools.ToolModels;
using ViSo.Dialogs.Controls;
using Bibles.Studies;
using Bibles.Studies.Models;
using Bibles.DataResources.Bookmarks;
using ViSo.Dialogs.TextEditor;
using ViSo.Dialogs.ModelViewer;
using Bibles.DataResources.Models.Bookmarks;
using System.Linq;

namespace Bibles.Search
{
    /// <summary>
    /// Interaction logic for SearchView.xaml
    /// </summary>
    public partial class SearchView : UserControlBase
    {
        public delegate void SearchReaderRequestEvent(object sender, string verseKey);

        public event SearchReaderRequestEvent SearchReaderRequest;

        private int rowIndex = 0;

        private bool showBibleColumn = false;

        private string selectedKey;

        private string[] searchSplitResult = null;

        private Dictionary<int, string> bibleNames = new Dictionary<int, string>();

        public SearchView()
        {
            this.InitializeComponent();

            this.Initialize();
        }

        private void Search_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                List<BibleVerseModel> result = null;

                SearchComparisonEnum comparisonType = (SearchComparisonEnum)this.uxSearchComparison.SelectedValue;

                int bibleId = this.uxSearchInBible.SelectedValue.ToInt32();

                switch (comparisonType)
                {
                    case SearchComparisonEnum.AllOfTheWords:

                        result = BiblesData.Database.SearchAllOfTheWords(this.uxSearchText.Text, bibleId, out this.searchSplitResult);

                        break;

                    case SearchComparisonEnum.AnyOfTheWords:

                        result = BiblesData.Database.SearchAnyOfTheWords(this.uxSearchText.Text, bibleId, out this.searchSplitResult);

                        break;

                    case SearchComparisonEnum.Exact:

                        this.searchSplitResult = new string[] { this.uxSearchText.Text };

                        result = BiblesData.Database.SearchExact(this.uxSearchText.Text, bibleId);

                        break;

                    case SearchComparisonEnum.LikeThisPhrase:
                    default:

                        this.searchSplitResult = new string[] { this.uxSearchText.Text };

                        result = BiblesData.Database.SearchLikeThisPhrase(this.uxSearchText.Text, bibleId);

                        break;
                }

                this.uxSearchPager.ItemsSource.Clear();

                this.uxSearchPager.ItemsSource.AddRange(result.ToArray());
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void SearchInBible_Changed(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataItemModel selectedBible = e.AddedItems[0].To<DataItemModel>();

                this.showBibleColumn = selectedBible.ItemKey.ToInt32() <= 0;

                this.uxBibleColumn.Width = this.showBibleColumn ? new GridLength(150, GridUnitType.Auto) : new GridLength(0);

                this.ResetGrid();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void OpenBookmark_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.selectedKey.IsNullEmptyOrWhiteSpace())
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                this.SearchReaderRequest?.Invoke(this, this.selectedKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Bookmark_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.selectedKey.IsNullEmptyOrWhiteSpace())
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                this.BookmarkVerse();
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

        private void LinkVerse_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.selectedKey.IsNullEmptyOrWhiteSpace())
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                Type linkType = Type.GetType("Bibles.Link.LinkEditor,Bibles.Link");

                BibleVerseModel verseModel = this.uxSearchPager
                    .ItemsSource
                    .Items
                    .FirstOrDefault(vk => ((BibleVerseModel)vk).BibleVerseKey == this.selectedKey)
                    .To<BibleVerseModel>();

                object[] args = new object[]
                {
                    Formatters.GetBibleFromKey(this.selectedKey),
                    verseModel
                };

                UserControlBase linkEditor = Activator.CreateInstance(linkType, args) as UserControlBase;

                string title = $"Link - {GlobalStaticData.Intance.GetKeyDescription(this.selectedKey)}";

                linkEditor.Height = this.Height;

                if (ControlDialog.ShowDialog(title, linkEditor, "AcceptLink", false).IsFalse())
                {
                    return;
                }

                int selectedVerse = Formatters.GetVerseFromKey(this.selectedKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void SearchPage_Changed(object sender, object[] selectedItems)
        {
            this.ResetGrid();

            this.LoadSearchResults(selectedItems.TryCast<BibleVerseModel>());
        }

        private void Verse_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                HighlightRitchTextBox box = (HighlightRitchTextBox)sender;

                this.selectedKey = ((BibleVerseModel)box.Tag).BibleVerseKey;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private RowDefinition GetRowDefinition()
        {
            RowDefinition result = new RowDefinition();

            result.Height = new System.Windows.GridLength(16, GridUnitType.Auto);

            return result;
        }

        private void Initialize()
        {
            foreach (DataItemModel enumItem in Enum<SearchComparisonEnum>.ToDataItemsEnumKey())
            {
                this.uxSearchComparison.Items.Add(enumItem);
            }

            this.uxSearchComparison.SelectedItemKey = SearchComparisonEnum.LikeThisPhrase;

            Task<List<BibleModel>> biblesList = BiblesData.Database.GetBibles();

            this.uxSearchInBible.Items.Add(new DataItemModel { ItemKey = 0, DisplayValue = "<ALL>" });

            foreach (BibleModel bible in biblesList.Result)
            {
                DataItemModel bibleItem = new DataItemModel { ItemKey = bible.BiblesId, DisplayValue = bible.BibleName };

                this.uxSearchInBible.Items.Add(bibleItem);

                this.bibleNames.Add(bible.BiblesId, bible.BibleName);
            }

            this.uxSearchInBible.SelectedItemKey = GlobalResources.UserPreferences.DefaultBible;
        }

        private void ResetGrid()
        {
            this.rowIndex = 0;

            this.uxResultGrid.Children.Clear();

            this.uxResultGrid.RowDefinitions.Clear();

            this.uxResultGrid.RowDefinitions.Add(this.GetRowDefinition());

            LableItem bible = new LableItem { Content = "Bible", FontWeight = FontWeights.Bold };

            LableItem verse = new LableItem { Content = "Verse", FontWeight = FontWeights.Bold };

            LableItem text = new LableItem { Content = "Verse Text", FontWeight = FontWeights.Bold };

            this.SetUiElementPosition(bible, 0);

            this.SetUiElementPosition(verse, 1);

            this.SetUiElementPosition(text, 2);

            ++this.rowIndex;
        }

        private void LoadSearchResults(BibleVerseModel[] result)
        {
            foreach (BibleVerseModel row in result)
            {
                this.uxResultGrid.RowDefinitions.Add(this.GetRowDefinition());

                if (this.showBibleColumn)
                {
                    int bibleId = Formatters.GetBibleFromKey(row.BibleVerseKey);

                    LableItem bible = new LableItem { Content = this.bibleNames[bibleId] };

                    this.SetUiElementPosition(bible, 0);
                }

                LableItem verse = new LableItem { Content = GlobalStaticData.Intance.GetKeyDescription(row.BibleVerseKey) };

                this.SetUiElementPosition(verse, 1);

                HighlightRitchTextBox text = new HighlightRitchTextBox
                {
                    Text = row.VerseText,
                    Tag = row,
                    BorderBrush = Brushes.Transparent,
                    IsReadOnly = true,
                    Margin = new Thickness(2, 0, 0, 15)
                };

                text.GotFocus += this.Verse_GotFocus;

                text.HighlightText(this.searchSplitResult);

                this.SetUiElementPosition(text, 2);

                ++this.rowIndex;
            }
        }

        private void SetUiElementPosition(UIElement element, int columnIndex)
        {
            Grid.SetRow(element, this.rowIndex);

            Grid.SetColumn(element, columnIndex);

            this.uxResultGrid.Children.Add(element);
        }

        private void BookmarkVerse()
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
                
            bookmark.SetVerse(this.selectedKey);

            if (studyHeaders.Count > 0)
            {
                #region STUDY BOOKMARKS

                StudyBookmarksModel studyMark = bookmark.CopyToObject(new StudyBookmarksModel()).To<StudyBookmarksModel>();

                List<DataItemModel> studyOptions = new List<DataItemModel>();

                studyOptions.Add(new DataItemModel { DisplayValue = $"<Default>", ItemKey = -1 });

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
        }
    }
}
