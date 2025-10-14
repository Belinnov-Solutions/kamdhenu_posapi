using Asp.Versioning;
using BELEPOS.DataModel;
using BELEPOS.Entity;
using BELEPOS.Helper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BELEPOS.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class SyncDataController : ControllerBase
    {

        private readonly BeleposContext _context;
        private readonly EPoshelper _eposHelper;
        private readonly IConfiguration _config;
        private readonly ILogger<OrderController> _logger;
        private readonly CentralDbContext _centralDB;


        public SyncDataController(BeleposContext context, EPoshelper ePoshelper, IConfiguration config, ILogger<OrderController> logger, CentralDbContext centralDb)
        {
            _context = context;
            _eposHelper = ePoshelper;
            _config = config;
            _logger = logger;
            _centralDB = centralDb;
        }

        [HttpGet("SyncOrders")]
        public async Task<IActionResult> SyncOrders(CancellationToken ct)
        {
            var result = await SyncOnceToCentralAsync(ct);

            await _eposHelper.WriteLogAsync(
                "Order",
                result.Orders,
                result.Success ? "Success" : "Failed",
                result.Success ? null :_eposHelper.Trunc(result.Error, 1000),
                ct,
                _logger
            );

            return result.Success ? Ok(result) : StatusCode(502, result);
        }


        private async Task<SyncResult> SyncOnceToCentralAsync(CancellationToken ct)
        {

            var batchSize = _config.GetValue<int?>("OrderSync:BatchSize") ?? 500;

            try
            {
                // Pull unsynced orders
                var batch = await _context.RepairOrders
                    .Include(r => r.RepairOrderParts)
                    .Include(r => r.OrderPayments)
                    .AsNoTracking()
                    .Where(r => r.WebUpload != true)
                    .OrderBy(r => r.CreatedAt)
                    .Take(batchSize)
                    .ToListAsync(ct);

                await _eposHelper.WriteLogAsync("Order", batch.Count, batch.Count == 0 ? "NoData" : "Pulled", null, ct, _logger);

                if (batch.Count == 0)
                {
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
                    ReceivedDate = _eposHelper.ToUtc(r.ReceivedDate),
                    ExpectedDeliveryDate = _eposHelper.ToUtc(r.ExpectedDeliveryDate),
                    StoreId = r.StoreId,
                    CreatedAt = _eposHelper.ToUtc(r.CreatedAt),
                    UpdatedAt = _eposHelper.ToUtc(r.UpdatedAt),
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
                    OrderDate = _eposHelper.ToUtc(r.OrderDate),

                    Parts = r.RepairOrderParts.Select(p => new RepairOrderPartDto
                    {
                        Id = p.Id,
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
                        CreatedAt = _eposHelper.ToUnspecified(p.CreatedAt),
                        UpdatedAt = _eposHelper.ToUnspecified(p.UpdatedAt),
                        BrandName = p.BrandName,
                        ProductType = p.ProductType,
                        Cancelled = p.Cancelled,
                        Delind = p.Delind ?? false,
                        TokenNumber = p.Tokennumber,
                        SubcategoryId = p.Subcategoryid ?? Guid.Empty,
                        OrderDate = p.OrderDate
                    }).ToList(),

                    Payments = r.OrderPayments.Select(op => new OrderPaymentDto
                    {
                        PaymentId = op.Paymentid,
                        RepairOrderId = op.Repairorderid,
                        Amount = op.Amount,
                        PaymentMethod = op.PaymentMethod,
                        PaidAt = _eposHelper.ToUnspecified(op.PaidAt),
                        CreatedAt =  _eposHelper.ToUnspecified(op.CreatedAt),
                        PartialPayment = op.PartialPayment,
                        TotalAmount = op.TotalAmount,
                        FullyPaid = op.FullyPaid,
                        RemainingAmount = op.Remainingamount,
                        OrderDate = op.OrderDate

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

                var result = await _eposHelper.OrderProductSync(payload, ct, _centralDB);
                if (result != "success")
                {
                    await _eposHelper.WriteLogAsync("Order", 0, "Failed", result, ct, _logger);
                    return new SyncResult
                    {
                        Success = false,
                        Orders = batch.Count,
                        Message = result,
                        RanAtUtc = DateTime.UtcNow
                    };

                }

                foreach (var r in batch)
                    r.WebUpload = true;
                _context.RepairOrders.UpdateRange(batch);
                await _context.SaveChangesAsync(ct);

                await _eposHelper.WriteLogAsync("Order", batch.Count, "Success", "Order Synced", ct, _logger);

                return new SyncResult
                {
                    Success = true,
                    Orders = batch.Count,
                    //Message = string.IsNullOrWhiteSpace(respText) ? "Synced." : respText,
                    Message = result,
                    RanAtUtc = DateTime.UtcNow
                };

            }
            catch (Exception ex)
            {
                //sw.Stop();
                _logger.LogError(ex, "SyncOnceToCentralAsync failed.");
                await _eposHelper.WriteLogAsync("Order", 0, "Failed", _eposHelper.Trunc($"{ex.Message} | {ex.StackTrace}", 1000), ct, _logger);
                return new SyncResult { Success = false, Error = ex.Message, RanAtUtc = DateTime.UtcNow };
            }
        }




        /*[HttpGet("SynProducts")]
        public async Task<IActionResult> SynProducts(CancellationToken ct)
        {
            var result = await _eposHelper.SyncAllProducts(_context, _centralDB, _eposHelper.Get_config(), ct);

           *//* await _eposHelper.WriteLogAsync(
                "Order",
                result.Orders,
                result.Success ? "Success" : "Failed",
                result.Success ? null : _eposHelper.Trunc(result.Error, 1000),
                ct,
                _logger
            );*//*

            return result.Success ? Ok(result) : StatusCode(502, result);
        }*/


    }



}
