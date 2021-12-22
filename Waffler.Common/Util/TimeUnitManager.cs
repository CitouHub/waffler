namespace Waffler.Common.Util
{
    public static class TimeUnitManager
    {
        public static int GetTimeValue(Variable.TimeUnit timeUnit, int? value)
        {
            if(value is null)
            {
                return 0;
            }

            return timeUnit switch
            {
                Variable.TimeUnit.Week => (int)value / (7 * 24 * 60),
                Variable.TimeUnit.Day => (int)value / (24 * 60),
                Variable.TimeUnit.Hour => (int)value / 60,
                Variable.TimeUnit.Minute => (int)value,
                _ => (int)value,
            };
        }

        public static Variable.TimeUnit GetTimeUnit(int? value)
        {
            if(value is null)
            {
                return Variable.TimeUnit.Other;
            }

            if(value == 0)
            {
                return Variable.TimeUnit.Minute;
            } 
            if (value % (7 * 24 * 60) == 0)
            {
                return Variable.TimeUnit.Week;
            }
            if (value % (24 * 60) == 0)
            {
                return Variable.TimeUnit.Day;
            }
            else if (value % 60 == 0)
            {
                return Variable.TimeUnit.Hour;
            }

            return Variable.TimeUnit.Minute;
        }
    }
}
