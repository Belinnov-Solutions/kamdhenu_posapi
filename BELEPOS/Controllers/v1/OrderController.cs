using Asp.Versioning;
using BELEPOS.DataModel;
using BELEPOS.Entity;
using BELEPOS.Helper;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text;
using System.Text.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;



namespace BELEPOS.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class OrderController : ControllerBase
    {

        private readonly BeleposContext _context;
        private readonly EPoshelper _eposHelper;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderController> _logger;
        private readonly CentralDbContext _centralDB;

        // simple in-process scheduler
        private static System.Threading.Timer? _syncTimer;
        private static readonly object _timerLock = new();

        public OrderController(BeleposContext context, EPoshelper ePoshelper, IConfiguration config, ILogger<OrderController> logger, CentralDbContext centralDb)
        {
            _context = context;
            _eposHelper = ePoshelper;
            _config = config;
            _logger = logger;
            _centralDB = centralDb;
        }




        
        

        #region  Add parts or update 
        [HttpPost("SavePart")]
        public async Task<ActionResult<ProductDto>> SavePart([FromBody] ProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingProduct = await _context.Products
                    .FirstOrDefaultAsync(p => p.Id == productDto.Id && p.StoreId == productDto.StoreId);

                if (existingProduct != null)
                {
                    // Update existing product (part)
                    existingProduct.StoreId = productDto.StoreId ?? throw new Exception("StoreId is required.");
                    existingProduct.WarehouseId = productDto.WarehouseId ?? Guid.Empty;
                    existingProduct.Name = productDto.ProductName;
                    existingProduct.Slug = productDto.Slug;
                    existingProduct.Sku = productDto.Sku;
                    existingProduct.SellingType = productDto.SellingType ?? "fixed";
                    existingProduct.CategoryId = productDto.CategoryId ?? throw new Exception("CategoryId is required.");
                    existingProduct.SubcategoryId = productDto.SubcategoryId ?? throw new Exception("SubcategoryId is required.");
                    existingProduct.BrandId = productDto.BrandId ?? throw new Exception("BrandId is required.");
                    existingProduct.ModelId = productDto.ModelId;
                    existingProduct.Unit = productDto.Unit ?? "piece";
                    existingProduct.Barcode = productDto.Barcode ?? productDto.Sku;
                    existingProduct.Description = productDto.Description ?? "";
                    existingProduct.IsVariable = productDto.IsVariable ?? false;
                    existingProduct.Price = productDto.Price ?? 0;
                    existingProduct.TaxType = productDto.TaxType ?? "none";
                    existingProduct.DiscountType = productDto.DiscountType ?? "none";
                    existingProduct.DiscountValue = productDto.DiscountValue ?? 0;
                    existingProduct.QuantityAlert = productDto.QuantityAlert ?? 5;
                    //  existingProduct.WarrantyType = productDto.WarrantyType ?? "none";
                    existingProduct.Manufacturer = productDto.Manufacturer ?? "unknown";
                    // existingProduct.ManufacturedDate = productDto.ManufacturedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
                    //  existingProduct.ExpiryDate = productDto.ExpiryDate;
                    existingProduct.Type = "part";
                    existingProduct.IsVisible = true;
                    existingProduct.Stock = productDto.Stock ?? 0;
                    existingProduct.DelInd = false;

                    _context.Products.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Part updated successfully.",
                        data = new { id = existingProduct.Id }
                    });
                }
                else
                {
                    // Add new product (part)
                    var newProduct = new Product
                    {
                        Id = Guid.NewGuid(),
                        StoreId = productDto.StoreId ?? throw new Exception("StoreId is required."),
                        WarehouseId = productDto.WarehouseId ?? Guid.Empty,
                        Name = productDto.ProductName,
                        Slug = productDto.Slug,
                        Sku = productDto.Sku,
                        SellingType = productDto.SellingType,
                        CategoryId = productDto.CategoryId ?? throw new Exception("CategoryId is required."),
                        SubcategoryId = productDto.SubcategoryId ?? throw new Exception("SubcategoryId is required."),
                        //ModelId = productDto.ModelId,
                        BrandId = productDto.BrandId ?? throw new Exception("BrandId is required."),
                        Unit = productDto.Unit ?? "piece",
                        Barcode = productDto.Barcode ?? productDto.Sku,
                        Description = productDto.Description ?? "",
                        IsVariable = productDto.IsVariable ?? false,
                        Price = productDto.Price ?? 0,
                        TaxType = productDto.TaxType ?? "none",
                        DiscountType = productDto.DiscountType ?? "none",
                        DiscountValue = productDto.DiscountValue ?? 0,
                        QuantityAlert = productDto.QuantityAlert ?? 5,
                        // WarrantyType = productDto.WarrantyType ?? "none",
                        Manufacturer = productDto.Manufacturer ?? "unknown",
                        //  ManufacturedDate = productDto.ManufacturedDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
                        //  ExpiryDate = productDto.ExpiryDate,
                        Type = ProductType.Part.ToString(),
                        IsVisible = true,
                        Stock = productDto.Stock,
                        DelInd = false
                    };

                    _context.Products.Add(newProduct);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Part added successfully.",
                        data = new { id = newProduct.Id }
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to save part.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        #endregion





        #region confirm Order
        [HttpPost("ConfirmOrder")]
        public async Task<IActionResult> ConfirmOrder([FromBody] RepairOrderDto request)
        {
            string printerName = _config.GetValue<string>("AppSettings:PrinterName");


            if (!await _eposHelper.StoreExists(request.StoreId))
                return BadRequest("Store not found or inactive");

            //string repairStatus = _eposHelper.DetermineRepairStatus(request);
            bool isUpdate = request.RepairOrderId != Guid.Empty;
            string repairStatus;
            if (isUpdate)
            {
                var existingOrder = await _context.RepairOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.RepairOrderId == request.RepairOrderId);

                repairStatus = _eposHelper.DetermineRepairStatus(request, existingOrder?.RepairStatus);
            }
            else
            {
                repairStatus = _eposHelper.DetermineRepairStatus(request);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var repairOrderId = isUpdate ? request.RepairOrderId : Guid.NewGuid();
                var ticketNo = isUpdate ? request.Tickets?.TicketNo : await _eposHelper.GenerateTicketNumberAsync();
                var orderNumber = isUpdate ? request.OrderNumber : await _eposHelper.GenerateOrderNumberAsync(_config);

                if (isUpdate)
                {
                    await _eposHelper.UpdateRepairOrder(repairOrderId, request, repairStatus);
                    await _eposHelper.UpdateTicket(repairOrderId, request, repairStatus);
                }
                else
                {
                    await _eposHelper.CreateRepairOrder(repairOrderId, orderNumber, request, repairStatus);
                    await _eposHelper.CreateTicket(repairOrderId, ticketNo, request, repairStatus);

                }


                if (request.Parts?.Any() == true)
                {
                    await _eposHelper.SaveParts(repairOrderId, request);
                    foreach (var part in request.Parts)
                        await _eposHelper.EnsureStockAndDeductAsync(part.ProductId, part.Quantity);
                }
                //await _eposHelper.SavePayments(isUpdate, repairOrderId, request);
                request.PaidAmount = request.TotalAmount;

                await _eposHelper.SavePayments(repairOrderId, request);


                // ✅ Mark as paid after payments are saved
                var repairOrder = await _context.RepairOrders.FirstOrDefaultAsync(r => r.RepairOrderId == repairOrderId);
                if (repairOrder != null)
                {
                    repairOrder.Paid = true; // Set paid status
                    _context.RepairOrders.Update(repairOrder);
                    await _context.SaveChangesAsync();
                }

                await _eposHelper.SaveChecklistResponses(repairOrderId, request);

                //  await _eposHelper.SendRepairTicketEmail(ticketNo, repairStatus, request);
                //await _eposHelper.PrintReceiptAsync(repairOrderId, printerName.ToString());


                await _eposHelper.PrintReceiptAsync(repairOrderId, printerName.ToString(), request);

                await transaction.CommitAsync();
                return Ok(new
                {
                    message = isUpdate ? "Order updated successfully." : "Order created successfully.",
                    repairOrderId,
                    orderNumber,
                    ticketNo
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "Failed to save order", error = ex.ToString() });
            }
        }
        #endregion

        #region GerRepairOrderSummart

        [HttpGet("GetRepairOrderSummary")]
        public async Task<IActionResult> GetRepairOrderSummary(Guid orderId, Guid ticketId, Guid storeId)
        {
            try
            {
                var summary = await (from ro in _context.RepairOrders
                                     join c in _context.Customers on ro.CustomerId equals c.CustomerId
                                     join rt in _context.RepairTickets on ro.RepairOrderId equals rt.OrderId
                                     join sc in _context.ServiceCatalogues on rt.Tasktypeid equals sc.TaskTypeId
                                     where rt.Ticketid == ticketId
                                           && ro.RepairOrderId == orderId
                                           && rt.Storeid == storeId
                                           && ro.Delind == false
                                     select new RepairOrderSummaryDto
                                     {
                                         RepairOrderId = ro.RepairOrderId,
                                         OrderNumber = ro.OrderNumber,
                                         CustomerName = c.CustomerName,
                                         TotalAmount = (decimal)ro.TotalAmount,
                                         TaskName = sc.TaskName,
                                         ServicePrice = sc.ServicePrice,
                                         //CreatedAt = ro.CreatedAt
                                     }).FirstOrDefaultAsync();

                if (summary == null)
                    return NotFound("No repair order found for the given parameters.");

                return Ok(summary);
            }
            catch (Exception ex)
            {

                //throw;
                return StatusCode(500, new { message = "Failed to save order", error = ex.Message });
            }


        }

        #endregion




        #region Get Parts
        //Created by Devansh
        [HttpGet("GetParts")]
        public async Task<IActionResult> GetParts()
        {
            try
            {
                var parts = await _context.Parts
                .Where(p => p.InStock == true)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
                return Ok(new
                {
                    message = "Parts fetched successfully.",
                    data = parts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching parts.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion


        #region get repair list
        /// <summary>
        /// Created by sunit
        /// Date 18-07-2025
        /// </summary>
        [HttpGet("GetTickets")]
        public async Task<IActionResult> GetTickets([FromQuery] Guid storeId)
        {

            try
            {
                bool storeExists = await _eposHelper.FindStore(storeId);
                if (!storeExists)
                    return BadRequest("Store not found or inactive");


                var tickets = await (
    from rt in _context.RepairTickets
    join ro in _context.RepairOrders on rt.OrderId equals ro.RepairOrderId
    join tasktype in _context.Products on rt.Tasktypeid equals tasktype.Id
    join cust in _context.Customers on ro.CustomerId equals cust.CustomerId into custJoin
    from customer in custJoin.DefaultIfEmpty()
    join tech in _context.Users on rt.Technicianid equals tech.Userid into techJoin
    from technician in techJoin.DefaultIfEmpty()
    where ro.Delind == false && rt.Storeid == storeId && rt.Cancelled == false
    orderby rt.Createdat descending
    select new RepairTicketsDto
    {
        Ticketid = rt.Ticketid,
        RepairOrderId = rt.OrderId ?? Guid.Empty,
        DeviceType = rt.DeviceType,
        DeviceColour = rt.DeviceColour,
        Model = rt.Model,
        Brand = rt.Brand,
        SerialNumber = rt.SerialNumber,
        ImeiNumber = rt.ImeiNumber,
        Isfinalsubmit = ro.Isfinalsubmit,
        Passcode = rt.Passcode,
        ServiceCharge = (decimal)rt.ServiceCharge,
        RepairCost = rt.Repaircost ?? 0,
        TechnicianId = rt.Technicianid,
        TechnicianName = technician != null ? technician.Username : null,
        Status = rt.Status,
        CustomerId = ro.CustomerId ?? Guid.Empty,
        CustomerName = customer != null ? customer.CustomerName : null, // <-- Add this
        CustomerNumber = customer != null ? customer.Phone : null,
        Contactmethod = ro.Contactmethod != null
         ? ro.Contactmethod.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
          : new List<string>(),
        OrderNumber = ro.OrderNumber,
        UserId = (Guid)ro.UserId,
        DueDate = rt.Duedate.HasValue ? rt.Duedate.Value.ToUniversalTime() : (DateTime?)null,
        CreatedAt = rt.Createdat.HasValue ? rt.Createdat.Value.ToUniversalTime() : (DateTime?)null,
        TaskTypeName = tasktype.Name,
        TaskTypeId = rt.Tasktypeid,

        TicketNo = rt.TicketNo
    }).ToListAsync();


                // Populate Notes and OrderParts manually
                foreach (var ticket in tickets)
                {
                    ticket.Notes = await _context.Ticketnotes
                        .Where(n => n.Ticketid == ticket.Ticketid)
                        .Select(n => new TicketsNotesDto
                        {
                            Id = n.Noteid,
                            Notes = n.Note,
                            Type = n.Type
                        }).ToListAsync();

                    ticket.OrderParts = await _context.RepairOrderParts
                                       .Where(p => p.RepairOrderId == ticket.RepairOrderId)
                                       .Select(p => new RepairOrderPartDto
                                       {
                                           Id = p.Id,
                                           ProductId = p.ProductId,
                                           ProductName = p.ProductName,
                                           ProductType = p.ProductType,
                                           Quantity = p.Quantity,
                                           Price = p.Price,
                                           Total = p.Total,
                                           SerialNumber = p.SerialNumber,
                                           BrandName = p.BrandName,

                                       }).ToListAsync();

                }

                return Ok(new
                {
                    message = "Tickets fetched successfully",
                    data = tickets
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    message = "An error occurred while fetching parts.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }

        }


        #endregion



        #region assign technician
        [HttpGet("AssignTechnician")]
        public async Task<IActionResult> UpdateTechnician([FromQuery] Guid ticketId, Guid orderId, Guid technicianId, string status)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ticket = await _context.RepairTickets
                    .FirstOrDefaultAsync(t => t.Ticketid == ticketId && t.OrderId == orderId);

                if (ticket == null)
                    return NotFound(new { message = "Ticket not found." });

                // ✅ Force UTC kind for DateTime fields going into DB
                if (ticket.Duedate.HasValue)
                    ticket.Duedate = DateTime.SpecifyKind(ticket.Duedate.Value, DateTimeKind.Utc);

                if (ticket.Createdat != null)
                    ticket.Createdat = DateTime.SpecifyKind(ticket.Createdat.Value, DateTimeKind.Utc);

                // ✅ Prevent reassignment to same technician
                if (ticket.Technicianid == technicianId)
                {
                    if (ticket.Status != status)
                    {
                        ticket.Status = status;
                        _context.RepairTickets.Update(ticket);
                        await _context.SaveChangesAsync();


                        return Ok(new { message = "Status updated successfully (technician unchanged)." });
                    }

                    return BadRequest(new { message = "Technician is already assigned to this ticket." });
                }

                // ✅ Reassign technician and update status
                ticket.Technicianid = technicianId;
                ticket.Status = status;

                _context.RepairTickets.Update(ticket);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Technician and status updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while updating technician.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion



        #region Get Repair Checklist
        // GET: api/Product/repair-checklist?deviceType=Mobile
        [HttpGet("GetRepairchecklist")]
        public async Task<IActionResult> GetRepairchecklist([FromQuery] string deviceType)
        {
            if (string.IsNullOrWhiteSpace(deviceType))
                return BadRequest(new { message = "DeviceType is required." });

            try
            {
                var checklist = await _context.RepairChecklists
                    .Where(r => r.DeviceType.ToLower() == deviceType.ToLower())
                    .Include(r => r.Category)
                    .ToListAsync();

                var result = checklist
                    .GroupBy(r => new { r.CategoryId, r.Category.Name })
                    .Select(group => new
                    {
                        categoryId = group.Key.CategoryId,
                        categoryName = group.Key.Name,
                        checklist = group.Select(item => new
                        {
                            id = item.Id,
                            checkText = item.CheckText,
                            isMandatory = item.IsMandatory
                        }).ToList()
                    }).ToList();

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        #endregion




        #region RepairOrderCheckList for order id and ticketid
        [HttpGet("RepairOrderCheckList")]
        public async Task<IActionResult> RepairOrderCheckList([FromQuery] Guid ticketId, [FromQuery] Guid orderId)
        {
            // Step 1: Fetch all checklist items (no device type filtering)
            var checklistItems = await _context.RepairChecklists
                .Include(c => c.Category)
                .ToListAsync();

            // Step 2: Fetch saved responses for the ticket + order
            var savedResponses = await _context.ChecklistResponses
                .Where(r => r.TicketId == ticketId && r.OrderId == orderId)
                .ToListAsync();

            // Step 3: Join checklist with saved responses and group by category
            var result = checklistItems
                .GroupBy(c => c.Category.Name)
                .Select(group => new
                {
                    CategoryName = group.Key,
                    Responses = group.Select(item => new
                    {
                        Label = item.CheckText,
                        Value = savedResponses.FirstOrDefault(r => r.ChecklistId == item.Id)?.Value,
                        RepairInspection = savedResponses.FirstOrDefault(r => r.ChecklistId == item.Id)?.RepairInspection,
                    })
                });

            return Ok(result);
        }
        #endregion






        #region Cancel Repair Order
        [HttpPut("CancelTicket")]
        public async Task<IActionResult> CancelRepairOrderAndTicket([FromBody] CancelRepairRequest request)
        {
            try
            {
                if (request.TicketId == Guid.Empty || request.OrderId == Guid.Empty)
                {
                    return BadRequest("Both ticketId and orderId are required.");
                }

                // Fetch the ticket
                var ticket = await _context.RepairTickets
                    .FirstOrDefaultAsync(t => t.Ticketid == request.TicketId);

                if (ticket == null)
                {
                    return NotFound("Repair ticket not found.");
                }

                // Validate that ticket belongs to order
                if (ticket.OrderId != request.OrderId)
                {
                    return BadRequest("The ticket does not belong to the provided order.");
                }

                // Fetch the order
                var order = await _context.RepairOrders
                    .FirstOrDefaultAsync(o => o.RepairOrderId == request.OrderId);

                if (order == null)
                {
                    return NotFound("Repair order not found.");
                }

                // Fetch parts
                var parts = await _context.RepairOrderParts
                    .Where(p => p.RepairOrderId == request.OrderId)
                    .ToListAsync();

                // Mark all as cancelled
                ticket.Cancelled = true;
                ticket.Cancelreason = request.Cancelreason;
                order.Cancelled = true;

                foreach (var part in parts)
                {
                    part.Cancelled = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Cancelled successfully.",
                    ticketId = request.TicketId,
                    orderId = request.OrderId,
                    //partsCancelled = parts.Count
                });
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    message = "An error occurred while cancelling the repair ticket and order.",
                    error = ex.Message
                });
            }
        }
        #endregion






        #region get ordered product
        [HttpGet("GetOrderedProduct")]
        public async Task<IActionResult> GetOrderedProduct([FromQuery] Guid storeId)
        {
            if (storeId == Guid.Empty)
                return BadRequest("storeId is required.");

            try
            {
                var ordersWithProducts = await _context.RepairOrders
                    .Where(ro => ro.StoreId == storeId && ro.Delind == false)
                    .OrderByDescending(ro => ro.OrderNumber)
                    .Select(ro => new
                    {
                        ro.RepairOrderId,
                        ro.OrderNumber,
                        ro.UserId,
                        ro.CustomerId,
                        ro.CreatedAt,
                        ro.TotalAmount,
                        Products = ro.RepairOrderParts
                            .Where(part => part.ProductType == "Product")
                            .Select(part => new
                            {
                                part.ProductId,
                                part.ProductName,
                                part.BrandName,
                                part.Quantity,
                                part.Price,
                                part.Total
                            }).ToList()
                    })
                    .Where(ro => ro.Products.Any()) // only include orders that have product-type parts
                    .ToListAsync();

                return Ok(new
                {
                    message = "Product orders fetched successfully",
                    data = ordersWithProducts
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching product orders.",
                    error = ex.Message
                });
            }
        }

        #endregion




        #region Daily Sales Report
        [HttpGet("SalesReport")]
        public async Task<IActionResult> GetSalesReport(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string groupBy = "day",
            [FromQuery] string? paymentMethod = null)
        {
            try
            {
                if (dateFrom.HasValue) dateFrom = DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc);
                if (dateTo.HasValue) dateTo = DateTime.SpecifyKind(dateTo.Value, DateTimeKind.Utc);

                // Base RepairOrders
                var orders = _context.RepairOrders
                    .Where(r => !(r.Delind ?? false) && !(r.Cancelled ?? false) && r.OrderDate != null);

                /*if (dateFrom.HasValue && dateTo.HasValue)
                    orders = orders.Where(r => r.CreatedAt >= dateFrom && r.CreatedAt <= dateTo);*/
                if (dateFrom.HasValue && dateTo.HasValue)
                    orders = orders.Where(r => r.OrderDate >= dateFrom && r.OrderDate <= dateTo);
                if (!string.IsNullOrEmpty(paymentMethod))
                    orders = orders.Where(r => r.PaymentMethod == paymentMethod);

                // Flatten parts + join Products + join Categories
                var parts = from r in orders
                            from p in r.RepairOrderParts
                            where p.ProductId != null && r.Delind == false && p.Cancelled == false && p.Delind == false
                            join pr in _context.Products on p.ProductId!.Value equals pr.Id
                            join c in _context.Categories on pr.CategoryId equals c.Categoryid
                            select new
                            {
                                r.RepairOrderId,
                                r.OrderDate,
                                Part = p,
                                Product = pr,
                                Category = c
                            };

                // Global summary (all categories within filter)
                var categorySummary = await parts
                    .GroupBy(x => new { x.Category.Categoryid, x.Category.CategoryName })
                    .Select(cg => new
                    {
                        CategoryId = cg.Key.Categoryid,
                        CategoryName = cg.Key.CategoryName,
                        TotalSalesAmount = cg.Sum(x => x.Part.Total ?? 0m),
                        ItemsSold = cg.Sum(x => (int?)x.Part.Quantity ?? 0)
                    })
                    .OrderByDescending(x => x.TotalSalesAmount)
                    .ToListAsync();

                // === Token list (grouped by TokenNumber, latest first) ===
                var tokens = await parts
                    .Where(x => !string.IsNullOrEmpty(x.Part.Tokennumber))
                    .GroupBy(x => new { x.Part.Tokennumber, x.Category.CategoryName })
                    .Select(g => new
                    {
                        TokenNumber = g.Key.Tokennumber,
                        CategoryName = g.Key.CategoryName,
                        TotalPrice = g.Sum(x => x.Part.Price * x.Part.Quantity),
                        LatestCreatedAt = g.Max(x => x.OrderDate)
                    })
                    .OrderByDescending(x => x.LatestCreatedAt) // latest token first
                    .ThenByDescending(x => x.TokenNumber)     // tie-breaker
                    .ToListAsync();

                object reportData;

                switch (groupBy?.ToLowerInvariant())
                {
                    case "day":
                        reportData = await parts
                            .GroupBy(x => x.OrderDate!.Value.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                TotalSalesAmount = g.Sum(x => x.Part.Total ?? 0m),
                                ItemsSold = g.Sum(x => (int?)x.Part.Quantity ?? 0),
                                Orders = g.Select(x => x.RepairOrderId).Distinct().Count(),

                                Categories = g.GroupBy(x => new { x.Category.Categoryid, x.Category.CategoryName })
                                    .Select(cg => new
                                    {
                                        CategoryId = cg.Key.Categoryid,
                                        CategoryName = cg.Key.CategoryName,
                                        TotalSalesAmount = cg.Sum(x => x.Part.Total ?? 0m),
                                        ItemsSold = cg.Sum(x => (int?)x.Part.Quantity ?? 0)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList(),

                                Products = g.GroupBy(x => new { x.Part.ProductId, x.Part.ProductName })
                                    .Select(pg => new
                                    {
                                        ProductId = pg.Key.ProductId,
                                        ProductName = pg.Key.ProductName,
                                        QtySold = pg.Sum(x => (int?)x.Part.Quantity ?? 0),
                                        TotalSalesAmount = pg.Sum(x => x.Part.Total ?? 0m)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList()
                            })
                            .OrderBy(x => x.Date)
                            .ToListAsync();
                        break;

                    case "week":
                        var weekParts = await parts.ToListAsync();
                        var cal = CultureInfo.InvariantCulture.Calendar;
                        reportData = weekParts
                            .GroupBy(x =>
                            {
                                var dt = x.OrderDate!.Value;
                                var week = cal.GetWeekOfYear(dt, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
                                return new { dt.Year, Week = week };
                            })
                            .Select(g => new
                            {
                                Week = $"{g.Key.Year}-W{g.Key.Week}",
                                TotalSalesAmount = g.Sum(x => x.Part.Total ?? 0m),
                                ItemsSold = g.Sum(x => (int?)x.Part.Quantity ?? 0),
                                Orders = g.Select(x => x.RepairOrderId).Distinct().Count(),

                                Categories = g.GroupBy(x => new { x.Category.Categoryid, x.Category.CategoryName })
                                    .Select(cg => new
                                    {
                                        CategoryId = cg.Key.Categoryid,
                                        CategoryName = cg.Key.CategoryName,
                                        TotalSalesAmount = cg.Sum(x => x.Part.Total ?? 0m),
                                        ItemsSold = cg.Sum(x => (int?)x.Part.Quantity ?? 0)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList(),

                                Products = g.GroupBy(x => new { x.Part.ProductId, x.Part.ProductName })
                                    .Select(pg => new
                                    {
                                        ProductId = pg.Key.ProductId,
                                        ProductName = pg.Key.ProductName,
                                        QtySold = pg.Sum(x => (int?)x.Part.Quantity ?? 0),
                                        TotalSalesAmount = pg.Sum(x => x.Part.Total ?? 0m)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList()
                            })
                            .OrderBy(x => x.Week)
                            .ToList();
                        break;

                    case "month":
                        reportData = await parts
                            .GroupBy(x => new { x.OrderDate!.Value.Year, x.OrderDate!.Value.Month })
                            .Select(g => new
                            {
                                Month = g.Key.Year + "-" + g.Key.Month,
                                TotalSalesAmount = g.Sum(x => x.Part.Total ?? 0m),
                                ItemsSold = g.Sum(x => (int?)x.Part.Quantity ?? 0),
                                Orders = g.Select(x => x.RepairOrderId).Distinct().Count(),

                                Categories = g.GroupBy(x => new { x.Category.Categoryid, x.Category.CategoryName })
                                    .Select(cg => new
                                    {
                                        CategoryId = cg.Key.Categoryid,
                                        CategoryName = cg.Key.CategoryName,
                                        TotalSalesAmount = cg.Sum(x => x.Part.Total ?? 0m),
                                        ItemsSold = cg.Sum(x => (int?)x.Part.Quantity ?? 0)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList(),

                                Products = g.GroupBy(x => new { x.Part.ProductId, x.Part.ProductName })
                                    .Select(pg => new
                                    {
                                        ProductId = pg.Key.ProductId,
                                        ProductName = pg.Key.ProductName,
                                        QtySold = pg.Sum(x => (int?)x.Part.Quantity ?? 0),
                                        TotalSalesAmount = pg.Sum(x => x.Part.Total ?? 0m)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList()
                            })
                            .OrderBy(x => x.Month)
                            .ToListAsync();
                        break;

                    default:
                        return BadRequest("Invalid groupBy parameter. Use 'day', 'week', or 'month'.");
                }

                return Ok(new
                {
                    filters = new { dateFrom, dateTo, groupBy, paymentMethod },
                    summary = new
                    {
                        TotalOrders = await orders.CountAsync(),
                        TotalSales = await parts.SumAsync(x => x.Part.Total ?? 0m),
                        CategorySummary = categorySummary
                    },
                    reportData,
                    tokens // <--- grouped & latest first
                });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch sales report", error = ex.Message });
            }
        }
        #endregion

        

        #region receipt by token
        [HttpGet("ReceiptByToken")]
        public async Task<IActionResult> GetReceiptByToken([FromQuery] string tokenNumber)
        {
            try
            {
                var items = await (
                    from r in _context.RepairOrders
                    from p in r.RepairOrderParts
                    where p.Tokennumber == tokenNumber && p.ProductId != null
                    join pr in _context.Products on p.ProductId!.Value equals pr.Id
                    join c in _context.Categories on pr.CategoryId equals c.Categoryid
                    select new
                    {
                        r.RepairOrderId,
                        r.CreatedAt,
                        p.Tokennumber,
                        ProductId = pr.Id,
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        Total = p.Total,
                        CategoryName = c.CategoryName
                    }
                ).ToListAsync();

                if (!items.Any())
                    return NotFound(new { message = "No items found for this token." });

                var receipt = new
                {
                    TokenNumber = tokenNumber,
                    Category = items.First().CategoryName,
                    CreatedAt = items.First().CreatedAt,
                    Items = items.Select(i => new
                    {
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.Price,
                        i.Total
                    }).ToList(),
                    ReceiptTotal = items.Sum(i => i.Total ?? 0m)
                };

                return Ok(receipt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch receipt", error = ex.Message });
            }
        }
        #endregion

        #region

        /*[HttpGet("ReceiptByTokenV2")]
        public async Task<IActionResult> GetReceiptByTokenV2([FromQuery] string tokenNumber, [FromQuery] DateTime? date = null)
        {
            try
            {
                //  If no date provided, use today's date (UTC safe)
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                var items = await (
                    from r in _context.RepairOrders
                    from p in r.RepairOrderParts
                    where p.Tokennumber == tokenNumber
                          && p.ProductId != null
                          && p.CreatedAt.HasValue
                          && p.CreatedAt.Value.Date == targetDate
                    join pr in _context.Products on p.ProductId!.Value equals pr.Id
                    join c in _context.Categories on pr.CategoryId equals c.Categoryid
                    select new
                    {
                        r.RepairOrderId,
                        r.CreatedAt,
                        r.PaymentMethod,        // Added PaymentMethod here
                        p.Tokennumber,
                        ProductId = pr.Id,
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        Total = p.Total,
                        CategoryName = c.CategoryName
                    }
                ).ToListAsync();

                if (!items.Any())
                    return NotFound(new { message = "No items found for this token on the given date." });

                var receipt = new
                {
                    TokenNumber = tokenNumber,
                    Date = targetDate,  // Return applied date filter
                    PaymentMethod = items.First().PaymentMethod, // Include payment method
                    Categories = items.Select(i => i.CategoryName).Distinct().ToList(),
                    CreatedAt = items.First().CreatedAt,
                    Items = items.Select(i => new
                    {
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.Price,
                        i.Total,
                        i.CategoryName
                    }).ToList(),
                    ReceiptTotal = items.Sum(i => i.Total ?? 0m)
                };

                return Ok(receipt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch receipt", error = ex.Message });
            }
        }*/


        


        [HttpGet("ReceiptByTokenV2")]
        public async Task<IActionResult> GetReceiptByTokenV2([FromQuery] string tokenNumber, [FromQuery] DateTime? date = null)
        {
            try
            {
                if (string.IsNullOrEmpty(tokenNumber))
                    return BadRequest(new { message = "Token number is required." });

                // Ensure UTC date and truncate time
                var targetDate = date.HasValue
                    ? DateTime.SpecifyKind(date.Value.Date, DateTimeKind.Utc)
                    : DateTime.UtcNow.Date;

                // Start and end of the day (UTC)
                var startDate = targetDate;
                var endDate = startDate.AddDays(1);

                // Query items
                var items = await (
                    from r in _context.RepairOrders
                    from p in r.RepairOrderParts
                    where p.Tokennumber == tokenNumber
                          && p.ProductId != null
                          && p.OrderDate.HasValue
                          && p.OrderDate >= startDate
                          && p.OrderDate < endDate
                          && r.Delind == false && p.Delind==false && p.Cancelled == false && r.Cancelled ==false
                    join pr in _context.Products on p.ProductId.Value equals pr.Id
                    join c in _context.Categories on pr.CategoryId equals c.Categoryid
                    select new
                    {
                        r.RepairOrderId,
                        r.CreatedAt,
                        r.PaymentMethod,
                        p.Tokennumber,
                        ProductId = pr.Id,
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        Total = p.Total,
                        CategoryName = c.CategoryName,
                        OrderDate = r.OrderDate
                    }
                ).ToListAsync();

                if (!items.Any())
                    return NotFound(new { message = "No items found for this token on the given date." });

                var firstItem = items.First();

                var receipt = new
                {
                    TokenNumber = tokenNumber,
                    Date = targetDate.ToString("yyyy-MM-dd"), // no 00:00:00
                    PaymentMethod = firstItem.PaymentMethod,
                    Categories = items.Select(i => i.CategoryName).Distinct().ToList(),
                    CreatedAt = DateTime.SpecifyKind(firstItem.OrderDate ?? DateTime.UtcNow, DateTimeKind.Utc),
                    Items = items.Select(i => new
                    {
                        i.ProductId,
                        i.ProductName,
                        i.Quantity,
                        i.Price,
                        i.Total,
                        i.CategoryName
                    }).ToList(),
                    ReceiptTotal = items.Sum(i => i.Total ?? 0m)
                };

                return Ok(receipt);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch receipt", error = ex.ToString() });
            }
        }





        #endregion


        [HttpDelete("CancelOrder")]
        public async Task<IActionResult> CancelOrder([FromQuery] string tokenNumber, [FromQuery] DateTime? date = null)
        {
            try
            {
                var targetDate = date?.Date ?? DateTime.UtcNow.Date;

                var orders = await _context.RepairOrders
                    .Include(r => r.RepairOrderParts)
                    .Where(r => r.RepairOrderParts.Any(p =>
                        p.Tokennumber == tokenNumber &&
                        p.CreatedAt.HasValue &&
                        p.CreatedAt.Value.Date == targetDate))
                    .ToListAsync();

                if (!orders.Any())
                    return NotFound(new { message = "No repair orders found for this token on the given date." });

                foreach (var order in orders)
                {
                    // Mark matching parts as cancelled/deleted
                    foreach (var part in order.RepairOrderParts.Where(p =>
                                 p.Tokennumber == tokenNumber &&
                                 p.CreatedAt.HasValue &&
                                 p.CreatedAt.Value.Date == targetDate))
                    {
                        part.Cancelled = true;
                        part.Delind = true;
                        order.WebUpload = false;
                        _context.RepairOrderParts.Update(part);
                    }

                    // ✅ Check if *all* parts in the order are cancelled/deleted
                    bool allPartsCancelled = order.RepairOrderParts.All(p => p.Cancelled==true && p.Delind==true);

                    if (allPartsCancelled)
                    {
                        order.Cancelled = true;
                        order.Delind = true;
                        order.WebUpload = false;
                        _context.RepairOrders.Update(order);
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Receipt deleted successfully.",
                    TokenNumber = tokenNumber,
                    Date = targetDate,
                    DeletedOrders = orders.Select(o => o.RepairOrderId)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to delete receipt", error = ex.Message });
            }
        }









        // ------------------ SYNC TO CENTRAL ------------------
        /*private async Task<SyncResult> SyncOnceToCentralAsync(CancellationToken ct)
        {
            var sw = Stopwatch.StartNew();
            var url = _config.GetValue<string>("OrderSync:CentralUrl");
            var batchSize = _config.GetValue<int?>("OrderSync:BatchSize") ?? 500;

            if (string.IsNullOrWhiteSpace(url))
            {
                var err = "OrderSync:CentralUrl not configured.";
                _logger.LogWarning(err);
                await WriteLogAsync("RepairOrder", 0, "Failed", err, ct);
                return new SyncResult { Success = false, Error = err };
            }

            try
            {
                // Pull unsynced orders
                var batch = await _context.RepairOrders
                    .Include(r => r.RepairOrderParts)
                    .Include(r => r.OrderPayments)
                    .AsNoTracking()
                    .Where(r => r.WebUpload != true && (r.Delind == null || r.Delind == false))
                    .OrderBy(r => r.CreatedAt)
                    .Take(batchSize)
                    .ToListAsync(ct);

                await WriteLogAsync("RepairOrder", batch.Count, batch.Count == 0 ? "NoData" : "Pulled", null, ct);

                if (batch.Count == 0)
                {
                    sw.Stop();
                    return new SyncResult { Success = true, Orders = 0, Message = "Nothing to sync.", RanAtUtc = DateTime.UtcNow };
                }

                // Build DTOs with proper DateTime handling
                var repairOrderDtos = batch.Select(r => new RepairOrderDto
                {
                    RepairOrderId = r.RepairOrderId,
                    OrderNumber = r.OrderNumber,
                    Paid = r.Paid,
                    PaymentMethod = r.PaymentMethod,
                    IssueDescription = r.IssueDescription,
                    RepairStatus = r.RepairStatus,
                    ReceivedDate = ToUtc(r.ReceivedDate),
                    ExpectedDeliveryDate = ToUtc(r.ExpectedDeliveryDate),
                    StoreId = r.StoreId,
                    CreatedAt = ToUtc(r.CreatedAt),
                    UpdatedAt = ToUtc(r.UpdatedAt),
                    Delind = r.Delind,
                    CustomerId = r.CustomerId,
                    UserId = r.UserId ?? Guid.Empty,
                    IsFinalSubmit = r.Isfinalsubmit,
                    TotalAmount = r.TotalAmount,
                    ProductType = r.ProductType,
                    Cancelled = r.Cancelled,
                    Contactmethod = string.IsNullOrEmpty(r.Contactmethod)
                        ? new List<string>()
                        : r.Contactmethod.Split(',').ToList(),
                    PaidAmount = r.Paidamount,
                    DiscountType = r.DiscountType,
                    DiscountValue = r.DiscountValue,
                    TaxPercent = r.TaxPercent,
                    OrderType = r.OrderType,

                    Parts = r.RepairOrderParts.Select(p => new RepairOrderPartDto
                    {
                        Id = Guid.NewGuid(),
                        RepairOrderId = r.RepairOrderId,
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        PartDescription = p.PartDescription,
                        DeviceType = p.DeviceType,
                        DeviceModel = p.DeviceModel,
                        SerialNumber = p.SerialNumber,
                        Quantity = p.Quantity,
                        Price = p.Price,
                        Total = p.Total,
                        CreatedAt = ToUnspecified(p.CreatedAt),
                        UpdatedAt = ToUnspecified(p.UpdatedAt),
                        BrandName = p.BrandName,
                        ProductType = p.ProductType,
                        Cancelled = p.Cancelled,
                        TokenNumber = p.Tokennumber,
                        SubcategoryId = p.Subcategoryid ?? Guid.Empty
                    }).ToList(),

                    Payments = r.OrderPayments.Select(op => new OrderPaymentDto
                    {
                        PaymentId = op.Paymentid,
                        RepairOrderId = op.Repairorderid,
                        Amount = op.Amount,
                        PaymentMethod = op.PaymentMethod,
                        PaidAt = ToUnspecified(op.PaidAt),
                        CreatedAt = ToUnspecified(op.CreatedAt),
                        PartialPayment = op.PartialPayment,
                        TotalAmount = op.TotalAmount,
                        FullyPaid = op.FullyPaid,
                        RemainingAmount = op.Remainingamount
                    }).ToList()
                }).ToList();

                var payload = new OrderSyncEnvelope
                {
                    SourceSystem = "LOCAL",
                    GeneratedUtc = DateTime.UtcNow,
                    RepairOrders = repairOrderDtos
                };

                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(100) };
                using var content = new StringContent(json, Encoding.UTF8, "application/json");
                var resp = await http.PostAsync(url, content, ct);
                var respText = await resp.Content.ReadAsStringAsync(ct);

                if (!resp.IsSuccessStatusCode)
                {
                    sw.Stop();
                    var errMsg = $"Central API {(int)resp.StatusCode} {resp.ReasonPhrase}: {respText}";
                    _logger.LogError("Order sync failed: {Status} {Reason}. Body: {Body}", (int)resp.StatusCode, resp.ReasonPhrase, respText);
                    await WriteLogAsync("RepairOrder", batch.Count, "Failed", Trunc(errMsg, 1000), ct);
                    return new SyncResult { Success = false, Error = errMsg, RanAtUtc = DateTime.UtcNow };
                }

                foreach (var r in batch)
                    r.WebUpload = true;

                _context.RepairOrders.UpdateRange(batch);
                await _context.SaveChangesAsync(ct);

                sw.Stop();
                await WriteLogAsync("RepairOrder", batch.Count, "Flipped", $"ElapsedMs={sw.ElapsedMilliseconds}", ct);

                return new SyncResult
                {
                    Success = true,
                    Orders = batch.Count,
                    Message = string.IsNullOrWhiteSpace(respText) ? "Synced." : respText,
                    RanAtUtc = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                //sw.Stop();
                _logger.LogError(ex, "SyncOnceToCentralAsync failed.");
                await WriteLogAsync("RepairOrder", 0, "Failed", Trunc($"{ex.Message} | {ex.StackTrace}", 1000), ct);
                return new SyncResult { Success = false, Error = ex.Message, RanAtUtc = DateTime.UtcNow };
            }
        }*/


        //comment by Sunit

        /*[HttpGet("OrderProductSync")]
        public async Task<object> OrderProductSync([FromBody] OrderSyncEnvelope payload, CancellationToken ct)
        {
            if (payload?.RepairOrders == null || payload.RepairOrders.Count == 0)
                return BadRequest(new { success = false, message = "Empty payload." });

            int importedOrders = 0;
            int importedParts = 0;
            int importedPayments = 0;

            await using var tx = await _centralDB.Database.BeginTransactionAsync(ct);
            try
            {
                foreach (var ro in payload.RepairOrders)
                {
                    var entity = await _centralDB.RepairOrders
                        .FirstOrDefaultAsync(x => x.RepairOrderId == ro.RepairOrderId, ct);

                    if (entity == null)
                    {
                        entity = new RepairOrder { RepairOrderId = ro.RepairOrderId };
                        _centralDB.RepairOrders.Add(entity);
                    }

                    // Update fields (UTC for RepairOrders)
                    entity.OrderNumber = ro.OrderNumber;
                    entity.Paid = ro.Paid;
                    entity.PaymentMethod = ro.PaymentMethod;
                    entity.IssueDescription = ro.IssueDescription;
                    entity.RepairStatus = ro.RepairStatus;
                    entity.ReceivedDate = ro.ReceivedDate;
                    entity.ExpectedDeliveryDate = ro.ExpectedDeliveryDate;
                    entity.StoreId = ro.StoreId;
                    entity.CreatedAt = ro.CreatedAt;
                    entity.UpdatedAt = DateTime.UtcNow;
                    entity.Delind = ro.Delind;
                    entity.CustomerId = ro.CustomerId;
                    entity.UserId = ro.UserId;
                    entity.Isfinalsubmit = ro.IsFinalSubmit;
                    entity.TotalAmount = ro.TotalAmount;
                    entity.ProductType = ro.ProductType;
                    entity.Cancelled = ro.Cancelled;
                    entity.Contactmethod = ro.Contactmethod != null ? string.Join(",", ro.Contactmethod) : null;
                    entity.Paidamount = ro.PaidAmount;
                    entity.DiscountType = ro.DiscountType;
                    entity.DiscountValue = ro.DiscountValue;
                    entity.TaxPercent = ro.TaxPercent;
                    entity.Status = ro.RepairStatus;
                    entity.OrderType = ro.OrderType;

                    importedOrders++;

                    // --- UPSERT REPAIR ORDER PARTS (Unspecified) ---
                    if (ro.Parts != null && ro.Parts.Count > 0)
                    {
                        foreach (var p in ro.Parts)
                        {
                            var existingPart = await _centralDB.RepairOrderParts
                                .FirstOrDefaultAsync(x => x.Id == p.Id, ct);

                            if (existingPart != null)
                            {
                                _centralDB.Entry(existingPart).CurrentValues.SetValues(new RepairOrderPart
                                {
                                    RepairOrderId = ro.RepairOrderId,
                                    ProductId = p.ProductId,
                                    ProductName = p.ProductName,
                                    PartDescription = p.PartDescription,
                                    DeviceType = p.DeviceType,
                                    DeviceModel = p.DeviceModel,
                                    SerialNumber = p.SerialNumber,
                                    Quantity = p.Quantity,
                                    Price = p.Price,
                                    Total = p.Total,
                                    CreatedAt = ToUnspecified(p.CreatedAt),
                                    UpdatedAt = DateTime.UtcNow,
                                    BrandName = p.BrandName,
                                    ProductType = p.ProductType,
                                    Cancelled = p.Cancelled,
                                    Tokennumber = p.TokenNumber,
                                    Subcategoryid = p.SubcategoryId
                                });
                            }
                            else
                            {
                                var newPart = new RepairOrderPart
                                {
                                    Id = p.Id == Guid.Empty ? Guid.NewGuid() : p.Id,
                                    RepairOrderId = ro.RepairOrderId,
                                    ProductId = p.ProductId,
                                    ProductName = p.ProductName,
                                    PartDescription = p.PartDescription,
                                    DeviceType = p.DeviceType,
                                    DeviceModel = p.DeviceModel,
                                    SerialNumber = p.SerialNumber,
                                    Quantity = p.Quantity,
                                    Price = p.Price,
                                    Total = p.Total,
                                    CreatedAt = ToUnspecified(p.CreatedAt),
                                    UpdatedAt = ToUnspecified(p.UpdatedAt),
                                    BrandName = p.BrandName,
                                    ProductType = p.ProductType,
                                    Cancelled = p.Cancelled,
                                    Tokennumber = p.TokenNumber,
                                    Subcategoryid = p.SubcategoryId
                                };
                                _centralDB.RepairOrderParts.Add(newPart);
                                importedParts++;
                            }
                        }
                    }

                    // --- UPSERT ORDER PAYMENTS (Unspecified) ---
                    if (ro.Payments != null && ro.Payments.Count > 0)
                    {
                        foreach (var pay in ro.Payments)
                        {
                            var existingPay = await _centralDB.OrderPayments
                                .FirstOrDefaultAsync(x => x.Paymentid == pay.PaymentId, ct);

                            if (existingPay != null)
                            {
                                _centralDB.Entry(existingPay).CurrentValues.SetValues(new OrderPayment
                                {
                                    Paymentid = pay.PaymentId,
                                    Repairorderid = pay.RepairOrderId,
                                    Amount = pay.Amount,
                                    PaymentMethod = pay.PaymentMethod,
                                    PaidAt = ToUnspecified(pay.PaidAt),
                                    CreatedAt = ToUnspecified(pay.CreatedAt),
                                    PartialPayment = pay.PartialPayment,
                                    TotalAmount = pay.TotalAmount,
                                    FullyPaid = pay.FullyPaid,
                                    Remainingamount = pay.RemainingAmount
                                });
                            }
                            else
                            {
                                var newPay = new OrderPayment
                                {
                                    Paymentid = pay.PaymentId == Guid.Empty ? Guid.NewGuid() : pay.PaymentId,
                                    Repairorderid = pay.RepairOrderId,
                                    Amount = pay.Amount,
                                    PaymentMethod = pay.PaymentMethod,
                                    PaidAt = ToUnspecified(pay.PaidAt),
                                    CreatedAt = ToUnspecified(pay.CreatedAt),
                                    PartialPayment = pay.PartialPayment,
                                    TotalAmount = pay.TotalAmount,
                                    FullyPaid = pay.FullyPaid,
                                    Remainingamount = pay.RemainingAmount
                                };
                                _centralDB.OrderPayments.Add(newPay);
                                importedPayments++;
                            }
                        }
                    }
                }

                await _centralDB.SaveChangesAsync(ct);
                await tx.CommitAsync(ct);

                return Ok(new { success = true, message = "Order batch imported." });
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to import batch.",
                    error = ex.Message,
                    inner = ex.InnerException?.Message,
                    stack = ex.StackTrace
                });
            }
        }*/

        //end











    }

}
