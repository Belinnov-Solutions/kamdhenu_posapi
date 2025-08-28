using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Transaction
{
    public Guid Transactionid { get; set; }

    public Guid Repairorderid { get; set; }

    public Guid? Ticketid { get; set; }

    public decimal Amountpaid { get; set; }

    public string? Paymentmethod { get; set; }

    public DateTime? Transactiondate { get; set; }

    public virtual RepairOrder Repairorder { get; set; } = null!;

    public virtual RepairTicket? Ticket { get; set; }
}
