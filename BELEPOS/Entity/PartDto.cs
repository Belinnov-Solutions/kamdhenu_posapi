namespace BELEPOS.Entity
{
    public class PartDto
    {

        public Guid? PartId { get; set; }
        public string PartName { get; set; } = null!;
        public string PartNumber { get; set; } = null!;
        public string? SerialNumber { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public int? Stock { get; set; }
        public int? OpeningStock { get; set; }
        public DateTime? OpeningStockDate { get; set; }
        public string? Location { get; set; }
        public bool? InStock { get; set; }
        public Guid? StoreId { get; set; }
    }
}
