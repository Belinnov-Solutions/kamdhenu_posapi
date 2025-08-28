using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Model
{
    public Guid ModelId { get; set; }

    public string Name { get; set; } = null!;

    public Guid BrandId { get; set; }

    public string? DeviceType { get; set; }

    public bool? Delind { get; set; }

    public virtual Brand Brand { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
