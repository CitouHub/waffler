namespace Waffler.Common.Util
{
    public static class TimeUnitManager
    {
        public static int GetTimeValue(Variable.TimeUnit timeUnit, int? minutes)
        {
            if(minutes is null)
            {
                return 0;
            }

            return timeUnit switch
            {
                Variable.TimeUnit.Week => (int)minutes / (7 * 24 * 60),
                Variable.TimeUnit.Day => (int)minutes / (24 * 60),
                Variable.TimeUnit.Hour => (int)minutes / 60,
                Variable.TimeUnit.Minute => (int)minutes,
                _ => (int)minutes,
            };
        }

        public static Variable.TimeUnit GetTimeUnit(int? minutes)
        {
            if(minutes is null)
            {
                return Variable.TimeUnit.Other;
            }

            if(minutes == 0)
            {
                return Variable.TimeUnit.Minute;
            } 
            if (minutes % (7 * 24 * 60) == 0)
            {
                return Variable.TimeUnit.Week;
            }
            if (minutes % (24 * 60) == 0)
            {
                return Variable.TimeUnit.Day;
            }
            else if (minutes % 60 == 0)
            {
                return Variable.TimeUnit.Hour;
            }

            return Variable.TimeUnit.Minute;
        }
    }
}
