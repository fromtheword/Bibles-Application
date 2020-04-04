using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace GeneralExtensions
{
    public static class XmlExstentions
    {
        public static string GetAttributeValue(this XElement element, string attributeName)
        {
            if (element == null || element.Attribute(attributeName) == null)
            {
                return string.Empty;
            }

            return element.Attribute(attributeName).Value;
        }

        public static string GetValueFromChild(this XElement parent, XDocument masterDocument, string childNodeName)
        {
            if (parent.Element(childNodeName) == null)
            {
                return string.Empty;
            }

            foreach (XElement child in parent.Element(childNodeName).Elements("greek"))
            {
                child.Value = child.Attribute("translit").Value;
            }

            foreach (XElement child in parent.Element(childNodeName).Elements("strongsref"))
            {
                if (child.Name.LocalName == "latin")
                {
                    continue;
                }

                string strongs = child.GetAttributeValue("strongs").PadLeft(5, '0');

                string language = child.GetAttributeValue("language");

                XElement referenceEntity = masterDocument.Root.Element("entries").Descendants("entry").FirstOrDefault(e => e.Attribute("strongs").Value == strongs);

                string kjvDef = referenceEntity == null || referenceEntity.Element("kjv_def") == null ? string.Empty : referenceEntity.Element("kjv_def").Value;

                child.Value = $"({strongs}) ({language}) {kjvDef}";
            }

            return parent.Element(childNodeName).Value;
        }

        public static string GetValue(this XElement element, XDocument masterDocument)
        {
            List<string> textValues = element.Nodes()
                        .Where(n => n.NodeType == XmlNodeType.Text)
                        .Select(n => n.ToString().Trim())
                        .ToList();

            if (textValues.Count == 0)
            {
                return string.Empty;
            }

            foreach (XElement child in element.Elements("greek"))
            {
                child.Value = child.Attribute("translit").Value;
            }

            foreach (XElement child in element.Elements("strongsref"))
            {
                if (child.Name.LocalName == "latin")
                {
                    continue;
                }

                string strongs = child.GetAttributeValue("strongs").PadLeft(5, '0');

                string language = child.GetAttributeValue("language");

                XElement referenceEntity = masterDocument.Root.Element("entries").Descendants("entry").FirstOrDefault(e => e.Attribute("strongs").Value == strongs);

                string kjvDef = referenceEntity == null || referenceEntity.Element("kjv_def") == null ? string.Empty : referenceEntity.Element("kjv_def").Value;

                child.Value = kjvDef;
            }

            string rawValue = element.Value.Replace("\n", string.Empty).Replace("\r", string.Empty);

            int indexOfValue = rawValue.IndexOf(textValues[0].Replace("\r", string.Empty).Replace("\n", string.Empty));

            return rawValue.Substring(indexOfValue);
        }
    }
}
