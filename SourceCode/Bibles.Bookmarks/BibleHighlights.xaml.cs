using Bibles.DataResources.Bookmarks;
using Bibles.DataResources;
using WPF.Tools.BaseClasses;
using GeneralExtensions;
using System.Linq;
using Bibles.DataResources.Aggregates;
using System;
using Bibles.Common;
using System.Windows;
using System.Collections.Generic;
using IconSet;
using WPF.Tools.Specialized;

namespace Bibles.Bookmarks
{
    /// <summary>
    /// Interaction logic for BibleHighlights.xaml
    /// </summary>
    public partial class BibleHighlights : UserControlBase
    {
        private readonly char[] keySplitChar = new  char[] { '*' };

        private BibleHighlightsModel selectedHighlight;

        private BibleHighlightsModel[] highlightModelsPage;

        public BibleHighlights()
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.LoadHighlights();
        }

        public BibleHighlightsModel SelectedHighlight 
        {
            get
            {
                return this.selectedHighlight;
            }

            set
            {
                this.selectedHighlight = value;

                base.OnPropertyChanged(() => this.SelectedHighlight);

                if (value == null)
                {
                    this.uxVerseText.Text = string.Empty;

                    return;
                }

                string[] keySplit = value.BibleVerseKeyId.Split(this.keySplitChar);

                BibleVerseModel verse = BiblesData.Database.GetVerse(keySplit[0]);

                this.uxVerseText.Text = verse.VerseText;

                List<HighlightVerseModel> verseColours = BiblesData.Database.GetVerseColours(verse.BibleVerseKey);

                foreach (HighlightVerseModel colour in verseColours)
                {
                    string[] itemSplit = colour.BibleVerseKeyId.Split(this.keySplitChar);

                    this.uxVerseText.HighlightText(itemSplit[1].ToInt32(), itemSplit[2].ToInt32(), ColourConverters.GetBrushfromHex(colour.HexColour));
                }
            }
        }
    
        public BibleHighlightsModel[] HighlightModels
        {
            get
            {
                return this.uxPager.ItemsSource.Items.TryCast<BibleHighlightsModel>();
            }

            set
            {
                this.uxPager.ItemsSource.Clear();

                this.uxPager.ItemsSource.AddRange(value);
            }
        }

        public BibleHighlightsModel[] HighlightModelsPage
        {
            get
            {
                return this.highlightModelsPage;
            }

            set
            {
                this.highlightModelsPage = value;

                base.OnPropertyChanged(() => this.HighlightModelsPage);
            }
        }

        public void LoadHighlights()
        {
            this.HighlightModels = BiblesData.Database.GetHighlights()
                .CopyToObject(typeof(BibleHighlightsModel))
                .TryCast<BibleHighlightsModel>()
                .ToArray();
        }

        private void Page_Changed(object sender, object[] selectedItems)
        {
            this.HighlightModelsPage = selectedItems.TryCast<BibleHighlightsModel>();
        }

        private void Delete_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedHighlight == null)
            {
                MessageDisplay.Show("Please select a Verse.");

                return;
            }

            try
            {
                string message = $"Are you sure you would like to delete {this.SelectedHighlight.Verse}?";

                if (MessageDisplay.Show(message, "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                BiblesData.Database.DeleteHighlight(this.SelectedHighlight.BibleVerseKeyId);

                this.uxPager.ItemsSource.Remove(this.SelectedHighlight);

                this.uxVerseText.Text = string.Empty;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
