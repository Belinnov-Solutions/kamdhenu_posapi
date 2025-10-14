using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BELEPOS.DataModel;

public partial class BeleposContext : DbContext
{
    public BeleposContext()
    {
    }

    public BeleposContext(DbContextOptions<BeleposContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<ChecklistCategory> ChecklistCategories { get; set; }

    public virtual DbSet<ChecklistResponse> ChecklistResponses { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Model> Models { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<OrderPayment> OrderPayments { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<PrintReceiptSetting> PrintReceiptSettings { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductVariant> ProductVariants { get; set; }

    public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

    public virtual DbSet<RepairChecklist> RepairChecklists { get; set; }

    public virtual DbSet<RepairOrder> RepairOrders { get; set; }

    public virtual DbSet<RepairOrderPart> RepairOrderParts { get; set; }

    public virtual DbSet<RepairTicket> RepairTickets { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<RolePermission> RolePermissions { get; set; }

    public virtual DbSet<ServiceCatalogue> ServiceCatalogues { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<SubCategory> SubCategories { get; set; }

    public virtual DbSet<SyncLog> SyncLogs { get; set; }

    public virtual DbSet<TaskType> TaskTypes { get; set; }

    public virtual DbSet<Tenant> Tenants { get; set; }

    public virtual DbSet<Ticketnote> Ticketnotes { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Warehouse> Warehouses { get; set; }

    /*protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=49.205.172.128;Database=KamdhenuTest;Username=postgres;Password=yellowbus");*/

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasPostgresExtension("pgcrypto")
            .HasPostgresExtension("uuid-ossp");

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("brands_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StoreId).HasColumnName("store_id");

            entity.HasOne(d => d.Store).WithMany(p => p.Brands)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("fk_brands_store");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Categoryid).HasName("categories_pkey");

            entity.Property(e => e.Categoryid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("categoryid");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(100)
                .HasColumnName("category_name");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Imagename)
                .HasMaxLength(100)
                .HasColumnName("imagename");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(false)
                .HasColumnName("is_visible");
            entity.Property(e => e.LastmodifiedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("lastmodified_at");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.WebUpload).HasDefaultValue(false);
        });

        modelBuilder.Entity<ChecklistCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("checklist_category_pkey");

            entity.ToTable("ChecklistCategory");

            entity.HasIndex(e => new { e.Name, e.CheckType }, "uq_category_name_type").IsUnique();

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CheckType)
                .HasMaxLength(20)
                .HasColumnName("check_type");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ChecklistResponse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("repair_checklist_response_pkey");

            entity.ToTable("ChecklistResponse");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.ChecklistId).HasColumnName("checklist_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.RepairInspection).HasColumnName("repairInspection");
            entity.Property(e => e.RespondedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("responded_at");
            entity.Property(e => e.TicketId).HasColumnName("ticket_id");
            entity.Property(e => e.Value).HasColumnName("value");

            entity.HasOne(d => d.Checklist).WithMany(p => p.ChecklistResponses)
                .HasForeignKey(d => d.ChecklistId)
                .HasConstraintName("fk_checklist");

            entity.HasOne(d => d.Order).WithMany(p => p.ChecklistResponses)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_order_id");

            entity.HasOne(d => d.Ticket).WithMany(p => p.ChecklistResponses)
                .HasForeignKey(d => d.TicketId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_ticket_id");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("customers_pkey");

            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(100)
                .HasColumnName("customer_name");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .HasColumnName("state");
            entity.Property(e => e.Zipcode)
                .HasMaxLength(10)
                .HasColumnName("zipcode");
        });

        modelBuilder.Entity<Model>(entity =>
        {
            entity.HasKey(e => e.ModelId).HasName("models_pkey");

            entity.Property(e => e.ModelId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("model_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(100)
                .HasColumnName("device_type");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");

            entity.HasOne(d => d.Brand).WithMany(p => p.Models)
                .HasForeignKey(d => d.BrandId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_models_brand");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Notificationid).HasName("Notification_pkey");

            entity.ToTable("Notification");

            entity.Property(e => e.Notificationid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("notificationid");
            entity.Property(e => e.Datecreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Notificationbody).HasColumnName("notificationbody");
            entity.Property(e => e.Notificationsubject).HasColumnName("notificationsubject");
            entity.Property(e => e.Notificationtype).HasColumnName("notificationtype");
            entity.Property(e => e.Notificationtypecode).HasColumnName("notificationtypecode");
            entity.Property(e => e.Referenceid).HasColumnName("referenceid");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("orders_pkey");

            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("order_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Discount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("discount");
            entity.Property(e => e.GrandTotal)
                .HasPrecision(10, 2)
                .HasColumnName("grand_total");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_date");
            entity.Property(e => e.Paid)
                .HasDefaultValue(false)
                .HasColumnName("paid");
            entity.Property(e => e.Paymentmethod)
                .HasMaxLength(50)
                .HasColumnName("paymentmethod");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Tax)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("tax");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Store).WithMany(p => p.Orders)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("orders_store_id_fkey");
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.OrderPartId).HasName("orderitems_pkey");

            entity.Property(e => e.OrderPartId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("order_part_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.ImeiNumber).HasColumnName("imei_number");
            entity.Property(e => e.Model).HasColumnName("model");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(255)
                .HasColumnName("product_name");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number");
            entity.Property(e => e.TotalPrice)
                .HasPrecision(10, 2)
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("order_parts_order_id_fkey");

            entity.HasOne(d => d.Product).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("order_parts_product_id_fkey");
        });

        modelBuilder.Entity<OrderPayment>(entity =>
        {
            entity.HasKey(e => e.Paymentid).HasName("orderpayment_pkey");

            entity.ToTable("OrderPayment");

            entity.Property(e => e.Paymentid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("paymentid");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.FullyPaid).HasDefaultValue(false);
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.PaidAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paid_at");
            entity.Property(e => e.PartialPayment)
                .HasDefaultValue(false)
                .HasColumnName("partial_payment");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.Remainingamount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.Repairorderid).HasColumnName("repairorderid");
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);

            entity.HasOne(d => d.Repairorder).WithMany(p => p.OrderPayments)
                .HasForeignKey(d => d.Repairorderid)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_orderpayment_repairorder");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("parts_pkey");

            entity.HasIndex(e => e.PartNumber, "parts_part_number_key").IsUnique();

            entity.Property(e => e.PartId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("part_id");
            entity.Property(e => e.BrandId).HasColumnName("brand_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.InStock)
                .HasDefaultValue(true)
                .HasColumnName("in_stock");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.OpeningStock).HasColumnName("opening_stock");
            entity.Property(e => e.OpeningStockDate).HasColumnName("opening_stock_date");
            entity.Property(e => e.PartName)
                .HasMaxLength(100)
                .HasColumnName("part_name");
            entity.Property(e => e.PartNumber)
                .HasMaxLength(50)
                .HasColumnName("part_number");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.SerialNumber).HasColumnName("serial_number");
            entity.Property(e => e.Stock).HasColumnName("stock");
            entity.Property(e => e.StoreId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Permissionid).HasName("permissions_pkey");

            entity.HasIndex(e => e.Permissionname, "permissions_name_key").IsUnique();

            entity.Property(e => e.Permissionid)
                .HasDefaultValueSql("nextval('permissions_permissionid_seq'::regclass)")
                .HasColumnName("permissionid");
            entity.Property(e => e.Permissionname)
                .HasMaxLength(100)
                .HasColumnName("permissionname");
        });

        modelBuilder.Entity<PrintReceiptSetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PrintReceiptSettings_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false)
                .HasColumnName("is_deleted");
            entity.Property(e => e.ReceiptName)
                .HasMaxLength(100)
                .HasColumnName("receipt_name");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("products_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Barcode)
                .HasMaxLength(100)
                .HasColumnName("barcode");
            entity.Property(e => e.BrandId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("brand_id");
            entity.Property(e => e.CategoryId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("category_id");
            entity.Property(e => e.DelInd)
                .HasDefaultValue(false)
                .HasColumnName("del_ind");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.DiscountType)
                .HasMaxLength(50)
                .HasColumnName("discount_type");
            entity.Property(e => e.DiscountValue)
                .HasPrecision(10, 2)
                .HasColumnName("discount_value");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.IsVariable)
                .HasDefaultValue(false)
                .HasColumnName("is_variable");
            entity.Property(e => e.IsVisible)
                .HasDefaultValue(false)
                .HasColumnName("is_visible");
            entity.Property(e => e.ManufacturedDate).HasColumnName("manufactured_date");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.QuantityAlert).HasColumnName("quantity_alert");
            entity.Property(e => e.Restock)
                .HasDefaultValue(false)
                .HasColumnName("restock");
            entity.Property(e => e.SellingType)
                .HasMaxLength(50)
                .HasColumnName("selling_type");
            entity.Property(e => e.Sku)
                .HasMaxLength(100)
                .HasColumnName("sku");
            entity.Property(e => e.Slug)
                .HasMaxLength(200)
                .HasColumnName("slug");
            entity.Property(e => e.Stock)
                .HasDefaultValue(0)
                .HasColumnName("stock");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.SubcategoryId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("subcategory_id");
            entity.Property(e => e.Tax)
                .HasPrecision(10, 2)
                .HasColumnName("tax");
            entity.Property(e => e.TaxType)
                .HasMaxLength(50)
                .HasColumnName("tax_type");
            entity.Property(e => e.Type)
                .HasMaxLength(100)
                .HasColumnName("type");
            entity.Property(e => e.Unit)
                .HasMaxLength(50)
                .HasColumnName("unit");
            entity.Property(e => e.Vendorid).HasColumnName("vendorid");
            entity.Property(e => e.WarehouseId).HasColumnName("warehouse_id");
            entity.Property(e => e.WarrantyType)
                .HasMaxLength(100)
                .HasColumnName("warranty_type");
            entity.Property(e => e.WebUpload).HasDefaultValue(false);

            entity.HasOne(d => d.Model).WithMany(p => p.Products)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("fk_model");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Imageid).HasName("productimages_pkey");

            entity.Property(e => e.Imageid)
                .HasDefaultValueSql("nextval('productimages_imageid_seq'::regclass)")
                .HasColumnName("imageid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Imagename)
                .HasMaxLength(255)
                .HasColumnName("imagename");
            entity.Property(e => e.Main)
                .HasDefaultValue(false)
                .HasColumnName("main");
            entity.Property(e => e.Productid).HasColumnName("productid");
        });

        modelBuilder.Entity<ProductVariant>(entity =>
        {
            entity.HasKey(e => e.VariantId).HasName("product_variants_pkey");

            entity.HasIndex(e => e.Sku, "product_variants_sku_key").IsUnique();

            entity.Property(e => e.VariantId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("variant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(0)
                .HasColumnName("quantity");
            entity.Property(e => e.Sku)
                .HasMaxLength(50)
                .HasColumnName("sku");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("updated_at");
            entity.Property(e => e.VariantName)
                .HasMaxLength(100)
                .HasColumnName("variant_name");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductVariants)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("product_variants_product_id_fkey");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Refreshtokenid).HasName("refreshtokens_pkey");

            entity.Property(e => e.Refreshtokenid)
                .HasDefaultValueSql("nextval('refreshtokens_refreshtokenid_seq'::regclass)")
                .HasColumnName("refreshtokenid");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdat");
            entity.Property(e => e.Expiresat)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("expiresat");
            entity.Property(e => e.Token).HasColumnName("token");
            entity.Property(e => e.UseridUuid).HasColumnName("userid_uuid");

            entity.HasOne(d => d.UseridUu).WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UseridUuid)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("refreshtokens_userid_fkey");
        });

        modelBuilder.Entity<RepairChecklist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("repair_checklist_pkey");

            entity.ToTable("RepairChecklist");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.CheckText).HasColumnName("check_text");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(50)
                .HasColumnName("device_type");
            entity.Property(e => e.IsMandatory)
                .HasDefaultValue(true)
                .HasColumnName("is_mandatory");

            entity.HasOne(d => d.Category).WithMany(p => p.RepairChecklists)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("fk_checklist_category");
        });

