# Albion
### Get methods from sentences, easily and effectively.
------
## Example:

> Remind me to go to the store to buy chocolates in two days, one hour and fifteen minutes.

#### with:
<!-- language: c# -->
    [Sentence("In {time} remind me to {todo}")]
    [Sentence("Remind me to {todo} in {time}")]
    public static string RemindMeIn(string todo, [Converter("In")]TimeSpan time)
    {
        DateTime remindtime = DateTime.Now;
        remindtime = remindtime.Add(time);
        return "I will remind you to " + todo + " "
              + remindtime.DayOfWeek + ", the " + remindtime.Day + "th of " + remindtime.ToString("MMM yyyy")
              + ", at " + remindtime.ToString("HH:mm") + ".";
    }
#### will return:

> I will remind you to go to the store to buy chocolates Saturday, the 16th of May 2015, at 00:38.  

------
## Features:
#### Easy to use
    Engine e = new Engine();
    e.Subscribe(typeof(Reminders));
    Answer a = e.Ask("Remind me to call mom in 12 minutes.");
    if (a.Failed)
        Console.WriteLine(a.Error);
    else
        a.Call();
#### You know what you do
    [Extension(ID = "strings")]
    static class Reminders
    {
        [Sentence("Remind me to {todo}", ID = "010")]
        public static string RemindMe(string todo) { ... }
    }
    
    ...
    
    Engine e = new Engine();
    e.AddExtensions(typeof(Reminders));
    Answer a = e.Ex("Remind me to call mom in 12 minutes.");
    if (a.ExtensionID == "strings" && a.ID == "010")
        return (string)a.Call();
#### Built for extensions
    [Extension]
    public static class Example
    { ... }
    new Albion().Subscribe(typeof(Example));
#### Import extensions from PCLs on runtime if you want
    // Since Xamarin doesn't support Assembly.Load*(), the following method isn't included in Albion.
    public static void AddExtensionsFromFiles(this Albion.Engine engine, params string[] paths)
    {
        foreach (string path in paths)
        {
            Assembly a = Assembly.LoadFrom(path);
            foreach (Type t in a.GetTypes())
                if (t.IsClass && t.GetCustomAttribute(typeof(ExtensionAttribute)) != null) engine.Subscribe(t);
        }
    }

Usage :

    // AlbionMaths.cs
    [Extension(ID = "maths")]
    public static class Special
    {
        [Sentence("{n} squared")]
        public static int Squared([Converter("Number")]int n)
        {
            return n * n;
        }
    }
    
    // Main.cs
    Engine engine = new Engine();
    engine.AddExtensionFromFile("AlbionMaths.dll");
    Answer answer = engine.Ask("Sixteen squared");
    int squared = (int)aa.Call();
    
    // Result: squared = 256
#### Useful converters
Some converters are included in Albion.Converters, but you can add your own if you want.

    [Sentence("Convert {number} to an integer")]
    public static string Convert([Converter("Number")]int number) { ... }
    // Will use Albion.Converters.Number(string).
    
    public static string Convert([Converter("Number", Converter = typeof(Maths))]int number) { ... }
    // Will use Maths.Number(string) if it exists. If it doesn't, throws an exception.

## TODO:
- Access the Sentence from inside.
