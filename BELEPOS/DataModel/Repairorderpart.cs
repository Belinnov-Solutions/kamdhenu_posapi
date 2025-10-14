using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class RepairOrderPart
{
    public Guid Id { get; set; }

    public Guid RepairOrderId { get; set; }

    public Guid? ProductId { get; set; }

    public string ProductName { get; set; } = null!;

    public string? PartDescription { get; set; }

    public string? DeviceType { get; set; }

    public string? DeviceModel { get; set; }

    public string? SerialNumber { get; set; }

    public int Quantity { get; set; }

    public decimal? Price { get; set; }

    public decimal? Total { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool? Delind { get; set; }

    public string? BrandName { get; set; }

    public string? ProductType { get; set; }

    public bool? Cancelled { get; set; }

    public string? Tokennumber { get; set; }

    public Guid? Subcategoryid { get; set; }

    public DateTime? OrderDate { get; set; }

    public virtual RepairOrder RepairOrder { get; set; } = null!;
}
