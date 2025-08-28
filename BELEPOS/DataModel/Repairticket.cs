using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class RepairTicket
{
    public Guid Ticketid { get; set; }

    public Guid Storeid { get; set; }

    public string? DeviceType { get; set; }

    public string? Ipaddress { get; set; }

    public Guid Userid { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? ImeiNumber { get; set; }

    public string? SerialNumber { get; set; }

    public string? Passcode { get; set; }

    public decimal? ServiceCharge { get; set; }

    public decimal? Repaircost { get; set; }

    public Guid? Technicianid { get; set; }

    public DateTime? Createdat { get; set; }

    public DateTime? Duedate { get; set; }

    public string? Status { get; set; }

    public Guid? Tasktypeid { get; set; }

    public Guid? OrderId { get; set; }

    public bool? Delind { get; set; }

    public string? TicketNo { get; set; }

    public bool? Cancelled { get; set; }

    public string? Cancelreason { get; set; }

    public string? DeviceColour { get; set; }

    public virtual ICollection<ChecklistResponse> ChecklistResponses { get; set; } = new List<ChecklistResponse>();

    public virtual Store Store { get; set; } = null!;
}
