using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibles.DataResources.Models.Strongs
{
    public class StrongsVerse
    {
        public string VerseKey { get; set; }

        public string StrongsReference { get; set; }

        public string ReferencedText { get; set; }

        public string VerseText { get; internal set; }

        public string VerseNumber { get; internal set; }
    }
}
