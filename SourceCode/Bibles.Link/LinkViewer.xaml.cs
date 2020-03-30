using Bibles.DataResources.Link;
using Bibles.Common;
using Bibles.Data;
using Bibles.DataResources;
using System;
using WPF.Tools.BaseClasses;
using WPF.Tools.CommonControls;
using GeneralExtensions;
using System.Windows;
using System.Collections.Generic;
using WPF.Tools.Specialized;
using WPF.Tools.Dictionaries;

namespace Bibles.Link
{
    /// <summary>
    /// Interaction logic for LinkViewer.xaml
    /// </summary>
    public partial class LinkViewer : UserControlBase
    {
        private string selectedLinkId;

        private string viewerVerseKey;

        private Dictionary<string, ModelsLink> modelsLinksDictionary = new Dictionary<string, ModelsLink>();

        private Dictionary<string, string> commentsDictionary = new Dictionary<string, string>();

        private List<string> deletedLinksList = new List<string>();

        public LinkViewer(string verseKey)
        {
            this.InitializeComponent();

            this.viewerVerseKey = verseKey;

            this.LoadTreeItems(verseKey);
        }

        public bool SaveComments()
        {
            if (!this.selectedLinkId.IsNullEmptyOrWhiteSpace())
            {
                BiblesData.Database.SaveLinkComments(this.selectedLinkId, this.uxComments.Text);
            }

            foreach(KeyValuePair<string, string> commentKayPair in this.commentsDictionary)
            {
                BiblesData.Database.SaveLinkComments(commentKayPair.Key, commentKayPair.Value);
            }

            return true;
        }

        public string[] GetDeletedLinks
        {
            get
            {
                if (this.deletedLinksList.Count > 0)
                {
                    this.deletedLinksList.Add(this.viewerVerseKey);
                }

                return this.deletedLinksList.ToArray();
            }
        }

        private void TreeItem_Changed(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                e.Handled = true;

                this.UpdateCommentsDictionary(this.selectedLinkId, this.uxComments.Text);
                
                string verseKey = ((TreeViewItemTool)this.uxLinkTree.SelectedItem).Tag.ParseToString();

                this.uxVerseText.Text = BiblesData.Database.GetVerse(verseKey).VerseText;

                if (this.modelsLinksDictionary.ContainsKey(verseKey))
                {
                    this.selectedLinkId = this.modelsLinksDictionary[verseKey].LinkKeyId;

                    if (this.commentsDictionary.ContainsKey(this.selectedLinkId))
                    {
                        this.uxComments.Text = this.commentsDictionary[this.selectedLinkId];
                    }
                    else
                    {
                        this.uxComments.Text = BiblesData.Database.GetLinkComments(this.selectedLinkId);

                        this.UpdateCommentsDictionary(this.selectedLinkId, this.uxComments.Text);
                    }
                }
                else
                {
                    this.uxComments.Text = string.Empty;

                    this.selectedLinkId = string.Empty;
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void OnDelete_Clicked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.uxLinkTree.SelectedItem == null)
            {
                MessageDisplay.Show("Please select a Link");

                return;
            }

            try
            {
                string verseKey = ((TreeViewItemTool)this.uxLinkTree.SelectedItem).Tag.ParseToString();

                if (!this.modelsLinksDictionary.ContainsKey(verseKey))
                {
                    MessageDisplay.Show("Cannot delete topmost parent item.");

                    return;
                }

                string message = $"{TranslationDictionary.Translate("Are you sure you would like to delete?")} {GlobalStaticData.Intance.GetKeyDescription(verseKey)}";

                if (MessageDisplay.Show(message, "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                ModelsLink deleteItem = this.modelsLinksDictionary[verseKey];

                BiblesData.Database.DeleteLink(deleteItem.LinkKeyId);

                this.modelsLinksDictionary.Remove(verseKey);

                TreeViewItemTool deleteTreeItemParent = ((TreeViewItemTool)this.uxLinkTree.SelectedItem).Parent.To<TreeViewItemTool>();

                deleteTreeItemParent.Items.Remove(this.uxLinkTree.SelectedItem);

                this.deletedLinksList.Add(verseKey);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private void LoadTreeItems(string verseKey)
        {
            ModelsLink verseTree = BiblesData.Database.GetLinkTree(verseKey);

            TreeViewItemTool item = new TreeViewItemTool 
            { 
                Header = GlobalStaticData.Intance.GetKeyDescription(verseTree.BibleVerseKey),
                Tag = verseTree.BibleVerseKey
            };

            this.uxLinkTree.Items.Add(item);

            this.LoadTreeItems(item, verseTree);
        }

        private void LoadTreeItems(TreeViewItemTool parent, ModelsLink verseTree)
        {
            foreach(ModelsLink child in verseTree.BibleVerseChildLinks)
            {
                TreeViewItemTool item = new TreeViewItemTool 
                {
                    Header = GlobalStaticData.Intance.GetKeyDescription(child.BibleVerseKey),
                    Tag = child.BibleVerseKey
                };

                parent.Items.Add(item);

                this.modelsLinksDictionary.Add(child.BibleVerseKey, child);

                if (child.BibleVerseChildLinks.Count > 0)
                {
                    this.LoadTreeItems(item, child);
                }
            }
        }

        private void UpdateCommentsDictionary(string linkId, string comments)
        {
            if (linkId.IsNullEmptyOrWhiteSpace())
            {
                return;
            }
            
            if (this.commentsDictionary.ContainsKey(linkId))
            {
                this.commentsDictionary.Remove(linkId);
            }

            this.commentsDictionary.Add(linkId, comments);
        }
    }
}
