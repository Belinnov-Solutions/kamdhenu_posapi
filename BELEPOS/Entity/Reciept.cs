namespace BELEPOS.Entity
{
    public class Reciept
    {
        public string OrderNumber { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TaxPercent { get; set; }
        public string DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public string StoreName { get; set; }
        public string StoreAddress { get; set; }
        public string StorePhone { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }

        public string? Tokennumber { get; set; }

        public Guid? Subcategoryid { get; set; }
        public Guid CategoryId { get; set; }
        public String? CategoryName { get; set; }

        public string? OrderType { get; set; }
        public string? PaymentMethod { get; set; }

    }
}
