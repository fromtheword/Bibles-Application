﻿using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.Studies.Models;
using GeneralExtensions;
using System;
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

        private void SelectedCategory_Changed(object sender, System.Windows.RoutedPropertyChangedEventArgs<object> e)
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

        private void EditStudy_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedStudyHeader == null)
            {
                MessageDisplay.Show("Please select a Study.");

                return;
            }

            try
            {

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

                    if (studyHeader.StudyHeaderId <= 0)
                    {
                        continue;
                    }

                    window.Focus();

                    this.CloseIfNotMainWindow(true);

                    return;
                }

                #endregion


                EditStudy edit = new EditStudy(this.SelectedStudyHeader);

                ControlDialog.Show(this.SelectedStudyHeader.StudyName, edit, "SaveStudy", autoSize:false);

                this.CloseIfNotMainWindow(true);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
    }
}
