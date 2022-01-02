using System;
using System.Web;

namespace Waffler.Service.Converter
{
    public static class DateTimeStringFormatConverter
    {
        public static string GetDateTimeString(DateTime dateTime)
        {
            var dateTimeString = HttpUtility.UrlEncode(dateTime.ToString("o"));

            //TODO: WHY is the Z included in some cases and in some not?!
            //This fix solves the problem for now...
            return dateTimeString.EndsWith("Z") ? dateTimeString : dateTimeString + "Z";
        }
    }
}
