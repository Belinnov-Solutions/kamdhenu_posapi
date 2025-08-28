using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Status
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public bool? Delind { get; set; }
}
