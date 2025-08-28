using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class ProductImage
{
    public int Imageid { get; set; }

    public Guid Productid { get; set; }

    public string Imagename { get; set; } = null!;

    public bool? Main { get; set; }

    public DateTime? Createdat { get; set; }

    public bool? Delind { get; set; }
}
