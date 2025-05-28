using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VoucherController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<VoucherController> _logger;

        public VoucherController(AppDbContext context, ILogger<VoucherController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Voucher - Get all active vouchers (for users)
        [HttpGet]
        public async Task<IActionResult> GetActiveVouchers()
        {
            try
            {
                var now = DateTime.UtcNow;
                var vouchers = await _context.Vouchers
                    .Where(v => v.IsActive && 
                               (v.StartDate == null || v.StartDate.Value.Date <= now.Date) &&
                               (v.EndDate == null || v.EndDate.Value.Date >= now.Date) &&
                               (v.UsageLimit == null || v.UsedCount < v.UsageLimit))
                    .Select(v => new
                    {
                        v.VoucherId,
                        v.Code,
                        v.DiscountType,
                        v.DiscountValue,
                        v.MaxDiscount,
                        v.MinOrderValue,
                        v.StartDate,
                        v.EndDate,
                        v.UsageLimit,
                        v.UsedCount,
                        RemainingUses = v.UsageLimit.HasValue ? v.UsageLimit.Value - v.UsedCount : (int?)null
                    })
                    .ToListAsync();

                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting active vouchers");
                return StatusCode(500, new { message = "An error occurred while retrieving vouchers" });
            }
        }

        // GET: api/Voucher/admin - Get all vouchers (admin only)
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllVouchers()
        {
            try
            {
                var vouchers = await _context.Vouchers
                    .OrderByDescending(v => v.CreatedAt)
                    .ToListAsync();

                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all vouchers");
                return StatusCode(500, new { message = "An error occurred while retrieving vouchers" });
            }
        }

        // GET: api/Voucher/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVoucher(int id)
        {
            try
            {
                var voucher = await _context.Vouchers.FindAsync(id);

                if (voucher == null)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                return Ok(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting voucher {VoucherId}", id);
                return StatusCode(500, new { message = "An error occurred while retrieving the voucher" });
            }
        }

        // POST: api/Voucher - Create new voucher (admin only)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateVoucher([FromBody] CreateVoucherModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if voucher code already exists
                var existingVoucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.Code == model.Code);

                if (existingVoucher != null)
                {
                    return BadRequest(new { message = "Voucher code already exists" });
                }

                var voucher = new Voucher
                {
                    Code = model.Code.ToUpper(),
                    DiscountType = model.DiscountType,
                    DiscountValue = model.DiscountValue,
                    MaxDiscount = model.MaxDiscount,
                    MinOrderValue = model.MinOrderValue,
                    StartDate = model.StartDate,
                    EndDate = model.EndDate,
                    UsageLimit = model.UsageLimit,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Vouchers.Add(voucher);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Voucher created: {VoucherCode}", voucher.Code);
                return CreatedAtAction(nameof(GetVoucher), new { id = voucher.VoucherId }, voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voucher");
                return StatusCode(500, new { message = "An error occurred while creating the voucher" });
            }
        }

        // PUT: api/Voucher/{id} - Update voucher (admin only)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateVoucher(int id, [FromBody] UpdateVoucherModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var voucher = await _context.Vouchers.FindAsync(id);
                if (voucher == null)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                // Check if new code already exists (if code is being changed)
                if (model.Code != voucher.Code)
                {
                    var existingVoucher = await _context.Vouchers
                        .FirstOrDefaultAsync(v => v.Code == model.Code && v.VoucherId != id);

                    if (existingVoucher != null)
                    {
                        return BadRequest(new { message = "Voucher code already exists" });
                    }
                }

                voucher.Code = model.Code.ToUpper();
                voucher.DiscountType = model.DiscountType;
                voucher.DiscountValue = model.DiscountValue;
                voucher.MaxDiscount = model.MaxDiscount;
                voucher.MinOrderValue = model.MinOrderValue;
                voucher.StartDate = model.StartDate;
                voucher.EndDate = model.EndDate;
                voucher.UsageLimit = model.UsageLimit;
                voucher.IsActive = model.IsActive;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Voucher updated: {VoucherCode}", voucher.Code);
                return Ok(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating voucher {VoucherId}", id);
                return StatusCode(500, new { message = "An error occurred while updating the voucher" });
            }
        }

        // DELETE: api/Voucher/{id} - Delete voucher (admin only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            try
            {
                var voucher = await _context.Vouchers.FindAsync(id);
                if (voucher == null)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                // Soft delete - just mark as inactive
                voucher.IsActive = false;
                await _context.SaveChangesAsync();

                _logger.LogInformation("Voucher deactivated: {VoucherCode}", voucher.Code);
                return Ok(new { message = "Voucher deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting voucher {VoucherId}", id);
                return StatusCode(500, new { message = "An error occurred while deleting the voucher" });
            }
        }

        // POST: api/Voucher/validate - Validate voucher for order
        [HttpPost("validate")]
        [Authorize]
        public async Task<IActionResult> ValidateVoucher([FromBody] ValidateVoucherModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var voucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.Code == model.Code.ToUpper());

                if (voucher == null)
                {
                    return BadRequest(new { message = "Invalid voucher code" });
                }

                // Check if voucher is active
                if (!voucher.IsActive)
                {
                    return BadRequest(new { message = "Voucher is not active" });
                }

                // Check if voucher is within valid date range
                var now = DateTime.UtcNow;
                if (voucher.StartDate.HasValue && voucher.StartDate.Value.Date > now.Date)
                {
                    return BadRequest(new { message = "Voucher is not yet active" });
                }

                if (voucher.EndDate.HasValue && voucher.EndDate.Value.Date < now.Date)
                {
                    return BadRequest(new { message = "Voucher has expired" });
                }

                // Check usage limit
                if (voucher.UsageLimit.HasValue && voucher.UsedCount >= voucher.UsageLimit.Value)
                {
                    return BadRequest(new { message = "Voucher usage limit exceeded" });
                }

                // Check minimum order value
                if (voucher.MinOrderValue.HasValue && model.OrderAmount < voucher.MinOrderValue.Value)
                {
                    return BadRequest(new { message = $"Minimum order value is {voucher.MinOrderValue:C}" });
                }

                // Check if user has already used this voucher
                var userVoucher = await _context.UserVouchers
                    .FirstOrDefaultAsync(uv => uv.UserId == userId && uv.VoucherId == voucher.VoucherId && uv.UsedAt.HasValue);

                if (userVoucher != null)
                {
                    return BadRequest(new { message = "You have already used this voucher" });
                }

                // Calculate discount
                decimal discountAmount = 0;
                if (voucher.DiscountType == DiscountType.PERCENTAGE)
                {
                    discountAmount = model.OrderAmount * (voucher.DiscountValue / 100);
                    if (voucher.MaxDiscount.HasValue && discountAmount > voucher.MaxDiscount.Value)
                    {
                        discountAmount = voucher.MaxDiscount.Value;
                    }
                }
                else
                {
                    discountAmount = voucher.DiscountValue;
                }

                // Ensure discount doesn't exceed order amount
                if (discountAmount > model.OrderAmount)
                {
                    discountAmount = model.OrderAmount;
                }

                return Ok(new
                {
                    isValid = true,
                    voucher = new
                    {
                        voucher.VoucherId,
                        voucher.Code,
                        voucher.DiscountType,
                        voucher.DiscountValue
                    },
                    discountAmount = discountAmount,
                    finalAmount = model.OrderAmount - discountAmount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher {VoucherCode}", model.Code);
                return StatusCode(500, new { message = "An error occurred while validating the voucher" });
            }
        }

        // POST: api/Voucher/apply - Apply voucher to order
        [HttpPost("apply")]
        [Authorize]
        public async Task<IActionResult> ApplyVoucher([FromBody] ApplyVoucherModel model)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                // Validate voucher first
                var validateModel = new ValidateVoucherModel
                {
                    Code = model.Code,
                    OrderAmount = model.OrderAmount
                };

                var validationResult = await ValidateVoucher(validateModel);
                if (validationResult is not OkObjectResult)
                {
                    return validationResult;
                }

                var voucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.Code == model.Code.ToUpper());

                // Create user voucher record
                var userVoucher = new UserVoucher
                {
                    UserId = userId,
                    VoucherId = voucher.VoucherId,
                    UsedAt = DateTime.UtcNow,
                    OrderId = model.OrderId
                };

                _context.UserVouchers.Add(userVoucher);

                // Increment voucher usage count
                voucher.UsedCount++;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Voucher {VoucherCode} applied by user {UserId} for order {OrderId}", 
                    voucher.Code, userId, model.OrderId);

                return Ok(new { message = "Voucher applied successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying voucher {VoucherCode}", model.Code);
                return StatusCode(500, new { message = "An error occurred while applying the voucher" });
            }
        }

        // GET: api/Voucher/user/history - Get user's voucher usage history
        [HttpGet("user/history")]
        [Authorize]
        public async Task<IActionResult> GetUserVoucherHistory()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var userVouchers = await _context.UserVouchers
                    .Where(uv => uv.UserId == userId)
                    .Include(uv => uv.Voucher)
                    .OrderByDescending(uv => uv.UsedAt)
                    .Select(uv => new
                    {
                        uv.UserVoucherId,
                        uv.UsedAt,
                        uv.OrderId,
                        Voucher = new
                        {
                            uv.Voucher.Code,
                            uv.Voucher.DiscountType,
                            uv.Voucher.DiscountValue
                        }
                    })
                    .ToListAsync();

                return Ok(userVouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user voucher history for user {UserId}", GetCurrentUserId());
                return StatusCode(500, new { message = "An error occurred while retrieving voucher history" });
            }
        }

        // Helper method to get current user ID
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }
    }

    // DTOs for voucher operations
    public class CreateVoucherModel
    {
        [Required(ErrorMessage = "Voucher code is required")]
        [StringLength(50, ErrorMessage = "Voucher code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Discount type is required")]
        public DiscountType DiscountType { get; set; }
        
        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Max discount must be greater than 0")]
        public decimal? MaxDiscount { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Min order value must be greater than 0")]
        public decimal? MinOrderValue { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be at least 1")]
        public int? UsageLimit { get; set; }
    }

    public class UpdateVoucherModel
    {
        [Required(ErrorMessage = "Voucher code is required")]
        [StringLength(50, ErrorMessage = "Voucher code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Discount type is required")]
        public DiscountType DiscountType { get; set; }
        
        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than 0")]
        public decimal DiscountValue { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Max discount must be greater than 0")]
        public decimal? MaxDiscount { get; set; }
        
        [Range(0.01, double.MaxValue, ErrorMessage = "Min order value must be greater than 0")]
        public decimal? MinOrderValue { get; set; }
        
        public DateTime? StartDate { get; set; }
        
        public DateTime? EndDate { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Usage limit must be at least 1")]
        public int? UsageLimit { get; set; }
        
        public bool IsActive { get; set; }
    }

    public class ValidateVoucherModel
    {
        [Required(ErrorMessage = "Voucher code is required")]
        [StringLength(50, ErrorMessage = "Voucher code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Order amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Order amount must be greater than 0")]
        public decimal OrderAmount { get; set; }
    }

    public class ApplyVoucherModel
    {
        [Required(ErrorMessage = "Voucher code is required")]
        [StringLength(50, ErrorMessage = "Voucher code cannot exceed 50 characters")]
        public string Code { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Order amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Order amount must be greater than 0")]
        public decimal OrderAmount { get; set; }
        
        public int? OrderId { get; set; }
    }
} 