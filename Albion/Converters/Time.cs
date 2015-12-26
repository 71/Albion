using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Albion
{
    public static partial class Converters
    {
        public static DateTime Date(string r)
        {
            r = StringToInt(r);
            string __next = Regex.Match(r, @"next (\w+)", RegexOptions.IgnoreCase).Groups[1].Value;
            Match _next = Regex.Match(__next, @"(monday|tuesday|wednesday|thurday|friday|saturday|sunday)|(week)|(month)|(year)", RegexOptions.IgnoreCase);
            DateTime next = DateTime.Today;
            if (_next.Groups[1].Value != "")
            {
                List<string> _day = new List<string>() { "monday", "tuesday", "wednesday", "thurday", "friday", "saturday", "sunday" };
                int _today = _day.IndexOf(next.DayOfWeek.ToString().ToLower());
                int _then = _day.IndexOf(_next.Groups[1].Value.ToLower());

                if (_today >= _then) next.AddDays(_then + 7 - _today);
                else next.AddDays(_then - _today);
            }
            else if (_next.Groups[2].Value != "") next.AddDays(7);
            else if (_next.Groups[3].Value != "") next.AddMonths(1);
            else if (_next.Groups[4].Value != "") next.AddYears(1);

            string _in = Regex.Match(r, @"\d{1,2} minutes?|\d{1,2} hours?|\d{1,2} days?|\d{1,2} weeks?", RegexOptions.IgnoreCase).Value;
            TimeSpan intime = (_in != "") ? In(_in) : TimeSpan.FromMilliseconds(0);

            string _at = Regex.Match(r, @"past|to|am|pm|:|h|hours?", RegexOptions.IgnoreCase).Value;
            DateTime attime = (_at != "") ? At(_at) : DateTime.MinValue;

            int day = new List<string>() { "monday", "tuesday", "wednesday", "thurday", "friday", "saturday", "sunday" }.IndexOf(Regex.Match(r, @"monday|tuesday|wednesday|thurday|friday|saturday|sunday").Value);

            Match datematch = Regex.Match(r, @"(\d{1,2})(?:th|nd|st|rd)|(january|february|march|april|may|june|july|august|september|october|november|december)|(d{1,4})", RegexOptions.IgnoreCase);
            int _dayinmth = int.Parse("0" + datematch.Groups[1].Value);
            int dayinmth = (NthDay(r) == null) ? _dayinmth : (int)NthDay(r);
            string month = datematch.Groups[2].Value;
            int year = int.Parse("0" + datematch.Groups[3].Value);

            DateTime Finale = DateTime.Now;

            if (!next.DayOfYear.Equals(DateTime.Now.DayOfYear) && !next.Year.Equals(DateTime.Now.Year))
            {
                Finale = next;
                if (attime != DateTime.MinValue)
                {
                    Finale = Finale.AddHours(attime.Hour);
                    Finale = Finale.AddMinutes(attime.Minute);
                }
                else
                {
                    Finale = Finale.AddHours(DateTime.Now.Hour);
                    Finale = Finale.AddMinutes(DateTime.Now.Minute);
                }
            }
            else if (_in != "")
            {
                Finale = Finale.Add(intime);

                if (attime != DateTime.MinValue)
                {
                    Finale = Finale.AddHours(attime.Hour);
                    Finale = Finale.AddMinutes(attime.Minute);
                }
            }
            else if (day != -1)
            {
                List<string> _day = new List<string>() { "monday", "tuesday", "wednesday", "thurday", "friday", "saturday", "sunday" };
                int _today = _day.IndexOf(next.DayOfWeek.ToString().ToLower());
                int _then = day;

                if (_today >= _then) Finale.AddDays(_then + 7 - _today);
                else Finale.AddDays(_then - _today);
            }
            else if (r.Contains("tomorrow"))
            {
                Finale = Finale.AddDays(r.Contains("the day after tomorrow") ? 2 : 1);
            }
            else if (attime != DateTime.MinValue)
            {
                Finale = Finale.AddHours(attime.Hour);
                Finale = Finale.AddMinutes(attime.Minute);
            }
            else
            {
                DateTime td = DateTime.Today;
                if (year > td.Year) Finale = Finale.AddYears(td.Year - year);
                if (month != "")
                {
                    int _mth = td.Month - new List<string>() { "january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december" }.IndexOf(month);
                    Finale = Finale.AddMonths(td.Month >= _mth ? _mth + 12 - td.Month : _mth - td.Month);
                }
                if (dayinmth != 0)
                    Finale = Finale.AddDays(td.Day >= dayinmth ? dayinmth + 30 - td.Day : dayinmth - td.Day);
            }

            return Finale;
        }

        public static int? NthDay(string r)
        {
            List<string> toten = new List<string> { "zero", "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "nineth" };
            List<string> totwenty = new List<string> { "ten", "eleven", "twelve", "thirteen", "fourteen", "fifteen", "sixteen", "seventeen", "eighteen", "nineteen" };
            List<string> tothirty = new List<string> { "twenty", "twenty-one", "twenty-two", "twenty-three", "twenty-four", "twenty-five", "twenty-six", "twenty-seven", "twenty-eight", "twenty-nine" };
            List<string> pastthirty = new List<string> { "thirty", "thirty-first" };

            foreach (string s in pastthirty) if (r.Contains(s) || r.Replace(' ', '-').Contains(s)) return pastthirty.IndexOf(s) + 30;
            foreach (string s in tothirty) if (r.Contains(s) || r.Replace(' ', '-').Contains(s)) return tothirty.IndexOf(s) + 20;
            foreach (string s in totwenty) if (r.Contains(s)) return totwenty.IndexOf(s) + 10;
            foreach (string s in toten) if (r.Contains(s)) return toten.IndexOf(s);
            return null;
        }

        public static TimeSpan In(string r)
        {
            // some regex should to the trick
            Match matches = Regex.Match(r, @"(\d{1,2}) minutes?|(\d{1,2}) hours?|(\d{1,2}) days?|(\d{1,2}) weeks?", RegexOptions.IgnoreCase);
            int minutes = (!String.IsNullOrEmpty(matches.Groups[1].Value)) ? Int32.Parse(matches.Groups[1].Value) : 0;
            int hours = (!String.IsNullOrEmpty(matches.Groups[2].Value)) ? Int32.Parse(matches.Groups[2].Value) : 0;
            int days = (!String.IsNullOrEmpty(matches.Groups[3].Value)) ? Int32.Parse(matches.Groups[3].Value) : 0;
            int weeks = (!String.IsNullOrEmpty(matches.Groups[4].Value)) ? Int32.Parse(matches.Groups[4].Value) : 0;
            days += weeks * 7;
            return new TimeSpan(days, hours, minutes, 0);
        }

        public static DateTime At(string r)
        {
            DateTime today = DateTime.Today;
            DateTime now = DateTime.Now;
            int form = (Regex.IsMatch(r, "past|to", RegexOptions.IgnoreCase)) ? 0 : (Regex.IsMatch(r, "AM|PM", RegexOptions.IgnoreCase)) ? 1 : 2;
            if (form == 0) // Half past two/Ten to twelve
            {
                Match matches = Regex.Match(r, @"(\d{1,2}) (to|past) (\d{1,2})", RegexOptions.IgnoreCase);
                int hour = int.Parse(matches.Groups[1].Value);
                int minute = int.Parse(matches.Groups[3].Value) * (matches.Groups[2].Value.ToLower() == "to" ? -1 : 1);
                if (hour < today.Hour) today.AddDays(1);
                today.AddHours(hour);
                today.AddMinutes(minute);
            }
            else if (form == 1) // AM / PM
            {
                Match matches = Regex.Match(r, @"(\d{1,2}) {0,1}(am|pm) {0,1}(\d{1,2})", RegexOptions.IgnoreCase);
                if (matches.Groups[1].Value == "") matches = Regex.Match(r.Replace(" ", ""), @"(\d{1,2}) {0,1}(am|pm)", RegexOptions.IgnoreCase);
                int hour = int.Parse(matches.Groups[1].Value);
                int minute = (matches.Groups[3] != null) ? int.Parse(matches.Groups[3].Value) : 0;
                if (hour < today.Hour) today.AddDays(1);
                today.AddHours(hour);
                today.AddMinutes(minute);
            }
            else // 24h format
            {
                Match matches = Regex.Match(r, @"(\d{1,2}) {0,1}(:|h|hour|hours) {0,1}(\d{1,2})", RegexOptions.IgnoreCase);
                if (matches.Groups[1].Value == "") matches = Regex.Match(r.Replace(" ", ""), @"(\d{1,2}) {0,1}(:|h|hour|hours)", RegexOptions.IgnoreCase);
                int hour = int.Parse(matches.Groups[1].Value);
                int minute = (matches.Groups[3] != null) ? int.Parse(matches.Groups[3].Value) : 0;
                if (hour < today.Hour) today.AddDays(1);
                today.AddHours(hour);
                today.AddMinutes(minute);
            }
            return today;
        }
    }
}
