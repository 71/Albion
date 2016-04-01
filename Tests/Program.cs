using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Albion;
using Shouldly;
using System.Diagnostics;

namespace Albion.Tests
{
    class Program
    {
        static Engine engine = new Engine();

        static void Main(string[] args)
        {
            engine.Register(typeof(CustomStaticClass));
            engine.Register(typeof(CustomClass));

            var customObj = new CustomClass() { Name = "John" };
            customObj.Name = "Greg";

            engine.ShouldSatisfyAllConditions(
                // Answers
                () => engine.Ask("Hello Greg").Call().ShouldBe("Hey, Greg"),
                () => engine.Ask("Bonjour Greg").Call().ShouldBe("No match found"),
                () => engine.Ask("Bonjour Greg", "fr").Call().ShouldBe("Salut, Greg"),
                () => engine.Ask("In ten hours and ten minutes, remind me to get flowers").Call().ShouldBe("I'll remind you to get flowers in 610 minutes"),
                () => engine.Ask("Remind me to eat").Call<string>().ShouldBe("I'll remind you to eat"),
                () => engine.Ask<string>("Whatever").ShouldNotBeNull(),
                () => engine.Ask("What's your name?").Call(customObj).ShouldBe("My name is Greg"),

                () => engine.Suggest("Order som").ShouldNotBeEmpty()
            );

            Console.WriteLine("Test pass successful. Enter 'exit' to quit.");

            string s;
            while ((s = Console.ReadLine()) != "exit")
            {
                Console.Clear();
                foreach (Suggestion sugg in engine.Suggest(s, SuggestionMatchType.Deep | SuggestionMatchType.Sentence))
                {
                    Console.WriteLine(sugg.ToString());
                }
            }
        }
    }
}
