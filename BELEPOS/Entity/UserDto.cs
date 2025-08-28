using System.ComponentModel.DataAnnotations.Schema;

namespace BELEPOS.Entity.Auth
{
    public class UserDto
    {
        public string Username { get; set; } = default!;
        public string? Password { get; set; } = default!;
        public bool IsActive { get; set; } = true;
        public string Role { get; set; } = default!;

        public Guid? UserId { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Description { get; set; }
        public string? Pin { get; set; }
        public Guid? StoreId { get; set; }
    }

    public class RoleDto
    {
        [Column("roleid")]
        public int RoleId { get; set; }

        [Column("rolename")]
        public string RoleName { get; set; } = default!;

    }
}
