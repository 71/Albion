# Albion
### Get methods from sentences, easily and effectively.
------
## Example:

> Remind me to go to the store to buy chocolates in two days, one hour and fifteen minutes.

#### with:
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
