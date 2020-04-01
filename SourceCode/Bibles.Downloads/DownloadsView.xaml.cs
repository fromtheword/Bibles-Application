using Bibles.Common;
using GeneralExtensions;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Resources;
using System.Threading.Tasks;
using System.Windows;
using ViSo.Common;
using WPF.Tools.BaseClasses;
using WPF.Tools.CommonControls;
using WPF.Tools.Specialized;

namespace Bibles.Downloads
{
    /// <summary>
    /// Interaction logic for DownloadsView.xaml
    /// </summary>
    public partial class DownloadsView : UserControlBase
    {
        private readonly string[] directoryNames = new string[] { "Bibles", "Books", "Studies", "Translations" };

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
                string message = "Are you sure that you want to download the selected items? This may take a while.";

                if (MessageDisplay.Show(message, "Are you Sure?", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return false;
                }

                return this.TryDownload();
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

        private bool TryDownload()
        {
            try
            {
                bool openDirectory = false;

                bool restart = false;

                string basePath = Paths.KnownFolder(KnownFolders.KnownFolder.Downloads);

                using (WebClient client = new WebClient())
                {
                    client.UseDefaultCredentials = false;

                    string username = LogonCredentials.UserName();

                    string password = LogonCredentials.Password();

                    client.Credentials = new NetworkCredential(username, password);

                    foreach (RepositoryContent repository in this.GetSelectedRepositories())
                    {
                        string fullName = Path.Combine(basePath, repository.Name);

                        client.DownloadFile(repository.DownloadUrl, fullName);

                        if (repository.Path.StartsWith("Bibles/"))
                        {
                            restart = true;

                            if (DownloadedBibleLoader.LoadBible(fullName))
                            {
                                File.Delete(fullName);
                            }
                        }
                        else if (repository.Path.StartsWith("Translations/"))
                        {
                            restart = true;

                            if (LoadTranslations.LoadFile(fullName))
                            {
                                File.Delete(fullName);
                            }
                        }
                        else if (repository.Path.StartsWith("Studies/"))
                        {
                            ImportBibleStudy studyImport = new ImportBibleStudy();

                            if (studyImport.ImportStudy(fullName))
                            {
                                File.Delete(fullName);
                            }
                        }
                        else
                        {
                            openDirectory = true;
                        }
                    }
                }

                if (openDirectory)
                {
                    Process.Start(basePath);
                }

                if (restart)
                {
                    ProcessStartInfo Info = new ProcessStartInfo();

                    Info.Arguments = "/C choice /C Y /N /D Y /T 1 & START \"\" \"" + Assembly.GetEntryAssembly().Location + "\"";
                    
                    Info.WindowStyle = ProcessWindowStyle.Hidden;

                    Info.CreateNoWindow = true;

                    Info.FileName = "cmd.exe";

                    Process.Start(Info);

                    Process.GetCurrentProcess().Kill();
                }
            }
            catch (Exception err)
            {
                this.Dispatcher.Invoke(() =>
                {
                    ErrorLog.ShowError(err);
                });

                return false;
            }

            return true;
        }

        private async void ConnectToGitHub()
        {
            string username = LogonCredentials.UserName();

            string password = LogonCredentials.Password();

            await Task.Run(() => 
            {
                this.Dispatcher.Invoke(() => 
                {                
                    this.uxMessage.Content = "Connecting with server"; 
                });

                Credentials credentials = new Credentials(username, password);

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
