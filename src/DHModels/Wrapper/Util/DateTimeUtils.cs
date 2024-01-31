namespace Microsoft.Purview.DataEstateHealth.DHModels.Wrapper.Util
{
    using System;
    using System.Globalization;

    public static class DateTimeUtils
    {
        public static string GetCurrentDatetimeStr()
        {
            return DateTime.UtcNow.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }

        public static string GetDateTimeStr(this DateTime dateTime)
        {
            return dateTime.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
        }
    }
}
