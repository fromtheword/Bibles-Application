using Bibles.Common;
using Bibles.DataResources.Aggregates;
using System;
using System.Windows.Controls;
using WPF.Tools.BaseClasses;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for ParallelReader.xaml
    /// </summary>
    public partial class ParallelReader : UserControlBase
    {
        public ParallelReader()
        {
            this.InitializeComponent();
        }

        public void SetBible(int bibleId)
        {
            this.uxReaderLeft.SetBible(bibleId);
        }

        public void SetChapter(string key)
        {
            this.uxReaderLeft.SetChapter(key);

            this.uxReaderRight.SetChapter(key);
        }

        public void SetVerse(string key)
        {
            this.uxReaderLeft.SetVerse(key);

            this.uxReaderRight.SetVerse(key);
        }

        private void LeftScroll_Changed(object sender, int visibleVerse, ScrollChangedEventArgs e)
        {
            this.uxReaderRight.uxVerseGridScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void LeftBook_Changed(object sender, string chapterkey)
        {
            try
            {
                this.uxReaderRight.SetChapter(chapterkey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void LeftVerseChanged(object sender, BibleVerseModel verse)
        {
            try
            {
                this.uxReaderRight.SelectVerse(verse.BibleVerseKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void RightScroll_Changed(object sender, int visibleVerse, ScrollChangedEventArgs e)
        {
            this.uxReaderLeft.uxVerseGridScroll.ScrollToVerticalOffset(e.VerticalOffset);
        }

        private void RightBook_Changed(object sender, string chapterkey)
        {
            try
            {
                this.uxReaderLeft.SetChapter(chapterkey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void RightVerseChanged(object sender, BibleVerseModel verse)
        {
            try
            {
                this.uxReaderLeft.SelectVerse(verse.BibleVerseKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
