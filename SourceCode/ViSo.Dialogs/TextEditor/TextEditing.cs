using GeneralExtensions;
using System.Windows.Controls;
using ViSo.Dialogs.Controls;

namespace ViSo.Dialogs.TextEditor
{
    public static class TextEditing
    {
        private static TextEditorControl textControl;

        public static bool? ShowDialog(string windowTitle, string text)
        {
            TextEditing.Text = text;

            textControl = new TextEditorControl(text);

            bool? result = ControlDialog.ShowDialog(windowTitle, textControl, string.Empty, autoSize:true, desiredSize: textControl.TextLenght);

            if (result.IsTrue())
            {
                TextEditing.Text = textControl.Text;
            }

            textControl = null;

            return result;
        }

        public static string Text
        {
            get;

            private set;
        }
    }
}
