using Bibles.Common;
using GeneralExtensions;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using ViSo.Common;
using WPF.Tools.BaseClasses;
using WPF.Tools.CommonControls;

namespace Bibles.Downloads
{
    /// <summary>
    /// Interaction logic for DownloadsView.xaml
    /// </summary>
    public partial class DownloadsView : UserControlBase
    {
        private readonly string[] directoryNames = new string[] { "Books", "Translations" };

        private GitHubClient gitHubClient;

        public DownloadsView()
        {
            this.InitializeComponent();


            this.Loaded += this.DownloadsView_Loaded;
        }
        
        public bool DownLoad()
        {
            try
            {
                string basePath = Paths.KnownFolder(KnownFolders.KnownFolder.Downloads);
                
                using (WebClient client = new WebClient())
                {
                    client.UseDefaultCredentials = false;

                    client.Credentials = new NetworkCredential("fromtheword.info@gmail.com", "Sonj@Vih@n2");


                    foreach (RepositoryContent repository in this.GetSelectedRepositories())
                    {
                        client.DownloadFile(repository.DownloadUrl, Path.Combine(basePath, repository.Name));
                    }
                }

                Process.Start(basePath);

                return true;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);

                return false;
            }
        }

        private void DownloadsView_Loaded(object sender, RoutedEventArgs e)
        {
            if (base.WasFirstLoaded)
            {
                return;
            }

            try
            {
                this.ConnectToGitHub();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private async void ConnectToGitHub()
        {
            await Task.Run(() => 
            { 
                Credentials credentials = new Credentials("fromtheword.info@gmail.com", "Sonj@Vih@n2");

                this.gitHubClient = new GitHubClient(new ProductHeaderValue("fromtheword"));

                this.gitHubClient.Credentials = credentials;

                IReadOnlyList<Repository> repositories = this.gitHubClient.Repository.GetAllForCurrent().Result;

                IReadOnlyList<RepositoryContent> allContent = this.gitHubClient.Repository.Content.GetAllContents(repositories[0].Id).Result;

                foreach (RepositoryContent content in allContent.Where(t => t.Type == ContentType.Dir))
                {
                    if (!this.directoryNames.Contains(content.Name))
                    {
                        continue;
                    }

                    TreeViewItemTool treeItem = null;

                    this.Dispatcher.Invoke(() => 
                    {
                        treeItem = new TreeViewItemTool { Header = content.Name, ResourceImageName = "Folder" };
                        
                        this.uxDownloadTree.Items.Add(treeItem);                
                    });

                    this.LoadDirectoryContent(content, treeItem, repositories[0].Id);
                }
            });
        }

        private void LoadDirectoryContent(RepositoryContent parentContent, TreeViewItemTool parentTreeItem, long repositoryId)
        {
            IReadOnlyList<RepositoryContent> repositoryContentList = this.gitHubClient.Repository.Content.GetAllContents(repositoryId, parentContent.Path).Result;

            foreach (RepositoryContent content in repositoryContentList)
            {
                TreeViewItemTool treeItem = null;

                this.Dispatcher.Invoke(() => 
                { 
                    treeItem = new TreeViewItemTool { Header = content.Name, IsCheckBox = (content.Type != ContentType.Dir) };
                });

                if (content.Type == ContentType.Dir)
                {
                    this.Dispatcher.Invoke(() => 
                    { 
                        treeItem.ResourceImageName = "Folder";
                    });

                    this.LoadDirectoryContent(content, treeItem, repositoryId);
                }
                else
                {
                    this.Dispatcher.Invoke(() => 
                    { 
                        treeItem.Tag = content;
                    });
                }

                this.Dispatcher.Invoke(() => 
                {
                    parentTreeItem.Items.Add(treeItem);
                });
            }
        }

        private List<RepositoryContent> GetSelectedRepositories()
        {
            List<RepositoryContent> result = new List<RepositoryContent>();

            foreach (TreeViewItemTool item in this.uxDownloadTree.Items)
            {
                result.AddRange(this.GetSelectedRepositories(item));
            }

            return result;
        }

        private List<RepositoryContent> GetSelectedRepositories(TreeViewItemTool parent)
        {
            List<RepositoryContent> result = new List<RepositoryContent>();

            foreach (TreeViewItemTool item in parent.Items)
            {
                if (!item.IsCheckBox)
                {
                    result.AddRange(this.GetSelectedRepositories(item));

                    continue;
                }

                if (item.IsChecked)
                {
                    result.Add(item.Tag.To<RepositoryContent>());
                }

            }

            return result;
        }
    }
}
