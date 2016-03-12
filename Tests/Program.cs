using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Albion;
using Shouldly;

namespace Albion.Tests
{
    class Program
    {
        static Engine engine = new Engine();

        static void Main(string[] args)
        {
            engine.Register(typeof(CustomClass));

            engine.ShouldSatisfyAllConditions(
                () => engine.Ask("Hello Greg").Call().ShouldBe("Hey, Greg"),
                () => engine.Ask("Bonjour Greg").ShouldBeNull(),
                () => engine.Ask("Bonjour Greg", "fr").Call().ShouldBe("Salut, Greg"),
                () => engine.Ask("In ten hours and ten minutes, remind me to get flowers").Call().ShouldBe("I'll remind you to get flowers in 610 minutes"),
                () => engine.Ask("Remind me to eat").Call<string>().ShouldBe("I'll remind you to eat"),
                () => engine.Ask<string>("Whatever").ShouldNotBeNull()
            );

            Console.WriteLine("Test pass successful.");
            Console.ReadKey();
        }
    }

    public static class CustomClass
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
        /// Different priorities: this method matches with *everything*, but is only called if nothing else if found.
        /// If such a method does not exist, hello.Answer("") will return null.
        /// </summary>
        [Sentence("{anything}")]
        public static string Anything(string anything)
        {
            return "No match found";
        }
    }
}
