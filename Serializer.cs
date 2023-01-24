using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace Query.Common
{
    public static class Serializer
    {
        public static XmlDocument ToXmlDocument(object input)
        {
            XmlSerializer ser = new XmlSerializer(input.GetType());
            XmlDocument xd = null;

            using (var sw = new StringWriter())
            {
                ser.Serialize(sw, input);
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(new StringReader(sw.ToString()), settings))
                {
                    xd = new XmlDocument();
                    xd.Load(xtr);
                }
            }

            return xd;
        }
    }
}
