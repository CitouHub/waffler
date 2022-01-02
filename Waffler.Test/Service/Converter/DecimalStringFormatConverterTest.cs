using Newtonsoft.Json;
using Waffler.Domain.Converter;
using Xunit;

namespace Waffler.Test.Service.Converter
{
    public class DecimalStringFormatConverterTest
    {
        private readonly DecimalStringFormatConverter _converter = new();

        [Theory]
        [InlineData(1.1, true)]
        [InlineData(-1.1, true)]
        public void CanConvert_Decimal(decimal value, bool expectedCanConvert)
        {
            //Assert
            Assert.Equal(expectedCanConvert, _converter.CanConvert(value.GetType()));
        }

        [Theory]
        [InlineData("1.1", false)]
        [InlineData("Hej", false)]
        [InlineData('X', false)]
        [InlineData(true, false)]
        [InlineData(1, false)]
        [InlineData(-1, false)]
        [InlineData(1U, false)]
        [InlineData(1L, false)]
        [InlineData(1UL, false)]
        [InlineData(1.1F, true)]
        [InlineData(-1.1F, true)]
        [InlineData(1.1D, true)]
        [InlineData(-1.1D, true)]
        public void CanConvert_OtherFormats(object value, bool expectedCanConvert)
        {
            //Assert
            Assert.Equal(expectedCanConvert, _converter.CanConvert(value.GetType()));
        }

        [Theory]
        [InlineData(1.1, "{\"Value\":\"1.1\"}")]
        [InlineData(-1.1, "{\"Value\":\"-1.1\"}")]
        [InlineData(1, "{\"Value\":\"1\"}")]
        [InlineData(-1, "{\"Value\":\"-1\"}")]
        public void WriteJson_Decimal(decimal value, string expectedString)
        {
            //Act
            var orderJson = JsonConvert.SerializeObject(new
            {
                Value = value
            }, new DecimalStringFormatConverter());

            //Assert
            Assert.Equal(expectedString, orderJson);
        }

        [Theory]
        [InlineData(1.1, "{\"Value\":\"1.1\"}")]
        [InlineData(-1.1, "{\"Value\":\"-1.1\"}")]
        [InlineData(1, "{\"Value\":\"1\"}")]
        [InlineData(-1, "{\"Value\":\"-1\"}")]
        public void WriteJson_Double(double value, string expectedString)
        {
            //Act
            var orderJson = JsonConvert.SerializeObject(new
            {
                Value = value
            }, new DecimalStringFormatConverter());

            //Assert
            Assert.Equal(expectedString, orderJson);
        }

        [Theory]
        [InlineData(1.1, "{\"Value\":\"1.1\"}")]
        [InlineData(-1.1, "{\"Value\":\"-1.1\"}")]
        [InlineData(1, "{\"Value\":\"1\"}")]
        [InlineData(-1, "{\"Value\":\"-1\"}")]
        public void WriteJson_Float(float value, string expectedString)
        {
            //Act
            var orderJson = JsonConvert.SerializeObject(new
            {
                Value = value
            }, new DecimalStringFormatConverter());

            //Assert
            Assert.Equal(expectedString, orderJson);
        }
    }
}
