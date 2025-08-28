using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Devicetype
{
    public Guid DeviceId { get; set; }

    public string DeviceType { get; set; } = null!;

    public bool? Delind { get; set; }
}
