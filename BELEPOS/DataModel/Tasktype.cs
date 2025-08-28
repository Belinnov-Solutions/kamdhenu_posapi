using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class TaskType
{
    public Guid TaskTypeId { get; set; }

    public string TaskTypeName { get; set; } = null!;

    public bool? Delind { get; set; }
}
