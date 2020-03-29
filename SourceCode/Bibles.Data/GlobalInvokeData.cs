using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using WPF.Tools.ToolModels;

namespace Bibles.Data
{
    public class GlobalInvokeData
    {
        public string GetBibleName(int bibleId)
        {
            return BiblesData.Database.GetBibleName(bibleId);
        }

        public string GetKeyDescription(string unknownKey)
        {
            return GlobalStaticData.Intance.GetKeyDescription(unknownKey);
        }

        public int GetChapterVerseCount(string unknownKey)
        {
            return GlobalStaticData.Intance.GetChapterVerseCount(unknownKey);
        }

        public DataItemModel[] ListedBibles()
        {
            return GlobalResources.ListedBibles();
        }

        public BibleVerseModel GetVerse(string verseKey)
        {
            return BiblesData.Database.GetVerse(verseKey);
        }
    }
}
