using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Demo.Data;
using Demo.Models;
using Demo.Models.ViewModel;
using Demo.Models.Momo;
using System.Security.Claims;

namespace Demo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMomoService? _momoService;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(IMomoService? momoService, AppDbContext context, ILogger<PaymentController> logger)
        {
            _momoService = momoService;
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreatePaymentUrl([FromBody] OrderInfoModel model)
        {
            try
            {
                _logger.LogInformation("Received payment request: OrderId={OrderId}, Amount={Amount}, OrderInfo={OrderInfo}", 
                    model?.OrderId, model?.Amount, model?.OrderInfo);

                if (model == null)
                {
                    _logger.LogWarning("Model is null");
                    return BadRequest(new { message = "Request body is null or invalid" });
                }

                // Validate model
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    _logger.LogWarning("Invalid model state: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { 
                        message = "Validation failed", 
                        errors = errors.ToList(),
                        modelState = ModelState.ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
                        )
                    });
                }

                _logger.LogInformation("Creating payment for order {OrderId} with amount {Amount}", model.OrderId, model.Amount);

                // Kiểm tra đơn hàng có tồn tại và thuộc về user hiện tại
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == model.OrderId && o.UserId == userId);

                if (order == null)
                {
                    return NotFound("Order not found or you don't have permission to access this order");
                }

                // Kiểm tra trạng thái đơn hàng
                if (order.Status != OrderStatus.Pending)
                {
                    return BadRequest("Order is not in pending status");
                }

                // Kiểm tra số tiền
                if (model.Amount != order.TotalAmount)
                {
                    return BadRequest("Payment amount does not match order total");
                }

                // Tạo payment URL với Momo
                if (_momoService == null)
                {
                    _logger.LogWarning("MoMo service is not available");
                    return StatusCode(503, new { message = "Payment service is temporarily unavailable" });
                }

                var response = await _momoService.CreatePaymentAsync(model);
                
                if (response.ResultCode == "0")
                {
                    _logger.LogInformation("Payment URL created successfully for order {OrderId}", model.OrderId);
                    return Ok(new { PayUrl = response.PayUrl, Message = "Payment URL created successfully" });
                }
                else
                {
                    _logger.LogWarning("Failed to create payment URL for order {OrderId}. ResultCode: {ResultCode}, Message: {Message}", 
                        model.OrderId, response.ResultCode, response.Message);
                    return BadRequest(new { Message = response.Message });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating payment URL for order {OrderId}", model.OrderId);
                return StatusCode(500, "An error occurred while creating payment URL");
            }
        }

        [HttpGet]
        public IActionResult PaymentCallBack()
        {
            try
            {
                if (_momoService == null)
                {
                    _logger.LogWarning("MoMo service is not available for callback");
                    return Redirect($"{GetFrontendUrl()}/payment/failed?message=Payment service unavailable");
                }

                var response = _momoService.PaymentExecuteAsync(HttpContext.Request.Query);
                
                _logger.LogInformation("Payment callback received for order {OrderId} with result {ResultCode}", 
                    response.OrderId, response.ResultCode);

                // Xử lý kết quả thanh toán
                if (response.ResultCode == "0")
                {
                    // Thanh toán thành công
                    _ = Task.Run(async () => await UpdateOrderPaymentStatus(response.OrderId, true, response.TransId));
                    
                    // Redirect về trang thành công
                    return Redirect($"{GetFrontendUrl()}/payment/success?orderId={response.OrderId}&transId={response.TransId}");
                }
                else
                {
                    // Thanh toán thất bại
                    _ = Task.Run(async () => await UpdateOrderPaymentStatus(response.OrderId, false, null));
                    
                    // Redirect về trang thất bại
                    return Redirect($"{GetFrontendUrl()}/payment/failed?orderId={response.OrderId}&message={response.Message}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment callback");
                return Redirect($"{GetFrontendUrl()}/payment/failed?message=Payment processing error");
            }
        }

        [HttpPost("test")]
        [Authorize]
        public IActionResult TestEndpoint([FromBody] OrderInfoModel model)
        {
            try
            {
                _logger.LogInformation("Test endpoint called with model: {@Model}", model);
                
                // Test user authentication
                var userId = GetCurrentUserId();
                _logger.LogInformation("Current user ID: {UserId}", userId);
                
                return Ok(new { 
                    message = "Test successful", 
                    model = model,
                    userId = userId,
                    isAuthenticated = User.Identity?.IsAuthenticated ?? false
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test endpoint");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("simple")]
        public IActionResult SimpleTest()
        {
            return Ok(new { message = "Simple test successful", timestamp = DateTime.UtcNow });
        }

        [HttpGet("status/{orderId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentStatus(int orderId)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized("User not authenticated");
                }

                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

                if (order == null)
                {
                    return NotFound("Order not found");
                }

                return Ok(new 
                { 
                    OrderId = order.Id,
                    Status = order.Status.ToString(),
                    TotalAmount = order.TotalAmount,
                    IsPaid = order.Status == OrderStatus.Processing || order.Status == OrderStatus.Shipped || order.Status == OrderStatus.Delivered
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment status for order {OrderId}", orderId);
                return StatusCode(500, "An error occurred while getting payment status");
            }
        }

        [HttpGet("test-momo")]
        public IActionResult TestMomoService()
        {
            try
            {
                _logger.LogInformation("Testing MoMo service instantiation");
                if (_momoService == null)
                {
                    return Ok(new { message = "MoMo service is null", timestamp = DateTime.UtcNow });
                }
                return Ok(new { message = "MoMo service is working", timestamp = DateTime.UtcNow });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing MoMo service");
                return StatusCode(500, new { message = ex.Message, stackTrace = ex.StackTrace });
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var userId) ? userId : 0;
        }

        private async Task UpdateOrderPaymentStatus(string orderId, bool isSuccess, string? transactionId)
        {
            try
            {
                if (int.TryParse(orderId, out var orderIdInt))
                {
                    var order = await _context.Orders.FindAsync(orderIdInt);
                    if (order != null && order.Status == OrderStatus.Pending)
                    {
                        if (isSuccess)
                        {
                            order.Status = OrderStatus.Processing;
                            _logger.LogInformation("Order {OrderId} payment successful. Updated status to Processing. TransactionId: {TransactionId}", 
                                orderIdInt, transactionId);
                        }
                        else
                        {
                            // Giữ nguyên status Pending để user có thể thử thanh toán lại
                            _logger.LogInformation("Order {OrderId} payment failed. Status remains Pending for retry", orderIdInt);
                        }

                        await _context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payment status for order {OrderId}", orderId);
            }
        }

        private string GetFrontendUrl()
        {
            // Có thể config trong appsettings.json
            return "http://localhost:3000"; // URL của frontend
        }
    }
} 