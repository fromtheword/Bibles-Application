using System.Collections.Generic;

namespace Bibles.DataResources.Link
{
    public class ModelsLink
    {
        public ModelsLink()
        {
            this.BibleVerseChildLinks = new List<ModelsLink>();
        }

        public string BibleVerseKey { get; set; }

        public string LinkKeyId { get; set; }

        public List<ModelsLink> BibleVerseChildLinks { get; set; }
    }
}
