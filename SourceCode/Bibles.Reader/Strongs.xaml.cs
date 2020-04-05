using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Models.Strongs;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Windows;
using ViSo.Dialogs.Controls;
using WPF.Tools.BaseClasses;
using WPF.Tools.Exstention;
using WPF.Tools.ModelViewer;

namespace Bibles.Reader
{
    /// <summary>
    /// Interaction logic for Strongs.xaml
    /// </summary>
    public partial class Strongs : UserControlBase
    {
        private string verseKey;

        bool reload = true;

        public Strongs()
        {
            this.InitializeComponent();

            this.Loaded += this.Strongs_Loaded;
        }

        public int BibleId { private get; set; }

        public string VerseKey
        {
            private get
            {
                return Formatters.RemoveBibleId(this.verseKey);
            }

            set
            {
                this.reload = this.verseKey != value;

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
            if (!this.reload)
            {
                return;
            }

            try
            {
                this.uxEntries.Items.Clear();

                List<StrongsEntry> entries = BiblesData.Database.GetStrongsEnteries(this.VerseKey);

                foreach (StrongsEntry entry in entries)
                {
                    this.uxEntries.Items.Add(entry);

                    int index = this.uxEntries.Items.Count - 1;

                    this.uxEntries[index].SetAllowHeaderCollapse(true);

                    this.uxEntries[index].ToggelCollaps(true);

                    this.uxEntries[index].Header = $"{entry.StrongsReference} - {entry.ReferencedText}";
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void ModelItem_Browse(object sender, string buttonKey)
        {
            try
            { 
                ModelViewObject modelObject = (ModelViewObject)sender;

                string strongsNumber = modelObject[0].GetValue().ParseToString();

                StrongsVerses verseView = new StrongsVerses(this.BibleId, strongsNumber);

                ControlDialog.Show(strongsNumber, verseView, string.Empty, showCancelButton:false, autoSize:false, owner:this.GetParentWindow());
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
