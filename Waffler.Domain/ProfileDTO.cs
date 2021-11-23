namespace Waffler.Domain
{
    public class ProfileDTO
    {
        public string Password { get; set; }
        public string ApiKey { get; set; }
        public int CandleStickSyncOffsetDays { get; set; }
    }
}
