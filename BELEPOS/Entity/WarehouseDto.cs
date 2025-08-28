using System.ComponentModel.DataAnnotations;

namespace BELEPOS.Entity
{
    public class WarehouseDto
    {

        public Guid Id { get; set; }
        public Guid StoreId { get; set; }

        public string? Name { get; set; }

        public string? ContactPerson { get; set; }
        
        [Phone]
        public string? Phone { get; set; }
       
        [Phone]
        public string? WorkPhone { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }

        public string? AddressLine1 { get; set; }

        public string? AddressLine2 { get; set; }

        public string? Country { get; set; }

        public string? State { get; set; }

        public string? City { get; set; }

        public string? Zipcode { get; set; }

    }
}
