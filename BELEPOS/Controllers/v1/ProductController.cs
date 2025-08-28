using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BELEPOS.DataModel;
using Asp.Versioning;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using BELEPOS.Entity;
using BELEPOS.Helper;
using System.Drawing.Drawing2D;
using System.Globalization;
using Microsoft.CodeAnalysis;
using ClosedXML.Excel;


namespace BELEPOS.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ProductController : ControllerBase
    {
        private readonly BeleposContext _context;

        private readonly EPoshelper _ePosHelper;

        private readonly IConfiguration _config;


        public ProductController(BeleposContext context, EPoshelper ePoshelper, IConfiguration config)
        {
            _context = context;
            _ePosHelper = ePoshelper;
            _config = config;
        }


        #region fetch category list
        // GET: api/Product/GetCategories
        //[Authorize]
        [HttpGet("GetCategories")]
        public async Task<ActionResult<Category>> GetCategory(Guid? storeId)
        {
            var query = _context.Categories.Where(c => c.Delind == false && c.StoreId == storeId);

            if (storeId.HasValue)
                query = query.Where(c => c.StoreId == storeId.Value);

            var categories = await query
                .Select(c => new CategoryDto
                {
                    CategoryId = c.Categoryid,
                    CategoryName = c.CategoryName,
                    Image = c.Imagename,
                    StoreId = c.StoreId,
                }).OrderBy(c => c.CategoryName)
                .ToListAsync();

            return Ok(new
            {
                message = "Category list fetched successfully.",
                data = categories
            });
        }
        #endregion



        #region save product
        // POST: api/Product/SaveCategories
        [HttpPost("SaveCategories")]
        public async Task<ActionResult<CategoryDto>> SaveCategories(CategoryDto categoryDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Categoryid == categoryDto.CategoryId && c.StoreId == categoryDto.StoreId && c.Delind == false);

                if (existingCategory != null)
                {
                    // Update existing
                    existingCategory.CategoryName = categoryDto.CategoryName!;
                    existingCategory.Imagename = categoryDto.Image!;
                    existingCategory.StoreId = categoryDto.StoreId;
                    existingCategory.LastmodifiedAt = DateTime.Now;
                    _context.Categories.Update(existingCategory);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Category updated successfully.",
                        data = new { id = existingCategory.Categoryid, }
                    });
                }
                else
                {
                    //add new category
                    var newCategory = new Category
                    {
                        CategoryName = categoryDto.CategoryName!,
                        Imagename = string.IsNullOrEmpty(categoryDto.Image) ? "default.png" : categoryDto.Image,
                        StoreId = categoryDto.StoreId,
                        Delind = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Categories.Add(newCategory);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Category added successfully.",
                        data = new { id = newCategory.Categoryid, }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.InnerException.ToString(),
                });

            }
        }
        #endregion


        #region save products
        [HttpPost("SaveProduct")]
        public async Task<ActionResult<ProductDto>> SaveProduct(ProductDto productDto, [FromForm] List<IFormFile> images)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productDto.Id && p.StoreId == productDto.StoreId);

                if (existingProduct != null)
                {
                    // Update existing product
                    existingProduct.Name = productDto.ProductName;
                    existingProduct.Slug = productDto.Slug;
                    existingProduct.Sku = productDto.Sku;
                    existingProduct.SellingType = productDto.SellingType;
                    existingProduct.CategoryId = productDto.CategoryId;
                    existingProduct.SubcategoryId = productDto.SubcategoryId;
                    existingProduct.BrandId = productDto.BrandId;
                    existingProduct.Unit = productDto.Unit;
                    existingProduct.Barcode = productDto.Barcode;
                    existingProduct.Description = productDto.Description;
                    existingProduct.IsVariable = productDto.IsVariable ?? false;
                    existingProduct.Price = productDto.Price;
                    existingProduct.Stock = productDto.Stock;
                    existingProduct.TaxType = productDto.TaxType;
                    existingProduct.DiscountType = productDto.DiscountType;
                    existingProduct.DiscountValue = productDto.DiscountValue;
                    existingProduct.QuantityAlert = productDto.QuantityAlert;
                    // existingProduct.WarrantyType = productDto.WarrantyType;
                    existingProduct.Manufacturer = productDto.Manufacturer;
                    //existingProduct.ManufacturedDate = productDto.ManufacturedDate;
                    // existingProduct.ExpiryDate = productDto.ExpiryDate;
                    existingProduct.Type = ProductType.Product.ToString();
                    existingProduct.Restock = productDto.Restock ?? false;


                    _context.Products.Update(existingProduct);
                    await _context.SaveChangesAsync();


                    if (productDto.IsVariable == true && productDto.Variants != null && productDto.Variants.Any())
                    {
                        await _ePosHelper.SaveOrUpdateProductVariants(existingProduct?.Id ?? existingProduct.Id, productDto.Variants);
                    }

                    await _ePosHelper.SaveProductImage(productDto, images);

                    return Ok(new
                    {
                        message = "Product updated successfully.",
                        data = new { id = existingProduct.Id }
                    });
                }
                else
                {
                    // Add new product
                    var newProduct = new Product
                    {
                        StoreId = productDto.StoreId,
                        Name = productDto.ProductName,
                        Slug = productDto.Slug,
                        Sku = productDto.Sku,
                        SellingType = productDto.SellingType,
                        CategoryId = productDto.CategoryId,
                        SubcategoryId = productDto.SubcategoryId,
                        BrandId = productDto.BrandId,
                        Unit = productDto.Unit,
                        Barcode = productDto.Barcode,
                        Description = productDto.Description,
                        IsVariable = productDto.IsVariable ?? false,
                        Price = productDto.Price,
                        Stock = productDto.Stock,
                        TaxType = productDto.TaxType,
                        DiscountType = productDto.DiscountType,
                        DiscountValue = productDto.DiscountValue,
                        QuantityAlert = productDto.QuantityAlert,
                        //WarrantyType = productDto.WarrantyType,
                        Manufacturer = productDto.Manufacturer,
                        //ManufacturedDate = productDto.ManufacturedDate,
                        //ExpiryDate = productDto.ExpiryDate,
                        Type = ProductType.Product.ToString(),
                        Restock = productDto.Restock ?? false,
                    };

                    _context.Products.Add(newProduct);
                    await _context.SaveChangesAsync();



                    /*// ✅ Save product variants if IsVariable
                    if (productDto.IsVariable == true && productDto.Variants != null && productDto.Variants.Any())
                    {
                        var existingVariants = await _context.ProductVariants
                            .Where(v => v.ProductId == newProduct.Id)
                            .ToListAsync();

                        // Soft delete
                        foreach (var v in existingVariants)
                        {
                            v.Delind = true;
                            v.UpdatedAt = DateTime.UtcNow;
                        }

                        // Add new variants
                        foreach (var variant in productDto.Variants)
                        {
                            var newVariant = new ProductVariant
                            {
                                ProductId = newProduct.Id,
                                VariantName = variant.VariantName,
                                Sku = variant.Sku,
                                Price = variant.Price,
                                Quantity = variant.Quantity,
                                Delind = false,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.ProductVariants.Add(newVariant);
                        }

                        await _context.SaveChangesAsync();
                    }*/

                    if (productDto.IsVariable == true && productDto.Variants != null && productDto.Variants.Any())
                    {



                        await _ePosHelper.SaveOrUpdateProductVariants(existingProduct?.Id ?? newProduct.Id, productDto.Variants);
                    }


                    //save product images
                    await _ePosHelper.SaveProductImage(productDto, images);


                    return Ok(new
                    {
                        message = "Product added successfully.",
                        data = new { id = newProduct.Id }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        #endregion


        #region fetch products
        [HttpGet("GetProducts")]
        public async Task<IActionResult> GetProducts(Guid storeId)
        {
            try
            {

                var products = await (
                                      from p in _context.Products
                                      join c in _context.Categories on p.CategoryId equals c.Categoryid into catGroup
                                      from c in catGroup.DefaultIfEmpty() // left join
                                      join sc in _context.SubCategories on p.SubcategoryId equals sc.Subcategoryid into subCatGroup
                                      from sc in subCatGroup.DefaultIfEmpty() // left join
                                      where p.DelInd == false && p.StoreId == storeId
                                      select new ProductDto
                                      {
                                          Id = p.Id,
                                          ProductName = p.Name,
                                          StoreId = p.StoreId,
                                          //  WarehouseId = p.WarehouseId,
                                          Slug = p.Slug,
                                          Sku = p.Sku,
                                          SellingType = p.SellingType,
                                          CategoryId = p.CategoryId,
                                          SubcategoryId = p.SubcategoryId,
                                          CategoryName = c != null ? c.CategoryName : null,
                                          SubcategoryName = sc != null ? sc.Name : null,

                                          //BrandId = p.BrandId,
                                          //BrandName = b.Name,
                                          Unit = p.Unit,
                                          Barcode = p.Barcode,
                                          Description = p.Description,
                                          // IsVariable = p.IsVariable,
                                          Price = p.Price,
                                          TaxType = p.TaxType,
                                          DiscountType = p.DiscountType,
                                          DiscountValue = p.DiscountValue,
                                          QuantityAlert = p.QuantityAlert,
                                         //WarrantyType = p.WarrantyType,
                                          Stock = p.Stock,
                                          Restock=p.Restock,

                                          // Manufacturer = p.Manufacturer,
                                          //ManufacturedDate = p.ManufacturedDate,
                                          // ExpiryDate = p.ExpiryDate,
                                          //Variants = new List<ProductVariantsDto>() // initialized empty
                                      }).ToListAsync();

                // Step 2: Get only variable product IDs
                var variableProductIds = products
                   .Where(p => p.IsVariable == true) // ✅ FIXED
                   .Select(p => p.Id)
                   .ToList();

                // Step 3: Fetch variants of those products
                var allVariants = await _context.ProductVariants
                    .Where(v => variableProductIds.Contains(v.ProductId) && v.Delind == false)
                    .ToListAsync();

                // Step 4: Map variants to parent products
                foreach (var product in products.Where(p => p.IsVariable == true)) // ✅ FIXED
                {
                    product.Variants = allVariants
                        .Where(v => v.ProductId == product.Id)
                        .Select(v => new ProductVariantsDto
                        {
                            VariantId = v.VariantId,
                            VariantName = v.VariantName,
                            Sku = v.Sku,
                            Price = v.Price,
                            Quantity = v.Quantity
                        })
                        .ToList();
                }
                return Ok(new
                {
                    message = "Product list fetched successfully.",
                    data = products
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Error fetching product list",
                    error = ex.Message
                });
            }
        }


        #endregion


        #region add/edit subcategories
        [HttpPost("SaveSubCategories")]
        public async Task<IActionResult> SaveSubCategories(SubCategoryDto subcategoryDto)
        {

            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                // Check if the related category exists
                var categoryExists = await _context.Categories.AnyAsync(c => c.Categoryid == subcategoryDto.CategoryId && c.Delind == false);
                /*if (!categoryExists)
                    return NotFound(new { message = "Category not found." });*/

                if (subcategoryDto.SubCategoryId != Guid.Empty)
                {
                    // Update existing
                    var subcategory = await _context.SubCategories.FindAsync(subcategoryDto.SubCategoryId!);
                    subcategory.Name = subcategoryDto.SubCategoryName!;
                    subcategory.Categoryid = subcategoryDto.CategoryId;
                    subcategory.Image = subcategoryDto.Image;
                    subcategory.Description = subcategoryDto.Description;
                    _context.SubCategories.Update(subcategory);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Subcategory updated successfully.", data = subcategory });
                }
                else
                {
                    // Create new
                    var newSubcategory = new SubCategory
                    {
                        Name = subcategoryDto.SubCategoryName!,
                        Categoryid = subcategoryDto.CategoryId,
                        Image = subcategoryDto.Image,
                        StoreId = subcategoryDto.StoreId,
                        Description = subcategoryDto.Description,
                        Delind = false
                    };
                    _context.SubCategories.Add(newSubcategory);
                    await _context.SaveChangesAsync();

                    return Ok(new { message = "Subcategory created successfully.", data = newSubcategory });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new { message = "Oops! Something went wrong. Please try again later." });
                //throw;
            }

        }
        #endregion


        #region get subcategories
        [HttpGet("GetSubcategories")]
        public async Task<IActionResult> GetSubcategoriesByCategory(Guid? storeId)
        {
            bool storeExists = await _ePosHelper.FindStore(storeId);


            if (!storeExists)
                return BadRequest("Store not found or inactive");

            try
            {
                var subcategories = await (from s in _context.SubCategories
                                           join c in _context.Categories
                                           on s.Categoryid equals c.Categoryid
                                           where s.Delind == false
                                                 && s.StoreId == storeId
                                                 && c.Delind == false
                                           select new SubCategoryDto
                                           {
                                               SubCategoryId = s.Subcategoryid,
                                               SubCategoryName = s.Name,
                                               Description = s.Description,
                                               Image = s.Image,
                                               StoreId = s.StoreId,
                                               CategoryId = s.Categoryid,
                                               CategoryName = c.CategoryName
                                           }).ToListAsync();


                return Ok(new { message = "Subcategories fetched successfully.", data = subcategories });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching subcategories.", error = ex.Message });
            }
        }

        #endregion


        //Cteated by Devansh
        #region Get Reference Data (Brands, Categories, Units)
        [HttpGet("GetReferenceData")]
        public async Task<IActionResult> GetReferenceData(Guid? storeId)
        {

            bool storeExists = await _ePosHelper.FindStore(storeId);

            if (!storeExists)
                return BadRequest("Store not found or inactive");

            try
            {
                // Fetch all brands
                var brands = await _context.Brands.Where(b => b.Delind == false && b.StoreId == storeId).Select(b => new BrandDto
                {
                    BrandId = b.Id,
                    BrandName = b.Name
                })
                    .OrderBy(b => b.BrandName)
                    .ToListAsync();

                // Fetch all units
                var units = await _context.Units
                    .Select(u => new UnitDto
                    {
                        UnitId = u.Id,
                        UnitName = u.Name
                    })
                    .OrderBy(u => u.UnitName)
                    .ToListAsync();

                // Fetch all categories (filtered by storeId and Delind)
                var categoryQuery = _context.Categories
                    .Where(c => c.Delind == false && c.StoreId == storeId);

                if (storeId.HasValue)
                {
                    categoryQuery = categoryQuery.Where(c => c.StoreId == storeId.Value);
                }

                //category list
                var categories = await categoryQuery
                    .Select(c => new CategoryDto
                    {
                        CategoryId = c.Categoryid,
                        CategoryName = c.CategoryName,
                        Image = c.Imagename,
                        StoreId = c.StoreId
                    })
                    .OrderBy(c => c.CategoryName)
                    .ToListAsync();


                // Fetch all technicians (filtered by storeId and IsActive)
                var technicians = await _context.Users
                    .Where(u => u.Role.Rolename == "Technician" && u.Isactive == true && u.Storeid == storeId && u.DelInd == false)
                .Select(u => new
                {
                    userId = u.Userid,
                    userName = u.Username,
                    roleName = u.Role.Rolename
                }).OrderBy(t => t.userName).ToListAsync();
                //technicians.Insert(0, new
                //{
                //    userId = (Guid?)null,
                //    userName = "Will allot technician later",
                //    roleName = "Technician"
                //});


                // Fetch all models
                var models = await _context.Models
                    .Where(m => m.Delind == false)
                    .Select(m => new ModelDto
                    {
                        ModelId = m.ModelId,
                        Name = m.Name,
                        BrandId = m.BrandId,
                        DeviceType = m.DeviceType
                    })
                    .OrderBy(m => m.Name)
                    .ToListAsync();
                //status
                var status = await _context.Statuses.Where(s => s.Delind == false)
                    .ToListAsync();

                //task type
                var serviceType = await _context.Products.Where(s => s.DelInd == false
                && s.Type == ProductType.Service.ToString())
                    .ToListAsync();


                //fech store list
                var stores = await _context.Stores
                .Where(s => s.DelInd == false) // Use DelInd instead of IsActive if applicable
                .Select(s => new StoreDto
                {
                    Id = s.Id,
                    TenantId = s.TenantId ?? Guid.Empty,
                    Name = s.Name,
                    Address = s.Address,
                    Username = s.Username,
                    Phone = s.Phone,
                    Email = s.Email,
                    IsActive = s.IsActive ?? true
                })
                .OrderBy(s => s.Name)
                .ToListAsync();

                bool isTicketPrint = _config.GetValue<bool>("AppSettings:IsTicketPrint");
                bool isBillPrint = _config.GetValue<bool>("AppSettings:IsBillPrint");

                return Ok(new
                {
                    message = "Reference data fetched successfully.",
                    data = new
                    {
                        brands,
                        units,
                        categories,
                        status,
                        serviceType,
                        models,
                        technicians,
                        stores,
                        isTicketPrint,  
                        isBillPrint
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching reference data.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion


        #region save brands

        [HttpPost("SaveBrands")]
        public async Task<IActionResult> SaveBrands([FromBody] ProductDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var brand = await _context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.BrandId && !(b.Delind ?? false));

            if (brand != null)
            {
                // ✅ Update
                brand.Id = (Guid)request.BrandId;
                brand.Name = request.BrandName.Trim();
                brand.Isactive = request.IsActive;
                brand.StoreId = request.StoreId; // future

                _context.Brands.Update(brand);
            }
            else
            {
                // ✅ Insert
                brand = new Brand
                {
                    Id = Guid.NewGuid(), // always set
                    Name = request.BrandName.Trim(),
                    CreatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                    Isactive = request.IsActive,
                    Delind = false,
                    StoreId = request.StoreId,



                    // StoreId = request.StoreId
                };

                await _context.Brands.AddAsync(brand);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = brand.Id == request.BrandId ? "Brand updated successfully." : "Brand created successfully.",
                data = new { id = brand.Id }
            });
        }

        #endregion


        #region upload products from excel

        [HttpPost("UploadProductExcel")]
        public async Task<IActionResult> UploadProductExcel(IFormFile File)
        {
            if (File == null || File.Length == 0)
                return BadRequest("No file uploaded.");

            var products = new List<Product>();
            var failedRows = new List<string>();
            int rowIndex = 2; // Start after header

            try
            {
                using (var stream = new MemoryStream())
                {
                    await File.CopyToAsync(stream);
                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheets.First();
                        var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // skip header

                        foreach (var row in rows)
                        {
                            try
                            {
                                var dto = new ProductDto
                                {
                                    ProductName = row.Cell(1).GetString(),
                                    Sku = row.Cell(5).GetString(),
                                    Price = row.Cell(6).GetValue<decimal?>(),
                                    QuantityAlert = row.Cell(7).GetValue<int?>(),
                                    //   ManufacturedDate = ParseDateOnly(row.Cell(8)),
                                    // ExpiryDate = ParseDateOnly(row.Cell(9)),
                                    Description = row.Cell(10).GetString(),
                                    Barcode = row.Cell(11).GetString(),
                                    CategoryName = row.Cell(2).GetString().Trim(),
                                    BrandName = row.Cell(4).GetString().Trim(),
                                    SubcategoryName = row.Cell(3).GetString().Trim(),
                                    Stock = int.Parse(row.Cell(12).GetString()),
                                    Unit = row.Cell(13).GetString(),
                                    StoreName = row.Cell(14).GetString(),

                                };

                                // Validate Category
                                var categoryId = await _context.Categories
                                    .Where(c => c.CategoryName.ToLower() == dto.CategoryName.ToLower())
                                    .Select(c => (Guid?)c.Categoryid)
                                    .FirstOrDefaultAsync();

                                if (categoryId == null)
                                {
                                    failedRows.Add($"Row {rowIndex}: Category '{dto.CategoryName}' not found");
                                    rowIndex++;
                                    continue;
                                }

                                var storeid = await _context.Stores
                                 .Where(s => s.Name.ToLower() == dto.StoreName.ToLower())
                                 .Select(s => (Guid?)s.Id)
                                 .FirstOrDefaultAsync();

                                // Validate Brand
                                var brandId = await _context.Brands
                                    .Where(b => b.Name.ToLower() == dto.BrandName.ToLower())
                                    .Select(b => (Guid?)b.Id)
                                    .FirstOrDefaultAsync();

                                if (brandId == null)
                                {
                                    failedRows.Add($"Row {rowIndex}: Brand '{dto.BrandName}' not found");
                                    rowIndex++;
                                    continue;
                                }

                                // Validate Subcategory
                                var subcategoryId = await _context.SubCategories
                                    .Where(s => s.Name.ToLower() == dto.SubcategoryName.ToLower())
                                    .Select(s => (Guid?)s.Subcategoryid)
                                    .FirstOrDefaultAsync();

                                if (subcategoryId == null)
                                {
                                    failedRows.Add($"Row {rowIndex}: Subcategory '{dto.SubcategoryName}' not found");
                                    rowIndex++;
                                    continue;
                                }

                                // Validate Dates
                                ////  if (dto.ManufacturedDate == null)
                                //  {
                                //      failedRows.Add($"Row {rowIndex}: Invalid Manufactured Date");
                                //      rowIndex++;
                                //      continue;
                                //  }
                                ////  if (dto.ExpiryDate == null)
                                //  {
                                //      failedRows.Add($"Row {rowIndex}: Invalid Expiry Date");
                                //      rowIndex++;
                                //      continue;
                                //  }

                                var product = new Product
                                {
                                    Id = Guid.NewGuid(),
                                    Name = dto.ProductName ?? "",
                                    Sku = dto.Sku,
                                    Price = dto.Price,
                                    QuantityAlert = dto.QuantityAlert,
                                    //   ManufacturedDate = dto.ManufacturedDate,
                                    //  ExpiryDate = dto.ExpiryDate,
                                    Description = dto.Description,
                                    Barcode = dto.Barcode,
                                    CategoryId = categoryId,
                                    BrandId = brandId,
                                    SubcategoryId = subcategoryId,
                                    StoreId = storeid,
                                    Unit = dto.Unit,
                                    Stock = dto.Stock
                                };

                                products.Add(product);
                            }
                            catch (Exception rowEx)
                            {
                                failedRows.Add($"Row {rowIndex}: {rowEx.Message}");
                            }

                            rowIndex++;
                        }
                    }
                }

                if (products.Any())
                {
                    _context.Products.AddRange(products);
                    await _context.SaveChangesAsync();
                }

                return Ok(new
                {
                    successCount = products.Count,
                    failedCount = failedRows.Count,
                    errors = failedRows
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Upload failed", error = ex.Message });
            }
        }

        private DateOnly? ParseDateOnly(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return null;

            // Handle actual Excel date cell
            if (cell.DataType == XLDataType.DateTime)
            {
                return DateOnly.FromDateTime(cell.GetDateTime());
            }

            // Otherwise, parse string manually
            var dateStr = cell.GetString()?.Trim();
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            string[] formats = { "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" };

            foreach (var fmt in formats)
            {
                if (DateTime.TryParseExact(dateStr, fmt, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dt))
                    return DateOnly.FromDateTime(dt);
            }

            return null;
        }

        #endregion



        ////////////////////////////////DELETE APIS//////////////////////////////////
        //#region Delete Category


        //[HttpDelete("DeleteCategory")]
        //public async Task<IActionResult> DeleteCategory(Guid categoryId, Guid storeId)
        //{
        //    try
        //    {
        //        var category = await _context.Categories
        //            .FirstOrDefaultAsync(c => c.Categoryid == categoryId
        //                                   && c.StoreId == storeId
        //                                   && c.Delind == false);

        //        if (category == null)
        //            return NotFound(new { message = "Category not found or already del." });

        //        category.Delind = true; // mark as deleted
        //        category.LastmodifiedAt = DateTime.Now;

        //        _context.Categories.Update(category);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { message = "Category deleted successfully ." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            message = ex.InnerException?.Message ?? ex.Message
        //        });
        //    }
        //}
        //#endregion



        //#region delete subcategory
        //[HttpDelete("DeleteSubCategory")]
        //public async Task<IActionResult> DeleteSubCategory(Guid subCategoryId, Guid storeId)
        //{
        //    try
        //    {
        //        var subCategory = await _context.SubCategories
        //            .FirstOrDefaultAsync(s => s.Subcategoryid == subCategoryId
        //                                   && s.StoreId == storeId
        //                                   && s.Delind == false);

        //        if (subCategory == null)
        //            return NotFound(new { message = "Subcategory not found or already deleted." });

        //        subCategory.Delind = true; // mark as deleted
        //        _context.SubCategories.Update(subCategory);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { message = "Subcategory deleted successfully (soft delete)." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            message = ex.InnerException?.Message ?? ex.Message
        //        });
        //    }
        //}
        //#endregion


        //#region delete product
        //// DELETE: api/Product/DeleteProduct/{id}
        //[HttpDelete("DeleteProduct")]
        //public async Task<IActionResult> DeleteProduct(Guid productId, Guid storeId)
        //{
        //    try
        //    {
        //        var product = await _context.Products
        //            .FirstOrDefaultAsync(p => p.Id == productId
        //                                   && p.StoreId == storeId
        //                                   && p.DelInd == false);

        //        if (product == null)
        //            return NotFound(new { message = "Product not found or already deleted." });

        //        product.DelInd = true; // soft delete
        //        _context.Products.Update(product);
        //        await _context.SaveChangesAsync();

        //        return Ok(new { message = "Product deleted successfully ." });
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new
        //        {
        //            message = ex.InnerException?.Message ?? ex.Message
        //        });
        //    }
        //}
        //#endregion


        #region Delete Category
        [HttpPost("DeleteCategory")]
        public async Task<IActionResult> DeleteCategory([FromQuery] Guid categoryId, [FromQuery] Guid storeId)
        {
            try
            {
                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.Categoryid == categoryId
                                           && c.StoreId == storeId
                                           && c.Delind == false);

                if (category == null)
                    return NotFound(new { message = "Category not found or already deleted." });

                category.Delind = true;
                category.LastmodifiedAt = DateTime.Now;

                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Category deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }
        #endregion


        #region Delete SubCategory
        [HttpPost("DeleteSubCategory")]
        public async Task<IActionResult> DeleteSubCategory([FromQuery] Guid subCategoryId, [FromQuery] Guid storeId)
        {
            try
            {
                var subCategory = await _context.SubCategories
                    .FirstOrDefaultAsync(s => s.Subcategoryid == subCategoryId
                                           && s.StoreId == storeId
                                           && s.Delind == false);

                if (subCategory == null)
                    return NotFound(new { message = "Subcategory not found or already deleted." });

                subCategory.Delind = true;
                _context.SubCategories.Update(subCategory);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Subcategory deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }
        #endregion


        #region Delete Product
        [HttpPost("DeleteProduct")]
        public async Task<IActionResult> DeleteProduct([FromQuery] Guid productId, [FromQuery] Guid storeId)
        {
            try
            {
                var product = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productId
                                           && p.StoreId == storeId
                                           && p.DelInd == false);

                if (product == null)
                    return NotFound(new { message = "Product not found or already deleted." });

                product.DelInd = true;
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }
        #endregion




    }
}
