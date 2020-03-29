using GeneralExtensions;
using System;
using System.Text;
using System.Windows;
using WPF.Tools.Dictionaries;
using WPF.Tools.Specialized;

namespace Bibles.Common
{
    public static class ErrorLog
    {
        public static void ShowError(Exception err)
        {
#if DEBUG
            StringBuilder errString = new StringBuilder();

            errString.AppendLine("Full Error Message");

            errString.AppendLine(err.GetFullExceptionMessage());

            errString.AppendLine();

            errString.AppendLine("Full Source");

            errString.AppendLine(err.ExstendedSource());

            MessageDisplay.Show(errString.ToString());

#else
            MessageDisplay.Show(TranslationDictionary.Translate(err.InnerExceptionMessage()));
#endif
        }

    }
}
