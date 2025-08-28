using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Warehouse
{
    public Guid Id { get; set; }

    public Guid StoreId { get; set; }

    public string Name { get; set; } = null!;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public string? WorkPhone { get; set; }

    public string? Email { get; set; }

    public string AddressLine1 { get; set; } = null!;

    public string? AddressLine2 { get; set; }

    public string Country { get; set; } = null!;

    public string State { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Zipcode { get; set; } = null!;

    public bool? IsActive { get; set; }

    public bool? DelInd { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public virtual Store Store { get; set; } = null!;
}
