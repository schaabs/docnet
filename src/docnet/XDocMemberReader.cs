﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace docnet
{
    internal class XDocReader : XDocReaderBase
    {
        private Dictionary<string, XDocTopicReader> _topicDict;

        public XDocMemberReader(XmlReader reader) : base(reader)
        {
            this.XDocId = ReadAttributeValue(reader, "name");
        }

        public string XDocId { get; set; }

        protected async Task ReadContentElementsAsync()
        {
            while (await _reader.ReadAsync())
            {
                if (_reader.IsStartElement())
                {
                    AddContent(new XDocMemberContentReader(_reader.ReadSubtree()));
                }
            }
        }

        private void AddTopic(XmlReader content)
        {
            XDocTopicReader topicList = null;

            if (!_contentDict.TryGetValue(content.Topic, out topicList))
            {
                topicList = new List<XDocMemberContentReader>();

                _contentDict[content.Topic] = topicList;
            }

            topicList.Add(content);
        }

    }
}
