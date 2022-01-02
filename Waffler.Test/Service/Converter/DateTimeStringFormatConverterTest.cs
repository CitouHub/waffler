using System;
using System.Web;

using Xunit;

using Waffler.Domain.Converter;

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
