using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Brand
{
    public string Name { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public Guid? StoreId { get; set; }

    public bool? Isactive { get; set; }

    public bool? Delind { get; set; }

    public Guid Id { get; set; }

    public virtual ICollection<Model> Models { get; set; } = new List<Model>();

    public virtual Store? Store { get; set; }
}
