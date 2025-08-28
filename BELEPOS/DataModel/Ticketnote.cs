using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Ticketnote
{
    public Guid Noteid { get; set; }

    public Guid Ticketid { get; set; }

    public string Note { get; set; } = null!;

    public string? Type { get; set; }

    public Guid Userid { get; set; }

    public DateTime? Datecreated { get; set; }

    public Guid? OrderId { get; set; }
}
