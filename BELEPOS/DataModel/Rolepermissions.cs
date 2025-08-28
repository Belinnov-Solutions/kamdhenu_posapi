using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

 namespace BELEPOS.Entity.Auth
    {
        [Table("rolepermissions")]
        public class RolePermission
        {
            [Column("roleid")]
            public int RoleId { get; set; }

            

            [Column("permissionid")]
            public int PermissionId { get; set; }

            //public Role Role { get; set; } = default!;
            //public Permission Permission { get; set; } = default!;
        }
    }

