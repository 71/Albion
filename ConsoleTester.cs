using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Albion;
using System.Diagnostics;

namespace AlbionConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Albion: Test console";
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.White;
            while (true)
            {
                var e = new Engine();
                e.AddExtensions(typeof(Commands));
                var v2 = new V2();
                v2.Subscribe(typeof(Commands));
                string i = "";
                while (true)
                {
                    var k = Console.ReadKey(true);
                    if (k.Key == ConsoleKey.Enter) break;
                    else if (k.Key == ConsoleKey.Backspace && i.Length == 0) i = "";
                    else if (k.Key == ConsoleKey.Backspace) i = i.Substring(0, i.Length - 1);
                    else i += k.KeyChar;

                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.WriteLine(i);
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.Write("\n\n\n");

                    if (i.Trim().Length == 0) continue;

                    foreach (var l in e.Suggest(i))
                    {
                        Console.Write(l.Result[0]);
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.Write(l.Result[1]);
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.Write(l.Result[2] + "\n");
                        Console.ForegroundColor = ConsoleColor.Gray;
                        Console.WriteLine((l.Description == "") ? "No description available" : l.Description);
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                }

                Console.ForegroundColor = ConsoleColor.DarkRed;
                Answer a = e.Ex(i);
                if (a == null || a.Failed) Console.WriteLine("\n" + "Command not recognized...");
                else Console.WriteLine("\n" + (string)a.Call());
                Console.ForegroundColor = ConsoleColor.White;

                i = "";
                Console.Beep(56, 400);
            }
            
        }
    }

    [Extension]
    static class Commands
    {
        [Sentence("Hey {who}")]
        public static string Hey(string who)
        {
            return "Hello, I'm " + who + "!";
        }

        [Sentence("Count from {one} to {two}")]
        public static string Count([AConvert]int one, [AConvert]int two) 
        {
            int de = one;
            if (one < two)
                for (; one != two + 1; one++) Console.WriteLine(Albion.Convert.Number(one));
            else if (one > two)
                for (; one != two - 1; one--) Console.WriteLine(Albion.Convert.Number(one));
            else if (one == two)
                Console.WriteLine(Albion.Convert.Number(one));

            return "\nCounted from " + de + " to " + two + ".";
        }

        [Sentence("Count to {one}")]
        public static string Count([AConvert]int one)
        {
            int de = one;
            if (one < 0)
                for (; one != 0 + 1; one++) Albion.Convert.Number(one);
            else if (one > 0)
                for (; one != 0 - 1; one--) Albion.Convert.Number(one);
            else if (one == 0)
                Albion.Convert.Number(one);

            return "\nCounted to " + one + ".";
        }

        [Sentence("Benchmark", Description = "Tries to ")]
        public static string Benchmark()
        {
            return Benchmark(1000);
        }

        [Sentence("Benchmark {i} times")]
        public static string Benchmark([AConvert]int i)
        {
            V2 v2 = new V2();
            v2.Subscribe(typeof(Commands));
            Engine e = new Engine();
            e.AddExtensions(typeof(Commands));

            Stopwatch s1 = Stopwatch.StartNew();
            for (int s = 0; s < i; s++) v2.Ask("Hey Greg!").Call();
            s1.Stop();
            Stopwatch s2 = Stopwatch.StartNew();
            for (int s = 0; s < i; s++) e.Ex("Hey Greg!").Call();
            s2.Stop();
            Stopwatch s3 = Stopwatch.StartNew();
            for (int s = 0; s < i; s++) e.Suggest("Count from fifteen to twenty");
            s3.Stop();
            return "Retrieved/executed " + i + " methods in " + s1.Elapsed.TotalMilliseconds + " milliseconds with V2.\n"
                 + "Retrieved/executed " + i + " methods in " + s2.Elapsed.TotalMilliseconds + " milliseconds with Albion.\n"
                 + "Got suggestions " + i + " times in " + s3.Elapsed.TotalMilliseconds + " milliseconds with Albion.";
        }

        [Sentence("Benchmark number convertor")]
        public static string BenchmarkNumber()
        {
            Stopwatch s1 = Stopwatch.StartNew();
            for (int s = 0; s < 500; s++) Albion.Convert.Number(s);
            s1.Stop();

            Stopwatch s2 = Stopwatch.StartNew();
            for (int s = 0; s < 500; s++) Albion.Convert.Number("Five hundred and twenty-one");
            s2.Stop();

            return "Converted 500 integers to English in " + s1.Elapsed.TotalMilliseconds + " milliseconds,\nand 500 strings to integers in " + s2.Elapsed.TotalMilliseconds + " milliseconds.";
        }

        [Sentence("Old benchmark {i} times")]
        public static string OldBenchmark([AConvert]int i)
        {
            Stopwatch s1 = Stopwatch.StartNew();
            Engine v2 = new Engine();
            v2.AddExtensions(typeof(Commands));
            s1.Stop();

            Stopwatch s2 = Stopwatch.StartNew();
            for (int s = 0; s < i; s++) v2.Ex("Hey Greg!").Call();
            s2.Stop();
            return "Created and initialized Albion in " + s1.Elapsed.TotalMilliseconds + " milliseconds; and retrieved & executed " + i + " methods in " + s2.Elapsed.TotalMilliseconds + " milliseconds.";
        }
    }
}
