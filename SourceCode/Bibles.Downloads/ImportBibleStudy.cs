using Bibles.Common;
using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace Bibles.Downloads
{
    public class ImportBibleStudy
    {
        public bool ImportStudy(string fileName)
        {
            try
            {
                StudyHeaderModel studyHeader = null;

                Dictionary<int, string> categoryNames = new Dictionary<int, string>();

                Dictionary<int, int> categoryIdMapping = new Dictionary<int, int>();

                List<StudyBookmarkModel> bookmarksList = new List<StudyBookmarkModel>();

                List<string> fileLines = new List<string>();

                fileLines.AddRange(File.ReadAllLines(fileName));

                int constantIndex = 0;

                foreach (string line in fileLines)
                {
                    #region ACTION SWITCH

                    if (line == Constants.StudyBookarkMark)
                    {
                        constantIndex = 1;

                        continue;
                    }
                    else if (line == Constants.StudyMark)
                    {
                        constantIndex = 2;

                        continue;
                    }
                    else if (line == Constants.StudyContentMark)
                    {
                        constantIndex = 3;

                        continue;
                    }

                    #endregion

                    if (constantIndex == 0)
                    {
                        #region CATEGORIES

                        StudyCategoryModel category = JsonConvert.DeserializeObject(line, typeof(StudyCategoryModel)).To<StudyCategoryModel>();

                        categoryNames.Add(category.StudyCategoryId, category.CategoryName);

                        StudyCategoryModel existing = BiblesData.Database.GetCategory(category.CategoryName);

                        if (existing == null)
                        {
                            int parentId = categoryIdMapping.ContainsKey(category.ParentStudyCategoryId) ?
                                categoryIdMapping[category.ParentStudyCategoryId]
                                :
                                0;

                            int newId = BiblesData.Database.InsertCategory(category.CategoryName, parentId);

                            categoryIdMapping.Add(category.StudyCategoryId, newId);
                        }
                        else
                        {
                            categoryIdMapping.Add(category.StudyCategoryId, existing.StudyCategoryId);
                        }

                        #endregion
                    }
                    else if (constantIndex == 1)
                    {
                        #region BOOKMARKS

                        StudyBookmarkModel bookmark = JsonConvert.DeserializeObject(line, typeof(StudyBookmarkModel)).To<StudyBookmarkModel>();

                        bookmarksList.Add(bookmark);

                        #endregion
                    }
                    else if (constantIndex == 2)
                    {
                        #region STUDY HEADER

                        studyHeader = JsonConvert.DeserializeObject(line, typeof(StudyHeaderModel)).To<StudyHeaderModel>();

                        studyHeader.StudyCategoryId = categoryIdMapping[studyHeader.StudyCategoryId];

                        StudyHeaderModel existing = BiblesData.Database.GetStudyInCategory(studyHeader.StudyName, studyHeader.StudyCategoryId);

                        if (existing != null)
                        {
                            studyHeader = existing.CopyTo(studyHeader);
                        }

                        studyHeader.StudyHeaderId = BiblesData.Database.InsertSubjectHeader(studyHeader);

                        #endregion

                        #region INSERT BOOKMARKS

                        foreach (StudyBookmarkModel bookmark in bookmarksList)
                        {
                            bookmark.StudyVerseKey = $"{studyHeader.StudyHeaderId}||{bookmark.VerseKey}";

                            BiblesData.Database.InsertStudyBookmarkModel(bookmark);
                        }

                        #endregion
                    }
                    else if (constantIndex == 3)
                    {
                        #region STUDY CONTENT

                        StudyContentModel content = JsonConvert.DeserializeObject(line, typeof(StudyContentModel)).To<StudyContentModel>();

                        content.StudyHeaderId = studyHeader.StudyHeaderId;

                        BiblesData.Database.InsertStudyContent(content);

                        #endregion
                    }
                }
            }
            catch (Exception err)
            {
                ErrorLog.ShowError(err);

                return false;
            }

            return true;
        }
    }
}
