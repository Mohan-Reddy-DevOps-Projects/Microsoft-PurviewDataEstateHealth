namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util
{
    using System;
    using System.Globalization;

    public static class TimeSpanUtils
    {
        public static TimeSpan ConvertToInvariantTimeSpan(string timeSpantoString)
        {
            if (String.IsNullOrWhiteSpace(timeSpantoString))
            {
                throw new ArgumentNullException(nameof(timeSpantoString), "value cannot be null or empty");
            }
            timeSpantoString = timeSpantoString.Trim();
            bool negativeFlag = false;
            long seconds = 0;
            int minutes = 0;
            int hours = 0;
            int days = 0;
            if (timeSpantoString.StartsWith("-", StringComparison.OrdinalIgnoreCase))
            {
                timeSpantoString = timeSpantoString.Substring(1);
                negativeFlag = true;
            }
            string[] splitTime = timeSpantoString.Split(':');

            //6:12:14:45 -> 6.12:14:45
            if (splitTime.Length == 4)
            {
                if (splitTime[0].Contains('.', StringComparison.OrdinalIgnoreCase))
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                }
                if (!int.TryParse(splitTime[0], out days))
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                }
                if (!int.TryParse(splitTime[1], out hours))
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                }
                if (!int.TryParse(splitTime[2], out minutes))
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                }
                seconds = Convert.ToInt64(Convert.ToDouble(splitTime[3], CultureInfo.InvariantCulture) * TimeSpan.TicksPerSecond);
            }
            else if (splitTime.Length == 1)
            {
                //10 -> 10.00:00:00
                if (!int.TryParse(splitTime[0], out days))
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                }
            }
            else if (splitTime.Length > 1 && splitTime.Length < 4)
            {
                // 10:10 -> 10:10:00   23:59:70 -> 1.00:00:10
                string[] splitDayandHour = splitTime[0].Split('.');
                if (splitDayandHour.Length > 1)
                {
                    if (!int.TryParse(splitDayandHour[0], out days))
                    {
                        throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                    }
                    if (!int.TryParse(splitDayandHour[1], out hours))
                    {
                        throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                    }
                }
                else
                {
                    if (!int.TryParse(splitDayandHour[0], out hours))
                    {
                        throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                    }
                }

                if (!int.TryParse(splitTime[1], out minutes))
                {
                    throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
                }
                if (splitTime.Length > 2)
                {
                    seconds = Convert.ToInt64(Convert.ToDouble(splitTime[2], CultureInfo.InvariantCulture) * TimeSpan.TicksPerSecond);
                }
            }
            else
            {
                throw new FormatException(string.Format(CultureInfo.InvariantCulture, "{0} : Bad Format.", timeSpantoString));
            }
            var timeSpan = new TimeSpan(days, hours, minutes, 0).Add(TimeSpan.FromTicks(seconds));
            if (negativeFlag)
            {
                timeSpan = timeSpan.Negate();
            }
            return timeSpan;
        }
    }
}
