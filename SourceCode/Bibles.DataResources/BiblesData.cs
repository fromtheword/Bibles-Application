using Bibles.Common;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.DataEnums;
using Bibles.DataResources.Link;
using Bibles.DataResources.Models.Categories;
using Bibles.DataResources.Models.Strongs;
using GeneralExtensions;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bibles.DataResources
{
    public class BiblesData
    {
        private static readonly Lazy<SQLiteAsyncConnection> LazyInitializer = new Lazy<SQLiteAsyncConnection>(() =>
        {
            return new SQLiteAsyncConnection(DbConstraints.DatabasePath, DbConstraints.Flags);
        });

        private static BiblesData viSoDatabase;

        private static SQLiteAsyncConnection database => BiblesData.LazyInitializer.Value;

        private static bool IsInitialized = false;

        private static object databaseLockObject = new object();

        public BiblesData()
        {
            InitializeAsync().SafeFireAndForget(false);
        }

        public static BiblesData Database
        {
            get
            {
                if (BiblesData.viSoDatabase == null)
                {
                    lock (BiblesData.databaseLockObject)
                    {
                        if (BiblesData.viSoDatabase == null)
                        {
                            BiblesData.viSoDatabase = new BiblesData();
                        }
                    }
                }

                return BiblesData.viSoDatabase;
            }
        }

        public object GlobalStaticData { get; private set; }

        #region USERPREFERENCE MODEL

        public UserPreferenceModel GetPreferences()
        {
            try
            {
                if (!database.TableMappings.Any(up => up.MappedType.Name == typeof(UserPreferenceModel).Name))
                {
                     database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(UserPreferenceModel)).ConfigureAwait(false);
                }

                Task<UserPreferenceModel> result = BiblesData.database.Table<UserPreferenceModel>().FirstOrDefaultAsync();

                return result.Result;
            }
            catch
            {
                return null;
            }
        }

        public int InsertPreference(UserPreferenceModel userPreference)
        {
            Task<UserPreferenceModel> existing = BiblesData.database.Table<UserPreferenceModel>().FirstOrDefaultAsync();

            if (existing.Result == null)
            {
                Task<int> newResult = BiblesData.database.InsertAsync(userPreference);

                int newResultValue = newResult.Result;

                return userPreference.UserId;
            }

            existing.Result.DefaultBible = userPreference.DefaultBible;

            existing.Result.LanguageId = userPreference.LanguageId;

            existing.Result.SynchronizzeTabs = userPreference.SynchronizzeTabs;

            existing.Result.Font = userPreference.Font;

            existing.Result.FontSize = userPreference.FontSize;

            existing.Result.LastReadVerse = userPreference.LastReadVerse;

            BiblesData.database.UpdateAsync(existing.Result);

            return existing.Result.UserId;
        }

        #endregion

        #region BIBLE MODEL

        public Task<List<BibleModel>> GetBibles()
        {
            return BiblesData.database.Table<BibleModel>()
                .ToListAsync();
        }

        public string GetBibleName(int bibleId)
        {
            Task<BibleModel> model = BiblesData.database.Table<BibleModel>().FirstOrDefaultAsync(bi => bi.BiblesId == bibleId);

            return model.Result == null ? string.Empty : model.Result.BibleName;
        }

        public BibleModel GetBible(string bibleName)
        {
            Task<BibleModel> existing = BiblesData.database.Table<BibleModel>().FirstOrDefaultAsync(b => b.BibleName == bibleName);

            return existing.Result;
        }

        public void InsertBible(BibleModel bible)
        {
            Task<BibleModel> existing = BiblesData.database.Table<BibleModel>().FirstOrDefaultAsync(b => b.BibleName == bible.BibleName);

            if (existing.Result != null)
            {
                return;
            }

            BiblesData.database.InsertAsync(bible);
        }

        #endregion

        #region BIBLE VERSES

        public void DeleteBibleVerses(int bibleId)
        {
            string bibkeKey = $"{bibleId}||";

            Task<int> result = BiblesData.database
                .Table<BibleVerseModel>()
                .DeleteAsync(bi => bi.BibleVerseKey.StartsWith(bibkeKey));
                        
            var check = result.Result;
        }

        public BibleVerseModel GetVerse(string verseKey)
        {
            return BiblesData.database.Table<BibleVerseModel>().FirstOrDefaultAsync(vk => vk.BibleVerseKey == verseKey).Result;
        }

        public Dictionary<int, BibleVerseModel> GetVerses(string bibleKey)
        {
            int bookKey = Formatters.GetChapterFromKey(bibleKey);

            if (bookKey < 0)
            {
                return new Dictionary<int, BibleVerseModel>();
            }

            string searchKey = $"{Formatters.GetBibleFromKey(bibleKey)}||{Formatters.GetBookFromKey(bibleKey)}||{Formatters.GetChapterFromKey(bibleKey)}||";
            
            Task<List<BibleVerseModel>> resultList = BiblesData.database
                .Table<BibleVerseModel>()
                .Where(v => v.BibleVerseKey.StartsWith(searchKey))
                .ToListAsync();

            if (resultList.Result == null)
            {
                return new Dictionary<int, BibleVerseModel>();
            }

            return resultList.Result
                .ToDictionary(vk => Formatters.GetVerseFromKey(vk.BibleVerseKey));
        }

        public void InsertBibleVerseBulk(List<BibleVerseModel> verseList)
        {
            Task<int> result = BiblesData.database.InsertAllAsync(verseList);

            int check = result.Result;
        }

        public void InsertBibleVerse(BibleVerseModel verse)
        {
            Task<BibleVerseModel> existing = BiblesData.database.Table<BibleVerseModel>().FirstOrDefaultAsync(v => v.BibleVerseKey == verse.BibleVerseKey);

            if (existing.Result != null)
            {
                existing.Result.VerseText = verse.VerseText;

                BiblesData.database.UpdateAsync(existing.Result);

                return;
            }

            BiblesData.database.InsertAsync(verse);
        }

        #endregion

        #region BOOKMARKMODEL

        public List<BookmarkModel> GetBookmarks()
        {
            return BiblesData.database.Table<BookmarkModel>().ToListAsync().Result;
        }

        public BookmarkModel GetBookmark(string verseKey)
        {
            Task<BookmarkModel> existing = BiblesData.database.Table<BookmarkModel>()
                .FirstOrDefaultAsync(bm => bm.VerseKey == verseKey);

            return existing.Result;
        }

        public void InsertBookmarkModel(BookmarkModel bookmark)
        {
            Task<BookmarkModel> existing = BiblesData.database.Table<BookmarkModel>()
                .FirstOrDefaultAsync(bm => bm.VerseKey == bookmark.VerseKey);

            if (existing.Result == null)
            {
                bookmark.BookmarkDate = DateTime.Now;

                BiblesData.database.InsertAsync(bookmark);
            }
            else
            {
                existing.Result.BookMarkName = bookmark.BookMarkName;

                existing.Result.Description = bookmark.Description;

                existing.Result.VerseRangeEnd = bookmark.VerseRangeEnd;

                BiblesData.database.UpdateAsync(existing.Result);
            }
        }

        public void DeleteBookmark(string bibleVerseKey)
        {
            BiblesData.database.Table<BookmarkModel>().DeleteAsync(b => b.VerseKey == bibleVerseKey);
        }

        #endregion

        #region STUDY BOOKMARKMODEL

        public void InsertStudyBookmarkModel(StudyBookmarkModel bookmark)
        {
            Task<StudyBookmarkModel> existing = BiblesData.database.Table<StudyBookmarkModel>()
                .FirstOrDefaultAsync(bm => bm.StudyVerseKey == bookmark.StudyVerseKey);

            if (existing.Result == null)
            {
                bookmark.BookmarkDate = DateTime.Now;

                BiblesData.database.InsertAsync(bookmark);
            }
            else
            {
                existing.Result.BookMarkName = bookmark.BookMarkName;

                existing.Result.Description = bookmark.Description;

                existing.Result.VerseRangeEnd = bookmark.VerseRangeEnd;

                BiblesData.database.UpdateAsync(existing.Result);
            }
        }

        public List<StudyBookmarkModel> GetStudyBookmarks(string verseKey)
        {
            Task<List<StudyBookmarkModel>> existing = BiblesData.database.Table<StudyBookmarkModel>()
                .Where(bm => bm.VerseKey == verseKey)
                .ToListAsync();

            return existing.Result;
        }

        public List<StudyBookmarkModel> GetStudyBookmarks(int studyHeaderId)
        {
            string studyHeaderIdKey = $"{studyHeaderId}||";

            Task<List<StudyBookmarkModel>> existing = BiblesData.database.Table<StudyBookmarkModel>()
                .Where(bm => bm.StudyVerseKey.StartsWith(studyHeaderIdKey))
                .ToListAsync();

            return existing.Result;
        }

        public void DeleteStudyBookmark(string studyVerseKey)
        {
            BiblesData.database.Table<StudyBookmarkModel>().DeleteAsync(b => b.StudyVerseKey == studyVerseKey);
        }


        #endregion

        #region LINK

        private static List<string> linkedList = new List<string>();

        private static char[] linkSplit = new char[] { '*'};

        public bool HaveLink(string bibleKey)
        {
            Task<LinkModel> result = BiblesData.database
                .Table<LinkModel>()
                .FirstOrDefaultAsync(li => li.LinkKeyId.Contains(bibleKey));

            return result.Result != null;
        }

        public string GetLinkComments(string bibleKey)
        {
            Task<LinkModel> result = BiblesData.database
                .Table<LinkModel>()
                .FirstOrDefaultAsync(li => li.LinkKeyId.Contains(bibleKey));

            return result.Result.Comments;
        }

        public void SaveLinkComments(string bibleKey, string comments)
        {
            Task<LinkModel> result = BiblesData.database
                .Table<LinkModel>()
                .FirstOrDefaultAsync(li => li.LinkKeyId.Contains(bibleKey));
            
            if (result.Result == null)
            {
                return;
            }

            result.Result.Comments = comments;

            BiblesData.database.UpdateAsync(result.Result);
        }

        public ModelsLink GetLinkTree(string bibleKey)
        {
            try
            {
                return this.GetLinkedVerses(bibleKey);
            }
            finally
            {
                BiblesData.linkedList.Clear();
            }
        }

        private ModelsLink GetLinkedVerses(string bibleKey)
        {
            BiblesData.linkedList.Add(bibleKey);

            string parentKey = $"{bibleKey}*";

            string childKey = $"*{bibleKey}";

            Task<List<LinkModel>> parentToChild = BiblesData.database
                .Table<LinkModel>()
                .Where(pl => pl.LinkKeyId.StartsWith(parentKey))
                .ToListAsync();

            Task<List<LinkModel>> childToParent = BiblesData.database
                .Table<LinkModel>()
                .Where(pl => pl.LinkKeyId.EndsWith(childKey))
                .ToListAsync();

            ModelsLink results = new ModelsLink { BibleVerseKey = bibleKey };

            foreach(LinkModel model in parentToChild.Result)
            {
                string[] verseSplit = model.LinkKeyId.Split(linkSplit);

                if (BiblesData.linkedList.Contains(verseSplit[1]))
                {
                    continue;
                }

                ModelsLink child = new ModelsLink { BibleVerseKey = verseSplit[1], LinkKeyId = model.LinkKeyId };

                child.BibleVerseChildLinks.AddRange(this.GetLinkedVerses(verseSplit[1]).BibleVerseChildLinks);

                results.BibleVerseChildLinks.Add(child);
            }

            foreach (LinkModel model in childToParent.Result)
            {
                string[] verseSplit = model.LinkKeyId.Split(linkSplit);

                if (BiblesData.linkedList.Contains(verseSplit[0]))
                {
                    continue;
                }

                ModelsLink child = new ModelsLink { BibleVerseKey = verseSplit[0], LinkKeyId = model.LinkKeyId };

                child.BibleVerseChildLinks.AddRange(this.GetLinkedVerses(verseSplit[0]).BibleVerseChildLinks);

                results.BibleVerseChildLinks.Add(child);
            }

            return results;
        }

        public void CreateLink (LinkModel link)
        {
            Task<LinkModel> exising = BiblesData.database.Table<LinkModel>().FirstOrDefaultAsync(li => li.LinkKeyId == link.LinkKeyId);

            if (exising.Result != null)
            {
                return;
            }

            BiblesData.database.InsertAsync(link);
        }
        
        public void DeleteLink(string linkKeyId)
        {
            BiblesData.database.Table<LinkModel>().DeleteAsync(b => b.LinkKeyId == linkKeyId);
        }

        #endregion

        #region HIGHLIGHT VERSE

        public List<HighlightVerseModel> GetHighlights()
        {
            Task<List<HighlightVerseModel>> result = BiblesData.database
                .Table<HighlightVerseModel>()
                .ToListAsync();

            return result.Result.OrderBy(hex => hex.HexColour).ToList();
        }

        public List<HighlightVerseModel> GetVerseColours(string bibleVerseKey)
        {
            string bibleVerseKeyId = $"{bibleVerseKey}*";

            Task<List<HighlightVerseModel>> result = BiblesData.database
                .Table<HighlightVerseModel>()
                .Where(hi => hi.BibleVerseKeyId.StartsWith(bibleVerseKeyId))
                .ToListAsync();

            return result.Result;
        }
        
        public void InsertVerseColour(string bibleVerseKey, int startIndex, int length, string hexColour)
        {
            string bibleVerseKeyId = $"{bibleVerseKey}*{startIndex}*{length}";

            Task<HighlightVerseModel> highlight = BiblesData.database
                .Table<HighlightVerseModel>()
                .FirstOrDefaultAsync(hi => hi.BibleVerseKeyId == bibleVerseKeyId);

            if (highlight.Result == null)
            {
                HighlightVerseModel model = new HighlightVerseModel
                {
                    BibleVerseKeyId = bibleVerseKeyId,
                    HexColour = hexColour
                };

                BiblesData.database.InsertAsync(model);
            }
            else
            {
                highlight.Result.HexColour = hexColour;

                BiblesData.database.UpdateAsync(highlight.Result);
            }
        }

        public void DeleteVerseColours(string bibleVerseKey)
        {
            string bibleVerseKeyId = $"{bibleVerseKey}*";

            BiblesData.database.Table<HighlightVerseModel>().DeleteAsync(hi => hi.BibleVerseKeyId.StartsWith(bibleVerseKeyId));
        }

        public void DeleteHighlight(string bibleVerseKeyId)
        {
            BiblesData.database.Table<HighlightVerseModel>().DeleteAsync(hi => hi.BibleVerseKeyId == bibleVerseKeyId);
        }

        #endregion

        #region SEARCH

        private readonly char[] splitFilter = new char[] { ' ', '.', ',', ';', ':', '?', '!', '|' };

        public List<BibleVerseModel> SearchExact(string word, int bibleId)
        {
            if (word.IsNullEmptyOrWhiteSpace())
            {
                return new List<BibleVerseModel>();
            }

            word = word.ToLower();

            string bibleKey = $"{bibleId}||";

            List<BibleVerseModel> result = new List<BibleVerseModel>();

            char[] searchPostFixes = new char[] { ' ', ',', '.', '?', '!', ':', '\'' };
            
            foreach (char item in searchPostFixes)
            {
                string searchWord = $" {word}{item}";

                Task<List<BibleVerseModel>> charResult = bibleId > 0 ?
                    BiblesData.database
                    .Table<BibleVerseModel>()
                    .Where(s => s.BibleVerseKey.StartsWith(bibleKey)
                                && s.VerseText.ToLower().Contains(searchWord))
                    .ToListAsync()
                    :
                    BiblesData.database
                    .Table<BibleVerseModel>()
                    .Where(s =>  s.VerseText.ToLower().Contains(searchWord))
                    .ToListAsync();

                result.AddRange(charResult.Result);
            }

            return result;
        }

        public List<BibleVerseModel> SearchAnyOfTheWords(string phrase, int bibleId, out string[] searchSplitResult)
        {               
            if (phrase.IsNullEmptyOrWhiteSpace())
            {
                searchSplitResult = new string[] { };

                return new List<BibleVerseModel>();
            }
            
            string searchPhrase = phrase.ToLower();
            
            string[] searchSplit = searchPhrase
                .Split(this.splitFilter)
                .Select(s => $" {s}")
                .ToArray();

            searchSplitResult = searchSplit;

            List<BibleVerseModel> result = new List<BibleVerseModel>();

            List<string> loadedVerses = new List<string>();

            foreach (string word in searchSplit)
            {
                StringBuilder sqlString = new StringBuilder();

                sqlString.Append($"select * from BibleVerseModel where lower(VerseText) like ('%{word}%')");

                if (bibleId > 0)
                {
                    string bibleKey = $"{bibleId}||";

                    sqlString.Append($" and BibleVerseKey like ('{bibleKey}%')");
                }

                Task<List<BibleVerseModel>> searchResult = BiblesData.database.QueryAsync<BibleVerseModel>(sqlString.ToString(), new object[] { });

                foreach (BibleVerseModel verse in searchResult.Result)
                {
                    if (loadedVerses.Contains(verse.BibleVerseKey))
                    {
                        continue;
                    }

                    loadedVerses.Add(verse.BibleVerseKey);

                    result.Add(verse);
                }
            }

            return result;
        }

        public List<BibleVerseModel> SearchAllOfTheWords(string phrase, int bibleId, out string[] searchSplitResult)
        {
            if (phrase.IsNullEmptyOrWhiteSpace())
            {
                searchSplitResult = new string[] { };

                return new List<BibleVerseModel>();
            }

            string searchPhrase = phrase.ToLower();

            string[] searchSplit = searchPhrase
                .Split(this.splitFilter)
                .ToArray();

            searchSplitResult = searchSplit;

            List<BibleVerseModel> result = new List<BibleVerseModel>();

            List<string> loadedVerses = new List<string>();

            StringBuilder andOrString = new StringBuilder();
            
            foreach (string word in searchSplit)
            {
                andOrString.Append("(");
                                    
                andOrString.Append($"lower(VerseText) like ('%{word}%') or ");

                andOrString.Remove(andOrString.Length - 3, 3);

                andOrString.Append(") and ");
            }

            andOrString.Remove(andOrString.Length - 4, 4);
            
            foreach (string word in searchSplit)
            {
                foreach (char item in this.splitFilter)
                {
                    StringBuilder sqlString = new StringBuilder();

                    string paddedWord = $"{word}{item}";

                    sqlString.Append($"select * from BibleVerseModel where {andOrString.ToString()}");

                    if (bibleId > 0)
                    {
                        string bibleKey = $"{bibleId}||";

                        sqlString.Append($" and BibleVerseKey like ('{bibleKey}%')");
                    }


                    string sqlTest = sqlString.ToString();

                    Task<List<BibleVerseModel>> searchResult = BiblesData.database.QueryAsync<BibleVerseModel>(sqlString.ToString(), new object[] { });

                    foreach(BibleVerseModel verse in searchResult.Result)
                    {
                        if (loadedVerses.Contains(verse.BibleVerseKey))
                        {
                            continue;
                        }

                        loadedVerses.Add(verse.BibleVerseKey);
                    
                        result.Add(verse);
                    }
                }
            }

            return result;
        }

        public List<BibleVerseModel> SearchLikeThisPhrase(string phrase, int bibleId)
        {
            if (phrase.IsNullEmptyOrWhiteSpace())
            {
                return new List<BibleVerseModel>();
            }

            string bibleKey = $"{bibleId}||";

            string searchPhrase = phrase.ToLower();
            
            Task<List<BibleVerseModel>> result = bibleId > 0 ?
                BiblesData.database
                .Table<BibleVerseModel>()
                .Where(s => s.BibleVerseKey.StartsWith(bibleKey)
                            && s.VerseText.ToLower().Contains(searchPhrase))
                .ToListAsync()
                :
                BiblesData.database
                .Table<BibleVerseModel>()
                .Where(s => s.VerseText.ToLower().Contains(searchPhrase))
                .ToListAsync();

            return result.Result;
        }

        #endregion

        #region STUDY CATEGORIES

        public int InsertCategory(string name, int parentId)
        {
            StudyCategoryModel category = new StudyCategoryModel
            {
                CategoryName = name,
                ParentStudyCategoryId = parentId
            };

            Task<int> insertTask = BiblesData.database.InsertAsync(category);

            int insetValue = insertTask.Result;

            return category.StudyCategoryId;
        }

        public string GetCategoryName(int studyCategoryId)
        {
            Task<StudyCategoryModel> result = BiblesData.database
                .Table<StudyCategoryModel>()
                .FirstOrDefaultAsync(c => c.StudyCategoryId == studyCategoryId);

            return result.Result == null ? string.Empty : result.Result.CategoryName;
        }

        public StudyCategoryModel GetCategory(string categoryName)
        {
            Task<StudyCategoryModel> result = BiblesData.database
                .Table<StudyCategoryModel>()
                .FirstOrDefaultAsync(sc => sc.CategoryName == categoryName);

            return result.Result;
        }

        public StudyCategoryModel GetCategory(int studyCategoryId)
        {
            Task<StudyCategoryModel> result = BiblesData.database
                .Table<StudyCategoryModel>()
                .FirstOrDefaultAsync(sc => sc.StudyCategoryId == studyCategoryId);

            return result.Result;
        }

        public List<CategoryTreeModel> GetCategoryTree()
        {
            Task<List<StudyCategoryModel>> categoriesTask = BiblesData.database
                .Table<StudyCategoryModel>()
                .ToListAsync();

            List<CategoryTreeModel> result = new List<CategoryTreeModel>();

            List<StudyCategoryModel> categoriesList = categoriesTask.Result;

            foreach (StudyCategoryModel category in categoriesList.Where(pi => pi.ParentStudyCategoryId == 0))
            {
                CategoryTreeModel resultItem = category.CopyToObject(new CategoryTreeModel()).To<CategoryTreeModel>();

                this.LoadTreeChildren(resultItem, categoriesList);

                result.Add(resultItem);
            }

            return result;
        }

        public void DeleteCategory(int studyCategoryId)
        {
            Task<StudyHeaderModel> headerTask = BiblesData.database
                .Table<StudyHeaderModel>()
                .FirstOrDefaultAsync(ca => ca.StudyCategoryId == studyCategoryId);

            if (headerTask.Result != null)
            {
                throw new ApplicationException("Cannot delete category whiles a study is attached to it. Please move the study before attempting to delete the category.");
            }
            
            Task<List<StudyCategoryModel>> currentCategories = BiblesData.database
                .Table<StudyCategoryModel>()
                .ToListAsync();

            StudyHeaderModel header = this.DeleteCategoryChildVerification(studyCategoryId, currentCategories.Result);

            if (header != null)
            {
                throw new ApplicationException("Cannot delete category whiles a study is attached to it. Please move the study before attempting to delete the category.");
            }

            this.DeleteChildCategoryTreeDown(studyCategoryId, currentCategories.Result);
        }

        private void DeleteChildCategoryTreeDown(int studyCategoryId, List<StudyCategoryModel> categoriesList)
        {
            BiblesData.database
                .Table<StudyCategoryModel>()
                .DeleteAsync(del => del.StudyCategoryId == studyCategoryId);

            foreach(StudyCategoryModel child in categoriesList.Where(dc => dc.ParentStudyCategoryId == studyCategoryId))
            {
                this.DeleteChildCategoryTreeDown(child.StudyCategoryId, categoriesList);
            }
        }

        private StudyHeaderModel DeleteCategoryChildVerification(int studyCategoryId, List<StudyCategoryModel> categoriesList)
        {
            if (categoriesList.Count == 0)
            {
                return null;
            }

            foreach(StudyCategoryModel child in categoriesList.Where(cl => cl.ParentStudyCategoryId == studyCategoryId))
            {
                Task<StudyHeaderModel> headerTask = BiblesData.database
                .Table<StudyHeaderModel>()
                .FirstOrDefaultAsync(ca => ca.StudyCategoryId == studyCategoryId);

                if (headerTask.Result != null)
                {
                    return headerTask.Result;
                }

                StudyHeaderModel header = this.DeleteCategoryChildVerification(child.StudyCategoryId, categoriesList);

                if (header != null)
                {
                    return header;
                }
            }

            return null;
        }

        private void LoadTreeChildren(CategoryTreeModel parent, List<StudyCategoryModel> categoriesList)
        {
            foreach(StudyCategoryModel child in categoriesList.Where(ci => ci.ParentStudyCategoryId == parent.StudyCategoryId))
            {
                CategoryTreeModel childItem = child.CopyToObject(new CategoryTreeModel()).To<CategoryTreeModel>();

                this.LoadTreeChildren(childItem, categoriesList);

                parent.ChildCategories.Add(childItem);
            }
        }

        #endregion

        #region STUDY SUBJECT HEADER

        public int InsertSubjectHeader(StudyHeaderModel header)
        {
            Task<StudyHeaderModel> existing = BiblesData.database
                .Table<StudyHeaderModel>()
                .FirstOrDefaultAsync(st => st.StudyHeaderId == header.StudyHeaderId);

            if (existing.Result == null)
            {
                Task<int>  result = BiblesData.database.InsertAsync(header);

                int resultCheck = result.Result;
                
                return header.StudyHeaderId;
            }
            else
            {
                existing.Result.StudyName = header.StudyName;

                existing.Result.Author = header.Author;

                existing.Result.StudyCategoryId = header.StudyCategoryId;

                BiblesData.database.UpdateAsync(existing.Result);

                return header.StudyHeaderId;
            }
        }

        public StudyHeaderModel GetStudyInCategory(string studyName, int studyCategoryId)
        {
            Task<StudyHeaderModel> result = BiblesData.database
                .Table<StudyHeaderModel>()
                .FirstOrDefaultAsync(ci => ci.StudyName == studyName
                                           && ci.StudyCategoryId == studyCategoryId);

            return result.Result;
        }
        public StudyHeaderModel GetStudyHeader(int studyHeaderId)
        {
            Task<StudyHeaderModel> result = BiblesData.database
                .Table<StudyHeaderModel>()
                .FirstOrDefaultAsync(ci => ci.StudyHeaderId == studyHeaderId);

            return result.Result;
        }

        public List<StudyHeaderModel> GetStudyHeaderByCategory(int categoryId)
        {
            Task<List<StudyHeaderModel>> result = BiblesData.database
                .Table<StudyHeaderModel>()
                .Where(ci => ci.StudyCategoryId == categoryId)                
                .ToListAsync();

            StudyHeaderModel[] resultArray = result.Result.ToArray();

            resultArray.SortLogically("StudyName");

            return resultArray.ToList();
        }


        #endregion

        #region STUDY CONTENT

        public StudyContentModel GetStudyContent(int studyHeaderId)
        {
            Task<StudyContentModel> taskContent = BiblesData.database
               .Table<StudyContentModel>()
               .FirstOrDefaultAsync(s => s.StudyHeaderId == studyHeaderId);

            return taskContent.Result;
        }

        public void InsertStudyContent(StudyContentModel content)
        {
            Task<StudyContentModel> taskContent = BiblesData.database
               .Table<StudyContentModel>()
               .FirstOrDefaultAsync(s => s.StudyHeaderId == content.StudyHeaderId);

            if (taskContent.Result == null)
            {
                BiblesData.database.InsertAsync(content);
            }
            else
            {
                taskContent.Result.Content = content.Content;

                BiblesData.database.UpdateAsync(taskContent.Result);
            }
        }

        #endregion

        #region LANGUAGE PREFERENCES

        public LanguageSetupModel GetLanguage(int languageId)
        {
            Task<LanguageSetupModel> result = BiblesData.database
                .Table<LanguageSetupModel>()
                .FirstOrDefaultAsync(li => li.LanguageId == languageId);

            return result.Result;
        }

        public LanguageSetupModel GetLanguage(string language)
        {
            Task<LanguageSetupModel> result = BiblesData.database
                .Table<LanguageSetupModel>()
                .FirstOrDefaultAsync(li => li.Language == language);

            return result.Result;
        }

        public List<LanguageSetupModel> GetLanguages()
        {
            Task<List<LanguageSetupModel>> result = BiblesData.database
                .Table<LanguageSetupModel>()
                .ToListAsync();

            return result.Result;
        }

        public List<TranslationMappingModel> GetTranslationMapping(int languageId)
        {
            Task<List<TranslationMappingModel>> result = BiblesData.database
                .Table<TranslationMappingModel>()
                .Where(li => li.LanguageId == languageId)
                .ToListAsync();

            return result.Result;
        }

        public int InsertLanguage(LanguageSetupModel language)
        {
            Task<LanguageSetupModel> existing = BiblesData.database
                .Table<LanguageSetupModel>()
                .FirstOrDefaultAsync(l => l.Language.ToLower() == language.Language.ToLower());

            if (existing.Result == null)
            {
                Task<int> response = BiblesData.database.InsertAsync(language);

                int responsevalue = response.Result;

                return language.LanguageId;
            }

            return existing.Result.LanguageId;
        }

        public int InsertTranslation(TranslationMappingModel translation)
        {
            Task<TranslationMappingModel> exiting = BiblesData.database
                .Table<TranslationMappingModel>()
                .FirstOrDefaultAsync(tr => tr.TranslationMappingId == translation.TranslationMappingId);

            if (exiting.Result == null)
            {
                Task<int> response = BiblesData.database.InsertAsync(translation);

                int responseValue = response.Result;

                return translation.TranslationMappingId;
            }

            exiting.Result.EnglishLanguage = translation.EnglishLanguage;

            exiting.Result.OtherLanguage = translation.OtherLanguage;

            BiblesData.database.UpdateAsync(exiting.Result);

            return exiting.Result.TranslationMappingId;
        }

        public void DeleteTranslationMapping(int translationMappingId)
        {
            BiblesData.database
                .Table<TranslationMappingModel>()
                .DeleteAsync(tm => tm.TranslationMappingId == translationMappingId);
        }

        #endregion

        #region CONCORDANCES

        public bool IsStrongsMapped()
        {
            var result = BiblesData.database.Table<StrongsVerseKeyModel>().FirstOrDefaultAsync();

            return result.Result != null;
        }

        public void InsertStrongsVerseKeysBulk(List<StrongsVerseKeyModel> verseList)
        {
            Task<int> result = BiblesData.database.InsertAllAsync(verseList);

            int check = result.Result;
        }

        public void InsertStrongsEntryModelBulk(List<StrongsEntryModel> entries)
        {
            Task<int> result = BiblesData.database.InsertAllAsync(entries);

            int check = result.Result;
        }

        public void InsertGreekEntryModelBulk(List<GreekEntryModel> entries)
        {
            Task<int> result = BiblesData.database.InsertAllAsync(entries);

            int check = result.Result;
        }

        public void InsertHebrewEntityModelBulk(List<HebrewEntityModel> entries)
        {
            Task<int> result = BiblesData.database.InsertAllAsync(entries);

            int check = result.Result;
        }

        public List<StrongsEntry> GetStrongsEnteries(string verseKey)
        {
            List<StrongsEntry> result = new List<StrongsEntry>();

            Task<List<StrongsVerseKeyModel>> verseKeysList = BiblesData.database
                .Table<StrongsVerseKeyModel>()
                .Where(vk => vk.VerseKey == verseKey)
                .ToListAsync();

            foreach(StrongsVerseKeyModel key in verseKeysList.Result)
            {
                string strongsKey = key.StrongsReference.Replace("H", "0").Replace("G", "0");

                Task<StrongsEntryModel> strongsEntry = BiblesData.database
                    .Table<StrongsEntryModel>()
                    .FirstOrDefaultAsync(sm => sm.StrongsNumber == strongsKey);

                if (strongsEntry.Result == null)
                {
                    continue;
                }

                StrongsEntry resultEntry = strongsEntry.Result.CopyToObject(new StrongsEntry()).To<StrongsEntry>();

                resultEntry.ReferencedText = key.ReferencedText;

                resultEntry.StrongsReference = key.StrongsReference;

                result.Add(resultEntry);
            }

            return result;
        }

        public List<StrongsEntry> GetGreekEnteries(string verseKey)
        {
            List<StrongsEntry> result = new List<StrongsEntry>();

            Task<List<StrongsVerseKeyModel>> verseKeysList = BiblesData.database
                .Table<StrongsVerseKeyModel>()
                .Where(vk => vk.VerseKey == verseKey)
                .ToListAsync();

            foreach (StrongsVerseKeyModel key in verseKeysList.Result)
            {
                string strongsKey = key.StrongsReference.Replace("H", "0").Replace("G", "0");

                Task<GreekEntryModel> greekEntry = BiblesData.database
                    .Table<GreekEntryModel>()
                    .FirstOrDefaultAsync(sm => sm.StrongsNumber == strongsKey);

                if (greekEntry.Result == null)
                {
                    continue;
                }

                StrongsEntry resultEntry = greekEntry.Result.CopyToObject(new StrongsEntry()).To<StrongsEntry>();

                resultEntry.ReferencedText = key.ReferencedText;

                resultEntry.StrongsReference = key.StrongsReference;

                result.Add(resultEntry);
            }

            return result;
        }

        public List<HebrewEntry> GetHebrewEnteries(string verseKey)
        {
            List<HebrewEntry> result = new List<HebrewEntry>();

            Task<List<StrongsVerseKeyModel>> verseKeysList = BiblesData.database
                .Table<StrongsVerseKeyModel>()
                .Where(vk => vk.VerseKey == verseKey)
                .ToListAsync();

            foreach (StrongsVerseKeyModel key in verseKeysList.Result)
            {
                string strongsKey = key.StrongsReference.Replace("G", "H");

                Task<HebrewEntityModel> greekEntry = BiblesData.database
                    .Table<HebrewEntityModel>()
                    .FirstOrDefaultAsync(sm => sm.StrongsNumber == strongsKey);

                if (greekEntry.Result == null)
                {
                    continue;
                }

                HebrewEntry resultEntry = greekEntry.Result.CopyToObject(new HebrewEntry()).To<HebrewEntry>();

                resultEntry.ReferencedText = key.ReferencedText;

                resultEntry.StrongsReference = key.StrongsReference;

                result.Add(resultEntry);
            }

            return result;
        }

        public List<StrongsVerse> GetStrongsVerseReferences(int biblesId, string strongsNumber)
        {
            string hebrewKey = $"H{(strongsNumber.Length == 5 ? strongsNumber.Substring(1) : strongsNumber)}";

            string greekKey = $"G{(strongsNumber.Length == 5 ? strongsNumber.Substring(1) : strongsNumber)}";
            
            Task<List<StrongsVerseKeyModel>> verseLinks = BiblesData.database
                .Table<StrongsVerseKeyModel>()
                .Where(vk => vk.StrongsReference == hebrewKey
                           ||vk.StrongsReference == greekKey)
                .ToListAsync();

            if (verseLinks.Result == null || verseLinks.Result.Count == 0)
            {
                return new List<StrongsVerse>();
            }
            
            List<StrongsVerse> result = new List<StrongsVerse>();

            result.AddRange(verseLinks.Result.Select(sv => new StrongsVerse             
            { 
                ReferencedText = sv.ReferencedText,
                StrongsReference = sv.StrongsReference,
                VerseKey = sv.VerseKey
            }));

            foreach(StrongsVerse verse in result)
            {
                string bibleVerseKey = $"{biblesId}||{verse.VerseKey}";

                BibleVerseModel bibleVers = this.GetVerse(bibleVerseKey);

                if (bibleVers == null)
                {
                    continue;
                }

                verse.VerseText = bibleVers.VerseText;

                verse.VerseNumber = verse.InvokeMethod("Bibles.Data.GlobalInvokeData,Bibles.Data", "GetKeyDescription", new object[] { verse.VerseKey, 0 }).ParseToString();
            }

            return result;
        }

        public void TruncateStrongs()
        {
            Task<List<StrongsEntryModel>> deleteResult = BiblesData.database.QueryAsync<StrongsEntryModel>("DELETE FROM StrongsEntryModel", new object[] { });

            List<StrongsEntryModel> result = deleteResult.Result;
        }

        public void TruncateGreekLexicon()
        {
            Task<List<GreekEntryModel>> deleteResult = BiblesData.database.QueryAsync<GreekEntryModel>("DELETE FROM GreekEntryModel", new object[] { });

            List<GreekEntryModel> result = deleteResult.Result;
        }

        //HebrewEntityModel
        public void TruncateHebrewLexicon()
        {
            Task<List<HebrewEntityModel>> deleteResult = BiblesData.database.QueryAsync<HebrewEntityModel>("DELETE FROM HebrewEntityModel", new object[] { });

            List<HebrewEntityModel> result = deleteResult.Result;
        }

        #endregion

        #region HAVE INSTALLED

        public bool IsInstalled(HaveInstalledEnum entity)
        {
            int entityModel = (int)entity;

            Task<HaveInstalledModel> result = BiblesData.database
                .Table<HaveInstalledModel>()
                .FirstOrDefaultAsync(em => em.EntityModel == entityModel);

            return result.Result != null;
        }

        public void InstallEntity(HaveInstalledEnum entity)
        {
            if (this.IsInstalled(entity))
            {
                return;
            }

            HaveInstalledModel haveInstlled = new HaveInstalledModel
            {
                EntityModel = (int)entity
            };

            Task<int> result = BiblesData.database.InsertAsync(haveInstlled);

            int check = result.Result;
        }

        #endregion

        private async Task InitializeAsync()
        {
            if (!BiblesData.IsInitialized)
            {
                if (!database.TableMappings.Any(bi => bi.MappedType.Name == typeof(BibleModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(BibleModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(bv => bv.MappedType.Name == typeof(BibleVerseModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.FullTextSearch4, typeof(BibleVerseModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(up => up.MappedType.Name == typeof(UserPreferenceModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(UserPreferenceModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(bm => bm.MappedType.Name == typeof(BookmarkModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.None, typeof(BookmarkModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(li => li.MappedType.Name == typeof(LinkModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.ImplicitIndex, typeof(LinkModel)).ConfigureAwait(false);
                }
                   
                if (!database.TableMappings.Any(hl => hl.MappedType.Name == typeof(HighlightVerseModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.ImplicitIndex, typeof(HighlightVerseModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(sc => sc.MappedType.Name == typeof(StudyCategoryModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(StudyCategoryModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(sh => sh.MappedType.Name == typeof(StudyHeaderModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(StudyHeaderModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(StudyContentModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.None, typeof(StudyContentModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(StudyBookmarkModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.None, typeof(StudyBookmarkModel)).ConfigureAwait(false);
                }
                
                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(LanguageSetupModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(LanguageSetupModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(TranslationMappingModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(TranslationMappingModel)).ConfigureAwait(false);
                }
                                
                
                if (!database.TableMappings.Any(enm => enm.MappedType.Name == typeof(HaveInstalledModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.None, typeof(HaveInstalledModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(StrongsVerseKeyModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.AutoIncPK, typeof(StrongsVerseKeyModel)).ConfigureAwait(false);
                }

                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(StrongsEntryModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.None, typeof(StrongsEntryModel)).ConfigureAwait(false);
                }

                // GreekEntryModel
                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(GreekEntryModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.None, typeof(GreekEntryModel)).ConfigureAwait(false);
                }

                // HebrewEntityModel
                if (!database.TableMappings.Any(scon => scon.MappedType.Name == typeof(HebrewEntityModel).Name))
                {
                    await database.CreateTablesAsync(CreateFlags.None, typeof(HebrewEntityModel)).ConfigureAwait(false);
                }

                BiblesData.IsInitialized = true;
            }
        }
    }
}
