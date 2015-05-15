# Albion
### Get methods from sentences, easily and effectively.
------
## Example:

> Remind me to go to the store to buy chocolates in two days, one hour and fifteen minutes.

#### with:
<!-- language: c# -->
    [Sentence("In {time:in} remind me to {todo}")]
    [Sentence("Remind me to {todo} in {time:in}")]
    public static string RemindMeIn(string todo, TimeSpan time)
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
    e.AddExtensions(typeof(Reminders));
    Answer a = e.Ex("Remind me to call mom in 12 minutes.");
    if (a.Failed)
        Console.WriteLine(a.Error);
    else
        a.Call();
#### You know what you do
    [Extension(ID = "strings")]
    static class Reminders
    {
        [Sentence("Remind me to {todo}", ID = "010")]
        public static string RemindMe(string todo)
        { ... }
        
        ...
    }
    
    Engine e = new Engine();
    e.AddExtensions(typeof(Reminders));
    Answer a = e.Ex("Remind me to call mom in 12 minutes.");
    if (a.ExtensionID == "strings" && a.ID == "010")
        return (string)a.Call();
    else
        return null;
#### Built for extensions
    [Extension]
    public static class Example
    { ... }
    new Albion().AddExtensions(typeof(Example));
#### Easy syntax
    Remind me to {todo} in {time:in}
- **{todo}** and **{time:in}** are variables
- **:in** means that the string *time* will be converter using Albion.Convert.In(string)
