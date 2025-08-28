using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Order
{
    public Guid OrderId { get; set; }

    public Guid StoreId { get; set; }

    public bool? Paid { get; set; }

    public string? Paymentmethod { get; set; }

    public decimal TotalAmount { get; set; }

    public decimal? Discount { get; set; }

    public decimal? Tax { get; set; }

    public decimal GrandTotal { get; set; }

    public string? Status { get; set; }

    public DateTime? OrderDate { get; set; }

    public Guid CustomerId { get; set; }

    public Guid UserId { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? Delind { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual Store Store { get; set; } = null!;
}
