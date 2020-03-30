using Bibles.Common;
using Bibles.DataResources.Models.Preferences;
using System;
using System.ComponentModel;
using WPF.Tools.BaseClasses;
using GeneralExtensions;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources;
using ViSo.Dialogs.ModelViewer;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using ViSo.Dialogs.Input;
using WPF.Tools.ToolModels;
using WPF.Tools.Specialized;
using System.Text;
using Newtonsoft.Json;
using System.IO;
using ViSo.Common;
using static ViSo.Common.KnownFolders;
using System.Diagnostics;
using Microsoft.Win32;
using WPF.Tools.Dictionaries;

namespace Bibles.Setup
{
    /// <summary>
    /// Interaction logic for TranslationSetup.xaml
    /// </summary>
    public partial class TranslationSetup : UserControlBase
    {
        private TranslationMapping selectedMapping;

        private TranslationMapping[] translationMappings;

        private List<int> deletedMappings = new List<int>();

        public TranslationSetup()
        {
            this.InitializeComponent();

            this.DataContext = this;

            this.SelectedLanguage = new LanguageSetup();
            
            this.SelectedLanguage.PropertyChanged += this.Language_Changed;

            this.uxLanguage.Items.Add(this.SelectedLanguage);
        }

        public bool SaveSetup()
        {
            try
            {
                foreach(TranslationMapping mapping in this.TranslationMappings)
                {
                    TranslationMappingModel model = new TranslationMappingModel
                    {
                        TranslationMappingId = mapping.TranslationMappingId,
                        LanguageId = this.SelectedLanguage.LanguageId,
                        EnglishLanguage = mapping.EnglishLanguage.ZipFile(),
                        OtherLanguage = mapping.OtherLanguage.ZipFile()
                    };

                    mapping.TranslationMappingId = BiblesData.Database.InsertTranslation(model);
                }

                foreach(int mappingId in this.deletedMappings)
                {
                    BiblesData.Database.DeleteTranslationMapping(mappingId);
                }

                return true;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);

                return false;
            }
        }

        public LanguageSetup SelectedLanguage { get; set; }

        public TranslationMapping SelectedMapping
        {
            get
            {
                return this.selectedMapping;
            }

            set
            {
                this.selectedMapping = value;

                base.OnPropertyChanged(() => this.SelectedMapping);
            }
        }

        public TranslationMapping[] TranslationMappings
        {
            get
            {
                return this.translationMappings;
            }

            set
            {
                this.translationMappings = value;

                base.OnPropertyChanged(() => this.TranslationMappings);
            }
        }
        
        private void Add_Cliked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedLanguage.LanguageId <= 0)
            {
                MessageDisplay.Show("Please select a Language");

                return;
            }

            try
            {
                TranslationMapping mapping = new TranslationMapping();

                if (ModelView.ShowDialog("New Mapping", mapping).IsFalse())
                {
                    return;
                }

                this.TranslationMappings = this.TranslationMappings.Add(mapping);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Edit_Cliked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedMapping == null)
            {
                MessageDisplay.Show("Please select a mapping");

                return;
            }

            try
            {
                TranslationMapping mapping = this.SelectedMapping.CopyTo(new TranslationMapping());

                if (ModelView.ShowDialog("Edit Mapping", mapping).IsFalse())
                {
                    return;
                }

                this.SelectedMapping = mapping.CopyTo(this.SelectedMapping);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Delete_Cliked(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedMapping == null)
            {
                MessageDisplay.Show("Please select a mapping");

                return;
            }

            try
            {
                string message = $"{TranslationDictionary.Translate("Are you sure you would like to delete?")} {this.SelectedMapping.EnglishLanguage} -> {this.SelectedMapping.OtherLanguage}?";

                if (MessageDisplay.Show(message, "Warning", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    return;
                }

                this.deletedMappings.Add(this.SelectedMapping.TranslationMappingId);

                this.TranslationMappings = this.TranslationMappings.Remove(this.SelectedMapping);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void Language_Changed(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                this.uxOtherLanguage.Header = this.SelectedLanguage.Language;

                TranslationMapping[] mappings = BiblesData.Database
                    .GetTranslationMapping(this.SelectedLanguage.LanguageId)
                    .Select(m => new TranslationMapping 
                            {
                                TranslationMappingId = m.TranslationMappingId,
                                LanguageId = m.LanguageId,
                                EnglishLanguage = m.EnglishLanguage.UnzipFile().ParseToString(),
                                OtherLanguage = m.OtherLanguage.UnzipFile().ParseToString()
                    })
                    .ToArray();

                this.TranslationMappings = mappings.OrderBy(ma => ma.EnglishLanguage).ToArray();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void LanguageSetup_Browse(object sender, string buttonKey)
        {
            try
            {
                if (InputBox.ShowDialog("New Language", "Language Name").IsFalse())
                {
                    return;
                }

                this.SelectedLanguage.Language = InputBox.Result;
                
                int languageId = BiblesData.Database.InsertLanguage(this.SelectedLanguage.CopyToObject(new LanguageSetupModel()).To<LanguageSetupModel>());

                this.uxLanguage[0, 0].AddComboBoxItem(new DataItemModel { DisplayValue = InputBox.Result, ItemKey = languageId });

                this.SelectedLanguage.LanguageId = languageId;
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void ExportTranslation_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedLanguage.LanguageId <= 0)
            {
                MessageDisplay.Show("Please select a Language");

                return;
            }

            try
            {
                StringBuilder result = new StringBuilder();

                foreach(TranslationMapping mapping in this.TranslationMappings)
                {
                    result.AppendLine(JsonConvert.SerializeObject(mapping));
                }

                string path = Path.Combine(Paths.KnownFolder(KnownFolder.Downloads), $"{this.SelectedLanguage.Language}_Translation_File.txt");

                File.WriteAllText(path, result.ToString());

                Process.Start(Paths.KnownFolder(KnownFolder.Downloads));
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void ImportTranslation_Cliked(object sender, RoutedEventArgs e)
        {
            if (this.SelectedLanguage.LanguageId <= 0)
            {
                MessageDisplay.Show("Please select a Language");

                return;
            }

            try
            {
                OpenFileDialog dlg = new OpenFileDialog();

                if (dlg.ShowDialog().IsFalse())
                {
                    return;
                }

                string[] mappingLines = File.ReadAllLines(dlg.FileName);

                List<TranslationMapping> result = new List<TranslationMapping>();

                foreach(string line in mappingLines)
                {
                    TranslationMapping mapping = JsonConvert.DeserializeObject(line, typeof(TranslationMapping)).To<TranslationMapping>();

                    mapping.TranslationMappingId = 0;

                    mapping.LanguageId = this.SelectedLanguage.LanguageId;

                    result.Add(mapping);
                }

                this.TranslationMappings = result.ToArray();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
