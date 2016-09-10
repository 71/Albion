using Albion.Parsers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

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
        public virtual string[] Examples { get; set; }

        /// <summary>
        /// Custom <see cref="IParser"/> for the type of the parameter.
        /// </summary>
        protected internal IParser CustomParser { get; protected set; }

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
                                        let parameters = ctr.GetParameters()
                                        where parameters.Length == ctorParameters.Length
                                        where parameters.All(para => para.ParameterType == ctorParameters[para.Position].GetType())
                                        select ctr).FirstOrDefault();
                
                if (ctor != null)
                {
                    CustomParser = (IParser)ctor.Invoke(ctorParameters);
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

    /// <summary>
    /// This attribute allows you to specify a list of strings
    /// that will all match.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class AnyAttribute : ParserAttribute
    {
        /// <summary>
        /// Create a new attribute, given a number of different possibilities.
        /// </summary>
        public AnyAttribute(params string[] possibilities)
        {
            CustomParser = new StringEnumParser(possibilities);
        }
    }

    /// <summary>
    /// This attribute allows you to specify a regex
    /// that a parameter must match.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public sealed class RegexAttribute : ParserAttribute
    {
        /// <summary>
        /// Create a new attribute, given its regex representation.
        /// </summary>
        public RegexAttribute(string pattern)
        {
            CustomParser = new MatchParser(new Regex(pattern));
        }

        /// <summary>
        /// Create a new attribute, given its regex representation.
        /// </summary>
        public RegexAttribute(string pattern, RegexOptions opts)
        {
            CustomParser = new MatchParser(new Regex(pattern, opts));
        }
    }
}
