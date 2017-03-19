# Albion
### Get methods from sentences, easily and effectively.
------
## Example:

> Remind me to go to the store to buy chocolates in two days, one hour and fifteen minutes.

#### with:
```csharp
[Sentence("In {time} remind me to {todo}")]
[Sentence("Remind me to {todo} in {time}")]
public static string RemindMeIn(string todo, TimeSpan time)
{
    DateTime remindtime = DateTime.Now;
    remindtime = remindtime.Add(time);
    return "I will remind you to " + todo + " "
          + remindtime.DayOfWeek + ", the " + remindtime.Day + "th of " + remindtime.ToString("MMM yyyy")
          + ", at " + remindtime.ToString("HH:mm") + ".";
}
```

#### will return:

> I will remind you to go to the store to buy chocolates Saturday, the 16th of May 2015, at 00:38.  

------
#### It's easy to use
```csharp
Engine engine = new Engine();
engine.Register(typeof(Reminders));
Answer a = engine.Ask("Remind me to go to the store to buy chocolates in two days, one hour and fifteen minutes.");
if (a == null)
    Console.WriteLine("No match found");
else if (a.ReturnType == typeof(string))
    Console.WriteLine(a.Call());
```

#### Way more than that
See [Wiki](../../wiki) to follow a [getting started](../../wiki/home) guide, learn [advanced uses](../../wiki/advanced-uses) or learn with [examples](../../wiki/examples).
