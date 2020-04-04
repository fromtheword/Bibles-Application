using WPF.Tools.Attributes;

namespace Bibles.DataResources.Models
{
    [ModelNameAttribute("Bibles")]
    public class StrongsEntry
    {
        [FieldInformationAttribute("Strong's", IsReadOnly = true, Sort = 1)]
        public string StrongsNumber { get; set; }

        [FieldInformationAttribute("Notes", IsReadOnly = true, Sort = 2)]
        public string Entry { get; set; }

        [FieldInformationAttribute("Greek Beta", IsReadOnly = true, Sort = 3)]
        public string GreekBeta { get; set; }

        [FieldInformationAttribute("Greek Unicode", IsReadOnly = true, Sort = 4)]
        public string GreekUnicode { get; set; }

        [FieldInformationAttribute("Greek Translit", IsReadOnly = true, Sort = 5)]
        public string GreekTranslit { get; set; }

        [FieldInformationAttribute("Pronunciation", IsReadOnly = true, Sort = 6)]
        public string Pronunciation { get; set; }

        [FieldInformationAttribute("Derivation", IsReadOnly = true, Sort = 7)]
        public string Derivation { get; set; }

        [FieldInformationAttribute("Strongs Definition", IsReadOnly = true, Sort = 8)]
        public string StrongsDefinition { get; set; }

        [FieldInformationAttribute("King James Definition", IsReadOnly = true, Sort = 9)]
        public string KingJamesDefinition { get; set; }

        public string ReferencedText { get; set; }

        public string StrongsReference { get; internal set; }
    }
}
