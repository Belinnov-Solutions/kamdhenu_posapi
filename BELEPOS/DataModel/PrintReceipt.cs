using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class PrintReceipt
{
    public Guid Id { get; set; }

    public string ReceiptName { get; set; } = null!;

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }
}
