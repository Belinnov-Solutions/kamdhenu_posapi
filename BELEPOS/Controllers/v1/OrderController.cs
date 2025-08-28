using Asp.Versioning;
using BELEPOS.DataModel;
using BELEPOS.Entity;
using BELEPOS.Helper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Spreadsheet;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
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

        public OrderController(BeleposContext context, EPoshelper ePoshelper)
        {
            _context = context;
            _eposHelper = ePoshelper;
        }




        #region  Add parts or update 
        /*[HttpPost("SavePart")]
        public async Task<IActionResult> SavePart([FromBody] PartDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            Part part = new Part();

            try
            {
                if (dto.PartId.HasValue && dto.PartId != Guid.Empty)
                {
                    // UPDATE
                    part = await _context.Parts.FindAsync(dto.PartId.Value);
                    if (part == null)
                        return NotFound(new { message = "Part not found with given ID." });

                    part.PartName = dto.PartName;
                    part.PartNumber = dto.PartNumber;
                    part.SerialNumber = dto.SerialNumber;
                    part.Description = dto.Description;
                    part.Price = dto.Price;
                    part.Stock = dto.Stock;
                    if (dto.OpeningStockDate.HasValue)
                        part.OpeningStockDate = DateTime.SpecifyKind(dto.OpeningStockDate.Value, DateTimeKind.Unspecified);

                    part.Location = dto.Location;
                    part.InStock = dto.InStock;
                    part.StoreId = dto.StoreId;
                    part.UpdatedAt = DateTime.Now;

                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Part updated successfully", data = part });
                }
                else
                {
                    // ADD NEW
                    part = new Part
                    {
                        PartId = Guid.NewGuid(),
                        PartName = dto.PartName,
                        PartNumber = dto.PartNumber,
                        SerialNumber = dto.SerialNumber,
                        Description = dto.Description,
                        Price = dto.Price,
                        Stock = dto.Stock,
                        OpeningStockDate = dto.OpeningStockDate.HasValue
                         ? DateTime.SpecifyKind(dto.OpeningStockDate.Value, DateTimeKind.Unspecified)
                         : null,
                        Location = dto.Location,
                        InStock = dto.InStock ?? true,
                        StoreId = dto.StoreId,
                        UpdatedAt = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified),
                        CreatedAt = DateTime.Now,


                        Delind = false
                    };

                    _context.Parts.Add(part);
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Part added successfully", data = part });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    message = "Failed to add parts.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }


        }*/

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


        #endregion






//        [HttpPost("ConfirmOrder")]
//        public async Task<IActionResult> ConfirmOrder([FromBody] RepairOrderDto request)
//        {

//            var storeExists = await _context.Stores
//              .AnyAsync(s => s.Id == request.StoreId && s.DelInd == false && s.IsActive == true);

//            if (!storeExists)
//                return BadRequest("Store not found or inactive");

//            var repairOrder = new RepairOrder();

//            using var transaction = await _context.Database.BeginTransactionAsync();
//            try
//            {


//                var isUpdate = request.RepairOrderId != Guid.Empty;
//                var repairOrderId = isUpdate ? request.RepairOrderId : Guid.NewGuid();
//                var ticketNo = isUpdate ? request.Tickets.TicketNo : await _eposHelper.GenerateTicketNumberAsync();
//                var orderNumber = isUpdate ? request.OrderNumber : await _eposHelper.GenerateOrderNumberAsync();

//                //decimal totalAmount = 0;
//                decimal servicePrice = 0;

//                //decimal partTotal = request.Parts?.Sum(p => p.Price * p.Quantity) ?? 0;

//                //var service = await _context.Products
//                //        .FirstOrDefaultAsync(s => s.Id == request.Tickets.TaskTypeId && s.Type == ProductType.Service.ToString());
//                //if (service != null)
//                //{
//                //    totalAmount = (decimal)(service.Price + partTotal);
//                //}
//                //else
//                //{
//                //    totalAmount = partTotal;
//                //}
//                decimal totalAmount = await _eposHelper.CalculateTotalAmountAsync(request);
//                decimal paidAmount = request.PaidAmount ?? 0;


