using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class ProductVariant
{
    public Guid VariantId { get; set; }

    public Guid ProductId { get; set; }

    public string VariantName { get; set; } = null!;

    public string Sku { get; set; } = null!;

    public decimal Price { get; set; }

    public int Quantity { get; set; }

    public bool Delind { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Product Product { get; set; } = null!;
}
