using System;

namespace Waffler.Function.Util
{
    public class FunctionDisabledException : Exception
    {
        public FunctionDisabledException() :
            base("Function is disabled")
        {

        }
    }
}