//                if (isUpdate)
//                {
//                    var existingOrder = await _context.RepairOrders.FirstOrDefaultAsync(o => o.RepairOrderId == repairOrderId);
//                    if (existingOrder == null)
//                        return NotFound("Repair order not found.");

//                    existingOrder.PaymentMethod = request.PaymentMethod;
//                    existingOrder.CustomerId = request.CustomerId;
//                    existingOrder.UserId = request.UserId;
//                    existingOrder.IssueDescription = request.IssueDescription;
//                    existingOrder.RepairStatus = request.RepairStatus ?? "Pending";
//                    existingOrder.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
//                    existingOrder.ReceivedDate = request.ReceivedDate?.ToUniversalTime();
//                    existingOrder.StoreId = request.StoreId;
//                    existingOrder.UpdatedAt = DateTime.Now.ToUniversalTime();
//                    existingOrder.Isfinalsubmit = request.IsFinalSubmit;
//                    //existingOrder.TotalAmount = existingOrder.TotalAmount + partTotal;
//                    existingOrder.TotalAmount = totalAmount;
//                    existingOrder.Contactmethod = request.Contactmethod != null
//                     ? string.Join(",", request.Contactmethod)
//                    : null;

//                    // Determine paid status
//                    existingOrder.Paid = request.RepairStatus == "Delivered";

//                    var ticket = await _context.RepairTickets.FirstOrDefaultAsync(t => t.OrderId == repairOrderId);
//                    if (ticket != null)
//                    {
//                        ticket.Storeid = request.StoreId ?? throw new Exception("StoreId is required for ticket");
//                        ticket.DeviceType = request.Tickets.DeviceType;
//                        ticket.Ipaddress = request.Tickets.IPAddress;
//                        ticket.Userid = request.UserId;
//                        ticket.Brand = request.Tickets.Brand;
//                        ticket.Model = request.Tickets.Model;
//                        ticket.ImeiNumber = request.Tickets.ImeiNumber;
//                        ticket.SerialNumber = request.Tickets.SerialNumber;
//                        ticket.Passcode = request.Tickets.Passcode;
//                        ticket.ServiceCharge = request.Tickets.ServiceCharge;
//                        ticket.Repaircost = request.Tickets.RepairCost;
//                        ticket.Technicianid = request.Tickets.TechnicianId;
//                        ticket.Duedate = request.Tickets.DueDate?.ToUniversalTime() ?? DateTime.UtcNow;
//                        ticket.Status = request.Tickets.Status ?? RepairStatus.Pending.ToString();
//                        ticket.Tasktypeid = request.Tickets.TaskTypeId;
//                        ticket.DeviceColour = request.Tickets.DeviceColour;

//                        if (request.Tickets.Notes?.Any() == true)
//                        {
//                            var existingNotes = await _context.Ticketnotes
//                                .Where(n => n.Ticketid == ticket.Ticketid).ToListAsync();

//                            foreach (var note in request.Tickets.Notes)
//                            {
//                                if (note.Id != Guid.Empty)
//                                {
//                                    var existingNote = existingNotes.FirstOrDefault(n => n.Noteid == note.Id);
//                                    if (existingNote != null)
//                                    {
//                                        existingNote.Note = note.Notes;
//                                        existingNote.Type = note.Type;
//                                    }
//                                }
//                                else
//                                {
//                                    _context.Ticketnotes.Add(new Ticketnote
//                                    {
//                                        Noteid = Guid.NewGuid(),
//                                        Note = note.Notes,
//                                        Type = note.Type,
//                                        Ticketid = ticket.Ticketid,
//                                        OrderId = repairOrderId,
//                                        Userid = request.UserId
//                                    });
//                                }
//                            }
//                        }
//                    }
//                }
//                else
//                {

