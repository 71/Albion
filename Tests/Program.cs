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
                () => engine.Ask("Bonjour Greg", "*").Call().ShouldBe("Salut, Greg"),
                () => engine.Ask("In ten hours and ten minutes, remind me to get flowers").Call().ShouldBe("I'll remind you to get flowers in 610 minutes"),
                () => engine.Ask("Remind me to eat").Call<string>().ShouldBe("I'll remind you to eat"),
                () => engine.Ask<string>("Whatever").ShouldNotBeNull(),
                () => engine.Ask("What's your name?").Call(customObj).ShouldBe("Your name is Greg"),
                () => engine.Ask("What's her name?").Call(customObj).ShouldBe("Her name is Greg"),
                () => engine.Ask("What's mine name?").Call(customObj).ShouldBe("No match found"),

                () => engine.Suggest("Order som").ShouldNotBeEmpty(),

                () => engine.Clear(),

                () => engine.Build()
                            .Sentence("Hello {you}", you => you.IsType<string>())
                            .Handler(o => "Hey, " + o.you),

                () => engine.Suggest("Hello Gr").ShouldNotBeEmpty(),

                () => engine.Ask<string>("Hello Greg").Call().ShouldBe("Hey, Greg"),
                () => engine.Ask("Hello Greg").Call().ShouldBe("Hey, Greg")
            );

            engine.Clear();
            engine.Register(typeof(CustomStaticClass), typeof(CustomClass));

            Console.WriteLine("Test pass successful. Enter 'exit' to quit.");

            string cmd = "";
            while (true)
            {
                var key = Console.ReadKey();

                Console.Clear();

                if (key.Key != ConsoleKey.Enter)
                {
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (cmd.Length > 0)
                            cmd = cmd.Substring(0, cmd.Length - 1);
                    }
                    else
                    {
                        cmd += key.KeyChar;
                    }

                    Console.WriteLine(cmd);
                    Console.WriteLine();

                    Console.ForegroundColor = ConsoleColor.White;
                    foreach (Suggestion s in engine.Suggest(cmd, "*", SuggestionMatchType.Normal | SuggestionMatchType.Sentence))
                    {
                        if (s.ID != "annoying")
                            Console.WriteLine(s.ToString());
                    }
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(cmd);
                    Console.WriteLine();

                    if (cmd == "exit")
                        Environment.Exit(0);

                    Answer a = engine.Ask(cmd);
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    if (a != null && a.ReturnType != typeof(void))
                        Console.WriteLine(a.Call(customObj));
                    Console.ResetColor();

                    cmd = "";
                }
            }
        }
    }
}
