using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class User
{
    public string Username { get; set; } = null!;

    public string Passwordhash { get; set; } = null!;

    public DateTime? Createdon { get; set; }

    public bool? Isactive { get; set; }

    public Guid? Roleid { get; set; }

    public Guid Userid { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Description { get; set; }

    public string? Pin { get; set; }

    public Guid? Storeid { get; set; }

    public bool? DelInd { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual Role? Role { get; set; }

    public virtual Store? Store { get; set; }
}