//                    var partProductIds = request.Parts?.Select(p => p.ProductId).ToList() ?? new();

                    
//                    repairOrder = new RepairOrder
//                    {
//                        RepairOrderId = repairOrderId,
//                        OrderNumber = orderNumber,
//                        PaymentMethod = request.PaymentMethod,
//                        CustomerId = request.CustomerId,
//                        UserId = request.UserId,
//                        IssueDescription = request.IssueDescription,
//                        RepairStatus = RepairStatus.Pending.ToString(),
//                        Contactmethod = request.Contactmethod != null ? string.Join(",", request.Contactmethod) : null,
//                        ExpectedDeliveryDate = request.ExpectedDeliveryDate,
//                        StoreId = request.StoreId,
//                        ReceivedDate = request.ReceivedDate?.ToUniversalTime(),
//                        CreatedAt = request.CreatedAt?.ToUniversalTime(),
//                        Isfinalsubmit = request.IsFinalSubmit,
//                        ProductType = request.ProductType,
//                        Delind = false,
//                        TotalAmount = totalAmount
//                    };

//                    // Determine paid
//                    repairOrder.Paid = request.RepairStatus! == "Delivered";
//                    _context.RepairOrders.Add(repairOrder);
//                    _context.OrderPayments.Add(new OrderPayment
//                    {
//                        Paymentid = Guid.NewGuid(),
//                        Repairorderid = repairOrderId,
//                        Amount = paidAmount,
//                        PaymentMethod = request.PaymentMethod,
//                        PartialPayment = paidAmount < totalAmount,
//                        // TotalAmountAtTime = totalAmount,
//                        // FullyPaid = paidAmount >= totalAmount,
//                        //CreatedBy = request.UserId,
//                        // PaidAt = DateTime.UtcNow,
//                        //CreatedAt = DateTime.UtcNow,

//                    });



//                    if (request.Tickets != null)
//                    {
//                        if (request.ProductType != ProductType.Product.ToString())
//                        {
//                            var ticket = new RepairTicket
//                            {
//                                Ticketid = Guid.NewGuid(),
//                                OrderId = repairOrderId,
//                                Storeid = request.StoreId ?? throw new Exception("StoreId is required"),
//                                DeviceType = request.Tickets.DeviceType,
//                                Ipaddress = request.Tickets.IPAddress,
//                                Userid = request.UserId,
//                                Brand = request.Tickets.Brand,
//                                Model = request.Tickets.Model,
//                                DeviceColour = request.Tickets.DeviceColour,
//                                ImeiNumber = request.Tickets.ImeiNumber,
//                                SerialNumber = request.Tickets.SerialNumber,
//                                Passcode = request.Tickets.Passcode,
//                                ServiceCharge = request.Tickets.ServiceCharge,
//                                Repaircost = servicePrice,
//                                Technicianid = request.Tickets.TechnicianId,
//                                Duedate = request.Tickets.DueDate?.ToUniversalTime(),
//                                Status = request.Tickets.Status ?? RepairStatus.Pending.ToString(),
//                                Tasktypeid = request.Tickets.TaskTypeId,
//                                TicketNo = ticketNo
//                            };

//                            _context.RepairTickets.Add(ticket);

//                            if (request.Tickets.Notes?.Any() == true)
//                            {
//                                foreach (var note in request.Tickets.Notes)
//                                {
//                                    _context.Ticketnotes.Add(new Ticketnote
//                                    {
//                                        Noteid = Guid.NewGuid(),
//                                        Note = note.Notes,
//                                        Type = note.Type,
//                                        Ticketid = ticket.Ticketid,
//                                        OrderId = repairOrderId,
//                                        Userid = request.UserId
//                                    });
//                                }
//                            }
//                        }
//                    }

//                }

//                if (request.Parts?.Any() == true)
//                {
//                    var existingParts = await _context.RepairOrderParts
//                        .Where(p => p.RepairOrderId == repairOrderId).ToListAsync();

//                    foreach (var part in request.Parts)
//                    {
//                        var existingPart = existingParts.FirstOrDefault(p => p.ProductId == part.ProductId);

