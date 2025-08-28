using System;

namespace BELEPOS.Entity
{
    public class ProductDto:BrandDto
    {
        public Guid Id { get; set; }
        public Guid? StoreId { get; set; }
        public Guid? WarehouseId { get; set; }

        public Guid? ModelId { get; set; }

        public string? ProductName { get; set; } = null!;


        public string? CategoryName { get; set; }


        public string? SubcategoryName { get; set; }

        public string? Slug { get; set; }
        public string? Sku { get; set; }
        public string? SellingType { get; set; }

        public Guid? CategoryId { get; set; }
        public Guid? SubcategoryId { get; set; }
      
       
        public String? Unit { get; set; }

        public string? Barcode { get; set; }
        public string? Description { get; set; }

        public string ? StoreName { get; set; } 

        public bool? IsVariable { get; set; }
        public decimal? Price { get; set; }

        public string? TaxType { get; set; }
        public string? DiscountType { get; set; }
        public decimal? DiscountValue { get; set; }
        public int? QuantityAlert { get; set; }

        public int? Stock { get; set; }

     //   public string? WarrantyType { get; set; }
        public string? Manufacturer { get; set; }
      //  public DateOnly? ManufacturedDate { get; set; }
      //  public DateOnly? ExpiryDate { get; set; }

        public List<ProductImageDto> ImageList { get; set; } = new();


        public List<ProductVariantsDto> Variants { get; set; } = new();

        public string? SerialNumber { get; set; }

        public decimal? Total { get; set; }

        public bool? Restock { get; set; }
    }


    public class BrandDto
    {
        public Guid? BrandId { get; set; }
        public string? BrandName { get; set; }

        public bool IsActive { get; set; }


    }


    public class ModelDto:BrandDto
    {
        public Guid ModelId { get; set; }

        public string? Name { get; set; }

        //public Guid BrandId { get; set; }

        public string? DeviceType { get; set; }
    }

    public class UnitDto
    {
        public int UnitId { get; set; }
        public string UnitName { get; set; }
    }


    public class ProductImageDto
    {
        public int ImageId { get; set; }
        public Guid ProductId { get; set; }
        public string ImageName { get; set; }
        public bool Main { get; set; }
        public bool DelInd { get; set; }
        public DateTime CreatedAt { get; set; }

        //public Product Product { get; set; }
    }


    public class ProductVariantsDto
    {
        public Guid? VariantId { get; set; }
        public string VariantName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
