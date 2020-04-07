using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.DataEnums;
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

        private bool reload = true;

        private HaveInstalledEnum loadInstalled;

        public Strongs(HaveInstalledEnum installed)
        {
            this.InitializeComponent();

            this.loadInstalled = installed;

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

                switch(this.loadInstalled)
                {
                    case HaveInstalledEnum.StrongsEntryModel:

                        #region STRONGS

                        List<StrongsEntry> strongEntries = BiblesData.Database.GetStrongsEnteries(this.VerseKey);

                        foreach (StrongsEntry entry in strongEntries)
                        {
                            this.uxEntries.Items.Add(entry);

                            int index = this.uxEntries.Items.Count - 1;

                            this.uxEntries[index].SetAllowHeaderCollapse(true);

                            this.uxEntries[index].ToggelCollaps(true);

                            this.uxEntries[index].Header = $"{entry.StrongsReference} - {entry.ReferencedText}";
                        }

                        #endregion

                        break;

                    case HaveInstalledEnum.HebrewEntityModel:

                        #region HEBREW

                        List<HebrewEntry> hebrewEntries = BiblesData.Database.GetHebrewEnteries(this.VerseKey);

                        foreach (HebrewEntry entry in hebrewEntries)
                        {
                            this.uxEntries.Items.Add(entry);

                            int index = this.uxEntries.Items.Count - 1;

                            this.uxEntries[index].SetAllowHeaderCollapse(true);

                            this.uxEntries[index].ToggelCollaps(true);

                            this.uxEntries[index].Header = $"{entry.StrongsReference} - {entry.ReferencedText}";
                        }

                        #endregion

                        break;

                    case HaveInstalledEnum.GreekEntryModel:

                        #region GREEK

                        List<StrongsEntry> greekEntries = BiblesData.Database.GetGreekEnteries(this.VerseKey);

                        foreach (StrongsEntry entry in greekEntries)
                        {
                            this.uxEntries.Items.Add(entry);

                            int index = this.uxEntries.Items.Count - 1;

                            this.uxEntries[index].SetAllowHeaderCollapse(true);

                            this.uxEntries[index].ToggelCollaps(true);

                            this.uxEntries[index].Header = $"{entry.StrongsReference} - {entry.ReferencedText}";
                        }

                        #endregion

                        break;

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
