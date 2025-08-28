using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class ServiceCatalogue
{
    public Guid ServiceId { get; set; }

    public string TaskName { get; set; } = null!;

    public string? ServiceDescription { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public decimal? ServicePrice { get; set; }

    public bool? Delind { get; set; }

    public Guid? TaskTypeId { get; set; }
}
