using Bibles.BookIndex;
using Bibles.Bookmarks;
using Bibles.Common;
using Bibles.Data;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.AvailableBooks;
using Bibles.DataResources.Bookmarks;
using Bibles.DataResources.Models.Preferences;
using Bibles.Downloads;
using Bibles.Reader;
using Bibles.Search;
using Bibles.Setup;
using Bibles.Studies;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ViSo.Dialogs.Controls;
using ViSo.Dialogs.ModelViewer;
using WPF.Tools.BaseClasses;
using WPF.Tools.Dictionaries;
using WPF.Tools.Specialized;
using WPF.Tools.TabControl;
using WPF.Tools.ToolModels;

namespace Bibles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : WindowBase
    {
        private string selectedItemKey = string.Empty;
        
        private Indexer uxIndexer = new Indexer();

        public MainWindow()
        {            
            this.InitializeComponent();

            foreach (MenuItem item in this.uxMainMenu.Items)
            {
                this.SetMenuTranslation(item);
            }

            this.uxMessageLable.Content = "Loading…";
            
            this.Closing += this.MainWindow_Closing;
            
            InitializeData initialData = new InitializeData();

            initialData.InitialDataLoadCompleted += this.InitialDataLoad_Completed;

            initialData.LoadEmbeddedBibles(this.Dispatcher, Application.Current.MainWindow.FontFamily);
        }

        public void LoadReader(int bibleId, string verseKey)
        {
            Reader.Reader reader = this.CreateReader(true);

            this.uxMainTab.Items.Add(reader);

            reader.SetBible(bibleId);

            if (!verseKey.IsNullEmptyOrWhiteSpace())
            {
                string bibleKey = Formatters.ChangeBible(verseKey, bibleId);

                reader.SetChapter(bibleKey);

                reader.SetVerse(bibleKey);
            }
        }

        #region MAIN WINDOW EVENTS

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                UserPreferenceModel preference = GlobalResources.UserPreferences;

                this.SetFont(preference.Font);

                this.SetFontSize(preference.FontSize);
              
                string biblesKey = ((Reader.Reader)this.uxMainTab.Items[0]).SelectedVerseKey;

                if (!Formatters.IsBiblesKey(biblesKey))
                {
                    int biblesId = ((Reader.Reader)this.uxMainTab.Items[0]).Bible.BibleId;

                    biblesKey = Formatters.ChangeBible(biblesKey, biblesId);
                }

                preference.LastReadVerse = biblesKey;

                BiblesData.Database.InsertPreference(preference);
            }
            catch (Exception err)
            {
                // DO NOTHING
            }
        }

        private void InitialDataLoad_Completed(object sender, string message, bool completed, Exception error)
        {
            try
            {
                if (error != null)
                {
                    throw error;
                }

                if (completed)
                {
                    this.uxMessageLable.Content = string.Empty;

                    this.InitializeTabs();

                    this.LoadDynamicMenus();

                    UserPreferenceModel preference = GlobalResources.UserPreferences;

                    this.SetFont(preference.Font);

                    this.SetFontSize(preference.FontSize);
                    
                    this.selectedItemKey = GlobalResources.UserPreferences.LastReadVerse;

                    int bibleId = !this.selectedItemKey.IsNullEmptyOrWhiteSpace() && Formatters.IsBiblesKey(this.selectedItemKey) ?
                        Formatters.GetBibleFromKey(this.selectedItemKey)
                        :
                        GlobalResources.UserPreferences.DefaultBible;
                    
                    ((Reader.Reader)this.uxMainTab.Items[0]).SetBible(bibleId);

                    ((Reader.Reader)this.uxMainTab.Items[0]).SetVerse(this.selectedItemKey);
                }
                else
                {
                    this.uxMessageLable.Content = message;
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void SeletedChapter_Changed(object sender, string key)
        {
            try
            {
                UserControlBase item = this.uxMainTab.Items[this.uxMainTab.SelectedIndex];

                item.InvokeMethod(item, "SetChapter", new object[] { key }, false);

                this.selectedItemKey = key;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void SelectedVerse_Changed(object sender, string key)
        {
            try
            {
                UserControlBase item = this.uxMainTab.Items[this.uxMainTab.SelectedIndex];

                item.InvokeMethod(item, "SetVerse", new object[] { key }, false);

                this.selectedItemKey = key;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void OnSelectedTabBible_Changed(object sender, ModelsBibleBook bible)
        {
            try
            {
                this.uxMainTab.SetHeaderName(this.uxMainTab.SelectedIndex, bible.BibleName);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void OnReaderSelectedVerse_Changed(object sender, BibleVerseModel verse)
        {
            try
            {
                this.selectedItemKey = verse.BibleVerseKey;

                if (GlobalResources.UserPreferences.SynchronizzeTabs)
                {

                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void LeftTabPin_Changed(object sender, bool isPined)
        {
            TabControlVertical item = (TabControlVertical)sender;

            this.uxMaingrid.ColumnDefinitions[0].Width = new GridLength(item.ActualWidth, GridUnitType.Auto);

            if (isPined)
            {
                this.uxMaingrid.ColumnDefinitions[1].Width = new GridLength(3);
            }
            else
            {
                this.uxMaingrid.ColumnDefinitions[1].Width = new GridLength(0);
            }
        }

        #endregion

        #region MENU ITEM EVENTS

        private void Exit_Cliked(object sender, RoutedEventArgs e)
        {
            this.Dispatcher.InvokeShutdown();
        }

        private void MenuBiblesItem_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = (MenuItem)sender;

                this.LoadReader(item.Tag.ToInt32(), this.selectedItemKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
   
        private void Bookmarks_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                BookmarksList bookmarks = new BookmarksList();

                bookmarks.BookmarkReaderRequest += this.BookmarkReader_Request;

                ControlDialog.Show("Bookmarks", bookmarks, string.Empty, owner:this, isTopMost:true, autoSize:false);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Search_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                SearchView search = new SearchView();

                search.SearchReaderRequest += this.SearchReader_Request;

                ControlDialog.Show("Search", search, string.Empty, owner:this, showCancelButton:false, autoSize:false);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Highlights_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                BibleHighlights highlight = new BibleHighlights();

                ControlDialog.Show("Highlights", highlight, string.Empty, showCancelButton:false, autoSize:false);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err); ;
            }
        }

        private void BookmarkReader_Request(object sender, ModelsBookmark bookmark)
        {
            try
            {
                this.LoadReader(Formatters.GetBibleFromKey(bookmark.VerseKey), bookmark.VerseKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void SearchReader_Request(object sender, string verseKey)
        {
            try
            {
                this.LoadReader(Formatters.GetBibleFromKey(verseKey), verseKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void ParallelReader_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                int bibleId = Formatters.GetBibleFromKey(this.selectedItemKey);

                if (bibleId <= 0)
                {
                    bibleId = GlobalResources.UserPreferences.DefaultBible;
                }

                ParallelReader reader = new ParallelReader { ShowCloseButton = true };

                reader.SetBible(bibleId);

                if (!this.selectedItemKey.IsNullEmptyOrWhiteSpace())
                {
                    reader.SetChapter(this.selectedItemKey);

                    reader.SetVerse(this.selectedItemKey);
                }

                this.uxMainTab.Items.Add(reader);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void NewStudy_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                EditStudy study = new EditStudy();

                ControlDialog.Show("New Study", study, "SaveStudy", owner:this, autoSize:false);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void OpenStudy_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                StudiesByCategory study = new StudiesByCategory();

                ControlDialog.ShowDialog("Open Study", study, string.Empty, autoSize:false, showOkButton:false);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void UserPreferences_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                UserPreference userPreferences = GlobalResources.UserPreferences.CopyToObject(new UserPreference()).To<UserPreference>();

                userPreferences.PropertyChanged += this.UserPreference_Changed;

                if (ModelView.ShowDialog("User Preferences", userPreferences).IsFalse())
                {
                    // Do this to reset the Font values
                    userPreferences = GlobalResources.UserPreferences.CopyToObject(new UserPreference()).To<UserPreference>();

                    return;
                }

                UserPreferenceModel updatePreference = userPreferences.CopyToObject(new UserPreferenceModel()).To<UserPreferenceModel>();

                BiblesData.Database.InsertPreference(updatePreference);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void UserPreference_Changed(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                UserPreference preference = (UserPreference)sender;

                switch (e.PropertyName)
                {
                    case "Font":

                        this.SetFont(preference.Font);
                        
                        break;

                    case "FontSize":

                        this.SetFontSize(preference.FontSize);
                        
                        break;
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void LanguageSetup_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                TranslationSetup setup = new TranslationSetup();

                //if (ControlDialog.ShowDialog("Translation Setup", setup, "SaveSetup", autoSize:false).IsFalse())
                ControlDialog.Show("Translation Setup", setup, "SaveSetup", owner: this, autoSize: false);
            }
            catch(Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Downloads_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                DownloadsView down = new DownloadsView();

                if (ControlDialog.ShowDialog("Downloads", down, "DownLoad", autoSize:false).IsFalse())
                {
                    return;
                }

            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void OnlineResources_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {
                OnlineResources resources = new OnlineResources();

                ControlDialog.Show("Resource Centre", resources, string.Empty, owner: this, autoSize:false);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void About_Cliked(object sender, RoutedEventArgs e)
        {
            About about = new About();

            ControlDialog.ShowDialog("About", about, string.Empty, showCancelButton: false);
        }

        #endregion

        private void SetMenuTranslation(MenuItem item)
        {
            item.Header = TranslationDictionary.Translate(item.Header.ParseToString());
            
            if (item.Items != null)
            {
                foreach (object child in item.Items)
                {
                    if (child.GetType() != typeof(MenuItem))
                    {
                        continue;
                    }

                    this.SetMenuTranslation((MenuItem)child);
                }
            }
        }

        private void SetFont(string fontName)
        {
            FontFamilyConverter fontConverter = new FontFamilyConverter();

            FontFamily font = fontConverter.ConvertFromString(fontName).To<FontFamily>();

            this.uxMainMenu.FontFamily = font;

            foreach (Window window in Application.Current.Windows)
            {
                window.FontFamily = font;
            }
        }

        private void SetFontSize(int size)
        {
            this.uxMainMenu.FontSize = size;

            foreach (Window window in Application.Current.Windows)
            {
                window.FontSize = size;
            }
        }

        private void LoadDynamicMenus()
        {
            Task<List<BibleModel>> biblesTask = BiblesData.Database.GetBibles();

            foreach (BibleModel bible in biblesTask.Result.OrderBy(n => n.BibleName))
            {
                MenuItem item = new MenuItem { Header = bible.BibleName, Tag = bible.BiblesId };

                //item.FontSize

                item.Click += this.MenuBiblesItem_Clicked;

                this.uxMenuBiles.Items.Add(item);
            }
        }

        private void InitializeTabs()
        {
            this.uxLeftTab.Items.Add(this.uxIndexer);

            this.uxIndexer.ChapterChanged += this.SeletedChapter_Changed;

            this.uxIndexer.VerseChanged += this.SelectedVerse_Changed;

            Reader.Reader reader = this.CreateReader(false);

            this.uxMainTab.Items.Add(reader);
        }

        private Reader.Reader CreateReader(bool showCloseButton)
        {
            Reader.Reader reader = new Reader.Reader { ShowCloseButton = showCloseButton };

            reader.BibleChanged += this.OnSelectedTabBible_Changed;

            reader.SelectedVerseChanged += this.OnReaderSelectedVerse_Changed;

            return reader;
        }
    }
}
