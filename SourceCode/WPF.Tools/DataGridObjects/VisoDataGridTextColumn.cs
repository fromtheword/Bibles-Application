using System.Windows.Controls;
using WPF.Tools.Dictionaries;

namespace WPF.Tools.DataGridObjects
{
    public class VisoDataGridTextColumn : DataGridTextColumn
    {
        public VisoDataGridTextColumn()
        {

        }

        public object HeaderTranslate
        {
            get
            {
                return base.Header;
            }

            set
            {
                base.Header = TranslationDictionary.Translate(value);
            }
        }
    }
}
