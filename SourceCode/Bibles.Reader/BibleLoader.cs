using Bibles.DataResources.Bookmarks;
using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using IconSet;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ViSo.Dialogs.Controls;
using WPF.Tools.BaseClasses;
using WPF.Tools.Specialized;
using System.Text;
using ViSo.Dialogs.ModelViewer;
using Bibles.Studies;
using Bibles.Studies.Models;

namespace Bibles.Reader
{
    internal static class BibleLoader
    {
        public delegate void LinkViewerClosedEvent(object sender, string verseKey);

        public static event LinkViewerClosedEvent LinkViewerClosed;

        private static readonly char[] veseSplitValues = new char[] { '*' };

        internal static HighlightRitchTextBox GetVerseAsTextBox(int bibleId, BibleVerseModel verse, int column)
        {
            HighlightRitchTextBox result = new HighlightRitchTextBox
            {
                Text = verse.VerseText,
                Tag = verse,
                BorderBrush = Brushes.Transparent,
                IsReadOnly = true,
                Margin = new Thickness(2, 0, 0, 15)
            };

            List<HighlightVerseModel> verseColours = BiblesData.Database.GetVerseColours(verse.BibleVerseKey);

            foreach(HighlightVerseModel colour in verseColours)
            {
                string[] itemSplit = colour.BibleVerseKeyId.Split(BibleLoader.veseSplitValues);

                result.HighlightText(itemSplit[1].ToInt32(), itemSplit[2].ToInt32(), ColourConverters.GetBrushfromHex(colour.HexColour));
            }
            
            Grid.SetRow(result, (Formatters.GetVerseFromKey(verse.BibleVerseKey) - 1));

            Grid.SetColumn(result, column);

            return result;
        }

        internal static StackPanel GetVerseNumberPanel(int bibleId, BibleVerseModel verse, int column)
        {
            StackPanel result = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            BibleLoader.RefreshVerseNumberPanel(result, bibleId, verse);
            
            Grid.SetRow(result, (Formatters.GetVerseFromKey(verse.BibleVerseKey) - 1));

            Grid.SetColumn(result, column);

            return result;
        }

        public static void RefreshVerseNumberPanel(StackPanel versePanel, int bibleId, BibleVerseModel verse)
        {
            versePanel.Children.Clear();

            UIElement[] children = BibleLoader.GetVerseNumberElements(bibleId, verse);

            versePanel.Children.Add(children[0]);

            for (int x = 1; x < children.Length; ++x)
            {
                if (children[x] == null)
                {
                    continue;
                }

                versePanel.Children.Add(children[x]);
            }
        }
        
        private static void Bookmark_Selected(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image item = (Image)sender;

                string verseKey = item.Tag.ParseToString();

                BookmarkModel bookmarkModel = BiblesData.Database.GetBookmark(verseKey);

                ModelsBookmark model = bookmarkModel.CopyToObject(new ModelsBookmark()).To<ModelsBookmark>();

                model.SetVerse(verseKey);

                if (ModelView.ShowDialog("Bookmark", model).IsFalse())
                {
                    return;
                }

                BookmarkModel dbModel = model.CopyToObject(new BookmarkModel()).To<BookmarkModel>();

                BiblesData.Database.InsertBookmarkModel(dbModel);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private static void StudyBookmark_Selected(object sender, RoutedEventArgs e)
        {
            try
            {
                MenuItem item = (MenuItem)sender;

                string studyKey = item.Tag.ParseToString();

                string[] keySplit = studyKey.Split(new string[] { "||" }, StringSplitOptions.None);

                int studyHeaderId = keySplit[0].ToInt32();

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

                    return;
                }

                #endregion

                StudyHeaderModel model = BiblesData.Database.GetStudyHeader(studyHeaderId.ToInt32());

                EditStudy edit = new EditStudy(model);

                ControlDialog.Show(model.StudyName, edit, "SaveStudy", autoSize: false);
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }

        private static void LinkImage_Selected(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Image item = (Image)sender;

                string verseKey = item.Tag.ParseToString();

                Type linkViewerType = Type.GetType("Bibles.Link.LinkViewer,Bibles.Link");

                UserControlBase linkViewer = Activator.CreateInstance(linkViewerType, new object[] { verseKey }) as UserControlBase;
                
                if (ControlDialog.ShowDialog("Link Viewer",linkViewer, "SaveComments", autoSize: false).IsFalse())
                {
                    return;
                }

                string[] deletedLinks = linkViewer.GetPropertyValue("GetDeletedLinks").To<string[]>();

                foreach (string key in deletedLinks)
                {
                    BibleLoader.LinkViewerClosed?.Invoke(linkViewer, key);
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);
            }
        }
        
