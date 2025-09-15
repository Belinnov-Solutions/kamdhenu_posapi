using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class SyncLog
{
    public Guid Id { get; set; }

    public string Entity { get; set; } = null!;

    public int RecordsProcessed { get; set; }

    public string Status { get; set; } = null!;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
}
