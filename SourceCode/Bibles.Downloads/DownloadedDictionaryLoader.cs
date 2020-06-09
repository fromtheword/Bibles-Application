using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.DataEnums;
using GeneralExtensions;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bibles.Downloads
{
    public class DownloadedDictionaryLoader
    {
        public static bool InstallDictionary(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);

            HaveInstalledEnum? entity = null;

            switch(fileName)
            {
                case "Life More Abundant – Interpreting Bible Prophecy":
                    entity = HaveInstalledEnum.LifeMoreAbundant;

                    break;

                default: return false;
            }

            DownloadedDictionaryLoader.InsertDictionary(entity.Value, filePath);

            return true;
        }

        private static void InsertDictionary(HaveInstalledEnum entity, string filePath)
        {
            int entityKeyId = (int)entity;

            string fileText = File.ReadAllText(filePath);

            string[] lineSplitOptions = new string[] { "||" };

            string[] fileLines = fileText.Split(new string[] { "|||" }, System.StringSplitOptions.RemoveEmptyEntries);

            Dictionary<string, DictionaryModel> result = new Dictionary<string, DictionaryModel>();

            foreach(string line in fileLines)
            {
                string[] lineSplit = line.Split(lineSplitOptions, System.StringSplitOptions.RemoveEmptyEntries);

                if (lineSplit.Length != 2
                    || lineSplit[0].IsNullEmptyOrWhiteSpace()
                    || lineSplit[1].IsNullEmptyOrWhiteSpace()
                    || result.ContainsKey(lineSplit[0]))
                {
                    // This should not be
                    continue;
                }

                DictionaryModel model = new DictionaryModel
                {
                    EntityModelKey = $"{entityKeyId}||{lineSplit[0]}",
                    EntityValue = lineSplit[1].ZipFile()
                };

                result.Add(lineSplit[0], model);
            }

            BiblesData.Database.DeleteDictionary(entity);

            int skipIndex = 0;

            int takeValue = 100;

            while (skipIndex <= result.Count)
            {
                BiblesData.Database.InsertDictionaryBulk(result.Values.Skip(skipIndex).Take(takeValue).ToList());

                skipIndex += takeValue;
            }

            BiblesData.Database.InstallEntity(entity);
        }
    }
}
