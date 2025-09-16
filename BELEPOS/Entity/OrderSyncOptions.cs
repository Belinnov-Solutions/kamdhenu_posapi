namespace BELEPOS.Entity
{
    public class OrderSyncOptions
    {
        public int IntervalMinutes { get; set; } = 5;
        public int BatchSize { get; set; } = 500;
    }
}
