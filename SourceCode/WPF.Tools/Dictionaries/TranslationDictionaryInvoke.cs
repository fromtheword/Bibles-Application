namespace WPF.Tools.Dictionaries
{
    public class TranslationDictionaryInvoke
    {
        public string Translate(string english)
        {
            return TranslationDictionary.Translate(english);
        }

        public object Translate(object english)
        {
            return TranslationDictionary.Translate(english);
        }
    }
}