        private static UIElement[] GetVerseNumberElements(int bibleId, BibleVerseModel verse)
        {
            UIElement[] result = new UIElement[4];

            Label labelVerse = new Label { Content = Formatters.GetVerseFromKey(verse.BibleVerseKey), Foreground = Brushes.LightGray, Tag = verse };

            result[0] = labelVerse;

            result[1] = BibleLoader.GetVerseBookmarkImage(bibleId, verse.BibleVerseKey);

            result[2] = BibleLoader.GetStudyBookmarkImage(bibleId, verse.BibleVerseKey);

            result[3] = BibleLoader.GetLinkImage(verse.BibleVerseKey);

            return result;
        }

        private static Image GetVerseBookmarkImage(int bibleId, string verseKey)
        {
            string bibleKey = Formatters.IsBiblesKey(verseKey) ?
                verseKey : $"{bibleId}||{verseKey}";

            BookmarkModel model = BiblesData.Database.GetBookmark(bibleKey);

            if (model == null)
            {
                return null;
            }

            ModelsBookmark bookmark = model.CopyToObject(new ModelsBookmark()).To<ModelsBookmark>();

            string imgToolTip = bookmark.BookMarkName.IsNullEmptyOrWhiteSpace() && bookmark.Description.IsNullEmptyOrWhiteSpace() ?
            bookmark.SelectedVerse : bookmark.BookMarkName.IsNullEmptyOrWhiteSpace() ?
            $"{bookmark.SelectedVerse}{Environment.NewLine}{bookmark.Description}" :
            $"{bookmark.SelectedVerse}{Environment.NewLine}{bookmark.BookMarkName}{Environment.NewLine}{Environment.NewLine}{bookmark.Description}";

            Image img = new Image
            {
                Source = IconSets.ResourceImageSource("BookmarkSmall", 16),
                ToolTip = imgToolTip,
                Opacity = 0.5,
                Tag = bibleKey
            };

            img.PreviewMouseLeftButtonUp += BibleLoader.Bookmark_Selected;

            return img;
        }

        private static Image GetStudyBookmarkImage(int bibleId,string verseKey)
        {
            string bibleKey = Formatters.IsBiblesKey(verseKey) ?
                verseKey : $"{bibleId}||{verseKey}";

            List<StudyBookmarkModel> modelsList = BiblesData.Database.GetStudyBookmarks(bibleKey);

            if (modelsList.Count <= 0)
            {
                return null;
            }


            Image img = new Image
            {
                Source = IconSets.ResourceImageSource("BookmarkSmallRed", 16),
                Opacity = 0.5
            };

            ContextMenu bookmarkMenu = new ContextMenu();

            StringBuilder imageTooltip = new StringBuilder();

            imageTooltip.AppendLine("(Right Click to Edit)");

            foreach (StudyBookmarkModel studyMark in modelsList)
            {
                MenuItem menuItem = new MenuItem { Header = studyMark.StudyName, Tag = studyMark.StudyVerseKey };

                menuItem.Click += BibleLoader.StudyBookmark_Selected;

                bookmarkMenu.Items.Add(menuItem);

                imageTooltip.AppendLine(studyMark.StudyName);
            }

            img.ContextMenu = bookmarkMenu;

            img.ToolTip = imageTooltip.ToString();

            return img;
        }

        
        public static Image GetLinkImage(string verseKey)
        {
            if (!BiblesData.Database.HaveLink(verseKey))
            {
                return null;
            }

            Image linkImage = new Image
            {
                Source = IconSets.ResourceImageSource("Link", 16),
                Opacity = 0.5,
                Tag = verseKey
            };

            linkImage.PreviewMouseLeftButtonUp += BibleLoader.LinkImage_Selected;

            return linkImage;
        }

    }
}