//                        if (existingPart != null)
//                        {
//                            existingPart.ProductName = part.ProductName;
//                            //existingPart.BrandName = part.BrandName;
//                            //  existingPart.PartDescription = part.PartDescription;
//                            // existingPart.DeviceType = part.DeviceType;
//                            // existingPart.DeviceModel = part.DeviceModel;
//                            //  existingPart.SerialNumber = part.SerialNumber;
//                            existingPart.Quantity = part.Quantity;
//                            existingPart.Price = part.Price;
//                            existingPart.Total = part.Price * part.Quantity;
//                            existingPart.ProductType = request.ProductType;
//                        }
//                        else
//                        {
//                            _context.RepairOrderParts.Add(new RepairOrderPart
//                            {
//                                Id = Guid.NewGuid(),
//                                RepairOrderId = repairOrderId,
//                                ProductId = part.ProductId,
//                                // BrandName = part.BrandName,
//                                ProductName = part.ProductName,
//                                // PartDescription = part.PartDescription,
//                                //  DeviceType = part.DeviceType,
//                                // DeviceModel = part.DeviceModel,
//                                // SerialNumber = part.SerialNumber,
//                                Quantity = part.Quantity,
//                                Price = part.Price,
//                                Total = part.Price * part.Quantity,
//                                ProductType = request.ProductType
//                            });
//                        }
//                    }
//                }

//                // Add payment order
//                //_context.OrderPayments.Add(new OrderPayment
//                //{
//                //    Paymentid = Guid.NewGuid(),
//                //    Repairorderid = repairOrderId,
//                //    Amount = (decimal)totalAmount,
//                //    PaymentMethod = request.PaymentMethod,
//                //    PartialPayment = request.RepairStatus == RepairStatus.Delivered.ToString() ? true : false,
//                //    //PaidAt = DateTime.Now.ToUniversalTime(),
//                //    //CreatedAt = DateTime.Now.ToUniversalTime(),


//                //});

//                await _context.SaveChangesAsync();




//                if (request.ChecklistResponses?.Responses?.Any() == true)
//                {
//                    var orderId = repairOrderId;

//                    if (orderId == Guid.Empty)
//                        return BadRequest("Checklist responses require a valid order ID.");

//                    var ticket = await _context.RepairTickets
//                        .FirstOrDefaultAsync(t => t.OrderId == orderId);

//                    if (ticket == null)
//                        return BadRequest("Checklist responses require a valid ticket. No ticket found for this order.");

//                    var ticketId = ticket.Ticketid;

//                    var checklistIds = request.ChecklistResponses.Responses
//                        .Select(r => r.ChecklistId)
//                        .ToList();

//                    var existingResponses = await _context.ChecklistResponses
//                        .Where(r => checklistIds.Contains(r.ChecklistId) && r.OrderId == orderId && r.TicketId == ticketId)
//                        .ToListAsync();

//                    foreach (var responseDto in request.ChecklistResponses.Responses)
//                    {
//                        var existing = existingResponses
//                            .FirstOrDefault(r => r.ChecklistId == responseDto.ChecklistId);

//                        if (existing != null)
//                        {
//                            existing.Value = responseDto.Value;
//                            existing.RepairInspection = responseDto.RepairInspection;
//                            existing.RespondedAt = DateTime.UtcNow;
//                        }
//                        else
//                        {
//                            _context.ChecklistResponses.Add(new ChecklistResponse
//                            {
//                                Id = Guid.NewGuid(),
//                                ChecklistId = responseDto.ChecklistId,
//                                OrderId = orderId,
//                                TicketId = ticketId,
//                                Value = responseDto.Value,
//                                RepairInspection = responseDto.RepairInspection,
//                                RespondedAt = DateTime.UtcNow
//                            });
//                        }
//                    }
//                }
//                await _context.SaveChangesAsync();// save checklist responses

//                // Fetch ticket again (in case it's newly created)
//                //var finalTicket = await _context.RepairOrders
//                //    .FirstOrDefaultAsync(t => t.OrderNumber == orderNumber);

//                //if (finalTicket != null)
//                //{
//                //    var customer = await _context.Customers
//                //        .FirstOrDefaultAsync(c => c.CustomerId == finalTicket.CustomerId);

//                //    var storeManager = await _context.Users
//                //    .Include(u => u.Role)
//                //     .FirstOrDefaultAsync(u => u.Storeid == finalTicket.StoreId && u.Role.Rolename == "Store Manager");


