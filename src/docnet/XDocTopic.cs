using System;
using System.Collections.Generic;
using System.IO;
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

        public static string Link(string link, string text = null)
        {
            return $"[{text ?? link}]({link})";
        }
        
    }

    internal abstract class Doc : IDocContent
    {
        private static object s_indexLock = new object();
        
        private static Dictionary<string, Doc> s_index;

        protected Doc()
        {
        }

        public static IEnumerable<Doc> AllDocs
        {
            get
            {
                lock (s_indexLock)
                {
                    //copying to a separate array so all enumeration of s_index is done under s_indexLock
                    return s_index.Values.ToArray();
                }
            }
        }

        public static Doc Get(string docId)
        {
            lock (s_indexLock)
            {
                if (!s_index.TryGetValue(docId, out Doc doc))
                {
                    doc = CreateDoc(docId);
                }

                return doc;
            }
        }

        public static T Get<T>(string docId)
            where T : Doc
        {
            if(typeof(T) != GetDocTypeFromId(docId))
            {
                throw new ArgumentException($"docId type: {docId[0]} does not match the requested type: {typeof(T)}", "docId");
            }

            return (T)Get(docId);
        }
       
        public static string GetLink(string docId)
        {
            lock (s_indexLock)
            {
                return s_index.TryGetValue(docId, out Doc doc) ? doc.Link : null;
            }
        }

        public string DocId { get; private set; }

        public abstract string Name { get; }

        public virtual string Path { get { return $"./{DocId.Replace(':', '_')}.md"; } }

        public virtual string Topic { get { return Name; } }

        public string Link { get { return MD.Link(Path, Name); } }

        public Doc Parent { get; set; }

        public string BreadCrumbTrail { get { return (Parent != null) ? $"{Parent.BreadCrumbTrail} > {Link}" : Link; } }

        public void AddContent(IDocContent content)
        {
            if(!Topics.TryGetValue(content.Topic, out DocTopic topic))
            {
                topic = new DocTopic(content);

                Topics[content.Topic] = topic;
            }
            else if (content is NamedDocContent)
            {

            }
        }

        public Dictionary<string, DocTopic> Topics { get; private set; }

        public Task WriteToStreamAsync(StreamWriter stream)
        {
            throw new NotImplementedException();
        }

        private static Doc CreateDoc(string docId)
        {
            Doc doc = null;

            var docType = GetDocTypeFromId(docId);

            doc = (Doc)Activator.CreateInstance(docType);

            s_index[docId] = doc;

            return doc;
        }

        private static Type GetDocTypeFromId(string docId)
        {
            switch (docId[0])
            {
                case 'T':
                    return typeof(ClassDoc);
                case 'M':
                    return typeof(MethodDoc);
                case 'F':
                    return typeof(FieldDoc);
                case 'P':
                    return typeof(PropertyDoc);
                case 'E':
                    return typeof(EventDoc);
                case 'N':
                    return typeof(NamespaceDoc);
                default:
                    throw new ArgumentException($"Unsupported document type: {docId[0]}", "docId");
            }
        }
    }

    internal abstract class MemberDoc<T> : Doc
        where T : MemberInfo
    {
        public T Member { get; set; }

        public override string Name { get { return Member?.DocName(); } }

        public virtual void AddMemberContent(T member)
        {

        }
    }

    internal sealed class ClassDoc : MemberDoc<Type>
    {
        public ClassDoc()
        {
            SubClasses = new HashSet<ClassDoc>();
        }

        public HashSet<ClassDoc> SubClasses { get; private set; }

        public HashSet<MethodDoc> Methods { get; private set; }

        public HashSet<PropertyDoc> Properties { get; set; }
    }
    internal sealed class MethodDoc : MemberDoc<MethodInfo>
    {
        public MethodDoc()
        {

        }
    }
    internal sealed class FieldDoc : MemberDoc<FieldInfo>
    {
        public FieldDoc()
        {

        }
    }
    internal sealed class PropertyDoc : MemberDoc<PropertyInfo>
    {
        public PropertyDoc()
        {

        }
    }

    internal sealed class EventDoc : MemberDoc<EventInfo>
    {
        public EventDoc()
        {

        }
    }

    internal sealed class CtorDoc : MemberDoc<ConstructorInfo>
    {
        public CtorDoc()
        {

        }
    }
    

    internal sealed class NamespaceDoc : Doc
    {
        public NamespaceDoc(string namepace)
        {
            Classes = new HashSet<ClassDoc>();
        }

        public override string Name => DocId.Substring(2);

        public HashSet<ClassDoc> Classes { get; private set; }
    }

    internal class AssemblyDoc : Doc
    {
        public AssemblyDoc(Assembly assembly)
        {
            Namespaces = new HashSet<NamespaceDoc>();

            foreach (var type in assembly.GetTypes())
            {
                var nsDoc = Doc.Get<NamespaceDoc>($"N:{type.Namespace}");

                var classDoc = Doc.Get<ClassDoc>(type.DocId());

                if(type.BaseType.Assembly == assembly)
                {
                    var baseDoc = Doc.Get<ClassDoc>(type.BaseType.DocId());

                    baseDoc.SubClasses.Add(classDoc);
                }
            }
        }

        public override string Name => Assembly.GetName().Name;

        public Assembly Assembly { get; private set; }

        public HashSet<NamespaceDoc> Namespaces { get; private set; }
    }

    internal class DocTopic : IDocContent
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

        public Task WriteToStreamAsync(StreamWriter stream)
        {
            throw new NotImplementedException();
        }
    }

    internal interface IDocContent
    {
        Task WriteToStreamAsync(StreamWriter stream);

        string Topic { get; }
    }

    internal class NamedDocContent : IDocContent
    {
        public NamedDocContent(string name)
        {

        }

        public string Name { get; private set; }
        
        public List<IDocContent> Content { get; private set; }
    }
    
}
