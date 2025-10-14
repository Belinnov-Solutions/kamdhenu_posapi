using System;
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
using DocumentFormat.OpenXml.InkML;


namespace BELEPOS.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class AdminController : ControllerBase
    {
        private readonly BeleposContext _context;

        public AdminController(BeleposContext context)
        {
            _context = context;
        }


        #region save warehouse
        [HttpPost("SaveWarehouse")]
        public async Task<ActionResult<WarehouseDto>> SaveWarehouse(WarehouseDto warehouseDto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var existingWarehouse = await _context.Warehouses
                    .FirstOrDefaultAsync(p => p.Id == warehouseDto.Id && p.StoreId == warehouseDto.StoreId && p.DelInd == false && p.IsActive == true);

                if (existingWarehouse != null)
                {
                    // Update existing warehouse
                    existingWarehouse.Name = warehouseDto.Name;
                    existingWarehouse.ContactPerson = warehouseDto.ContactPerson;
                    existingWarehouse.Phone = warehouseDto.Phone;
                    existingWarehouse.WorkPhone = warehouseDto.WorkPhone;
                    existingWarehouse.Email = warehouseDto.Email;
                    existingWarehouse.AddressLine1 = warehouseDto.AddressLine1;
                    existingWarehouse.AddressLine2 = warehouseDto.AddressLine2;
                    existingWarehouse.City = warehouseDto.City;
                    existingWarehouse.State = warehouseDto.State;
                    existingWarehouse.Country = warehouseDto.Country;
                    existingWarehouse.Zipcode = warehouseDto.Zipcode;
                    existingWarehouse.UpdatedAt = DateTime.Now;
                    _context.Warehouses.Update(existingWarehouse);
                    await _context.SaveChangesAsync();

                    return Ok(new
                    {
                        message = "Warehouse updated successfully.",
                        data = new { id = existingWarehouse.Id }
                    });
                }
                else
                {
                    // Add new warehouse
                    var newWareHouse = new Warehouse
                    {
                        StoreId = warehouseDto.StoreId,
                        Name = warehouseDto.Name,
                        ContactPerson = warehouseDto.ContactPerson,
                        Phone = warehouseDto.Phone,
                        WorkPhone = warehouseDto.WorkPhone,
                        Email = warehouseDto.Email,
                        AddressLine1 = warehouseDto.AddressLine1,
                        AddressLine2 = warehouseDto.AddressLine2,
                        City = warehouseDto.City,
                        State = warehouseDto.State,
                        Country = warehouseDto.Country,
                        Zipcode = warehouseDto.Zipcode,
                        CreatedAt = DateTime.UtcNow,
                    };
                    _context.Warehouses.Add(newWareHouse);
                    await _context.SaveChangesAsync();
                    return Ok(new
                    {
                        message = "Warehouses added successfully.",
                        data = new { id = newWareHouse.Id }
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

        #region Get warehouse

        [HttpGet("GetWarehousesByStore")]
        public async Task<IActionResult> GetWarehousesByStore(Guid storeId)
        {
            try
            {
                var warehouses = await _context.Warehouses
                    .Where(w => w.StoreId == storeId && w.DelInd == false && w.IsActive == true)
                    .Select(w => new WarehouseDto
                    {
                        Id = w.Id,
                        StoreId = w.StoreId,
                        Name = w.Name,
                        ContactPerson = w.ContactPerson,
                        Phone = w.Phone,
                        WorkPhone = w.WorkPhone,
                        Email = w.Email,
                        AddressLine1 = w.AddressLine1,
                        AddressLine2 = w.AddressLine2,
                        City = w.City,
                        State = w.State,
                        Country = w.Country,
                        Zipcode = w.Zipcode,
                    })
                    .ToListAsync();

                return Ok(new
                {
                    message = "Warehouses fetched successfully.",
                    data = warehouses
                });
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



        #region Store Management

        [HttpGet("GetStores")]
        public async Task<IActionResult> GetStores([FromQuery] Guid tenantId)
        {
            var stores = await _context.Stores
                .Where(s => s.TenantId == tenantId && s.DelInd == false)
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

            return Ok(new
            {
                message = "Stores fetched successfully.",
                data = stores
            });
        }
        #endregion

        #region Save Store

        // POST: api/Product/SaveStore

        [HttpPost("SaveStore")]
        //[Authorize(Roles = "SuperAdmin")] // Or your preferred policy
        public async Task<IActionResult> SaveStore(StoreDto storeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                Store? store;
                if (storeDto.Id.HasValue)
                {
                    store = await _context.Stores.FirstOrDefaultAsync(s => s.Id == storeDto.Id && s.TenantId == storeDto.TenantId && s.DelInd == false);

                    if (store == null)
                        return NotFound(new { message = "Store not found." });

                    // Update existing
                    store.Name = storeDto.Name;
                    store.Address = storeDto.Address;
                    store.Username = storeDto.Username;
                    store.Phone = storeDto.Phone;
                    store.Email = storeDto.Email;
                    store.IsActive = storeDto.IsActive;
                    _context.Stores.Update(store);
                }
                else
                {
                    // Add new store
                    store = new Store
                    {
                        Id = Guid.NewGuid(),
                        TenantId = storeDto.TenantId,
                        Name = storeDto.Name,
                        Address = storeDto.Address,
                        Username = storeDto.Username,
                        Phone = storeDto.Phone,
                        Email = storeDto.Email,
                        IsActive = storeDto.IsActive,
                        DelInd = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Stores.Add(store);
                }

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = storeDto.Id.HasValue ? "Store updated successfully." : "Store added successfully.",
                    data = new { id = store.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        #endregion

        [HttpGet("SyncLogs")]
        public async Task<IActionResult> GetSyncLogs([FromQuery] int top = 50)
        {
            var logs = await _context.SyncLogs
                .OrderByDescending(l => l.CreatedAt)
                .Take(top)
                .ToListAsync();

            return Ok(logs);
        }


        //[Authorize(Policy = "user.manage")]
        [HttpGet("AllowedRoles")]
        public async Task<IActionResult> GetAllowedRoles()
        {
            var username = User.Identity?.Name;
            if (username == null)
                return Unauthorized();

            var currentUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            if (currentUser == null || currentUser.Roleid == null)
                return BadRequest("Unable to determine current user role.");

            var currentRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.Roleid == currentUser.Roleid);

            if (currentRole == null)
                return BadRequest("Current user's role not found.");

            var currentRoleOrder = currentRole.RoleOrder;

            // Only return RoleName
            var allowedRoles = await _context.Roles
                .Where(r => r.RoleOrder > currentRoleOrder)
                .OrderBy(r => r.RoleOrder)
                    .Select(r => r.Rolename)

                .ToListAsync();


            return Ok(allowedRoles);
        }



        #region Get Store(s)

        // GET: api/Product/GetStore?tenantId={tenantId}
        [HttpGet("GetStore")]
        public async Task<IActionResult> GetStore()
        {
            try
            {
                var stores = await _context.Stores
                    .Where(s => s.DelInd == false && s.IsActive == true)
                    .OrderBy(s => s.Name)
                    .Select(s => new
                    {
                        s.Id,
                        s.TenantId,
                        s.Name,
                        s.Address,
                        s.Username,
                        s.Phone,
                        s.Email,
                    })
                    .ToListAsync();

                return Ok(stores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        #endregion



        [HttpPost("SaveReceiptSettings")]
        public async Task<IActionResult> SaveReceiptSettings([FromBody] List<ReceiptSettingDto> receiptSettings)
        {
            if (receiptSettings == null || !receiptSettings.Any())
                return BadRequest("No receipt settings provided.");

            foreach (var setting in receiptSettings)
            {
                if (string.IsNullOrWhiteSpace(setting.ReceiptName))
                    continue;

                // Find existing receipt
                var existing = await _context.PrintReceiptSettings
                    .FirstOrDefaultAsync(x => x.ReceiptName.ToLower() == setting.ReceiptName.ToLower() && !x.IsDeleted);

                if (existing != null)
                {
                    existing.IsActive = setting.IsActive;
                }
                // Skip if receipt does not exist
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Receipt settings updated successfully." });
        }




        [HttpGet("GetReceiptSetting")]
        public async Task<IActionResult> GetReceiptList(CancellationToken ct)
        {
            var receipts = await _context.PrintReceiptSettings
                .Where(x => !x.IsDeleted)
                .Select(x => new
                {
                    x.Id,
                    x.ReceiptName,
                    x.IsActive
                })
                .OrderBy(x => x.ReceiptName)
                .ToListAsync(ct);

            if (receipts == null || receipts.Count == 0)
                return NotFound("No receipts found.");
            //return Ok(message = "", receipts);
            return Ok(new { message = "Receipt Setting list feched successfully!", data = receipts });
        }



    }


}