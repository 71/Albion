# Albion
### Get methods from sentences, easily and effectively.
------
## Example:

> Remind me to go to the store to buy chocolates in two days, one hour and fifteen minutes.

#### with:
`````csharp
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
````
#### will return:

> I will remind you to go to the store to buy chocolates Saturday, the 16th of May 2015, at 00:38.  

------
## Features:
#### Easy to use
````csharp
Engine albion = new Engine();
albion.Subscribe(typeof(Reminders));
Answer a = albion.Ask("Remind me to call mom in 12 minutes.");
if (a.Failed)
    Console.WriteLine(a.Error);
else
    a.Call();
````
#### You know what you do
````csharp
[Extension(ID = "reminders")]
public static class Reminders
{
    [Sentence("Remind me to {todo}", ID = "simple")]
    public static string RemindMe(string todo) { ... }
}

...

Engine albion = new Engine();
albion.Subscribe(typeof(Reminders));
Answer a = albion.Ask("Remind me to call mom in 12 minutes.");
if (a.ExAttr.ID == "reminders" && a.Returns == typeof(string))
    return a.Call<string>();
````
#### Built for extensions
````csharp
[Extension]
public static class Example
{ ... }
albion.Subscribe(typeof(Example));
````
#### Useful converters
Some converters are included in Albion.Converters, but you can add your own if you want.
````csharp
[Sentence("Convert {number} to an integer")]
public static string Convert([Converter("Number")]int number) { ... }
// Will use Albion.Converters.Number(string).
    
public static string Convert([Converter("Number", Converter = typeof(Maths))]int number) { ... }
// Will use Maths.Number(string) if it exists. If it doesn't, throws an exception.
````
#### Language support
`````csharp
[Sentence("Hello", Language = "en-us")]
public static SayHello() { }
````
*Note*: The language should be in [this](https://msdn.microsoft.com/en-us/library/ms533052\(v=vs.85\).aspx) format.
