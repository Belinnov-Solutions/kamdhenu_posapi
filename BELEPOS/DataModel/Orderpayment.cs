using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class OrderPayment
{
    public Guid Paymentid { get; set; }

    public Guid Repairorderid { get; set; }

    public decimal Amount { get; set; }

    public string? PaymentMethod { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? PartialPayment { get; set; }

    public decimal TotalAmount { get; set; }

    public bool FullyPaid { get; set; }

    public decimal? Remainingamount { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual RepairOrder Repairorder { get; set; } = null!;
}
