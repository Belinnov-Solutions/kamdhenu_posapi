using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Part
{
    public Guid PartId { get; set; }

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

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? StoreId { get; set; }

    public bool? Delind { get; set; }

    public Guid? ModelId { get; set; }

    public Guid? BrandId { get; set; }
}
