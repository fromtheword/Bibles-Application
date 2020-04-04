using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Models;
using System.Collections.Generic;
using System.Windows;
using WPF.Tools.BaseClasses;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for Strongs.xaml
    /// </summary>
    public partial class Strongs : UserControlBase
    {
        private string verseKey;

        public Strongs()
        {
            this.InitializeComponent();

            this.Loaded += this.Strongs_Loaded;
        }

        public string VerseKey
        {
            private get
            {
                return Formatters.RemoveBibleId(this.verseKey);
            }

            set
            {
                this.verseKey = value;

                if (this.IsLoaded)
                {
                    this.LoadEntries();
                }
            }
        }
        private void Strongs_Loaded(object sender, RoutedEventArgs e)
        {
            this.LoadEntries();
        }

        private void LoadEntries()
        {
            this.uxEntries.Items.Clear();

            List<StrongsEntry> entries = BiblesData.Database.GetStrongsEnteries(this.VerseKey);

            foreach(StrongsEntry entry in entries)
            {
                this.uxEntries.Items.Add(entry);

                int index = this.uxEntries.Items.Count - 1;

                this.uxEntries[index].SetAllowHeaderCollapse(true);

                this.uxEntries[index].ToggelCollaps(true);

                this.uxEntries[index].Header = $"{entry.StrongsReference} - {entry.ReferencedText}";
            }
        }
    }
}
