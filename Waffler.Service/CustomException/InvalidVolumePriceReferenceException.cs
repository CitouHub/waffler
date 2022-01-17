using System;

namespace Waffler.Service.CustomException
{
    public class InvalidVolumePriceReferenceException : Exception
    {
        public InvalidVolumePriceReferenceException()
            : base("Volume can not be used as a price reference")
        {
        }
    }
}