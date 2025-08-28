using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Role
{
    public string Rolename { get; set; } = null!;

    public Guid Roleid { get; set; }

    public int? RoleOrder { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
