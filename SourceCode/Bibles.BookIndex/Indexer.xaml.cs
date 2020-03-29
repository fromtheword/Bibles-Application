using Bibles.DataResources.BibleBooks;
using Bibles.Data;
using Bibles.Data.DataEnums;
using System.Linq;
using WPF.Tools.BaseClasses;
using GeneralExtensions;
using System.ComponentModel;
using System.Windows.Data;
using System;
using Bibles.Common;

namespace Bibles.BookIndex
{
    /// <summary>
    /// Interaction logic for Indexer.xaml
    /// </summary>
    public partial class Indexer : UserControlBase
    {
        public delegate void BookChangeEvent(object sender, string key);

        public delegate void ChapterChangedEvent(object sender, string key);

        public delegate void VerseChangedEvent(object sender, string key);

        public event BookChangeEvent BookChange;

        public event ChapterChangedEvent ChapterChanged;

        public event VerseChangedEvent VerseChanged;

        private BookModel selectedOldTetamentBook;

        private BookModel selectedNewTetamentBook;

        private ChapterModel selectedChapter;

        private VerseModel selectedVerse;

        private BookModel[] oldTestamentBooks;
        
        private BookModel[] newTestamentBooks;

        private ChapterModel[] bookChapters;

        private VerseModel[] chapterVerses;

        public Indexer()
        {
            this.InitializeComponent();

            this.DataContext = this;
            
            this.OldTestamentBooks = GlobalStaticData.Intance.GetTestament(TestamentEnum.O).ToArray();

            this.NewTestamentBooks = GlobalStaticData.Intance.GetTestament(TestamentEnum.N).ToArray();
        }

        public BookModel SelectedOldTestamentBook
        {
            get
            {
                return this.selectedOldTetamentBook;
            }

            set
            {
                if (value != null && this.SelectedNewTestamentBook != null)
                {
                    this.SelectedNewTestamentBook = null;
                }

                this.selectedOldTetamentBook = value;

                base.OnPropertyChanged(() => this.SelectedOldTestamentBook);

                this.BookChapters = value == null ? new ChapterModel[] { } : value.Chapters.Values.ToArray();

                if (value != null)
                {
                    this.BookChange?.Invoke(this, value.BookKey);
                }
            }
        }

        public BookModel SelectedNewTestamentBook
        {
            get
            {
                return this.selectedNewTetamentBook;
            }

            set
            {
                if (value != null && this.SelectedOldTestamentBook != null)
                {
                    this.SelectedOldTestamentBook = null;
                }

                this.selectedNewTetamentBook = value;

                base.OnPropertyChanged(() => this.SelectedNewTestamentBook);

                this.BookChapters = value == null ? new ChapterModel[] { } : value.Chapters.Values.ToArray();

                if (value != null)
                {
                    this.BookChange?.Invoke(this, value.BookKey);
                }
            }
        }

        public ChapterModel SelectedChapter
        {
            get
            {
                return this.selectedChapter;
            }

            set
            {
                this.selectedChapter = value;

                base.OnPropertyChanged(() => this.SelectedChapter);

                this.ChapterVerses = value == null ? new VerseModel[] { } : value.Verses.Values.ToArray();

                if (value != null)
                {
                    this.ChapterChanged?.Invoke(this, value.ChapterKey);
                }
            }
        }

        public VerseModel SelectedVerse
        {
            get
            {
                return this.selectedVerse;
            }

            set
            {
                this.selectedVerse = value;

                base.OnPropertyChanged(() => this.SelectedVerse);

                if (value != null)
                {
                    this.VerseChanged?.Invoke(this, value.VerseKey);
                }
            }
        }

        public BookModel[] OldTestamentBooks
        {
            get
            {
                return this.oldTestamentBooks;
            }

            set
            {
                this.oldTestamentBooks = value;

                base.OnPropertyChanged(() => this.OldTestamentBooks);
            }
        }
    
        public BookModel[] NewTestamentBooks
        {
            get
            {
                return this.newTestamentBooks;
            }

            set
            {
                this.newTestamentBooks = value;

                base.OnPropertyChanged(() =>  this.NewTestamentBooks);            }
        }    
    
        public ChapterModel[] BookChapters
        {
            get
            {
                return this.bookChapters;
            }

            set
            {
                this.bookChapters = value;

                base.OnPropertyChanged(() => this.BookChapters);

                this.SelectedChapter = value.HasElements() ? value[0] : null;
            }
        }
    
        public VerseModel[] ChapterVerses
        {
            get
            {
                return this.chapterVerses;
            }

            set
            {
                this.chapterVerses = value;

                base.OnPropertyChanged(() => this.ChapterVerses);
            }
        }

        private void SearchOldTestament_TextChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.OldTestamentBooks);

                string searchText = this.uxOldTestamentSearch.Text.ToLower();

                collectionView.Filter = f =>
                {
                    BookModel item = f as BookModel;

                    return item.BookName.ToLower().Contains(searchText);
                };
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void SearchNewTestament_TextChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                ICollectionView collectionView = CollectionViewSource.GetDefaultView(this.NewTestamentBooks);

                string searchText = this.uxNewTestamentSearch.Text.ToLower();

                collectionView.Filter = f =>
                {
                    BookModel item = f as BookModel;

                    return item.BookName.ToLower().Contains(searchText);
                };
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
