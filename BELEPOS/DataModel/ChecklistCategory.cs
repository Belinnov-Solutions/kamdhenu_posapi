using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class ChecklistCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string CheckType { get; set; } = null!;

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<RepairChecklist> RepairChecklists { get; set; } = new List<RepairChecklist>();
}
