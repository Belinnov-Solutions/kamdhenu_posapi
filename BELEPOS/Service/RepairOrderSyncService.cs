using BELEPOS.DataModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BELEPOS.Service
{
    public class RepairOrderSyncService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public RepairOrderSyncService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<BeleposContext>();
                    var log = new SyncLog { Entity = "RepairOrders" };

                    try
                    {
                        // Fetch all paid repair orders not yet uploaded
                        var ordersToSync = await db.RepairOrders
                            .Where(o => o.RepairStatus == "Fixed" && !o.WebUpload)
                            .ToListAsync(stoppingToken);

                        foreach (var order in ordersToSync)
                        {
                            // TODO: Upload to central system
                            order.WebUpload = true;
                            db.RepairOrders.Update(order);
                        }

                        await db.SaveChangesAsync(stoppingToken);

                        log.RecordsProcessed = ordersToSync.Count;
                        log.Status = "Success";
                    }
                    catch (Exception ex)
                    {
                        log.Status = "Failed";
                        log.ErrorMessage = ex.Message;
                    }
                    finally
                    {
                        db.SyncLogs.Add(log);
                        await db.SaveChangesAsync(stoppingToken);
                    }
                }

                // Wait 5 minutes before next run
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
