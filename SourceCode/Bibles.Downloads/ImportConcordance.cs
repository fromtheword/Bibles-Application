using Bibles.DataResources;
using Bibles.DataResources.Aggregates;
using Bibles.DataResources.DataEnums;
using GeneralExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Bibles.Downloads
{
    internal class ImportConcordance
    {
        internal bool Import(string fullFileName)
        {
            string fileName = Path.GetFileNameWithoutExtension(fullFileName);

            switch(fileName)
            {
                case "Strongs Concordance":

                    return this.ImportStrongs(fullFileName);

                case "Greek Lexicon":

                    return this.ImportGreekLexicon(fullFileName);

                case "Hebrew Lexicon":

                    return this.ImportHebrewLexicon(fullFileName);
            }

            return false;
        }

        private bool ImportStrongs(string fullFileName)
        {
            BiblesData.Database.TruncateStrongs();

            XDocument doc = XDocument.Load(fullFileName);

            List<StrongsEntryModel> strongsEntries = new List<StrongsEntryModel>();

            foreach (XElement entry in doc.Root.Descendants("entry"))
            {
                strongsEntries.Add(new StrongsEntryModel
                {
                    StrongsNumber = entry.GetAttributeValue("strongs"),
                    Entry = entry.GetValue(doc),
                    GreekBeta = entry.Element("greek").GetAttributeValue("BETA"),
                    GreekUnicode = entry.Element("greek").GetAttributeValue("unicode"),
                    GreekTranslit = entry.Element("greek").GetAttributeValue("translit"),
                    Pronunciation = entry.Element("pronunciation").GetAttributeValue("strongs"),
                    Derivation = entry.GetValueFromChildStandardStrongs(doc, "strongs_derivation"),
                    StrongsDefinition = entry.GetValueFromChildStandardStrongs(doc, "strongs_def"),
                    KingJamesDefinition = entry.GetValueFromChildStandardStrongs(doc, "kjv_def"),
                });
            }
            
            int skipIndex = 0;

            int takeValue = 500;

            while (skipIndex <= strongsEntries.Count)
            {
                List<StrongsEntryModel> addList = strongsEntries.Skip(skipIndex).Take(takeValue).ToList();

                BiblesData.Database.InsertStrongsEntryModelBulk(addList);

                skipIndex += takeValue;
            }

            BiblesData.Database.InstallEntity(HaveInstalledEnum.StrongsEntryModel);

            return true;
        }

        private bool ImportGreekLexicon(string fullFileName)
        {
            BiblesData.Database.TruncateGreekLexicon();

            XDocument doc = XDocument.Load(fullFileName);

            List<GreekEntryModel> strongsEntries = new List<GreekEntryModel>();

            foreach (XElement entry in doc.Root.Descendants("entry"))
            {
                strongsEntries.Add(new GreekEntryModel
                {
                    StrongsNumber = entry.GetAttributeValue("strongs"),
                    Entry = entry.GetValue(doc),
                    GreekBeta = entry.Element("greek").GetAttributeValue("BETA"),
                    GreekUnicode = entry.Element("greek").GetAttributeValue("unicode"),
                    GreekTranslit = entry.Element("greek").GetAttributeValue("translit"),
                    Pronunciation = entry.Element("pronunciation").GetAttributeValue("strongs"),
                    Derivation = entry.GetValueFromChildStandardStrongs(doc, "strongs_derivation"),
                    StrongsDefinition = entry.GetValueFromChildStandardStrongs(doc, "strongs_def"),
                    KingJamesDefinition = entry.GetValueFromChildStandardStrongs(doc, "kjv_def"),
                });
            }

            int skipIndex = 0;

            int takeValue = 500;

            while (skipIndex <= strongsEntries.Count)
            {
                List<GreekEntryModel> addList = strongsEntries.Skip(skipIndex).Take(takeValue).ToList();

                BiblesData.Database.InsertGreekEntryModelBulk(addList);

                skipIndex += takeValue;
            }

            BiblesData.Database.InstallEntity(HaveInstalledEnum.GreekEntryModel);

            return true;
        }
    
        private bool ImportHebrewLexicon(string fullFileName)
        {
            BiblesData.Database.TruncateHebrewLexicon();

            XDocument doc = XDocument.Load(fullFileName, LoadOptions.SetBaseUri);

            doc.Descendants()
                .Attributes()
                .Where(x => x.IsNamespaceDeclaration)
                .Remove();

            List<HebrewEntityModel> strongsEntries = new List<HebrewEntityModel>();

            foreach (XElement entry in doc.Root.Elements())
            {
                entry.RemoveNamespaces();

                string id = entry.GetAttributeValue("id");

                strongsEntries.Add(new HebrewEntityModel
                {
                    StrongsNumber = $"{id.Substring(0, 1)}{id.Substring(1).PadLeft(4, '0')}",
                    Note = entry.GetValueFromChildHebrewLexicon(doc, "note"),
                    Source = entry.GetValueFromChildHebrewLexicon(doc, "source"),
                    Meaning = entry.GetValueFromChildHebrewLexicon(doc, "meaning"),
                    Usage = entry.GetValueFromChildHebrewLexicon(doc, "usage"),
                    Pos = entry.Element("w").GetAttributeValue("pos"),
                    Pron = entry.Element("w").GetAttributeValue("pron"),
                    Xlit = entry.Element("w").GetAttributeValue("xlit"),
                    Language = entry.Element("w").GetNamespaceAttributeValue("xml:lang")
                });
            }

            int skipIndex = 0;

            int takeValue = 500;

            while (skipIndex <= strongsEntries.Count)
            {
                List<HebrewEntityModel> addList = strongsEntries.Skip(skipIndex).Take(takeValue).ToList();

                BiblesData.Database.InsertHebrewEntityModelBulk(addList);

                skipIndex += takeValue;
            }

            BiblesData.Database.InstallEntity(HaveInstalledEnum.HebrewEntityModel);

            return true;
        }
    }
}
