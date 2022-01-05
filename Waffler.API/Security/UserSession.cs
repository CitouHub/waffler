using System;

namespace Waffler.API.Security
{
    public static class UserSession
    {
        private static readonly int SessionValidMinuets = 60 * 20; //20 minutes

        public static string ApiKey { private set; get; }
        private static DateTime Expiration;

        public static void New()
        {
            ApiKey = Guid.NewGuid().ToString();
            Expiration = DateTime.UtcNow.AddMinutes(SessionValidMinuets);
        }

        public static bool IsValid()
        {
            return DateTime.UtcNow < Expiration;
        }

        public static void Refresh()
        {
            Expiration = DateTime.UtcNow.AddMinutes(SessionValidMinuets);
        }
    }
}