using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class OrderItem
{
    public Guid OrderPartId { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductId { get; set; }

    public Guid? BrandId { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? ImeiNumber { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? Delind { get; set; }

    public string? ProductName { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product Product { get; set; } = null!;
}
