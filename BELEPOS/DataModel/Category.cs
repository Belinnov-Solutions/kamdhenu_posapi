using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Category
{
    public Guid Categoryid { get; set; }

    public string CategoryName { get; set; } = null!;

    public string Imagename { get; set; } = null!;

    public bool? Delind { get; set; }

    public DateTime? CreatedAt { get; set; }

    public Guid? StoreId { get; set; }

    public DateTime? LastmodifiedAt { get; set; }

    public bool? IsVisible { get; set; }
}
