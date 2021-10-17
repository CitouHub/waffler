using System;
using System.Diagnostics;

namespace Waffler.Function.Util
{
    [AttributeUsage(AttributeTargets.Method)]
    public class DebugDisableAttribute : Attribute, IDisposable
    {
        public DebugDisableAttribute()
        {
            if (Debugger.IsAttached)
            {
                throw new FunctionDisabledException();
            }
        }

        public void Dispose()
        {
        }
    }
}
