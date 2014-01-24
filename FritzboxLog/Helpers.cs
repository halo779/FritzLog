using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace FritzboxLog
{
    public static class Helpers
    {
        public static DateTime FromUnixTime(this long unixTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return epoch.AddSeconds(unixTime);
        }

        public static long ToUnixTime(this DateTime date)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return Convert.ToInt64((date.ToUniversalTime() - epoch).TotalSeconds);
        }

        public static double FeetToMeters(double Feet)
        {
            return Feet * 0.304800610;
        }

        public static string HuntForLine(this FileInfo info, Predicate<string> regex)
        {
            string line;

            using (var reader = new StreamReader(info.FullName))
            {
                while ((line = reader.ReadLine()) != null)
                {
                    if (regex(line))
                    {
                        return line.ToString();
                    }
                }
            }

            return null;
        }
    }
}
