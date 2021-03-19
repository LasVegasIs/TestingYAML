namespace Core.Extensions.DateTimeExtensions
{
    public static class DateTimeExtensions
    {
        // Private b/c you need IntoReversedOrderedKey most certainly
        public static long IntoReversedTick(this System.DateTime time)
        {
            return (System.DateTime.MaxValue - time).Ticks;
        }

        public static string IntoReversedOrderedKey(this System.DateTime time)
        {
            return time.IntoReversedTick().ToString().PadLeft(19);
        }

        private static long IntoReversedTick(this System.DateTimeOffset time)
        {
            return (System.DateTimeOffset.MaxValue - time).Ticks;
        }

        public static string IntoReversedOrderedKey(this System.DateTimeOffset time)
        {
            return time.IntoReversedTick().ToString().PadLeft(19);
        }

        public static string ToIsoString(this System.DateTime time)
        {
            return time.ToUniversalTime().ToString("o");     // ISO 8601 format
        }
    }
}
