using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.Models.VerseNotes;
using GeneralExtensions;
using System;
using System.Windows;
using ViSo.Dialogs.TextEditor;
using WPF.Tools.BaseClasses;
using WPF.Tools.Specialized;

namespace Bibles.Bookmarks
{
    /// <summary>
    /// Interaction logic for VersesNotes.xaml
    /// </summary>
    public partial class VersesNotes : UserControlBase
    {
        public delegate void VerseNoteGridModelRequestEvent(object sender, VerseNoteGridModel verseNote);

        public event VerseNoteGridModelRequestEvent VerseNoteGridModelRequest;

        private VerseNoteGridModel selectedNote;

        private VerseNoteGridModel[] notes;

        private VerseNoteGridModel[] gridNotes;

        public VersesNotes()
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.Notes = BiblesData.Database.GetAllVerseNotes().ToArray();
        }

        public VerseNoteGridModel SelectedNote
        {
            get
            {
                return this.selectedNote;
            }

            set
            {
                this.selectedNote = value;

                base.OnPropertyChanged(() => this.SelectedNote);

                if (value == null)
                {
                    return;
                }

                BibleVerseModel verseModel = BiblesData.Database.GetVerse(value.BibleVerseKey);

                VerseNotesModel verseNotes = BiblesData.Database.GetVerseNotes(value.BibleVerseKey);

                this.uxBibleVerse.Text = verseModel.VerseText;

                this.uxVerseNotes.Text = verseNotes.FootNote.UnzipFile().ParseToString();
            }
        }
    
        public VerseNoteGridModel[] Notes
        {
            get
            {
                return this.notes;
            }

            set
            {
                this.notes = value;

                this.uxNotesPager.ItemsSource.AddRange(value);
            }
        }

        public VerseNoteGridModel[] GridNotes
        {
            get
            {
                return this.gridNotes;
            }

            set
            {
                this.gridNotes = value;

                base.OnPropertyChanged(() => this.GridNotes);
            }
        }

        private void Page_Changed(object sender, object[] selectedItems)
        {
            this.GridNotes = selectedItems.TryCast<VerseNoteGridModel>();
        }

        private void OpenVers_Cliked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedNote == null)
            {
                MessageDisplay.Show("Please select a Verse Note");

                return;
            }

            try
            {
                this.VerseNoteGridModelRequest?.Invoke(this, this.SelectedNote);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void EditVerse_Cliked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedNote == null)
            {
                MessageDisplay.Show("Please select a Verse Note");

                return;
            }

            try
            {
                if (TextEditing.ShowDialog(this.SelectedNote.Verse, this.uxVerseNotes.Text).IsFalse())
                {
                    return;
                }

                if (TextEditing.Text.IsNullEmptyOrWhiteSpace())
                {
                    this.DeleteSelectedVerse();

                    return;
                }

                BiblesData.Database.InsertVerseNote(new VerseNotesModel { BibleVerseKey = this.SelectedNote.BibleVerseKey, FootNote = TextEditing.Text.ZipFile() });

                this.uxVerseNotes.Text = TextEditing.Text;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void DeleteVerse_Cliked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedNote == null)
            {
                MessageDisplay.Show("Please select a Verse Note");

                return;
            }

            try
            {
                this.DeleteSelectedVerse();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void DeleteSelectedVerse()
        {
            string message = $"Are you sure you would like to delete notes for {this.SelectedNote.Verse}?";

            if (MessageDisplay.Show(message, "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            BiblesData.Database.DeleteVerseNote(this.SelectedNote.BibleVerseKey);

            this.uxNotesPager.RemoveItem(this.SelectedNote);

            this.SelectedNote = null;

            this.uxBibleVerse.Text = string.Empty;

            this.uxVerseNotes.Text = string.Empty;
        }
    }
}
