namespace BELEPOS.Entity
{
    public class StoreDto
    {
        public Guid? Id { get; set; } 
        public Guid TenantId { get; set; }
        public string Name { get; set; } = "";
        public string? Address { get; set; }
        public string? Username { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
