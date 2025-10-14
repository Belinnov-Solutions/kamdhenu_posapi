namespace BELEPOS.Entity
{
    public class OrderSyncEnvelope
    {
        public string? SourceSystem { get; set; }
        public DateTime GeneratedUtc { get; set; }
        public List<RepairOrderDto> RepairOrders { get; set; } = new();
    }
}
