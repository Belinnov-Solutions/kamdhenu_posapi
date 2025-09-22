using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class SubCategory
{
    public Guid Subcategoryid { get; set; }

    public Guid Categoryid { get; set; }

    public string Name { get; set; } = null!;

    public string? Image { get; set; }

    public string? Code { get; set; }

    public string? Description { get; set; }

    public bool? Delind { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? StoreId { get; set; }

    public bool? WebUpload { get; set; }
}
