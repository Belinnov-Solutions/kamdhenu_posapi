using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Customer
{
    public Guid CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Country { get; set; }

    public string? Zipcode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? Delind { get; set; }
}
