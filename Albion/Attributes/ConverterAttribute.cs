using Albion.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Albion.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class ConverterAttribute : Attribute
    {
        public string[] Examples { get; protected set; }
        public string ClassName { get; protected set; }
        public IParser CustomParser { get; protected set; }

        /// <summary>
        /// Tries to automatically convert the given value.
        /// If can't convert, returns null.
        /// </summary>
        public ConverterAttribute(string parsername, params string[] customexamples)
        {
            Examples = customexamples;
            ClassName = parsername;
            var ctype = Type.GetType(ClassName, false);

            if (ctype != null)
            {
                if (ctype.GetTypeInfo().IsSubclassOf(typeof(IParser)))
                    throw new Exception("No corresponding parser found.");

                CustomParser = (IParser)ctype.GetTypeInfo().DeclaredConstructors.FirstOrDefault(x => x.GetParameters().Length == 0)?.Invoke(new object[0]);
            }
        }
    }
}
