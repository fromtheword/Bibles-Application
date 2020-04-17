using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Models.Strongs;
using System;
using System.Collections.Generic;
using System.Windows;
using WPF.Tools.BaseClasses;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for StrongsVerses.xaml
    /// </summary>
    public partial class StrongsVerses : UserControlBase
    {
        private string selectedVerseKey;

        private StrongsVerse selectedVerse;

        private StrongsVerse[] verses;

        public StrongsVerses(int bibleId, string strongsNumber, string verseKey)
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.BibleId = bibleId;

            this.StrongsNumber = strongsNumber;

            this.selectedVerseKey = verseKey;

            this.InitializeVerses();
        }

        public int BibleId { get; set; }

        public string StrongsNumber { get; set; }

        public StrongsVerse SelectedVerse
        {
            get
            {
                return this.selectedVerse;
            }

            set
            {
                this.selectedVerse = value;

                base.OnPropertyChanged(() => this.SelectedVerse);

                this.uxBibleVerse.Text = value == null ? string.Empty : value.VerseText;

                this.uxBibleVerse.HighlightText(value.ReferencedText);
            }
        }

        public StrongsVerse[] Verses
        {
            get
            {
                return this.verses;
            }

            set
            {
                this.verses = value;

                base.OnPropertyChanged(() => this.Verses);
            }
        }

        private void OpenVers_Cliked(object sender, RoutedEventArgs e)
        {
            try
            {

            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void InitializeVerses()
        {
            try
            {
                this.Verses = BiblesData.Database.GetStrongsVerseReferences(this.BibleId, this.StrongsNumber, this.selectedVerseKey).ToArray();

                this.uxOccurrences.Content = $"Occurrences: {this.Verses.Length}";
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
