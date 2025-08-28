using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class RepairChecklist
{
    public Guid Id { get; set; }

    public string DeviceType { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public bool? IsMandatory { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? CheckText { get; set; }

    public virtual ChecklistCategory Category { get; set; } = null!;

    public virtual ICollection<ChecklistResponse> ChecklistResponses { get; set; } = new List<ChecklistResponse>();
}
