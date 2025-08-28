using System;
using System.Collections.Generic;

namespace BELEPOS.DataModel;

public partial class Product
{
    public Guid Id { get; set; }

    public Guid? StoreId { get; set; }

    public Guid? WarehouseId { get; set; }

    public string Name { get; set; } = null!;

    public string? Slug { get; set; }

    public string? Sku { get; set; }

    public string? SellingType { get; set; }

    public string? Unit { get; set; }

    public string? Barcode { get; set; }

    public string? Description { get; set; }

    public bool? IsVariable { get; set; }

    public decimal? Price { get; set; }

    public string? TaxType { get; set; }

    public string? DiscountType { get; set; }

    public decimal? DiscountValue { get; set; }

    public int? QuantityAlert { get; set; }

    public string? WarrantyType { get; set; }

    public string? Manufacturer { get; set; }

    public DateOnly? ManufacturedDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public Guid? CategoryId { get; set; }

    public Guid? SubcategoryId { get; set; }

    public Guid? BrandId { get; set; }

    public string? Type { get; set; }

    public bool? DelInd { get; set; }

    public bool? IsVisible { get; set; }

    public int? Stock { get; set; }

    public Guid? ModelId { get; set; }

    public bool Restock { get; set; }

    public decimal? Tax { get; set; }

    public Guid? Vendorid { get; set; }

    public virtual Model? Model { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<ProductVariant> ProductVariants { get; set; } = new List<ProductVariant>();
}
