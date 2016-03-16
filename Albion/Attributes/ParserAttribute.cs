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
    public class ParserAttribute : Attribute
    {
        protected string[] examples = new string[0];
        public string[] Examples { get { return examples; } set { examples = value; } }

        protected IParser customParser = null;
        public IParser CustomParser { get { return customParser; } }

        public ParserAttribute()
        {

        }

        public ParserAttribute(Type parser, params object[] ctorParameters)
        {
            if (parser != null && parser.GetTypeInfo().IsSubclassOf(typeof(IParser)))
            {
                ConstructorInfo ctor = (from ctr in parser.GetTypeInfo().DeclaredConstructors
                                        where ctr.GetParameters().All(para => para.ParameterType == ctorParameters[para.Position].GetType())
                                        select ctr).FirstOrDefault();
                
                if (ctor != null)
                {
                    customParser = (IParser)ctor.Invoke(ctorParameters);
                }
                else
                {
                    throw new ArgumentNullException(String.Format("{0} does not provide a correct constructor.", parser));
                }
            }
            else
            {
                throw new ArgumentException(String.Format("{0} is not a correct parser.", parser));
            }
        }
    }
}
