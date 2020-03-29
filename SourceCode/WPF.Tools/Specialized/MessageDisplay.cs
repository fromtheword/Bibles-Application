using System.Windows;
using WPF.Tools.Dictionaries;

namespace WPF.Tools.Specialized
{
    public class MessageDisplay
    {
        public static MessageBoxResult Show(string message)
        {
            return MessageBox.Show(TranslationDictionary.Translate(message));
        }

        public static MessageBoxResult Show(string message, string caption)
        {
            return MessageBox.Show(TranslationDictionary.Translate(message), TranslationDictionary.Translate(caption));
        }

        public static MessageBoxResult Show(string message, string caption, MessageBoxButton button)
        {
            return MessageBox.Show(TranslationDictionary.Translate(message), TranslationDictionary.Translate(caption), button);
        }
    }
}