        modelBuilder.Entity<RepairOrder>(entity =>
        {
            entity.HasKey(e => e.RepairOrderId).HasName("repairorders_pkey");

            entity.HasIndex(e => e.OrderNumber, "repairorders_order_number_key").IsUnique();

            entity.Property(e => e.RepairOrderId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("repair_order_id");
            entity.Property(e => e.Cancelled)
                .HasDefaultValue(false)
                .HasColumnName("cancelled");
            entity.Property(e => e.Contactmethod)
                .HasMaxLength(50)
                .HasColumnName("contactmethod");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("created_at");
            entity.Property(e => e.CustomerId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("customer_id");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.DiscountType).HasMaxLength(50);
            entity.Property(e => e.DiscountValue)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.ExpectedDeliveryDate).HasColumnName("expected_delivery_date");
            entity.Property(e => e.Isfinalsubmit)
                .HasDefaultValue(false)
                .HasColumnName("isfinalsubmit");
            entity.Property(e => e.IssueDescription).HasColumnName("issue_description");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(50)
                .HasColumnName("order_number");
            entity.Property(e => e.OrderType)
                .HasMaxLength(100)
                .HasColumnName("order_type");
            entity.Property(e => e.Paid)
                .HasDefaultValue(false)
                .HasColumnName("paid");
            entity.Property(e => e.Paidamount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
            entity.Property(e => e.ProductType)
                .HasMaxLength(100)
                .HasColumnName("product_type");
            entity.Property(e => e.ReceivedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("received_date");
            entity.Property(e => e.RepairStatus)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Pending'::character varying")
                .HasColumnName("repair_status");
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.TaxPercent)
                .HasPrecision(5, 2)
                .HasDefaultValueSql("0");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("total_amount");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("user_id");
            entity.Property(e => e.WebUpload).HasDefaultValue(false);
        });

        modelBuilder.Entity<RepairOrderPart>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("repairorderparts_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("id");
            entity.Property(e => e.BrandName)
                .HasMaxLength(100)
                .HasColumnName("brand_name");
            entity.Property(e => e.Cancelled)
                .HasDefaultValue(false)
                .HasColumnName("cancelled");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.DeviceModel)
                .HasMaxLength(100)
                .HasColumnName("device_model");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(100)
                .HasColumnName("device_type");
            entity.Property(e => e.OrderDate).HasColumnName("order_date");
            entity.Property(e => e.PartDescription).HasColumnName("part_description");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.ProductType)
                .HasMaxLength(100)
                .HasColumnName("product_type");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.RepairOrderId).HasColumnName("repair_order_id");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .HasColumnName("serial_number");
            entity.Property(e => e.Subcategoryid).HasColumnName("subcategoryid");
            entity.Property(e => e.Tokennumber)
                .HasMaxLength(50)
                .HasColumnName("tokennumber");
            entity.Property(e => e.Total)
                .HasPrecision(10, 2)
                .HasColumnName("total");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.RepairOrder).WithMany(p => p.RepairOrderParts)
                .HasForeignKey(d => d.RepairOrderId)
                .HasConstraintName("fk_repair_order");
        });

        modelBuilder.Entity<RepairTicket>(entity =>
        {
            entity.HasKey(e => e.Ticketid).HasName("repairtickets_pkey");

            entity.Property(e => e.Ticketid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("ticketid");
            entity.Property(e => e.Brand)
                .HasMaxLength(100)
                .HasColumnName("brand");
            entity.Property(e => e.Cancelled)
                .HasDefaultValue(false)
                .HasColumnName("cancelled");
            entity.Property(e => e.Cancelreason).HasColumnName("cancelreason");
            entity.Property(e => e.Createdat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnName("createdat");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.DeviceColour)
                .HasMaxLength(50)
                .HasColumnName("device_colour");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(100)
                .HasColumnName("device_type");
            entity.Property(e => e.Duedate).HasColumnName("duedate");
            entity.Property(e => e.ImeiNumber)
                .HasMaxLength(50)
                .HasColumnName("imei_number");
            entity.Property(e => e.Ipaddress)
                .HasMaxLength(50)
                .HasColumnName("ipaddress");
            entity.Property(e => e.Model)
                .HasMaxLength(100)
                .HasColumnName("model");
            entity.Property(e => e.OrderId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("order_id");
            entity.Property(e => e.Passcode)
                .HasMaxLength(50)
                .HasColumnName("passcode");
            entity.Property(e => e.Repaircost)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("repaircost");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .HasColumnName("serial_number");
            entity.Property(e => e.ServiceCharge)
                .HasPrecision(10, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("service_charge");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Storeid).HasColumnName("storeid");
            entity.Property(e => e.Tasktypeid).HasColumnName("tasktypeid");
            entity.Property(e => e.Technicianid).HasColumnName("technicianid");
            entity.Property(e => e.TicketNo)
                .HasMaxLength(100)
                .HasColumnName("ticket_no");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Store).WithMany(p => p.RepairTickets)
                .HasForeignKey(d => d.Storeid)
                .HasConstraintName("fk_store");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Roleid).HasName("roles_pkey");

            entity.HasIndex(e => e.Rolename, "roles_rolename_key").IsUnique();

            entity.Property(e => e.Roleid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("roleid");
            entity.Property(e => e.RoleOrder).HasColumnName("role_order");
            entity.Property(e => e.Rolename)
                .HasMaxLength(50)
                .HasColumnName("rolename");
        });

        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.HasNoKey();

            entity.Property(e => e.Permissionid).HasColumnName("permissionid");
            entity.Property(e => e.Roleid).HasColumnName("roleid");

            entity.HasOne(d => d.Permission).WithMany()
                .HasForeignKey(d => d.Permissionid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("rolepermissions_permissionid_fkey");

            entity.HasOne(d => d.Role).WithMany()
                .HasForeignKey(d => d.Roleid)
                .HasConstraintName("rolepermissions_roleid_fkey");
        });

        modelBuilder.Entity<ServiceCatalogue>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("servicecatalogue_pkey");

            entity.ToTable("ServiceCatalogue");

            entity.HasIndex(e => e.TaskName, "servicecatalogue_task_name_key").IsUnique();

            entity.Property(e => e.ServiceId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("service_id");
            entity.Property(e => e.Brand)
                .HasMaxLength(255)
                .HasColumnName("brand");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.Model)
                .HasMaxLength(250)
                .HasColumnName("model");
            entity.Property(e => e.ServiceDescription).HasColumnName("service_description");
            entity.Property(e => e.ServicePrice)
                .HasPrecision(10, 2)
                .HasColumnName("service_price");
            entity.Property(e => e.TaskName)
                .HasMaxLength(100)
                .HasColumnName("task_name");
            entity.Property(e => e.TaskTypeId)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("task_type_id");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("status_pkey");

            entity.ToTable("Status");

            entity.HasIndex(e => e.StatusName, "status_status_name_key").IsUnique();

            entity.Property(e => e.StatusId)
                .HasDefaultValueSql("nextval('status_status_id_seq'::regclass)")
                .HasColumnName("status_id");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("stores_pkey");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.DelInd).HasDefaultValue(false);
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.TenantId).HasColumnName("tenant_id");
            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .HasColumnName("username");

            entity.HasOne(d => d.Tenant).WithMany(p => p.Stores)
                .HasForeignKey(d => d.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("stores_tenant_id_fkey");
        });

        modelBuilder.Entity<SubCategory>(entity =>
        {
            entity.HasKey(e => e.Subcategoryid).HasName("subcategories_pkey");

            entity.Property(e => e.Subcategoryid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("subcategoryid");
            entity.Property(e => e.Categoryid).HasColumnName("categoryid");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Delind)
                .HasDefaultValue(true)
                .HasColumnName("delind");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Image).HasColumnName("image");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.WebUpload).HasDefaultValue(false);
        });

        modelBuilder.Entity<SyncLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("SyncLogs_pkey");

            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone");
            entity.Property(e => e.Entity).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(50);
        });

        modelBuilder.Entity<TaskType>(entity =>
        {
            entity.HasKey(e => e.TaskTypeId).HasName("tasktype_pkey");

            entity.ToTable("TaskType");

            entity.HasIndex(e => e.TaskTypeName, "tasktype_task_type_name_key").IsUnique();

            entity.Property(e => e.TaskTypeId)
                .ValueGeneratedNever()
                .HasColumnName("task_type_id");
            entity.Property(e => e.Delind)
                .HasDefaultValue(false)
                .HasColumnName("delind");
            entity.Property(e => e.TaskTypeName)
                .HasMaxLength(50)
                .HasColumnName("task_type_name");
        });

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tenants_pkey");

            entity.ToTable("tenants");

            entity.Property(e => e.Id)
                .HasDefaultValueSql("uuid_generate_v4()")
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("now()")
                .HasColumnName("created_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Ticketnote>(entity =>
        {
            entity.HasKey(e => e.Noteid).HasName("ticketnotes_pkey");

            entity.ToTable("ticketnotes");

            entity.Property(e => e.Noteid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("noteid");
            entity.Property(e => e.Datecreated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.Note).HasColumnName("note");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Ticketid).HasColumnName("ticketid");
            entity.Property(e => e.Type)
                .HasMaxLength(50)
                .HasColumnName("type");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("units_pkey");

            entity.ToTable("units");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Userid).HasName("users_pkey");

            entity.HasIndex(e => e.Userid, "users_userid_uuid_key").IsUnique();

            entity.HasIndex(e => e.Username, "users_username_key").IsUnique();

            entity.Property(e => e.Userid)
                .HasDefaultValueSql("gen_random_uuid()")
                .HasColumnName("userid");
            entity.Property(e => e.Createdon)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createdon");
            entity.Property(e => e.DelInd)
                .HasDefaultValue(false)
                .HasColumnName("del_ind");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Isactive)
                .HasDefaultValue(true)
                .HasColumnName("isactive");
            entity.Property(e => e.Passwordhash).HasColumnName("passwordhash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Pin)
                .HasMaxLength(10)
                .HasColumnName("pin");
            entity.Property(e => e.Roleid).HasColumnName("roleid");
            entity.Property(e => e.Storeid).HasColumnName("storeid");
            entity.Property(e => e.Username)
                .HasMaxLength(50)
                .HasColumnName("username");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.Roleid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_roleid_fkey");

            entity.HasOne(d => d.Store).WithMany(p => p.Users)
                .HasForeignKey(d => d.Storeid)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("users_storeid_fkey");
        });

        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("warehouse_pkey");

            entity.ToTable("warehouse");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AddressLine1)
                .HasMaxLength(200)
                .HasColumnName("address_line1");
            entity.Property(e => e.AddressLine2)
                .HasMaxLength(200)
                .HasColumnName("address_line2");
            entity.Property(e => e.City)
                .HasMaxLength(100)
                .HasColumnName("city");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.Country)
                .HasMaxLength(100)
                .HasColumnName("country");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DelInd)
                .HasDefaultValue(false)
                .HasColumnName("del_ind");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(15)
                .HasColumnName("phone");
            entity.Property(e => e.State)
                .HasMaxLength(100)
                .HasColumnName("state");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.WorkPhone)
                .HasMaxLength(15)
                .HasColumnName("work_phone");
            entity.Property(e => e.Zipcode)
                .HasMaxLength(10)
                .HasColumnName("zipcode");

            entity.HasOne(d => d.Store).WithMany(p => p.Warehouses)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("fk_store");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
