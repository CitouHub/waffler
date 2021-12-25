using System;

using Waffler.Domain;

namespace Waffler.Test.Helper
{
    public static class ProfileHelper
    {
        public static ProfileDTO GetProfile()
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
