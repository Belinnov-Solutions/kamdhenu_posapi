
using Asp.Versioning;
using BCrypt.Net;
using BELEPOS.DataModel;
using BELEPOS.Entity; // For RefreshToken
using BELEPOS.Entity.Auth;
using BELEPOS.Helper;
using BELEPOS.Service;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BELEPOS.Controllers.v1
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class UserController : ControllerBase
    {
        private readonly BeleposContext _context;
        private readonly JwtService _jwtService;
        private readonly EPoshelper _eposHelper;

        public UserController(BeleposContext context, JwtService jwtService, EPoshelper ePoshelper)
        {
            _context = context;
            _jwtService = jwtService;
            _eposHelper = ePoshelper;
        }

        #region 🔑 Login Endpoint


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username && u.Isactive==true);
            var user = await _context.Users
                        .Include(u => u.Role) // Join role table
                        .Include(u => u.Store) // Join store table
                        .FirstOrDefaultAsync(u => u.Username == request.Username && u.Isactive == true && u.DelInd == false);

            if (user == null)
                return Unauthorized("Invalid credentials");

            bool passwordMatch;
            if (user.Passwordhash.StartsWith("$2a$") || user.Passwordhash.StartsWith("$2b$"))
                passwordMatch = BCrypt.Net.BCrypt.Verify(request.Password, user.Passwordhash);
            else
            {
                passwordMatch = (user.Passwordhash == request.Password);
                if (passwordMatch)
                {
                    user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(request.Password);
                    await _context.SaveChangesAsync();
                }
            }

            if (!passwordMatch)
                return Unauthorized("Invalid credentials");

            var token = _jwtService.GenerateTokenWithPermissions(user);

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString(),
                Expiresat = DateTime.Now.AddDays(7),
                Createdat = DateTime.Now,
                UseridUuid = user.Userid
            };
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();

            Response.Cookies.Append("access_token", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddMinutes(60) // optional helper
            });

            // You may still return refresh token (if needed), and public info
            return Ok(new
            {
                message = "Login successful",
                refreshToken = refreshToken?.Token, // optional, you can move this to cookie too
                user = new
                {
                    user.Userid,
                    user.Username,
                    user.Role.Rolename,
                    user.Isactive,
                    user.Createdon,
                    user.Storeid,
                    user.Store.Name,
                    user.Store.Address,
                    user.Store.Phone,
                    user.Store.Email
                }
            });


        }

        #endregion

        #region Logout
        //Logout Endpoint
        //[Authorize]
        [HttpGet("logout")]
        public async Task<IActionResult> Logout(Guid userId)
        {
            // ✅ Get current user ID from JWT claims
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            /*if (!Guid.TryParse(userIdClaim, out Guid userId))
                return Unauthorized("Invalid user ID");*/

            // ✅ Delete ALL refresh tokens for this user (logout from all sessions)
            var tokens = await _context.RefreshTokens
                .Where(t => t.UseridUuid == userId)
                .ToListAsync();

            if (tokens.Any())
            {
                _context.RefreshTokens.RemoveRange(tokens);
                await _context.SaveChangesAsync();
            }

            // ✅ Delete access_token cookie
            Response.Cookies.Delete("access_token");

            return Ok(new { message = "Logged out from all sessions successfully" });
        }


        #endregion


        #region 👤 Get All Users


        /*[HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers()
        {
            try
            {
                var users = await _context.Users
                    .Include(u => u.Role) // Include Role details via foreign key
                    .Select(u => new
                    {

                        u.Username,
                        u.Email,
                        u.Phone,
                        u.Description,
                        u.Pin,
                        StoreName = u.Store != null ? u.Store.Name : null,
                        u.Isactive,
                        u.Createdon,
                        Role = u.Role != null ? u.Role.Rolename : null
                    })
                    .OrderByDescending(u => u.Createdon)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Users fetched successfully.",
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch users.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }*/


        [HttpGet("GetUsers")]
        public async Task<IActionResult> GetUsers([FromQuery] Guid userId)
        {
            try
            {
                // Step 1: Get current user
                var currentUser = await _context.Users
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.Userid == userId && u.DelInd == false);

                if (currentUser == null)
                    return Unauthorized("User not found");

                // Step 2: Base query
                var query = _context.Users.Where(u => u.DelInd == false && u.Isactive == true)
                    .Include(u => u.Role)
                    .Include(u => u.Store)
                    .AsQueryable();

                // Step 3: Apply filtering via helper
                query = _eposHelper.ApplyUserAccessFilter(query, currentUser);

                // Step 4: Shape response
                var users = await query
                    .Select(u => new
                    {
                        u.Userid,
                        u.Username,
                        u.Email,
                        u.Phone,
                        u.Description,
                        u.Pin,
                        u.Storeid,
                        StoreName = u.Store != null ? u.Store.Name : null,
                        u.Isactive,
                        u.Createdon,
                        Role = u.Role != null ? u.Role.Rolename : null
                    })
                    .OrderByDescending(u => u.Createdon)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Users fetched successfully.",
                    data = users
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch users.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        #endregion



        #region
        // Upsert User Endpoint (Create or Update)
        // Created by Devansh - updated for new schema
        //[Authorize(Policy = "user.manage")]
        [HttpPost("UpsertUser")]
        public async Task<IActionResult> UpsertUser([FromBody] UserDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validate role by rolename
            var roleEntity = await _context.Roles.FirstOrDefaultAsync(r => r.Rolename == request.Role);
            if (roleEntity == null)
                return BadRequest("Invalid role specified.");

            // Check if user already exists
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Userid == request.UserId && u.DelInd == false);



            if (user != null)
            {
                // Update existing user
                if (!string.IsNullOrEmpty(request.Password))
                    user.Passwordhash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                user.Username = request.Username;
                user.Isactive = request.IsActive;
                user.Roleid = roleEntity.Roleid;
                user.Phone = request.Phone;
                user.Email = request.Email;
                user.Description = request.Description;
                user.Pin = request.Pin;
                user.Storeid = request.StoreId;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User updated successfully.", data = user.Userid });
            }
            else
            {
                // Create new user
                var newUser = new User
                {
                    Userid = Guid.NewGuid(),
                    Username = request.Username,
                    Passwordhash = string.IsNullOrWhiteSpace(request.Password) ? null : BCrypt.Net.BCrypt.HashPassword(request.Password),

                    Isactive = request.IsActive,
                    Createdon = DateTime.Now,
                    Roleid = roleEntity.Roleid,
                    Phone = request.Phone,
                    Email = request.Email,
                    Description = request.Description,
                    Pin = request.Pin,
                    Storeid = request.StoreId
                };
                if (string.IsNullOrWhiteSpace(request.Password))
                    return BadRequest("Password is required for new users.");


                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "User created successfully.", data = newUser.Userid });
            }
        }
        #endregion


        #region add customer

        [HttpPost("AddCustomer")]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerDto customerDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var customer = new Customer
                {

                    CustomerName = customerDto.FullName,
                    Phone = customerDto.Phone,
                    Email = customerDto.Email,
                    Address = customerDto.Address,
                    City = customerDto.City,
                    State = customerDto.State,
                    Country = customerDto.Country,
                    Zipcode = customerDto.Zipcode,
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Customer added successfully.",
                    data = new { data = customer }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }

        #endregion

        #region  get all customers
        [HttpGet("GetCustomers")]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var customers = await _context.Customers
                    .Select(c => new
                    {
                        c.CustomerId,
                        CustomerName = c.CustomerName,
                        Phone = c.Phone,
                        Email = c.Email,
                        Address = c.Address,
                        City = c.City,
                        State = c.State,
                        Country = c.Country,
                        Zipcode = c.Zipcode
                    })
                    .OrderBy(c => c.CustomerName)
                    .ToListAsync();

                return Ok(new
                {
                    message = "Customers fetched successfully.",
                    data = customers
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Failed to fetch customers.",
                    error = ex.InnerException?.Message ?? ex.Message
                });
            }
        }
        #endregion


    }
}


