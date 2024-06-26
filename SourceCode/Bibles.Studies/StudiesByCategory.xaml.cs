﻿using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.Studies.Models;
using GeneralExtensions;
using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Windows;
using ViSo.Dialogs.Controls;
using WPF.Tools.BaseClasses;
using WPF.Tools.Exstention;
using WPF.Tools.Specialized;

namespace Bibles.Studies
{
    /// <summary>
    /// Interaction logic for StudiesByCategory.xaml
    /// </summary>
    public partial class StudiesByCategory : UserControlBase
    {
        private EditStudy newStudy;

        private StudyHeaderModel selectedStudyHeader;

        private StudyHeaderModel[] categoryStudyHeaders;

        public StudiesByCategory()
        {
            this.InitializeComponent();

            this.DataContext = this;
        }

        public StudyHeaderModel SelectedStudyHeader
        {
            get
            {
                return this.selectedStudyHeader;
            }

            set
            {
                this.selectedStudyHeader = value;

                base.OnPropertyChanged(() => this.SelectedStudyHeader);
            }
        }

        public StudyHeaderModel[] CategoryStudyHeaders
        {
            get
            {
                return this.categoryStudyHeaders;
            }

            set
            {
                this.categoryStudyHeaders = value;

                base.OnPropertyChanged(() => this.CategoryStudyHeaders);
            }
        }

        public bool TryEditStudy()
        {
            if (this.SelectedStudyHeader == null)
            {
                MessageDisplay.Show("Please select a Study.");

                return false;
            }

            #region CHECK FOR OPEN STUDIES

            foreach (Window window in Application.Current.Windows)
            {
                if (window.GetType() != typeof(ControlWindow))
                {
                    continue;
                }

                UserControlBase controlBase = window.GetPropertyValue("ControlContent").To<UserControlBase>();

                if (controlBase.GetType() != typeof(EditStudy))
                {
                    continue;
                }

                StudyHeader studyHeader = controlBase.GetPropertyValue("SubjectHeader").To<StudyHeader>();

                if (studyHeader.StudyHeaderId <= 0
                    || studyHeader.StudyHeaderId != this.SelectedStudyHeader.StudyHeaderId)
                {
                    continue;
                }

                window.Focus();

                return true;
            }

            #endregion

            EditStudy edit = new EditStudy(this.SelectedStudyHeader);

            ControlDialog.Show(this.SelectedStudyHeader.StudyName, edit, "SaveStudy", autoSize: false, owner: Application.Current.MainWindow);

            return true;                
        }

        private void SelectedCategory_Changed(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            try
            {
                StudyCategoryModel category = this.uxStudyCategories.SelectedCategory;

                if (category == null)
                {
                    this.CategoryStudyHeaders = new StudyHeaderModel[] { };

                    return;
                }

                this.CategoryStudyHeaders = BiblesData.Database.GetStudyHeaderByCategory(category.StudyCategoryId).ToArray();
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void NewStudy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.newStudy = new EditStudy();

                ControlDialog.ControlDialogClosing += this.ControlDialog_Closing;

                ControlDialog.Show("New Study", this.newStudy, "SaveStudy", autoSize: false, owner: Application.Current.MainWindow);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void ControlDialog_Closing(object sender, CancelEventArgs e)
        {
            try
            {
                    this.uxStudyCategories.Initialize();

                    if (this.newStudy != null
                        && this.newStudy.SubjectHeader != null
                        && this.newStudy.SubjectHeader.StudyHeaderId > 0)
                    {
                        this.uxStudyCategories.SelectCategory(this.newStudy.SubjectHeader.StudyHeaderId);
                    }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void EditStudy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedStudyHeader == null)
            {
                MessageDisplay.Show("Please select a Study.");

                return;
            }

            try
            {
                if (this.TryEditStudy())
                {
                    this.CloseIfNotMainWindow(true);
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private void ImportStudy_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog openDlg = new OpenFileDialog();

                openDlg.Title = "Import Study";

                openDlg.DefaultExt = ".study";

                openDlg.Filter = "Bible Study (.study)|*.study";

                if (openDlg.ShowDialog(this.GetParentWindow()).IsFalse())
                {
                    return;
                }

                this.InvokeMethod("Bibles.Downloads.ImportBibleStudy,Bibles.Downloads", "ImportStudy", new object[] { openDlg.FileName });

                this.uxStudyCategories.Initialize();

                MessageDisplay.Show("Import Completed.");
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
