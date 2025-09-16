using BELEPOS.DataModel;
using BELEPOS.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BELEPOS.Service
{
    public class OrderSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly ILogger<OrderSyncWorker> _log;
        private readonly OrderSyncOptions _opts;

        public OrderSyncWorker(IServiceProvider sp, IOptions<OrderSyncOptions> opts, ILogger<OrderSyncWorker> log)
        {
            _sp = sp;
            _opts = opts.Value;
            _log = log;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromMinutes(_opts.IntervalMinutes));
            _log.LogInformation("OrderSync worker started; interval {Minutes} min", _opts.IntervalMinutes);

            // run once at startup
            await SyncOnce(stoppingToken);

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await SyncOnce(stoppingToken);
            }
        }

        private async Task SyncOnce(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var local = scope.ServiceProvider.GetRequiredService<BeleposContext>();
            var central = scope.ServiceProvider.GetRequiredService<CentralDbContext>();

            try
            {
                // === SYNC REPAIR ORDERS ===
                var repairOrders = await local.RepairOrders
                    .Include(r => r.RepairOrderParts)
                    .Include(r => r.ChecklistResponses)
                    .Where(x => !x.WebUpload)
                    .OrderBy(x => x.CreatedAt)
                    .Take(_opts.BatchSize)
                    .ToListAsync(ct);

                if (repairOrders.Count > 0)
                {
                    _log.LogInformation("Syncing {Count} repair orders...", repairOrders.Count);

                    foreach (var ro in repairOrders)
                    {
                        try
                        {
                            var exists = await central.RepairOrders
                                .AsNoTracking()
                                .AnyAsync(c => c.RepairOrderId == ro.RepairOrderId, ct);

                            if (!exists)
                            {
                                central.RepairOrders.Add(ro);

                                // RepairOrderParts
                                foreach (var part in ro.RepairOrderParts)
                                {
                                    var partExists = await central.RepairOrderParts
                                        .AsNoTracking()
                                        .AnyAsync(p => p.Id == part.Id, ct);
                                    if (!partExists) central.RepairOrderParts.Add(part);
                                }

                                // OrderPayments
                                var payments = await local.OrderPayments
                                    .Where(p => p.Repairorderid == ro.RepairOrderId)
                                    .ToListAsync(ct);

                                foreach (var pay in payments)
                                {
                                    var payExists = await central.OrderPayments
                                        .AsNoTracking()
                                        .AnyAsync(p => p.Paymentid == pay.Paymentid, ct);
                                    if (!payExists) central.OrderPayments.Add(pay);
                                }

                                await central.SaveChangesAsync(ct);
                            }

#if NET7_0_OR_GREATER
                            await local.RepairOrders
                                .Where(x => x.RepairOrderId == ro.RepairOrderId && !x.WebUpload)
                                .ExecuteUpdateAsync(s => s.SetProperty(p => p.WebUpload, true), ct);
#else
                            ro.WebUpload = true;
                            local.RepairOrders.Update(ro);
                            await local.SaveChangesAsync(ct);
#endif
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Failed syncing RepairOrder {OrderId}", ro.RepairOrderId);
                        }
                    }

                    _log.LogInformation("RepairOrder sync complete.");
                }

                // === SYNC PRODUCTS ===
                var products = await local.Products
       .Where(p => (bool)!p.WebUpload)  // only unsynced products
       .OrderBy(p => p.Id)
       .Take(_opts.BatchSize)
       .ToListAsync(ct);


                if (products.Count > 0)
                {
                    _log.LogInformation("Syncing {Count} products...", products.Count);

                    foreach (var prod in products)
                    {
                        try
                        {
                            var exists = await central.Products
                                .AsNoTracking()
                                .AnyAsync(c => c.Id == prod.Id, ct);

                            if (!exists)
                            {
                                central.Products.Add(prod);
                            }
                            prod.WebUpload = true;
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(ex, "Failed syncing Product {ProductId}", prod.Id);
                        }
                    }

                    await central.SaveChangesAsync(ct);
                    await local.SaveChangesAsync(ct);
                    _log.LogInformation("Product sync complete.");
                }
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Sync batch failed.");
            }
        }
    }
}
