using Xunit;

using Waffler.Common;
using Waffler.Domain.Converter;

namespace Waffler.Test.Util
{
    public class TimeUnitManagerTest
    {
        [Theory]
        [InlineData(null, Variable.TimeUnit.Other)]
        [InlineData(0, Variable.TimeUnit.Minute)]
        [InlineData(59, Variable.TimeUnit.Minute)]
        [InlineData(60, Variable.TimeUnit.Hour)]
        [InlineData(60 * 23, Variable.TimeUnit.Hour)]
        [InlineData(60 * 24 - 1, Variable.TimeUnit.Minute)]
        [InlineData(60 * 24, Variable.TimeUnit.Day)]
        [InlineData(60 * 24 * 6, Variable.TimeUnit.Day)]
        [InlineData(60 * 24 * 7 - 1, Variable.TimeUnit.Minute)]
        [InlineData(60 * 24 * 7, Variable.TimeUnit.Week)]
        [InlineData(60 * 24 * 8, Variable.TimeUnit.Day)]
        [InlineData(60 * 24 * 14, Variable.TimeUnit.Week)]
        public void TimeUnitManager_GetTimeUnit(int? minutes, Variable.TimeUnit expectedTimeUnit)
        {
            //Act
            var timeUnit = TimeUnitFormatConverter.GetTimeUnit(minutes);

            //Assert
            Assert.Equal(expectedTimeUnit, timeUnit);
        }

        [Theory]
        [InlineData(Variable.TimeUnit.Other, null, 0)]
        [InlineData(Variable.TimeUnit.Minute, 30, 30)]
        [InlineData(Variable.TimeUnit.Hour, 2 * 60, 2)]
        [InlineData(Variable.TimeUnit.Day, 2 * 60 * 24, 2)]
        [InlineData(Variable.TimeUnit.Week, 2 * 60 * 24 * 7, 2)]
        public void TimeUnitManager_GetTimeValue(Variable.TimeUnit timeUnit, int? minuts, int expectedUnits)
        {
            //Act
            var units = TimeUnitFormatConverter.GetTimeValue(timeUnit, minuts);

            //Assert
            Assert.Equal(expectedUnits, units);
        }
    }
}
