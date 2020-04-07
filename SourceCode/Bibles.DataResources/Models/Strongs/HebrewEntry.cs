using WPF.Tools.Attributes;

namespace Bibles.DataResources.Models.Strongs
{
    [ModelNameAttribute("Concordance")]
    public class HebrewEntry
    {
        [FieldInformationAttribute("Strong's", IsReadOnly = true, Sort = 1)]
        [BrowseButton("StrongsNumberKey", "", "Search")]
        public string StrongsNumber { get; set; }

        [FieldInformationAttribute("Notes", IsReadOnly = true, Sort = 2)]
        public string Note { get; set; }

        [FieldInformationAttribute("Source", IsReadOnly = true, Sort = 3)]
        public string Source { get; set; }

        [FieldInformationAttribute("Meaning", IsReadOnly = true, Sort = 4)]
        public string Meaning { get; set; }

        [FieldInformationAttribute("Meaning", IsReadOnly = true, Sort = 5)]
        public string Usage { get; set; }

        [FieldInformationAttribute("Pos", IsReadOnly = true, Sort = 6)]
        public string Pos { get; set; }

        [FieldInformationAttribute("Pronunciation", IsReadOnly = true, Sort = 7)]
        public string Pron { get; set; }

        [FieldInformationAttribute("Xlit", IsReadOnly = true, Sort = 8)]
        public string Xlit { get; set; }

        [FieldInformationAttribute("Language", IsReadOnly = true, Sort = 9)]
        public string Language { get; set; }

        public string ReferencedText { get; internal set; }

        public string StrongsReference { get; internal set; }
    }
}
