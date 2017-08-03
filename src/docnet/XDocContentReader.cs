using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace docnet
{
    internal class XDocContentReader : XDocReaderBase, IDocContent
    {
        public XDocContentReader(XmlReader reader) : base(reader)
        {
            Topic = reader.Name;

            Name = ReadAttributeValue("name") ?? ReadAttributeValue("cref");
        }

        public string Topic { get; private set; }

        public string Name { get; private set;  }

        public string ToMarkdown()
        {
            throw new NotImplementedException();
        }
    }
}
