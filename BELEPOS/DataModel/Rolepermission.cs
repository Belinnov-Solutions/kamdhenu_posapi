using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class RolePermission
{
    public int Permissionid { get; set; }

    public Guid? Roleid { get; set; }

    public virtual Permission Permission { get; set; } = null!;

    public virtual Role? Role { get; set; }
}
