using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class RepairOrder
{
    public Guid RepairOrderId { get; set; }

    public string OrderNumber { get; set; } = null!;

    public bool? Paid { get; set; }

    public string? PaymentMethod { get; set; }

    public string? IssueDescription { get; set; }

    public string? RepairStatus { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    public Guid? StoreId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? Delind { get; set; }

    public Guid? CustomerId { get; set; }

    public Guid? UserId { get; set; }

    public bool? Isfinalsubmit { get; set; }

    public decimal? TotalAmount { get; set; }

    public string? ProductType { get; set; }

    public bool? Cancelled { get; set; }

    public string? Contactmethod { get; set; }

    public decimal? Paidamount { get; set; }

    public string? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    public decimal? TaxPercent { get; set; }

    public string? Status { get; set; }

    public bool WebUpload { get; set; }

    public string? OrderType { get; set; }

    public virtual ICollection<ChecklistResponse> ChecklistResponses { get; set; } = new List<ChecklistResponse>();

    public virtual ICollection<RepairOrderPart> RepairOrderParts { get; set; } = new List<RepairOrderPart>();
}
