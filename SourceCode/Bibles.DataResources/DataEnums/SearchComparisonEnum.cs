using System.ComponentModel;

namespace Bibles.DataResources.DataEnums
{
    public enum SearchComparisonEnum
    {
        [Description("Exact")]
        Exact,

        [Description("Any of These")]
        AnyOfTheWords,

        [Description("All of These")]
        AllOfTheWords,

        [Description("Like this Phrase")]
        LikeThisPhrase
    }
}
