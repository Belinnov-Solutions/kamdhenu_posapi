using BELEPOS.DataModel;
using Razor.Templating.Core;
using BELEPOS.Entity;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using System.Net;
using System.Net.Mail;

namespace BELEPOS.Helper
{
    public class EPoshelper
    {

        private readonly BeleposContext _context;

        public EPoshelper(BeleposContext context)
        {
            _context = context;
        }


        #region save product images
        public async Task SaveProductImage(ProductDto productDto, List<IFormFile> images)
        {

            try
            {

                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images/Product");
                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                // 🔁 Step 1: Process existing images (update or delete)
                if (productDto.ImageList != null)
                {
                    foreach (var meta in productDto.ImageList)
                    {
                        if (meta.ImageId != null && meta.ImageId > 0)
                        {
                            var existingImage = await _context.ProductImages.FirstOrDefaultAsync(x =>
                                x.Imageid == meta.ImageId && x.Productid == productDto.Id);

                            if (existingImage != null)
                            {
                                if (meta.DelInd)
                                {
                                    // 🗑️ Delete image file from folder
                                    var fullPath = Path.Combine(uploadPath, existingImage.Imagename);
                                    if (File.Exists(fullPath))
                                        File.Delete(fullPath);

                                    _context.ProductImages.Remove(existingImage);
                                }
                                else
                                {
                                    // ✏️ Update Main flag only
                                    existingImage.Main = meta.Main;
                                    _context.ProductImages.Update(existingImage);
                                }
                            }
                        }
                    }
                }

                // 📷 Step 2: Add new uploaded images (assume new images are in order)
                if (images != null && images.Any())
                {
                    var newImages = productDto.ImageList
                        .Where(m => m.ImageId == null && !m.DelInd)
                        .ToList();

                    for (int i = 0; i < newImages.Count && i < images.Count; i++)
                    {
                        var image = images[i];
                        var meta = newImages[i];

                        var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(image.FileName)}";
                        var filePath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await image.CopyToAsync(stream);
                        }

                        var imageEntity = new ProductImage
                        {
                            Productid = productDto.Id,
                            Imagename = fileName,
                            Main = meta.Main,
                            Createdat = DateTime.UtcNow,
                            Delind = false
                        };

                        _context.ProductImages.Add(imageEntity);
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {

                //throw;
            }
        }
        #endregion

        #region generate order number
        public async Task<string> GenerateOrderNumberAsync()
        {
            // Define prefix format like "ORD-250718"
            string prefix = $"ORD-{DateTime.UtcNow:yyMMdd}";

            // Define today's range in UTC
            var utcToday = DateTime.UtcNow.Date;
            var utcTomorrow = utcToday.AddDays(1);

            // Fetch today's orders
            var todayOrders = await _context.RepairOrders
                .Where(o => o.CreatedAt >= utcToday && o.CreatedAt < utcTomorrow)
                .ToListAsync();

            // Extract max suffix from today's order numbers with matching prefix
            var maxSuffix = todayOrders
                .Select(o => o.OrderNumber)
                .Where(n => !string.IsNullOrEmpty(n) && n.StartsWith(prefix))
                .Select(n =>
                {
                    // Extract numeric part after the prefix (e.g., from "ORD-25071801", get "01")
                    string suffixPart = n.Length > prefix.Length ? n.Substring(prefix.Length) : "0";
                    return int.TryParse(suffixPart, out int suffix) ? suffix : 0;
                })
                .DefaultIfEmpty(0)
                .Max();

            //Calculate next suffix and pad to 2 digits
            //int nextSuffix = maxSuffix + 1;
            //string suffixStr = nextSuffix.ToString("D3");

            //// Final result e.g. "ORD-25071801"
            //return $"{prefix}{suffixStr}";
            //}

            int suffix = maxSuffix + 1;
            string newOrderNumber;

            do
            {
                newOrderNumber = $"{prefix}{suffix.ToString("D3")}";
                suffix++;
            }
            while (await _context.RepairOrders.AnyAsync(o => o.OrderNumber == newOrderNumber));

            return newOrderNumber;
        }
        #endregion




        #region genrate Ticket Number
        public async Task<string> GenerateTicketNumberAsync()
        {
            var now = DateTime.UtcNow;
            string prefix = $"TKT-{now:yyMM}"; // e.g., "TKT-2507"

            var startOfMonth = DateTime.SpecifyKind(new DateTime(now.Year, now.Month, 1), DateTimeKind.Utc);
            var startOfNextMonth = DateTime.SpecifyKind(startOfMonth.AddMonths(1), DateTimeKind.Utc);

            // Count tickets created in the current month
            var count = await _context.RepairTickets
                .CountAsync(t => t.Createdat >= startOfMonth && t.Createdat < startOfNextMonth);

            int nextNumber = count + 1;
            string ticketNumber = $"{prefix}-{nextNumber:D3}"; // e.g., TKT-2507-001

            return ticketNumber;

        }

        internal async Task<bool> FindStore(Guid? storeId)
        {
            var storeExists = await _context.Stores
               .AnyAsync(s => s.Id == storeId && s.DelInd == false && s.IsActive == true);

            return storeExists;
        }

        #endregion


        /*public async Task SaveOrUpdateProductVariants(Guid productId, List<ProductVariantsDto> variants)
        {
            if (variants == null || !variants.Any()) return;

            var existingVariants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            // Soft delete existing variants
            foreach (var v in existingVariants)
            {
                v.Delind = true;
                v.UpdatedAt = DateTime.UtcNow;
            }

            // Add new variants
            foreach (var variant in variants)
            {
                var newVariant = new ProductVariant
                {
                    ProductId = productId,
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

        public async Task SaveOrUpdateProductVariants(Guid productId, List<ProductVariantsDto> variants)
        {
            if (variants == null || !variants.Any()) return;

            var existingVariants = await _context.ProductVariants
                .Where(v => v.ProductId == productId)
                .ToListAsync();

            // Track IDs of incoming variants
            var incomingIds = variants
                .Where(v => v.VariantId != Guid.Empty)
                .Select(v => v.VariantId)
                .ToHashSet();

            /* // Soft-delete those that are not in the incoming list
             foreach (var existing in existingVariants)
             {
                 if (!incomingIds.Contains(existing.VariantId))
                 {
                     existing.Delind = true;
                     existing.UpdatedAt = DateTime.UtcNow;
                 }
             }*/

            // Add new or update existing
            foreach (var variant in variants)
            {
                if (variant.VariantId != Guid.Empty)
                {
                    // Try to update existing
                    var existing = existingVariants.FirstOrDefault(v => v.VariantId == variant.VariantId);
                    if (existing != null)
                    {
                        existing.VariantName = variant.VariantName;
                        existing.Sku = variant.Sku;
                        existing.Price = variant.Price;
                        existing.Quantity = variant.Quantity;
                        existing.Delind = false;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                }
                else
                {
                    // Add new
                    var newVariant = new ProductVariant
                    {

                        ProductId = productId,
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
            }

            await _context.SaveChangesAsync();
        }

        #region Filter for access
        public IQueryable<User> ApplyUserAccessFilter(IQueryable<User> query, User currentUser)
        {
            var currentRoleOrder = currentUser.Role?.RoleOrder ?? int.MaxValue;

            // SuperAdmin can view all users
            if (currentRoleOrder == 1)
                return query;

            // All others can view users with lower role_order and same store
            return query
                .Where(u => u.Role.RoleOrder > currentRoleOrder)
                .Where(u => u.Storeid == currentUser.Storeid);
        }
        #endregion



        #region confirm order logic
        public async Task<bool> StoreExists(Guid? storeId)
        {
            return await _context.Stores.AnyAsync(s => s.Id == storeId && s.DelInd == false && s.IsActive == true);
        }

        public async Task<decimal> GetServicePrice(Guid? taskTypeId)
        {
            if (taskTypeId == null) return 0;
            var service = await _context.Products
                .FirstOrDefaultAsync(s => s.Id == taskTypeId && s.Type == ProductType.Service.ToString());
            return service?.Price ?? 0;
        }

        public async Task UpdateRepairOrder(Guid repairOrderId, RepairOrderDto request, string repairStatus)
        {
            var existingOrder = await _context.RepairOrders.FirstOrDefaultAsync(o => o.RepairOrderId == repairOrderId);
            if (existingOrder == null) throw new Exception("Repair order not found.");


            existingOrder.PaymentMethod = request.PaymentMethod;
            existingOrder.CustomerId = request.CustomerId;
            existingOrder.UserId = request.UserId;
            existingOrder.IssueDescription = request.IssueDescription;
            existingOrder.RepairStatus = repairStatus;
            existingOrder.ExpectedDeliveryDate = request.ExpectedDeliveryDate;
            existingOrder.ReceivedDate = request.ReceivedDate?.ToUniversalTime();
            existingOrder.StoreId = request.StoreId;
            existingOrder.UpdatedAt = DateTime.Now.ToUniversalTime();
            existingOrder.Isfinalsubmit = request.IsFinalSubmit;
            //existingOrder.TotalAmount = existingOrder.TotalAmount + partTotal;

            existingOrder.Contactmethod = request.Contactmethod != null
             ? string.Join(",", request.Contactmethod)
            : null;

            if (!string.IsNullOrEmpty(request.DiscountType))
                existingOrder.DiscountType = request.DiscountType;

            if (request.DiscountValue.HasValue)
                existingOrder.DiscountValue = request.DiscountValue.Value;

            if (request.TaxPercent.HasValue)
                existingOrder.TaxPercent = request.TaxPercent.Value;

            if (request.TotalAmount.HasValue)
                existingOrder.TotalAmount = request.TotalAmount.Value;

            if (request.PaidAmount.HasValue)
                existingOrder.Paidamount = request.PaidAmount.Value;

            // Determine paid status
            existingOrder.Paid = request.RepairStatus == "Delivered";

        }



        public async Task CreateRepairOrder(Guid repairOrderId, string orderNumber, RepairOrderDto request, string repairStatus)
        {
            var repairOrder = new RepairOrder
            {
                RepairOrderId = repairOrderId,
                OrderNumber = orderNumber,
                PaymentMethod = request.PaymentMethod,
                CustomerId = request.CustomerId,
                UserId = request.UserId,
                IssueDescription = request.IssueDescription,
                RepairStatus = repairStatus,
                Contactmethod = request.Contactmethod != null ? string.Join(",", request.Contactmethod) : null,
                ExpectedDeliveryDate = request.ExpectedDeliveryDate,
                StoreId = request.StoreId,
                ReceivedDate = request.ReceivedDate?.ToUniversalTime(),
                CreatedAt = request.CreatedAt?.ToUniversalTime(),
                Isfinalsubmit = request.IsFinalSubmit,
                ProductType = request.ProductType,
                Delind = false,
                TotalAmount = request.TotalAmount,
                DiscountType = request.DiscountType,
                DiscountValue = request.DiscountValue ?? 0,
                TaxPercent = request.TaxPercent ?? 0,
                Paid = request.RepairStatus == "Delivered",
                Paidamount = request.PaidAmount ?? 0,


            };
            _context.RepairOrders.Add(repairOrder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTicket(Guid repairOrderId, RepairOrderDto request, string status)
        {
            var ticket = await _context.RepairTickets.FirstOrDefaultAsync(t => t.OrderId == repairOrderId);
            if (ticket != null)
            {
                ticket.Storeid = request.StoreId ?? throw new Exception("StoreId is required");
                ticket.DeviceType = request.Tickets.DeviceType;
                ticket.Ipaddress = request.Tickets.IPAddress;
                ticket.Userid = request.UserId;
                ticket.Brand = request.Tickets.Brand;
                ticket.Model = request.Tickets.Model;
                ticket.ImeiNumber = request.Tickets.ImeiNumber;
                ticket.SerialNumber = request.Tickets.SerialNumber;
                ticket.Passcode = request.Tickets.Passcode;
                ticket.ServiceCharge = request.Tickets.ServiceCharge;
                ticket.Repaircost = request.Tickets.RepairCost;
                ticket.Technicianid = request.Tickets.TechnicianId;
                ticket.Duedate = request.Tickets.DueDate?.ToUniversalTime() ?? DateTime.UtcNow;
                ticket.Status = status;
                ticket.Tasktypeid = request.Tickets.TaskTypeId;
                ticket.DeviceColour = request.Tickets.DeviceColour;
                await SaveTicketNotes(ticket.Ticketid, repairOrderId, request.Tickets.Notes, request.UserId);
            }
        }

       

        public async Task CreateTicket(Guid repairOrderId, string ticketNo, RepairOrderDto request, string repairStatus)
        {
            if (request.Tickets != null && (request.ProductType != ProductType.Product.ToString() || request.ProductType == ""))
            {
                var ticket = new RepairTicket
                {
                    Ticketid = Guid.NewGuid(),
                    OrderId = repairOrderId,
                    Storeid = request.StoreId ?? throw new Exception("StoreId is required"),
                    DeviceType = request.Tickets.DeviceType,
                    Ipaddress = request.Tickets.IPAddress,
                    Userid = request.UserId,
                    Brand = request.Tickets.Brand,
                    Model = request.Tickets.Model,
                    DeviceColour = request.Tickets.DeviceColour,
                    ImeiNumber = request.Tickets.ImeiNumber,
                    SerialNumber = request.Tickets.SerialNumber,
                    Passcode = request.Tickets.Passcode,
                    ServiceCharge = request.Tickets.ServiceCharge,
                    Repaircost = request.Tickets.ServiceCharge,
                    Technicianid = request.Tickets.TechnicianId,
                    Duedate = request.Tickets.DueDate?.ToUniversalTime(),
                    Status = repairStatus,
                    Tasktypeid = request.Tickets.TaskTypeId,
                    TicketNo = ticketNo
                };
                _context.RepairTickets.Add(ticket);
                await _context.SaveChangesAsync();
                await SaveTicketNotes(ticket.Ticketid, repairOrderId, request.Tickets.Notes, request.UserId);
            }
        }


        public async Task SaveTicketNotes(Guid ticketId, Guid repairOrderId, List<TicketsNotesDto> notes, Guid? userId)
        {
            if (notes?.Any() != true) return;
            foreach (var note in notes)
            {
                _context.Ticketnotes.Add(new Ticketnote
                {
                    Noteid = Guid.NewGuid(),
                    Note = note.Notes,
                    Type = note.Type,
                    Ticketid = ticketId,
                    OrderId = repairOrderId,
                    Userid = userId ?? Guid.Empty
                });
            }
        }

        public async Task SaveParts(Guid repairOrderId, RepairOrderDto request)
        {
            if (request.Parts?.Any() != true) return;

            var existingParts = await _context.RepairOrderParts
                .Where(p => p.RepairOrderId == repairOrderId)
                .ToListAsync();

            foreach (var part in request.Parts)
            {
                var existingPart = existingParts.FirstOrDefault(p => p.ProductId == part.ProductId);
                if (existingPart != null)
                {
                    existingPart.ProductName = part.ProductName;
                   // existingPart.BrandName = part.BrandName;
                 //   existingPart.PartDescription = part.PartDescription;
                    //existingPart.DeviceType = part.DeviceType;
                    //existingPart.DeviceModel = part.DeviceModel;
                  //  existingPart.SerialNumber = part.SerialNumber;
                    existingPart.Quantity = part.Quantity;
                    existingPart.Price = part.Price;
                    existingPart.Total = part.Price * part.Quantity;
                    existingPart.ProductType = request.ProductType;
                   // existingPart.Partnumber = part.PartNumber;
                }
                else
                {
                    _context.RepairOrderParts.Add(new RepairOrderPart
                    {
                        Id = Guid.NewGuid(),
                        RepairOrderId = repairOrderId,
                        ProductId = part.ProductId,
                       // BrandName = part.BrandName,
                        ProductName = part.ProductName,
                     //   PartDescription = part.PartDescription,
                       // Partnumber = part.PartNumber,
                    //    DeviceType = part.DeviceType,
                     //   DeviceModel = part.DeviceModel,
                       // SerialNumber = part.SerialNumber,
                        Quantity = part.Quantity,
                        Price = part.Price,
                        Total = part.Price * part.Quantity,
                        ProductType = request.ProductType
                    });
                }
            }
        }

        public async Task SavePayments(Guid repairOrderId, RepairOrderDto request)
        {
            if (request.PaidAmount.HasValue)
            {
                // Get the order
                var repairOrder = await _context.RepairOrders
                    .FirstOrDefaultAsync(r => r.RepairOrderId == repairOrderId);

                if (repairOrder == null)
                    throw new Exception("Repair order not found");

                // Calculate amounts
                decimal totalAmount = repairOrder.TotalAmount ?? 0;
                decimal alreadyPaid = await _context.OrderPayments
                    .Where(p => p.Repairorderid == repairOrderId)
                    .SumAsync(p => p.Amount);

                decimal newPaid = request.PaidAmount.Value;
                decimal totalPaid = alreadyPaid + newPaid;
                decimal remaining = totalAmount - totalPaid;

                // Create payment record
                var newPayment = new OrderPayment
                {
                    Paymentid = Guid.NewGuid(),
                    Repairorderid = repairOrderId,
                    Amount = newPaid,
                    PaymentMethod = request.PaymentMethod,
                  //  FullyPaid = remaining <= 0,
                 //   Remainingamount = remaining < 0 ? 0 : remaining,
                    TotalAmount = totalAmount,
                 //   CreatedAt = request.ReceivedDate?.ToUniversalTime() ?? DateTime.UtcNow,
                   // PaidAt = request.ReceivedDate?.ToUniversalTime() ?? DateTime.UtcNow
                };

                _context.OrderPayments.Add(newPayment);
            }
        }

        public async Task SaveChecklistResponses(Guid repairOrderId, RepairOrderDto request)
        {
            if (request.ChecklistResponses?.Responses?.Any() != true) return;

            var ticket = await _context.RepairTickets.FirstOrDefaultAsync(t => t.OrderId == repairOrderId);
            if (ticket == null) throw new Exception("No ticket found for this order.");

            var checklistIds = request.ChecklistResponses.Responses.Select(r => r.ChecklistId).ToList();
            var existingResponses = await _context.ChecklistResponses
                .Where(r => checklistIds.Contains(r.ChecklistId) && r.OrderId == repairOrderId && r.TicketId == ticket.Ticketid)
                .ToListAsync();

            foreach (var responseDto in request.ChecklistResponses.Responses)
            {
                var existing = existingResponses.FirstOrDefault(r => r.ChecklistId == responseDto.ChecklistId);
                if (existing != null)
                {
                    existing.Value = responseDto.Value;
                    existing.RepairInspection = responseDto.RepairInspection;
                    existing.RespondedAt = DateTime.UtcNow;
                }
                else
                {
                    _context.ChecklistResponses.Add(new ChecklistResponse
                    {
                        Id = Guid.NewGuid(),
                        ChecklistId = responseDto.ChecklistId,
                        OrderId = repairOrderId,
                        TicketId = ticket.Ticketid,
                        Value = responseDto.Value,
                        RepairInspection = responseDto.RepairInspection,
                        RespondedAt = DateTime.UtcNow
                    });
                }
                await _context.SaveChangesAsync();
            }
        }


        #endregion

        internal decimal? CalculateProductPrice(Product p, string? discountInPercent, string? discountInAmount)
        {
            var discountedPrice = p.DiscountType == discountInPercent
                     ? p.Price - (p.Price * (p.DiscountValue ?? 0) / 100)
                     : p.DiscountType == discountInAmount
                         ? p.Price - (p.DiscountValue ?? 0)
                         : p.Price;

            discountedPrice += discountedPrice * (p.Tax ?? 0) / 100;

            return discountedPrice;





        }

        #region send repair ticket email job
        // --- enqueue Repair job (non-blocking) ---
        /*internal void SendRepairTicketEmail(Customer? customer, string ticketNo, string repairStatus, RepairOrderDto request)
        {
            _emailQueue.QueueEmail(new EmailJob
            {
                NotificationTypeCode = "TICKET_CREATED", // match DB template NotificationTypeCode
                RecipientEmail = customer?.Email ?? string.Empty, // From DB or request
                RecipientName = customer?.CustomerName ?? string.Empty,

                Placeholders = new Dictionary<string, string>
             {
                { "ticketid", ticketNo },
                    { "status", repairStatus },
                { "datecreated", (request.Tickets?.DueDate ?? DateTime.UtcNow).ToString("dd-MMM-yyyy") },
                { "customername", customer.CustomerName ?? string.Empty },
                {"companyname", "NearNerd" }
            }
            });
        }*/



        /// ////////////////////////// Email to customer if tweaks needed ///////////////////////////

        //public async Task SendRepairTicketEmail(string ticketNo, string repairStatus, RepairOrderDto request)
        //{
        //    var customer = await _context.Customers
        //        .FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId && c.Delind == false);

        //    _emailQueue.QueueEmail(new EmailJob
        //    {
        //        NotificationTypeCode = "TICKET_CREATED",
        //        RecipientEmail = customer?.Email ?? string.Empty,
        //        RecipientName = customer?.CustomerName ?? string.Empty,
        //        Placeholders = new Dictionary<string, string>
        //{
        //    { "ticketid", ticketNo },
        //    { "status", repairStatus },
        //    { "duedate", (request.Tickets?.DueDate ?? DateTime.UtcNow).ToString("dd-MMM-yyyy") },
        //    { "customername", customer?.CustomerName ?? string.Empty },
        //    { "companyname", "NearNerd" }
        //}
        //    });
        //}

        #endregion


        #region Send Email Update to store manager if stock deducted + stock reduction logic
        // In EPoshelper
        public async Task EnsureStockAndDeductAsync(Guid? productId, int quantity, bool throwIfInsufficient = false)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productId && p.Restock == true);

            if (product == null) return; // Not stock-managed

            var available = product.Stock ?? 0;

            // Your requirement: stop if stock is 0
            if (available <= 0)
                throw new InvalidOperationException($"Stock unavailable for {product.Name}.");

            // Optional: also block if not enough stock
            if (throwIfInsufficient && available < quantity)
                throw new InvalidOperationException(
                    $"Insufficient stock for {product.Name}. Requested {quantity}, available {available}.");

            // Deduct stock
            product.Stock = available - quantity;
            if (product.Stock < 0) product.Stock = 0;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            // Low-stock notification Email for future use



            //if (product.Stock < product.QuantityAlert)
            //{
            //    var storeManager = await _context.Users
            //        .Where(u => u.Storeid == product.StoreId && u.Role.Rolename == "Store Manager" && u.DelInd == false)
            //        .Select(u => new { u.Email, u.Username })
            //        .FirstOrDefaultAsync();

            //    if (storeManager != null && !string.IsNullOrEmpty(storeManager.Email))
            //    {
            //        _emailQueue.QueueEmail(new EmailJob
            //        {
            //            NotificationTypeCode = "PRODUCT_LOW_STOCK",
            //            RecipientEmail = storeManager.Email,
            //            RecipientName = storeManager.Username,
            //            Placeholders = new Dictionary<string, string>
            //    {
            //        { "productname", product.Name },
            //        { "currentstock", product.Stock.ToString() },
            //        { "alertquantity", product.QuantityAlert.ToString() },
            //        { "storeid", product.StoreId.ToString() }
            //    }
            //        });
            //    }
        }
        #endregion

        #region Status
        public string DetermineRepairStatus(RepairOrderDto request, string? currentStatus = null)
        {
            // If it's already Fixed and final submit is true → mark as Completed
            if (currentStatus == "Fixed" && request.IsFinalSubmit == true)
                return "Completed";



            // If final submit is true → mark as Fixed
            if (request.IsFinalSubmit == true)
                return "Fixed";

            // If technician assigned → mark as In Process
            if (request.Tickets?.TechnicianId != null && request.Tickets.TechnicianId != Guid.Empty)
                return "In Process";

            // Default → Open
            return "Open";
        }

        #endregion

    }








}

