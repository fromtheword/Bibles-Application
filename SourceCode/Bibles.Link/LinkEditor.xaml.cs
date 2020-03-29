using Bibles.Common;
using Bibles.Data;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using System;
using System.Windows;
using System.Windows.Media;
using WPF.Tools.BaseClasses;
using WPF.Tools.Exstention;
using WPF.Tools.Specialized;

namespace Bibles.Link
{
    /// <summary>
    /// Interaction logic for LinkEditor.xaml
    /// </summary>
    public partial class LinkEditor : UserControlBase
    {
        private BibleVerseModel parentVerse;

        private BibleVerseModel childVerse;

        public LinkEditor(int bibleId, BibleVerseModel verse)
        {
            this.InitializeComponent();

            this.parentVerse = verse;

            this.uxParentVerse.Content = verse.VerseText;

            this.uxReader.SetBible(bibleId);

            this.SetVerseLinkText();
        }

        public bool AcceptLink()
        {
            try
            {
                LinkModel link = new LinkModel
                {
                    LinkKeyId = $"{this.parentVerse.BibleVerseKey}*{this.childVerse.BibleVerseKey}",
                    Comments = this.uxLinkComments.Text
                };

                BiblesData.Database.CreateLink(link);

                string message = GlobalStaticData.Intance.GetKeyDescription(this.parentVerse.BibleVerseKey) +
                    " was linked to " +
                    GlobalStaticData.Intance.GetKeyDescription(this.childVerse.BibleVerseKey) +
                    "." +
                    Environment.NewLine + Environment.NewLine +
                    "Would you like to link another one?";

                if (MessageDisplay.Show(message, "Link Another?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return true;
                }

                this.childVerse = null;

                this.SetVerseLinkText();

                return false;

            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);

                return false;
            }
        }

        private void IndexerChapter_Changed(object sender, string key)
        {
            this.uxReader.SetChapter(key);
        }

        private void IndexerVerse_Changed(object sender, string key)
        {
            this.uxReader.SetVerse(key);
        }

        private void ChildVerse_Changed(object sender, BibleVerseModel verse)
        {
            try
            {
                this.childVerse = verse;

                this.SetVerseLinkText();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void SetVerseLinkText()
        {
            if (this.childVerse == null)
            {
                this.uxLinkDescription.Content = $"{GlobalStaticData.Intance.GetKeyDescription(this.parentVerse.BibleVerseKey)} -> ??";

                this.uxLinkDescription.Foreground = Brushes.Red;
            }
            else
            {
                this.uxLinkDescription.Content = $"{GlobalStaticData.Intance.GetKeyDescription(this.parentVerse.BibleVerseKey)} -> {GlobalStaticData.Intance.GetKeyDescription(this.childVerse.BibleVerseKey)}";

                this.uxLinkDescription.Foreground = Brushes.Black;
            }
        }

    }
}
