using BELEPOS.DataModel;
using BELEPOS.Entity;
using DocumentFormat.OpenXml.Bibliography;
using Microsoft.EntityFrameworkCore;
using Razor.Templating.Core;
using Razor.Templating.Core;
using System.Drawing;
using System.Drawing.Printing;
using System.Drawing.Printing;
using System.Globalization;
using System.Net;
using System.Net.Mail;
using System.Text;


namespace BELEPOS.Helper
{
    public class EPoshelper
    {

        private readonly BeleposContext _context;

        private const int ReceiptWidth = 42;
        private readonly IConfiguration _config;

        public EPoshelper(BeleposContext context, IConfiguration config)
        {

            _context = context;
            _config = config;
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

        /*public async Task SaveParts(Guid repairOrderId, RepairOrderDto request)
        {
            if (request.Parts?.Any() != true) return;

            var existingParts = await _context.RepairOrderParts
                .Where(p => p.RepairOrderId == repairOrderId)
                .ToListAsync();

            // ✅ Get today's max token
            //var utcToday = DateTime.UtcNow.Date;
            var utcToday = DateTime.Now;
            var utcTomorrow = utcToday.AddDays(1);
            //var todayTokens = "TOKEN 1";

            var todayTokens = await _context.RepairOrderParts
                .Where(p => p.CreatedAt >= utcToday && p.CreatedAt < utcTomorrow)
                .Select(p => p.Tokennumber)
                .ToListAsync();


            int maxToken = todayTokens
                .Select(t => int.TryParse(t.Replace("TOKEN-", ""), out int n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            int tokenCounter = maxToken + 1;

            // ✅ Group parts by SubcategoryId
            var groupedParts = request.Parts.GroupBy(p => p.SubcategoryId);

            foreach (var group in groupedParts)
            {
                string token = $"{tokenCounter}";

                foreach (var part in group)
                {
                    var existingPart = existingParts.FirstOrDefault(p => p.ProductId == part.ProductId);
                    if (existingPart != null)
                    {
                        existingPart.ProductName = part.ProductName;
                        existingPart.Quantity = part.Quantity;
                        existingPart.Price = part.Price;
                        existingPart.Total = part.Price * part.Quantity;
                        existingPart.ProductType = request.ProductType;
                        existingPart.Tokennumber = token;   // ✅ assign token
                        existingPart.Subcategoryid = part.SubcategoryId;
                        //existingPart.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        _context.RepairOrderParts.Add(new RepairOrderPart
                        {
                            Id = Guid.NewGuid(),
                            RepairOrderId = repairOrderId,
                            ProductId = part.ProductId,
                            ProductName = part.ProductName,
                            Quantity = part.Quantity,
                            Price = part.Price,
                            Total = part.Price * part.Quantity,
                            ProductType = request.ProductType,
                            Tokennumber = token,           // ✅ assign token
                            Subcategoryid = part.SubcategoryId,
                            // CreatedAt = DateTime.UtcNow
                        });
                    }
                }

                tokenCounter++; // ✅ new token for next subcategory
            }
        }*/



        /*public async Task SaveParts(Guid repairOrderId, RepairOrderDto request)
        {
            try
            {
                if (request.Parts?.Any() != true) return;

                var existingParts = await _context.RepairOrderParts
                    .Where(p => p.RepairOrderId == repairOrderId)
                    .ToListAsync();

                // ✅ Reset token counter daily
                var today = DateTime.Now.Date;       // Start of today in UTC
                var tomorrow = today.AddDays(1);        // Start of tomorrow

                *//*// ✅ Get all tokens created today
                var todayTokens = await _context.RepairOrderParts
                    .Where(p => p.CreatedAt >= today && p.CreatedAt < tomorrow)
                    .Select(p => p.Tokennumber)
                    .ToListAsync();

                // ✅ Parse numeric part of tokens
                int maxToken = todayTokens
                    .Select(t => int.TryParse(t.Replace("TOKEN-", ""), out int n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max();*//*


                // ✅ Get all tokens created today
                var todayTokens = await _context.RepairOrderParts
                    .Where(p => p.CreatedAt >= today && p.CreatedAt < tomorrow)
                    .Select(p => p.Tokennumber)
                    .ToListAsync();

                // ✅ Ensure list is not null
                if (todayTokens == null || !todayTokens.Any())
                {
                    todayTokens = new List<string>();
                }

                *//* // ✅ Parse numeric part of tokens
                 int maxToken = todayTokens
                     .Select(t => int.TryParse(t?.Replace("TOKEN-", ""), out int n) ? n : 0)
                     .DefaultIfEmpty(0)
                     .Max();*//*

                int maxToken = todayTokens
                    .Select(t => int.TryParse(t, out int n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                // ✅ Start from 1 if no tokens exist for today
                int tokenCounter = maxToken == 0 ? 1 : maxToken + 1;

                // ✅ Group parts by SubcategoryId
                var groupedParts = request.Parts.GroupBy(p => p.SubcategoryId);

                foreach (var group in groupedParts)
                {
                    // Use TOKEN-{number} format
                    string token = $"{tokenCounter}";

                    foreach (var part in group)
                    {
                        var existingPart = existingParts.FirstOrDefault(p => p.ProductId == part.ProductId);
                        if (existingPart != null)
                        {
                            existingPart.ProductName = part.ProductName;
                            existingPart.Quantity = part.Quantity;
                            existingPart.Price = part.Price;
                            existingPart.Total = part.Price * part.Quantity;
                            existingPart.ProductType = request.ProductType;
                            existingPart.Tokennumber = token;
                            existingPart.Subcategoryid = part.SubcategoryId;
                            existingPart.UpdatedAt = DateTime.Now.Date;
                        }
                        else
                        {
                            _context.RepairOrderParts.Add(new RepairOrderPart
                            {
                                Id = Guid.NewGuid(),
                                RepairOrderId = repairOrderId,
                                ProductId = part.ProductId,
                                ProductName = part.ProductName,
                                Quantity = part.Quantity,
                                Price = part.Price,
                                Total = part.Price * part.Quantity,
                                ProductType = request.ProductType,
                                Tokennumber = token,
                                Subcategoryid = part.SubcategoryId,
                                CreatedAt = DateTime.Now.Date
                            });
                        }
                    }

                    tokenCounter++; // ✅ Next token for next subcategory
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }*/


        public async Task SaveParts(Guid repairOrderId, RepairOrderDto request)
        {
            try
            {
                if (request.Parts?.Any() != true) return;

                var existingParts = await _context.RepairOrderParts
                    .Where(p => p.RepairOrderId == repairOrderId)
                    .ToListAsync();

                // ✅ Reset token counter daily
                var today = DateTime.Now.Date;
                var tomorrow = today.AddDays(1);

                // ✅ Get all tokens created today
                var todayTokens = await _context.RepairOrderParts
                    .Where(p => p.CreatedAt >= today && p.CreatedAt < tomorrow)
                    .Select(p => p.Tokennumber)
                    .ToListAsync();

                if (todayTokens == null || !todayTokens.Any())
                {
                    todayTokens = new List<string>();
                }

                // ✅ Parse numeric tokens
                int maxToken = todayTokens
                    .Select(t => int.TryParse(t, out int n) ? n : 0)
                    .DefaultIfEmpty(0)
                    .Max();

                int tokenCounter = maxToken == 0 ? 1 : maxToken + 1;

                // ✅ Group and order by SubcategoryId
                var groupedParts = request.Parts
                    .GroupBy(p => p.SubcategoryId)
                    .OrderBy(g => g.Key);

                foreach (var group in groupedParts)
                {
                    string token = tokenCounter.ToString();

                    foreach (var part in group)
                    {
                        var existingPart = existingParts.FirstOrDefault(p => p.ProductId == part.ProductId);
                        if (existingPart != null)
                        {
                            existingPart.ProductName = part.ProductName;
                            existingPart.Quantity = part.Quantity;
                            existingPart.Price = part.Price;
                            existingPart.Total = part.Price * part.Quantity;
                            existingPart.ProductType = request.ProductType;
                            existingPart.Tokennumber = token;
                            existingPart.Subcategoryid = part.SubcategoryId;
                            existingPart.UpdatedAt = DateTime.Now;
                        }
                        else
                        {
                            _context.RepairOrderParts.Add(new RepairOrderPart
                            {
                                Id = Guid.NewGuid(),
                                RepairOrderId = repairOrderId,
                                ProductId = part.ProductId,
                                ProductName = part.ProductName,
                                Quantity = part.Quantity,
                                Price = part.Price,
                                Total = part.Price * part.Quantity,
                                ProductType = request.ProductType,
                                Tokennumber = token,
                                Subcategoryid = part.SubcategoryId,
                                CreatedAt = DateTime.Now
                            });
                        }
                    }

                    tokenCounter++; // ✅ sequential: 1, 2, 3...
                }
            }
            catch (Exception)
            {
                throw;
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


        /////////////////////////////////PRINT///////////////////////////////////////////////////
        ///

        /*public async Task PrintReceiptAsync(Guid repairOrderId, string printerName)
        {
            var receiptData = await (
                from ro in _context.RepairOrders
                join s in _context.Stores on ro.StoreId equals s.Id
                join c in _context.Customers on ro.CustomerId equals c.CustomerId into custJoin
                from c in custJoin.DefaultIfEmpty()
                join p in _context.RepairOrderParts on ro.RepairOrderId equals p.RepairOrderId
                join pc in _context.SubCategories on p.Subcategoryid equals pc.Subcategoryid into pcJoin
                from pc in pcJoin.DefaultIfEmpty()
                where ro.RepairOrderId == repairOrderId
                select new Reciept
                {
                    OrderNumber = ro.OrderNumber,
                    TotalAmount = ro.TotalAmount,
                    TaxPercent = ro.TaxPercent,
                    DiscountType = ro.DiscountType,
                    DiscountValue = ro.DiscountValue,
                    StoreName = s.Name,
                    StoreAddress = s.Address,
                    StorePhone = s.Phone,
                    CustomerName = c != null ? c.CustomerName : "Walk-in",
                    CustomerPhone = c != null ? c.Phone : "",
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Total = p.Total,
                    Tokennumber = p.Tokennumber,
                    Subcategoryid = p.Subcategoryid,
                    CategoryName = pc != null ? pc.Name : string.Empty

                }
            ).ToListAsync();

            if (!receiptData.Any())
                return;

            // ✅ Customer bill
            PrintCustomerReceipt(receiptData, printerName);

            // ✅ Subcategory slips
            PrintSubcategorySlips(receiptData, printerName);
        }*/





        /*public async Task PrintReceiptAsync(Guid repairOrderId, string printerName)
        {
            var receiptData = await (
                from ro in _context.RepairOrders
                join s in _context.Stores on ro.StoreId equals s.Id
                join c in _context.Customers on ro.CustomerId equals c.CustomerId into custJoin
                from c in custJoin.DefaultIfEmpty()
                join p in _context.RepairOrderParts on ro.RepairOrderId equals p.RepairOrderId
                join pc in _context.SubCategories on p.Subcategoryid equals pc.Subcategoryid into pcJoin
                from pc in pcJoin.DefaultIfEmpty()
                where ro.RepairOrderId == repairOrderId
                select new Reciept
                {
                    OrderNumber = ro.OrderNumber,
                    TotalAmount = ro.TotalAmount,
                    TaxPercent = ro.TaxPercent,
                    DiscountType = ro.DiscountType,
                    DiscountValue = ro.DiscountValue,
                    StoreName = s.Name,
                    StoreAddress = s.Address,
                    StorePhone = s.Phone,
                    CustomerName = c != null ? c.CustomerName : "Walk-in",
                    CustomerPhone = c != null ? c.Phone : "",
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Total = p.Total,
                    Tokennumber = p.Tokennumber,
                    Subcategoryid = p.Subcategoryid,
                    CategoryName = pc != null ? pc.Name : string.Empty

                }
            ).ToListAsync();

            if (!receiptData.Any())
                return;

            // ✅ Customer bill
            PrintCustomerReceipt(receiptData, printerName);

            // ✅ Subcategory slips
            PrintSubcategorySlips(receiptData, printerName);
        }*/


        public async Task PrintReceiptAsync(Guid repairOrderId, string printerName, RepairOrderDto request)
        {
            var receiptData = await (
                from ro in _context.RepairOrders
                join s in _context.Stores on ro.StoreId equals s.Id
                join c in _context.Customers on ro.CustomerId equals c.CustomerId into custJoin
                from c in custJoin.DefaultIfEmpty()
                join p in _context.RepairOrderParts on ro.RepairOrderId equals p.RepairOrderId
                join pc in _context.SubCategories on p.Subcategoryid equals pc.Subcategoryid into pcJoin
                from pc in pcJoin.DefaultIfEmpty()
                where ro.RepairOrderId == repairOrderId
                select new Reciept
                {
                    OrderNumber = ro.OrderNumber,
                    TotalAmount = ro.TotalAmount,
                    TaxPercent = ro.TaxPercent,
                    DiscountType = ro.DiscountType,
                    DiscountValue = ro.DiscountValue,
                    StoreName = s.Name,
                    StoreAddress = s.Address,
                    StorePhone = s.Phone,
                    CustomerName = c != null ? c.CustomerName : "Walk-in",
                    CustomerPhone = c != null ? c.Phone : "",
                    ProductName = p.ProductName,
                    ProductType = p.ProductType,
                    Quantity = p.Quantity,
                    Price = p.Price,
                    Total = p.Total,
                    Tokennumber = p.Tokennumber,
                    Subcategoryid = p.Subcategoryid,
                    CategoryName = pc != null ? pc.Name : string.Empty

                }
            ).ToListAsync();

            if (!receiptData.Any())
                return;

            // ✅ Customer bill
            PrintCustomerReceipt(receiptData, printerName, request);

            // ✅ Subcategory slips
            PrintSubcategorySlips(receiptData, printerName, request);
        }

        private void PrintCustomerReceipt(List<Reciept> receiptData, string printerName, RepairOrderDto request)
        {

            string partialPrint = _config.GetValue<string>("AppSettings:PartialPrint");

            
            var receipt = receiptData.First();
            StringBuilder sb = new StringBuilder();

            // Centered store header with "bold" address
            if (partialPrint == "false")
            {
                AddCenteredLine(sb, receipt.StoreName.ToUpper());
                AddCenteredLine(sb, MakeBold(receipt.StoreAddress)); // Bold simulation
                AddCenteredLine(sb, "Phone: " + receipt.StorePhone);
                AddSeparator(sb);
                AddLine(sb, $"Order #: {receipt.OrderNumber}");
            }
            
            
            // Order information
            
            //AddLine(sb, $"Customer: {receipt.CustomerName}");
            AddLine(sb, $"Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
            AddSeparator(sb);

            // Column headers
            AddLine(sb, "Item".PadRight(25) + "Qty".PadRight(5) + "Total".PadLeft(8));
            AddSeparator(sb);

            // Items
            /*foreach (var item in receiptData)
            {
                string name = item.ProductName.Length > 25 ?
                    item.ProductName.Substring(0, 22) + "..." :
                    item.ProductName.PadRight(25);
                string qty = item.Quantity.ToString().PadRight(5);
                string total = item.Total?.ToString("0.00").PadLeft(8);

                AddLine(sb, name + qty + total);
            }*/


            foreach (var item in request.Parts)
            {
                string name = item.ProductName.Length > 25 ?
                    item.ProductName.Substring(0, 22) + "..." :
                    item.ProductName.PadRight(25);
                string qty = item.Quantity.ToString().PadRight(5);
                string total = item.Price?.ToString("0.00").PadLeft(8);

                AddLine(sb, name + qty + total);
            }

            AddSeparator(sb);



            /* decimal subtotal = receiptData.Sum(x => (decimal)(x.Total ?? 0));
             decimal discount = (receipt.DiscountValue ?? 0);
             decimal taxPercent = (receipt.TaxPercent ?? 0);
             decimal tax = (subtotal - discount) * taxPercent / 100;
             decimal grandTotal = subtotal - discount + tax;*/


            decimal subtotal = (decimal)(request.SubTotal ?? 0);
            decimal taxAmount = (request.TaxAmount ?? 0);
            decimal grandTotal = request.TotalAmount??0;

            // Totals
            AddLine(sb, "Subtotal:" + subtotal.ToString("0.00").PadLeft(29));
            if (partialPrint == "false")
            {
                //AddLine(sb, "Discount:" + discount.ToString("0.00").PadLeft(29));
                AddLine(sb, "Tax:" + taxAmount.ToString("0.00").PadLeft(34));
                AddLine(sb, "TOTAL:" + grandTotal.ToString("0.00").PadLeft(31));
            }
            else
            {
                decimal grandTotalWithoutTax = grandTotal - taxAmount;
                AddLine(sb, "TOTAL:" + grandTotalWithoutTax.ToString("0.00").PadLeft(32));
            }

           
            AddSeparator(sb);

            // Footer
            //AddCenteredLine(sb, "THANK YOU, VISIT AGAIN!");

            //return sb.ToString();
            RawPrint(sb.ToString(), printerName);


        }
    
        private void PrintSubcategorySlips(List<Reciept> receiptData, string printerName, RepairOrderDto request)
        {
            // 🔹 Group by SubcategoryId
            var grouped = receiptData.GroupBy(r => r.Subcategoryid);
            string partialPrint = _config.GetValue<string>("AppSettings:PartialPrint");
            

            foreach (var group in grouped)
            {
                decimal subTotal = 0m;

                var first = group.First();
                var sb = new StringBuilder();

                if (partialPrint == "false")
                {
                    sb.AppendLine($"Order #: {first.OrderNumber}");
                }

                // ✅ Token Number (from DB for each subcategory group)
                sb.AppendLine($"Token #: {first.Tokennumber}");

                // ✅ Subcategory/Counter name
                sb.AppendLine($"Counter: {first.CategoryName}");

                sb.AppendLine($"Date: {DateTime.Now:dd/MM/yyyy HH:mm}");
                sb.AppendLine(new string('-', 30));

                // Properly aligned column headers
                //sb.AppendLine("Item".PadRight(22) + "Qty".PadLeft(8));
                AddLine(sb, "Item".PadRight(25) + "Qty".PadRight(5) + "Total".PadLeft(8));
                //sb.AppendLine(new string('-', 30));
                AddSeparator(sb);

                foreach (var item in group)
                {
                    string name = item.ProductName.Length > 25 ?
                    item.ProductName.Substring(0, 22) + "..." :
                    item.ProductName.PadRight(25);
                    string qty = item.Quantity.ToString().PadRight(5);
                    string priceAmout = item.Total?.ToString("0.00").PadLeft(8);
                    var qty2 = item.Quantity;
                    var unit = Convert.ToDecimal(item.Total, CultureInfo.InvariantCulture);
                    var lineAmt = unit * qty2;
                    string amt = unit.ToString().PadLeft(8); ;
                    subTotal += lineAmt;
                    sb.AppendLine(name + qty + amt);
                }


                AddSeparator(sb);

                decimal total = subTotal;
                decimal grandTotal = total;
                
                //sb.AppendLine(new string('-', 30));
                sb.AppendLine($"{"Total",-27}{subTotal,11:0.00}");
                //sb.AppendLine("------------------------------");
                //sb.AppendLine("   --- END OF SLIP ---   ");
                sb.AppendLine("\x1D\x56\x00"); // ✅ Autocut

                RawPrint(sb.ToString(), printerName);
            }
        }

        // Helper method to format product names
        private string FormatProductName(string name, int maxLength)
        {
            if (string.IsNullOrEmpty(name))
                return new string(' ', maxLength);

            if (name.Length <= maxLength)
                return name.PadRight(maxLength);

            // Truncate with ellipsis for long names
            return name.Substring(0, maxLength - 3) + "...";
        }

        private void RawPrint(string text, string printerName)
        {
            PrintDocument pd = new PrintDocument();
            pd.PrinterSettings.PrinterName = printerName;
            pd.PrintPage += (s, e) =>
            {
                e.Graphics.DrawString(text, new Font("Consolas", 9), Brushes.Black, new PointF(10, 10));
            };
            pd.Print();
        }

        private void AddLine(StringBuilder sb, string text)
        {
            sb.AppendLine(text);
        }

        private void AddCenteredLine(StringBuilder sb, string text)
        {
            if (text.Length > ReceiptWidth)
            {
                sb.AppendLine(text);
                return;
            }

            int padding = (ReceiptWidth - text.Length) / 2;
            sb.AppendLine(new string(' ', padding) + text);
        }

        private void AddSeparator(StringBuilder sb)
        {
            sb.AppendLine(new string('-', ReceiptWidth));
        }

        private string MakeBold(string text)
        {
            // Simulate bold text by using uppercase and adding a slight underline effect
            return text.ToUpper() + "\n" + new string('~', text.Length);
        }
    }
}


