using System;
using System.Web;

using Waffler.Service.Converter;

using Xunit;

namespace Waffler.Test.Service.Converter
{
    public class DateTimeStringFormatConverterTest
    {
        [Fact]
        public void GetDateTimeString()
        {
            //Act
            var dateTime = DateTime.UtcNow;
            var dateTimeString = DateTimeStringFormatConverter.GetDateTimeString(dateTime);

            //Assert
            Assert.True(HttpUtility.UrlDecode(dateTimeString) != dateTimeString);
            Assert.EndsWith("Z", dateTimeString);
        }
    }
}
