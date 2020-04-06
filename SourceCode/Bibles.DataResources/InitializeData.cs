using Bibles.DataResources.Aggregates;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using WPF.Tools.Functions;

namespace Bibles.DataResources
{
    public class InitializeData
    {
        private string[] bibleNames = new string[] { "English King James Version", "Xhosa" };

        private string systemDefaultbible = "English King James Version";

        public delegate void InitialDataLoadCompletedEvent(object sender, string message, bool completed, Exception error);

        public event InitialDataLoadCompletedEvent InitialDataLoadCompleted;
        
        public async void LoadEmbeddedBibles(Dispatcher dispatcher, FontFamily defaultFont)
        {
            Task<List<BibleModel>> loadedBiles = BiblesData.Database.GetBibles();
            
            await Task.Run(() => 
            {                
                try
                {
                    #region LOAD BIBLES

                    if (loadedBiles.Result.Count == 0)
                    {
                        foreach (string bible in bibleNames)
                        {
                            dispatcher.Invoke(() =>
                            {
                                this.InitialDataLoadCompleted?.Invoke(this, $"Loading...{bible}", false, null);
                            });

                            BibleModel bibleModel = loadedBiles.Result.FirstOrDefault(l => l.BibleName == bible);

                            if (bibleModel == null)
                            {
                                bibleModel = new BibleModel
                                {
                                    BiblesId = 0,
                                    BibleName = bible
                                };

                                BiblesData.Database.InsertBible(bibleModel);

                                BibleModel added = BiblesData.Database.GetBible(bible);

                                while (added == null)
                                {
                                    Sleep.ThreadWait(100);

                                    added = BiblesData.Database.GetBible(bible);
                                }

                                bibleModel.BiblesId = added.BiblesId;
                            }

                            this.LoadBibleVerses(dispatcher, bibleModel);

                            if (bible == this.systemDefaultbible)
                            {
                                UserPreferenceModel userPref = new UserPreferenceModel
                                {
                                    DefaultBible = bibleModel.BiblesId,
                                    Font = defaultFont.ParseToString(),
                                    FontSize = 12,
                                    SynchronizzeTabs = false,
                                    LanguageId = 0,
                                    LastReadVerse = $"{bibleModel.BiblesId}||01O||1||1||"
                                };

                                BiblesData.Database.InsertPreference(userPref);
                            }
                        }
                    }

                    #endregion

                    this.LoadStrongsConcordance(dispatcher);
                }
                catch (Exception err)
                {
                    dispatcher.Invoke(() =>
                    {
                        this.InitialDataLoadCompleted?.Invoke(this, string.Empty, false, err);
                    });
                }

                dispatcher.Invoke(() =>
                {
                    this.InitialDataLoadCompleted?.Invoke(this, string.Empty, true, null);
                });
            });
        }
        
        private void LoadBibleVerses(Dispatcher dispatcher, BibleModel bibleModel)
        {
            string bibleFormatName = bibleModel.BibleName
                .Replace(' ', '_')
                .Replace('-', '_');

            var bible = typeof(Properties.Resources)
              .GetProperties(BindingFlags.Static | BindingFlags.NonPublic |
                             BindingFlags.Public)
              .Where(p => p.PropertyType == typeof(string) && p.Name == bibleFormatName)
              .Select(x => new { Bible = x.GetValue(null, null) })
              .FirstOrDefault();

            List<string> verses = bible.Bible
                .ParseToString()
                .Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries)
                .ToList();


            List<BibleVerseModel> bulkList = new List<BibleVerseModel>();

            foreach (string verseLine in verses)
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

            while(skipIndex <= bulkList.Count)
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
                        dispatcher.Invoke(() =>
                        {
                            this.InitialDataLoadCompleted?.Invoke(this, $"Loading...{bibleModel.BibleName}", false, null);
                        });

                        foreach (BibleVerseModel verseModel in addList)
                        {
                            BiblesData.Database.InsertBibleVerse(verseModel);
                        }
                    }
                }
            }                   
        }

        private void LoadStrongsConcordance(Dispatcher dispatcher)
        {
            if (BiblesData.Database.IsStrongsMapped())
            {
                return;
            }

            #region MAPPED VERSES

            dispatcher.Invoke(() =>
            {
                this.InitialDataLoadCompleted?.Invoke(this, "Loading Concordance Links", false, null);
            });

            var mappedVersesText = typeof(Properties.Resources)
                  .GetProperties(BindingFlags.Static | BindingFlags.NonPublic |
                                 BindingFlags.Public)
                  .Where(p => p.PropertyType == typeof(string) && p.Name == "StrongsVerseMapping")
                  .Select(x => new { Map = x.GetValue(null, null) })
                  .FirstOrDefault();

            List<string> mapedVerseList = mappedVersesText.Map
                  .ParseToString()
                  .Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries)
                  .ToList();

            char[] charSplitArray = new char[] { '*' };

            List<StrongsVerseKeyModel> versesKeys = new List<StrongsVerseKeyModel>();

            foreach (string line in mapedVerseList)
            {
                string[] lineSplit = line.Split(charSplitArray, StringSplitOptions.None);

                versesKeys.Add(new StrongsVerseKeyModel
                {
                    VerseKey = lineSplit[0],
                    StrongsReference = lineSplit[1],
                    ReferencedText = lineSplit[2]
                });
            }

            dispatcher.Invoke(() =>
            {
                this.InitialDataLoadCompleted?.Invoke(this, "Inserting Concordance Links", false, null);
            });

            int skipIndex = 0;

            int takeValue = 500;

            while (skipIndex <= versesKeys.Count)
            {
                List<StrongsVerseKeyModel> addList = versesKeys.Skip(skipIndex).Take(takeValue).ToList();

                BiblesData.Database.InsertStrongsVerseKeysBulk(addList);

                skipIndex += takeValue;
            }

            #endregion
            
            dispatcher.Invoke(() =>
            {
                this.InitialDataLoadCompleted?.Invoke(this, "Completed", false, null);
            });
        }
    }
}
