using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace docnet
{
    internal static class MD
    {
        public static string Bold(string str)
        {
            return $"**{str}**";
        }
    }

    internal class Doc
    {
        protected Doc()
        {
        }

        public static Doc Get(string docId)
        {
            if(!Index.TryGetValue(docId, out Doc doc))
            {
                doc = CreateDoc(docId);
            }

            return doc;
        }
       
        public static string GetLink(string docId)
        {
            return Index.TryGetValue(docId, out Doc doc) ? doc.Link : MD.Bold(docId);
        }

        public static Dictionary<string, Doc> Index;

        public string DocId { get; private set; }

        public MemberInfo Member { get; set; }

        public string Name { get { return Member?.Name; } }

        public string Link { get { return $"[{Name}]({Path})"; } }

        public string Path { get; protected set; }

        public void AddContent(IDocContent content)
        {
            if(!Topics.TryGetValue(content.Topic, out DocTopic topic))
            {
                topic = new DocTopic(content);

                Topics[content.Topic] = topic;
            }
        }

        public Dictionary<string, DocTopic> Topics { get; private set; }

        private static Doc CreateDoc(string docId)
        {
            Doc doc = null;

            switch(docId[0])
            {
                case 'T':
                    doc = new ClassDoc();
                    break;
                case 'M':
                    doc = new MethodDoc();
                    break;
                case 'F':
                    doc = new FieldDoc();
                    break;
                case 'P':
                    doc = new PropertyDoc();
                    break;
                case 'E':
                    doc = new EventDoc();
                    break;
            }

            Index[docId] = doc;

            return doc;
        }
    }
    
    internal sealed class ClassDoc : Doc
    {
        public ClassDoc()
        {

        }
    }
    internal sealed class MethodDoc : Doc
    {
        public MethodDoc()
        {

        }
    }
    internal sealed class FieldDoc : Doc
    {
        public FieldDoc()
        {

        }
    }
    internal sealed class PropertyDoc : Doc
    {
        public PropertyDoc()
        {

        }
    }
    internal sealed class EventDoc : Doc
    {
        public EventDoc()
        {

        }
    }

    internal class DocTopic
    {
        public DocTopic(IDocContent content) : this(content.Topic)
        {
            Content.Add(content);
        }

        public DocTopic(string topic)
        {
            Topic = topic;

            Content = new List<IDocContent>();
        }

        public string Topic { get; private set; }

        public List<IDocContent> Content { get; private set; }
    }

    internal interface IDocContent
    {
        string ToMarkdown();

        string Topic { get; }
    }
    
}
