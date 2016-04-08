using Albion.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Albion
{
    /// <summary>
    /// This attribute indicates that the following parameter will be parsed specifically,
    /// and may contain custom examples used when calling <see cref="Engine.Suggest(string)"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class ParserAttribute : Attribute
    {
        /// <summary>
        /// Custom examples used when calling <see cref="Engine.Suggest(string)"/>.
        /// </summary>
        public string[] Examples { get { return examples; } set { examples = value; } }
        private string[] examples = new string[0];

        /// <summary>
        /// Custom <see cref="IParser"/> for the type of the parameter
        /// </summary>
        public IParser CustomParser { get { return customParser; } }
        private IParser customParser = null;

        /// <summary>
        /// Indicates that the following parameter will have the custom examples <see cref="Examples"/>.
        /// Not giving <see cref="Examples"/> is useless when using this constructor.
        /// </summary>
        public ParserAttribute()
        {

        }

        /// <summary>
        /// Indicates that the following parameter will be parsed using the parser <paramref name="parser"/>,
        /// which will be constructedd using the parameters <paramref name="ctorParameters"/>.
        /// </summary>
        /// <param name="parser"></param>
        /// <param name="ctorParameters"></param>
        public ParserAttribute(Type parser, params object[] ctorParameters)
        {
            if (parser != null && parser.GetTypeInfo().ImplementedInterfaces.Contains(typeof(IParser)))
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
                    throw new ArgumentException(String.Format("{0} does not provide a correct constructor.", parser));
                }
            }
            else
            {
                throw new ArgumentException(String.Format("{0} is not a correct parser.", parser));
            }
        }
    }
}
