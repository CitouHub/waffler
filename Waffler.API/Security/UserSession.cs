using System;

namespace Waffler.API.Security
{
    public static class UserSession
    {
        public static int SessionValidSeconds { get; set; }

        public static string ApiKey { private set; get; }
        private static DateTime Expiration;

        public static void New()
        {
            ApiKey = Guid.NewGuid().ToString();
            Expiration = DateTime.UtcNow.AddSeconds(SessionValidSeconds);
        }

        public static bool IsValid()
        {
            return DateTime.UtcNow < Expiration;
        }

        public static void Refresh()
        {
            if(IsValid())
            {
                Expiration = DateTime.UtcNow.AddSeconds(SessionValidSeconds);
            }
        }
    }
}