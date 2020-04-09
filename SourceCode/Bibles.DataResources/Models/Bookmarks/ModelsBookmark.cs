using Bibles.Common;
using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Text;
using WPF.Tools.Attributes;
using WPF.Tools.BaseClasses;
using WPF.Tools.ModelViewer;
using WPF.Tools.ToolModels;

namespace Bibles.DataResources.Bookmarks
{
    [ModelNameAttribute("Bookmark")]
    public class ModelsBookmark : ModelsBase
    {
        private string bookMarkName;

        private string bibleName;

        private string description;
        
        private string selectedVerse;
        
        private int verseRangeEnd;
        
        private DateTime bookmarkDate;

        public string VerseKey { get; set; }

        [FieldInformationAttribute("Name", Sort = 1)]
        public string BookMarkName 
        {
            get
            {
                return this.bookMarkName;
            }

            set
            {
                this.bookMarkName = value;

                base.OnPropertyChanged(() => this.BookMarkName);
            }
        }

        public string BibleName
        {
            get
            {
                return this.bibleName;
            }

            set
            {
                this.bibleName = value;

                base.OnPropertyChanged(() => this.BibleName);
            }
        }

        [FieldInformationAttribute("Description", Sort = 2)]
        [BrowseButtonAttribute("DescriptionBrowse", "", "Edit")]
        public string Description
        {
            get
            {
                return this.description;
            }

            set
            {
                this.description = value;

                base.OnPropertyChanged(() => this.Description);
            }
        }

        [FieldInformationAttribute("Selected Verse", Sort = 3, IsReadOnly = true)]
        public string SelectedVerse
        {
            get
            {
                return this.selectedVerse;
            }

            set
            {
                this.selectedVerse = value;

                base.OnPropertyChanged(() => this.SelectedVerse);
            }
        }

        [FieldInformationAttribute("To Verse", Sort = 4)]
        [ValuesSource("ToVersesRange")]
        [ItemType(ModelItemTypeEnum.ComboBox, IsComboboxEditable = false)]
        public int VerseRangeEnd
        {
            get
            {
                return this.verseRangeEnd;
            }

            set
            {
                this.verseRangeEnd = value;

                base.OnPropertyChanged(() => this.VerseRangeEnd);

                base.OnPropertyChanged(() => this.VerseText);
            }
        }
    
        public string VerseText
        {
            get
            {
                string verseAssembly = "Bibles.Data.GlobalInvokeData,Bibles.Data";

                StringBuilder result = new StringBuilder();

                int verseNumber = Formatters.GetVerseFromKey(this.VerseKey);

                BibleVerseModel verseModel = this.InvokeMethod(verseAssembly, "GetVerse", new object[] { this.VerseKey }).To<BibleVerseModel>();

                result.AppendLine($"{verseNumber}. {verseModel.VerseText}");

                if (this.VerseRangeEnd > verseNumber)
                {
                    string mainKey = $"{Formatters.GetBibleFromKey(this.VerseKey)}||{Formatters.GetBookFromKey(this.VerseKey)}||{Formatters.GetChapterFromKey(this.VerseKey)}||";

                    while (verseNumber < this.VerseRangeEnd)
                    {
                        ++verseNumber;
                        
                        result.AppendLine();

                        string itemKey = $"{mainKey}{verseNumber}||";

                        BibleVerseModel nextVerseModel = this.InvokeMethod(verseAssembly, "GetVerse", new object[] { itemKey }).To<BibleVerseModel>();

                        result.AppendLine($"{verseNumber}. {nextVerseModel.VerseText}");
                    }                
                }

                return result.ToString();
            }
        }

        public DateTime BookmarkDate
        {
            get
            {
                return this.bookmarkDate;
            }

            set
            {
                this.bookmarkDate = value;

                base.OnPropertyChanged(() => this.BookmarkDate);
            }
        }
    
        public DataItemModel[] ToVersesRange
        {
            get;

            set;
        }
    
        public void SetVerse(string verseKey)
        {
            this.VerseKey = verseKey;

            this.SelectedVerse = this.InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "GetKeyDescription", new object[] { verseKey, this.VerseRangeEnd }).ParseToString();

            int bibleId = Formatters.GetBibleFromKey(verseKey);

            this.BibleName = this.InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "GetBibleName", new object[] { bibleId }).ParseToString();

            int chapterVerseCount = this.InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "GetChapterVerseCount", new object[] { verseKey }).ToInt32();
            
            int selectedVerse = Formatters.GetVerseFromKey(verseKey);

            List<DataItemModel> toRangeList = new List<DataItemModel>();

            while(selectedVerse < chapterVerseCount)
            {
                ++selectedVerse;

                toRangeList.Add(new DataItemModel { DisplayValue = selectedVerse.ToString(), ItemKey = selectedVerse });
            }

            this.ToVersesRange = toRangeList.ToArray();
        }
    }
}