//                //    var emailModel = new EmailTicketViewModel
//                //    {
//                //        TicketNumber = finalTicket.OrderNumber,
//                //        Status = finalTicket.RepairStatus,
//                //        DueDate = finalTicket.ExpectedDeliveryDate ?? DateTime.UtcNow,
//                //        //RecipientName = customer?.Customername ?? "Customer"
//                //    };

//                //    if (!string.IsNullOrEmpty(customer?.Email))
//                //    {
//                //        await _eposHelper.SendRepairTicketEmailAsync(emailModel, customer.Email, finalTicket.UserId);
//                //    }

//                //    if (storeManager != null && !string.IsNullOrEmpty(storeManager.Email))
//                //    {
//                //        await _eposHelper.SendRepairTicketEmailAsync(emailModel, storeManager.Email, finalTicket.UserId);
//                //    }
//                //}

//                await transaction.CommitAsync();

//                return Ok(new
//                {
//                    message = isUpdate ? "Order updated successfully." : "Order created successfully.",
//                    repairOrderId,
//                    orderNumber,
//                    ticketNo
//                });
//            }
//            catch (Exception ex)
//            {
//                await transaction.RollbackAsync();
//                return StatusCode(500, new
//                {
//                    message = "Failed to save order",
//                    error = ex.ToString()
//                });
//            }
//        }


