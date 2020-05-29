using WPF.Tools.BaseClasses;

namespace Bibles.DataResources.Models.VerseNotes
{
    public class VerseNoteGridModel : ModelsBase
    {
        private string bibleVerseKey;

        private string bible;

        private string verse;

        public string BibleVerseKey
        {
            get
            {
                return this.bibleVerseKey;
            }

            set
            {
                this.bibleVerseKey = value;

                base.OnPropertyChanged("BibleVerseKey");
            }
        }
    
        public string Bible
        {
            get
            {
                return this.bible;
            }

            set
            {
                this.bible = value;

                base.OnPropertyChanged("Bible");
            }
        }
    
        public string Verse
        {
            get
            {
                return this.verse;
            }

            set
            {
                this.verse = value;

                base.OnPropertyChanged("Verse");
            }
        }
    }
}
