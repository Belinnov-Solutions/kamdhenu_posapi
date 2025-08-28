namespace BELEPOS.Entity
{
    public class CustomerDto
    {
        public Guid CustomerId { get; set; }
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public string? Address { get; set; }
        
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? Zipcode { get; set; }
        
    }
}
