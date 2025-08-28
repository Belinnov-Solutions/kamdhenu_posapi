using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class RefreshToken
{
    public int Refreshtokenid { get; set; }

    public string Token { get; set; } = null!;

    public DateTime? Createdat { get; set; }

    public DateTime? Expiresat { get; set; }

    public Guid? UseridUuid { get; set; }

    public virtual User? UseridUu { get; set; }
}
