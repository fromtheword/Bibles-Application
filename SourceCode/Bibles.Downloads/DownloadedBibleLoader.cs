using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WPF.Tools.Functions;

namespace Bibles.Downloads
{
    internal class DownloadedBibleLoader
    {
        internal static bool LoadBible(string path)
        {
            string bibleName = Path.GetFileNameWithoutExtension(path);

            BibleModel bibleModel = BiblesData.Database.GetBible(bibleName);

            if (bibleModel != null)
            {
                return true;
            }

            bibleModel = new BibleModel
            {
                BiblesId = 0,
                BibleName = bibleName
            };

            BiblesData.Database.InsertBible(bibleModel);

            BibleModel added = BiblesData.Database.GetBible(bibleName);

            while (added == null)
            {
                Sleep.ThreadWait(100);

                added = BiblesData.Database.GetBible(bibleName);
            }

            bibleModel.BiblesId = added.BiblesId;

            List<string> bibleVerses = new List<string>();

            List<BibleVerseModel> bulkList = new List<BibleVerseModel>();

            bibleVerses.AddRange(File.ReadAllLines(path));

            foreach (string verseLine in bibleVerses)
            {
                int breakIndex = verseLine.LastIndexOf("||") + 2;

                string verseKey = verseLine.Substring(0, breakIndex);

                string verseText = verseLine.Substring(breakIndex, verseLine.Length - breakIndex);

                BibleVerseModel verseModel = new BibleVerseModel
                {
                    BibleVerseKey = $"{bibleModel.BiblesId}||{verseKey}",
                    VerseText = verseText
                };

                bulkList.Add(verseModel);
            }

            int skipIndex = 0;

            int takeValue = 500;

            try
            {
                while (skipIndex <= bulkList.Count)
                {
                    List<BibleVerseModel> addList = bulkList.Skip(skipIndex).Take(takeValue).ToList();

                    BiblesData.Database.InsertBibleVerseBulk(addList);

                    skipIndex += takeValue;

                    string checkKey = addList.Last().BibleVerseKey;

                    int waitIndex = 0;

                    while (BiblesData.Database.GetVerse(checkKey) == null)
                    {
                        Sleep.ThreadWait(200);

                        ++waitIndex;

                        if (waitIndex >= 20)
                        {
                            foreach (BibleVerseModel verseModel in addList)
                            {
                                BiblesData.Database.InsertBibleVerse(verseModel);
                            }
                        }
                    }
                }
            }
            catch (Exception err)
            {
                throw;
            }

            return true;
        }
    }
}
