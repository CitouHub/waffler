using System;
using Waffler.Data;
using Waffler.Domain;

namespace Waffler.Test.Helper
{
    public static class ProfileHelper
    {
        public static WafflerProfile GetProfile()
        {
            return new WafflerProfile()
            {
                ApiKey = Guid.NewGuid().ToString(),
                CandleStickSyncFromDate = DateTime.UtcNow.AddDays(-90),
                Password = "Test password hash"
            };
        }

        public static ProfileDTO GetProfileDTO()
        {
            return new ProfileDTO()
            {
                ApiKey = Guid.NewGuid().ToString(),
                CandleStickSyncFromDate = DateTime.UtcNow.AddDays(-90),
                Password = "Test password hash"
            };
        }
    }
}
