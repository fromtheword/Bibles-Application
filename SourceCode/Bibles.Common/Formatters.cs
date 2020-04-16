using GeneralExtensions;
using System;
using System.Collections.Generic;

namespace Bibles.Common
{
    public static class Formatters
    {
        public readonly static string[] KeySplitValue = new string[] { "||" };

        public static bool IsBiblesKey(string key)
        {
            string[] keyItems = null;

            return Formatters.IsBiblesKey(key, out keyItems);
        }
        
        public static bool IsBiblesKey(string key, out string[] keyItems)
        {
            if (key.IsNullEmptyOrWhiteSpace())
            {
                keyItems = new string[] { };

                return false;
            }

            keyItems = key.Split(Formatters.KeySplitValue, StringSplitOptions.RemoveEmptyEntries);

            return Formatters.IsBiblesKey(keyItems);
        }

        public static bool IsBiblesKey(string[] keyItems)
        {
            if (keyItems.Length == 1)
            {
                return false;
            }

            // Bible Key's start with the Bible index First
            // Book Key's constains an O or N for Old or New Testaments
            return keyItems[0].IsNumberic() && !keyItems[1].IsNumberic();
        }

        public static int GetBibleFromKey(string bibleKey)
        {
            if (bibleKey.IsNullEmptyOrWhiteSpace()
                || !Formatters.IsBiblesKey(bibleKey))
            {
                return -1;
            }

            string[] keySplit = bibleKey.Split(Formatters.KeySplitValue, StringSplitOptions.RemoveEmptyEntries);

            return keySplit.Length >= 1 ? keySplit[0].ToInt32() : -1;
        }
        
        public static int GetChapterFromKey(string bibleKey)
        {
            if (bibleKey.IsNullEmptyOrWhiteSpace())
            {
                return -1;
            }

            string[] keySplit = null;

            bool isBibleKey = Formatters.IsBiblesKey(bibleKey, out keySplit);

            return isBibleKey ? 
                keySplit.Length >= 3 ? keySplit[2].ToInt32() : 1
                :
                keySplit.Length >= 2 ? keySplit[1].ToInt32() : 1;
        }

        public static int GetVerseFromKey(string bibleKey)
        {
            string[] keySplit = Formatters.CreateBibleKeySplit(bibleKey);
         
            if(keySplit.Length == 3 && keySplit[0].IsNumberic())
            {
                return 1;
            }

            return keySplit.Length >= 4 ? keySplit[3].ToInt32() : 1;
        }
        
        public static string GetBookFromKey(string bibleKey)
        {
            string[] keySplit = null;

            bool isBibleKey = Formatters.IsBiblesKey(bibleKey, out keySplit);

            return isBibleKey ?
                keySplit.Length >= 2 ? keySplit[1] : string.Empty
                :
                keySplit.Length >= 1 ? keySplit[0] : string.Empty;
        }

        public static string ChangeBible(string bibleKey, int bibleId)
        {
            string[] splitkeys = null;

            if (Formatters.IsBiblesKey(bibleKey, out splitkeys))
            {
                splitkeys[0] = bibleId.ToString();

                return splitkeys.Concatenate("||");
            }

            List<string> splitList = new List<string>();

            splitList.Add(bibleId.ToString());

            splitList.AddRange(splitkeys);

            return splitList.ToArray().Concatenate("||");
        }

        public static string ChangeChapter(string bibleKey, int nextChapter)
        {
            string[] splitkeys = null;

            if (Formatters.IsBiblesKey(bibleKey, out splitkeys))
            {
                splitkeys[2] = nextChapter.ToString();

                return splitkeys.Concatenate("||");
            }

            splitkeys[1] = nextChapter.ToString();

            return splitkeys.Concatenate("||");
        }

        public static string ChangeVerse(string bibleKey, int nextVerse)
        {
            string[] keyItems = null;

            bool isBibleKey = Formatters.IsBiblesKey(bibleKey, out keyItems);

            if ((isBibleKey && keyItems.Length < 4)
                || (!isBibleKey && keyItems.Length < 3))
            {
                return bibleKey;
            }

            keyItems[keyItems.Length - 1] = nextVerse.ToString();

            return keyItems.Concatenate("||");
        }
        
        public static string RemoveBibleId(string bibleKey)
        {
            string[] keyItems = null;

            if (!Formatters.IsBiblesKey(bibleKey, out keyItems))
            {
                return bibleKey;
            }

            return $"{keyItems[1]}||{keyItems[2]}||{(keyItems.Length <= 3 ? "1" : keyItems[3])}||";
        }
    
        public static string[] CreateBibleKeySplit(string key)
        {
            string[] keySplit = null;

            bool isBibleKey = Formatters.IsBiblesKey(key, out keySplit);

            if (isBibleKey)
            {
                return keySplit;
            }

            if (keySplit.Length == 2)
            {
                return new string[] { "Empty", keySplit[0], keySplit[1], "1" };
            }

            return new string[] { "Empty", keySplit[0], keySplit[1], keySplit[2] };
        }
    }
}
