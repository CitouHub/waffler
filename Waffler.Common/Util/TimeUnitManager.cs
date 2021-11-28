namespace Waffler.Common.Util
{
    public static class TimeUnitManager
    {
        public static int GetTimeValue(Variable.TimeUnit timeUnit, int value)
        {
            return timeUnit switch
            {
                Variable.TimeUnit.Week => value / (7 * 24 * 60),
                Variable.TimeUnit.Day => value / (24 * 60),
                Variable.TimeUnit.Hour => value / 60,
                Variable.TimeUnit.Minute => value,
                _ => value,
            };
        }

        public static Variable.TimeUnit GetTimeUnit(int value)
        {
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
