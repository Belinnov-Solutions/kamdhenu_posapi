using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class ChecklistResponse
{
    public Guid Id { get; set; }

    public Guid? TicketId { get; set; }

    public Guid? OrderId { get; set; }

    public Guid ChecklistId { get; set; }

    public bool? Value { get; set; }

    public DateTime? RespondedAt { get; set; }

    public string? RepairInspection { get; set; }

    public virtual RepairChecklist Checklist { get; set; } = null!;

    public virtual RepairOrder? Order { get; set; }

    public virtual RepairTicket? Ticket { get; set; }
}
