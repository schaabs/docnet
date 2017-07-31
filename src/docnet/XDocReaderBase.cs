using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace docnet
{
    internal abstract class XDocReaderBase
    {
        protected XmlReader _reader;

        protected XDocReaderBase(XmlReader reader)
        {
            _reader = reader;
        }

        protected string ReadAttributeValue(string attrName)
        {
            return ReadAttributeValue(_reader, attrName);
        }

        protected static string ReadAttributeValue(XmlReader reader, string attrName)
        {

            for (int i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);

                if (reader.Name == attrName)
                {
                    reader.MoveToElement();

                    return reader.Value;
                }
            }

            reader.MoveToElement();

            return null;
        }
    }
}
