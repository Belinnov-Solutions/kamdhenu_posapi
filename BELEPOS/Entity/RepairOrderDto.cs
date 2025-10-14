

namespace BELEPOS.Entity
{
    public class RepairOrderDto
    {

        public Guid RepairOrderId { get; set; } = Guid.Empty;
        public string? OrderNumber { get; set; }
        public bool? Paid { get; set; }
        public string? PaymentMethod { get; set; }

        public string? OrderType { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? RepairStatus { get; set; }
        public Guid? CustomerId { get; set; } = Guid.Empty;
        public Guid UserId { get; set; }
        public string? IssueDescription { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public Guid? StoreId { get; set; }
        public bool? Cancelled { get; set; }

        public List<string>? Contactmethod { get; set; } = new();

        public bool? IsFinalSubmit { get; set; }
        public List<OrderPaymentDto>? Payments { get; set; }
        public List<RepairOrderPartDto>? Parts { get; set; }

        public RepairTicketsDto? Tickets { get; set; }

        public DateTime? ReceivedDate { get; set; }

        public DateTime? CreatedAt { get; set; }

        public DateTime? OrderDate { get; set; }

        public string? ProductType { get; set; }

        public decimal? PaidAmount { get; set; }
        public string? DiscountType { get; set; }  // "Flat" or "Percent"
        public decimal? DiscountValue { get; set; } = 0;
        public decimal? TaxPercent { get; set; } = 0;

        public decimal? TaxAmount { get; set; } = 0;

        public decimal? SubTotal { get; set; } = 0;

        public decimal? TotalAmount { get; set; }
        public bool? Delind { get; set; }
        //public List<RepairOrderPartDto>? RepairOrderPartDto { get; set; }
        public RepairChecklistDto? ChecklistResponses { get; set; }

    }


    public class RepairOrderPartDto/* : ProductDto*/
    {
        public Guid Id { get; set; }
        public Guid RepairOrderId { get; set; }

        public Guid? ProductId { get; set; }

        public Guid SubcategoryId { get; set; }

        public string ProductName { get; set; }

        public string? BrandName { get; set; }
        public string? PartDescription { get; set; }
        public string? DeviceType { get; set; }
        public string? DeviceModel { get; set; }
        public string? SerialNumber { get; set; }
        public int Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? Total { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public DateTime? OrderDate { get; set; }

        public bool? Delind { get; set; }
        public bool? Cancelled { get; set; }

        public string? TokenNumber { get; set; }

        public string ProductType { get; set; }
    }
    public class OrderPaymentDto
    {
        public Guid PaymentId { get; set; }
        public Guid RepairOrderId { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool? PartialPayment { get; set; }
        public decimal TotalAmount { get; set; }
        public bool FullyPaid { get; set; }
        public decimal? RemainingAmount { get; set; }

        public DateTime? OrderDate { get; set; }


    }

    public class RepairOrderSummaryDto
    {
        public Guid RepairOrderId { get; set; }
        public string? OrderNumber { get; set; }
        public string? CustomerName { get; set; }
        public decimal? TotalAmount { get; set; }
        public string? TaskName { get; set; }
        public decimal? ServicePrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid Id { get; set; }
        public string Status { get; set; } // "Paid", "Pending", etc.
        public bool WebUpload { get; set; } = false; // Flag to track sync
    }
}
