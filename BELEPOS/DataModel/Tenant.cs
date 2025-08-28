using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Tenant
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Store> Stores { get; set; } = new List<Store>();
}
