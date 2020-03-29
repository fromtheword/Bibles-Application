using Bibles.Common;
using GeneralExtensions;
using WPF.Tools.BaseClasses;

namespace Bibles.DataResources.Bookmarks
{
    public class BibleHighlightsModel : ModelsBase
    {
        private string bibleVerseKeyId;

        private string hexColour;

        public string BibleVerseKeyId
        {
            get
            {
                return this.bibleVerseKeyId;
            }

            set
            {
                this.bibleVerseKeyId = value;

                base.OnPropertyChanged(() => this.BibleVerseKeyId);
            }
        }

        public string Bible
        {
            get
            {
                int bibleId = Formatters.GetBibleFromKey(this.BibleVerseKeyId);

                return this.InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "GetBibleName", new object[] { bibleId })
                    .ParseToString();
            }
        }

        public string Verse
        {
            get
            {
                return this.InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "GetKeyDescription" , new object[] { this.BibleVerseKeyId })
                    .ParseToString();
            }
        }

        public string HexColour
        {
            get
            {
                return this.hexColour;
            }

            set
            {
                this.hexColour = value;

                base.OnPropertyChanged(() => this.HexColour);
            }
        }
    }
}
