using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Notification
{
    public Guid Notificationid { get; set; }

    public string? Notificationtype { get; set; }

    public string? Notificationtypecode { get; set; }

    public string? Notificationsubject { get; set; }

    public string? Notificationbody { get; set; }

    public DateTime? Datecreated { get; set; }

    public bool? Delind { get; set; }

    public Guid? Referenceid { get; set; }
}
