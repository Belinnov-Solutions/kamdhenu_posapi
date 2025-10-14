namespace BELEPOS.Entity
{
    public class SyncResult
    {
        public bool Success { get; set; }
        public int Orders { get; set; }
        public string? Error { get; set; }
        public string? Message { get; set; }
        public DateTime RanAtUtc { get; set; }
    }
}
