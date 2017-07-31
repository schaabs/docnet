using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace docnet
{ 
    internal static class MemberInfoExtensions
    {
        public static string DocName(this MemberInfo mInfo)
        {
            if (mInfo is Type)
            {
                return DocName((Type)mInfo);
            }

            if (mInfo is MethodInfo)
            {
                return DocName((MethodInfo)mInfo);
            }

            if (mInfo is ConstructorInfo)
            {
                return DocName((ConstructorInfo)mInfo);
            }

            return mInfo.Name;
        }

        public static string DocName(this ConstructorInfo ctorInfo)
        {
            var buff = new StringBuilder();

            buff.Append(DocName(ctorInfo.DeclaringType));

            buff.Append('(');

            buff.Append(string.Join(", ", ctorInfo.GetParameters().Select(p => DocName(p.ParameterType))));

            buff.Append(')');

            return buff.ToString();
        }

        public static string DocName(this Type type)
        {
            var buff = new StringBuilder();

            buff.Append(type.Name.Split(new char[] { '`' }, 2)[0].TrimEnd('&'));

            var genParams = string.Join(", ", type.GetGenericArguments().Select(t => DocName(t)));

            if (!string.IsNullOrEmpty(genParams))
            {
                buff.Append('<');

                buff.Append(genParams);

                buff.Append('>');
            }

            return buff.ToString();
        }

        public static string DocName(this MethodInfo method)
        {
            var buff = new StringBuilder();

            buff.Append(method.Name.Split(new char[] { '`' }, 2)[0]);

            var genArgs = string.Join(", ", method.GetGenericArguments().Select(t => t.Name));

            if (!string.IsNullOrEmpty(genArgs))
            {
                buff.Append('<');

                buff.Append(genArgs);

                buff.Append('>');
            }

            buff.Append('(');

            buff.Append(string.Join(", ", method.GetParameters().Select(p => DocName(p.ParameterType))));

            buff.Append(')');

            return buff.ToString();
        }

        public static string DocId(this MemberInfo mInfo)
        {
            var buff = new StringBuilder();

            //append the prefix and the namespace
            buff.Append(GetXDocIdPrefix(mInfo));

            var type = mInfo as Type ?? mInfo.DeclaringType;

            buff.Append(type.FullName.Replace('+', '.'));

            var typeGenArgs = type.GetGenericArguments().Select(t => t.Name).ToArray();

            if (!(mInfo is Type))
            {
                var ctorInfo = mInfo as ConstructorInfo;
                var methInfo = mInfo as MethodInfo;
                var propInfo = mInfo as PropertyInfo;

                string[] mGenArgs = new string[] { };
                ParameterInfo[] mParams = new ParameterInfo[] { };

                buff.Append('.');

                if (ctorInfo != null)
                {
                    buff.Append('#');
                }

                buff.Append(mInfo.Name);

                if (methInfo != null)
                {
                    mGenArgs = methInfo.GetGenericArguments().Select(t => t.Name).ToArray();
                    mParams = methInfo.GetParameters();

                    if (mGenArgs.Length > 0)
                    {
                        buff.Append("``");

                        buff.Append(mGenArgs.Length);
                    }
                }
                if (propInfo != null)
                {
                    mParams = propInfo.GetIndexParameters();
                }

                if (mParams.Length > 0)
                {
                    buff.Append("(");

                    buff.Append(string.Join(",", mParams.Select(pInfo => GetParameterTypeString(pInfo.ParameterType, typeGenArgs, mGenArgs) + (pInfo.IsOut ? "@" : ""))));

                    buff.Append(")");
                }

            }
            return buff.ToString();
        }

        private static string GetParameterTypeString(Type paramType, string[] typeGenArgs, string[] memberGenArgs)
        {
            var typeStr = paramType.FullName;

            if (typeStr == null)
            {
                int index = -1;

                if (paramType.IsByRef)
                {
                    typeStr = GetParameterTypeString(paramType.GetElementType(), typeGenArgs, memberGenArgs) + "@";
                }
                else if (paramType.IsGenericParameter)
                {
                    if ((index = Array.IndexOf(typeGenArgs, paramType.Name)) >= 0)
                    {
                        typeStr = $"`{index}";
                    }
                    else if ((index = Array.IndexOf(memberGenArgs, paramType.Name)) >= 0)
                    {
                        typeStr = $"``{index}";
                    }
                }
                else if (paramType.IsGenericType)
                {
                    var genArgsStr = string.Join(",", paramType.GetGenericArguments().Select(t => GetParameterTypeString(t, typeGenArgs, memberGenArgs)));
                    typeStr = $"{paramType.Name.Split('`')[0]}{{{genArgsStr}}}";
                }
            }

            return typeStr;
        }


        private static string GetXDocIdPrefix(MemberInfo mInfo)
        {
            switch (mInfo.MemberType)
            {
                case MemberTypes.TypeInfo:
                case MemberTypes.NestedType:
                    return "T:";
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    return "M:";
                case MemberTypes.Field:
                    return "F:";
                case MemberTypes.Property:
                    return "P:";
                case MemberTypes.Event:
                    return "E:";
                default:
                    throw new ArgumentException($"Unsuported MemberType {mInfo.MemberType}", "mInfo");
            }
        }
    }
}
