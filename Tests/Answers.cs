using Albion.Parsers;
using System;

namespace Albion.Tests
{
    public class CustomClass
    {
        public string Name { get; set; }

        public CustomClass() { }

        /// <summary>
        /// Support for dynamic objects
        /// </summary>
        [Sentence("What's {pronoun} name?")]
        public string MyName([Any("my", "your", "his", "her", "its", "our", "their")] string pronoun)
        {
            return char.ToUpper(pronoun[0]) + pronoun.Substring(1) + " name is " + Name;
        }
    }

    public static class CustomStaticClass
    {
        /// <summary>
        /// Support for multiple languages, and accessing the sentence attribute
        /// </summary>
        [Sentence("Hello {world}", "Hello")]
        [Sentence("Bonjour {world}", "Bonjour", Language = "fr")]
        public static string Hello(SentenceAttribute sentence, string world = "world")
        {
            return (sentence.Language == "fr" ? "Salut, " : "Hey, ") + world;
        }

        /// <summary>
        /// Support for Nullables, default values, and multiple sentences!
        /// </summary>
        [Sentence("Remind me to {todo} in {time}", "In {time}, remind me to {todo}", "Remind me to {todo}")]
        public static string RemindMe(string todo, TimeSpan? time = null)
        {
            return time.HasValue
                ? String.Format("I'll remind you to {0} in {1} minutes", todo, time.Value.TotalMinutes)
                : String.Format("I'll remind you to {0}", todo);
        }

        /// <summary>
        /// Custom parsers!
        /// </summary>
        [Sentence("Order {food}", "Order some {food}")]
        public static string Order([Parser(typeof(StringEnumParser), "eggs?", "tomato(es)?", "bacon")] string food)
        {
            return "Alright, we'll buy some " + food + ".";
        }

        /// <summary>
        /// Different priorities: this method matches with *everything*, but is only called if nothing else if found.
        /// If such a method does not exist, hello.Answer("") will return null.
        /// </summary>
        [Sentence("{anything}", ID = "annoying")]
        public static string Anything(string anything)
        {
            return "No match found";
        }
    }
}