//#endregion


        [HttpPost("ConfirmOrder")]
        public async Task<IActionResult> ConfirmOrder([FromBody] RepairOrderDto request)
        {

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
                var orderNumber = isUpdate ? request.OrderNumber : await _eposHelper.GenerateOrderNumberAsync();

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

                await _eposHelper.SaveParts(repairOrderId, request);
                if (request.Parts?.Any() == true)
                {
                    foreach (var part in request.Parts)
                        await _eposHelper.EnsureStockAndDeductAsync(part.ProductId, part.Quantity);
                }
                //await _eposHelper.SavePayments(isUpdate, repairOrderId, request);


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


        /*#region Cancel Repair Order
        [HttpGet("CancelTicket")]
        public async Task<IActionResult> CancelRepairOrderAndTicket([FromQuery] Guid tikcetId, Guid orderId)
        {
            try
            {
                if (tikcetId == Guid.Empty || orderId == Guid.Empty)
                {
                    return BadRequest("Both ticketId and orderId are required.");
                }

                // Fetch the ticket
                var ticket = await _context.Repairtickets
                    .FirstOrDefaultAsync(t => t.Ticketid == tikcetId);

                if (ticket == null)
                {
                    return NotFound("Repair ticket not found.");
                }

                // Validate that ticket belongs to order
                if (ticket.OrderId != orderId)
                {
                    return BadRequest("The ticket does not belong to the provided order.");
                }

                // Fetch the order
                var order = await _context.Repairorders
                    .FirstOrDefaultAsync(o => o.RepairOrderId == orderId);

                if (order == null)
                {
                    return NotFound("Repair order not found.");
                }

                // Fetch parts
                var parts = await _context.Repairorderparts
                    .Where(p => p.RepairOrderId == orderId)
                    .ToListAsync();

                // Mark all as cancelled
                ticket.Cancelled = true;
                order.Cancelled = true;

                foreach (var part in parts)
                {
                    part.Cancelled = true;
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Ticket has been cancelled successfully.",
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
        #endregion*/



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


        ///////////////////////////////////////////////  Reports ////////////////////////////////////////////////////

        #region Daily Sales Report
        [HttpGet("SalesReport")]
        public async Task<IActionResult> GetSalesReport(
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] Guid? categoryId,
            [FromQuery] string groupBy = "day")
        {
            try
            {
                if (dateFrom.HasValue)
                    dateFrom = DateTime.SpecifyKind(dateFrom.Value, DateTimeKind.Utc);
                if (dateTo.HasValue)
                    dateTo = DateTime.SpecifyKind(dateTo.Value, DateTimeKind.Utc);

                var query = _context.RepairOrders
                    .Include(r => r.RepairOrderParts)
                    .Where(r => !(r.Delind ?? false) && !(r.Cancelled ?? false));

                if (dateFrom.HasValue && dateTo.HasValue)
                    query = query.Where(r => r.CreatedAt >= dateFrom && r.CreatedAt <= dateTo);

                if (categoryId.HasValue)
                {
                    query = query.Where(r => r.RepairOrderParts
                        .Any(p => _context.Products
                            .Where(pr => pr.CategoryId == categoryId.Value)
                            .Select(pr => pr.Id)
                            .Contains(p.ProductId ?? Guid.Empty)
                        ));
                }

                object reportData;

                switch (groupBy.ToLower())
                {
                    case "day":
                        reportData = await query
                            .GroupBy(r => r.CreatedAt.Value.Date)
                            .Select(g => new
                            {
                                Date = g.Key,
                                TotalSalesAmount = g.SelectMany(r => r.RepairOrderParts).Sum(p => p.Total ?? 0m),
                                ItemsSold = g.SelectMany(r => r.RepairOrderParts).Sum(p => (int?)p.Quantity ?? 0),
                                Orders = g.Count(),

                                Products = g.SelectMany(r => r.RepairOrderParts)
                                    .GroupBy(p => new { p.ProductId, p.ProductName })
                                    .Select(pg => new
                                    {
                                        pg.Key.ProductId,
                                        pg.Key.ProductName,
                                        QtySold = pg.Sum(x => (int?)x.Quantity ?? 0),
                                        TotalSalesAmount = pg.Sum(x => x.Total ?? 0m)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList()
                            })
                            .OrderBy(x => x.Date)
                            .ToListAsync();
                        break;

                    case "week":
                        var weeklyData = await query.ToListAsync();
                        reportData = weeklyData
                            .GroupBy(r =>
                            {
                                var cal = System.Globalization.CultureInfo.InvariantCulture.Calendar;
                                var week = cal.GetWeekOfYear(r.CreatedAt.Value,
                                    System.Globalization.CalendarWeekRule.FirstFourDayWeek,
                                    DayOfWeek.Monday);
                                return new { Year = r.CreatedAt.Value.Year, Week = week };
                            })
                            .Select(g => new
                            {
                                Week = g.Key.Year + "-W" + g.Key.Week,
                                TotalSalesAmount = g.SelectMany(r => r.RepairOrderParts).Sum(p => p.Total ?? 0m),
                                ItemsSold = g.SelectMany(r => r.RepairOrderParts).Sum(p => (int?)p.Quantity ?? 0),
                                Orders = g.Count(),

                                Products = g.SelectMany(r => r.RepairOrderParts)
                                    .GroupBy(p => new { p.ProductId, p.ProductName })
                                    .Select(pg => new
                                    {
                                        pg.Key.ProductId,
                                        pg.Key.ProductName,
                                        QtySold = pg.Sum(x => (int?)x.Quantity ?? 0),
                                        TotalSalesAmount = pg.Sum(x => x.Total ?? 0m)
                                    })
                                    .OrderByDescending(x => x.TotalSalesAmount)
                                    .ToList()
                            })
                            .OrderBy(x => x.Week)
                            .ToList();
                        break;

                    case "month":
                        reportData = await query
                            .GroupBy(r => new { r.CreatedAt.Value.Year, r.CreatedAt.Value.Month })
                            .Select(g => new
                            {
                                Month = g.Key.Year + "-" + g.Key.Month,
                                TotalSalesAmount = g.SelectMany(r => r.RepairOrderParts).Sum(p => p.Total ?? 0m),
                                ItemsSold = g.SelectMany(r => r.RepairOrderParts).Sum(p => (int?)p.Quantity ?? 0),
                                Orders = g.Count(),

                                Products = g.SelectMany(r => r.RepairOrderParts)
                                    .GroupBy(p => new { p.ProductId, p.ProductName })
                                    .Select(pg => new
                                    {
                                        pg.Key.ProductId,
                                        pg.Key.ProductName,
                                        QtySold = pg.Sum(x => (int?)x.Quantity ?? 0),
                                        TotalSalesAmount = pg.Sum(x => x.Total ?? 0m)
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
                    filters = new { dateFrom, dateTo, categoryId, groupBy },
                    reportData
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to fetch sales report", error = ex.Message });
            }
        }
        #endregion





    }

}