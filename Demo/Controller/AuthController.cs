using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Demo.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using Demo.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Demo.Models.Services;
using System.ComponentModel.DataAnnotations;
using Demo.Data;
using Microsoft.EntityFrameworkCore;
namespace Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _dbContext;

        public AuthController(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IEmailService emailService,
            AppDbContext dbContext)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
            _dbContext = dbContext;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            AppUser? createdUser = null;
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    });
                }

                // Check if user already exists
                var existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Email already exists" }
                    });
                }

                var user = new AppUser
                {
                    UserName = model.Username                                                                               ,
                    Email = model.Email,
                    FullName = model.FullName,
                    EmailConfirmed = false
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Failed to create user: {Errors}", string.Join(", ", result.Errors.Select(e => e.Description)));
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Errors = result.Errors.Select(e => e.Description).ToArray()
                    });
                }

                createdUser = user;

                // Add user to default role
                var roleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("Failed to add user to role: {Errors}", string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                    await _userManager.DeleteAsync(user);
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Không thể thêm vai trò cho người dùng" }
                    });
                }

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to generate email confirmation token for user: {Email}", model.Email);
                    await _userManager.DeleteAsync(user);
                    return StatusCode(500, new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Không thể tạo token xác thực email" }
                    });
                }
                var confirmationLink = $"{_configuration["ClientUrl"]}/confirm-email?userId={user.Id}&token={Uri.EscapeDataString(token)}";

                try
                {
                    // Send confirmation email
                    await _emailService.SendVerificationEmail(user.Email, confirmationLink);
                    _logger.LogInformation("User created and confirmation email sent successfully: {Email}", model.Email);

                    return Ok(new AuthResponse
                    {
                        Success = true,
                        Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản."
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending confirmation email for user: {Email}", model.Email);
                    // Không xóa user trong trường hợp này vì có thể gửi lại email sau
                    return StatusCode(500, new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Đăng ký thành công nhưng không thể gửi email xác thực. Vui lòng liên hệ admin để được hỗ trợ." }
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration");
                // Nếu user đã được tạo nhưng có lỗi xảy ra, xóa user
                if (createdUser != null)
                {
                    try
                    {
                        await _userManager.DeleteAsync(createdUser);
                    }
                    catch (Exception deleteEx)
                    {
                        _logger.LogError(deleteEx, "Error deleting user after failed registration: {Email}", model.Email);
                    }
                }
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Có lỗi xảy ra trong quá trình đăng ký. Vui lòng thử lại sau." }
                });
            }
        }

        [HttpGet("verify-email-status/{userId}")]
        public async Task<IActionResult> VerifyEmailStatus(string userId)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Không tìm thấy người dùng" }
                    });
                }

                var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = isConfirmed ? "Email đã được xác thực" : "Email chưa được xác thực",
                    User = new UserInfo { EmailConfirmed = isConfirmed }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verifying email status for user {UserId}", userId);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Có lỗi xảy ra khi kiểm tra trạng thái xác thực email" }
                });
            }
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailModel model)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Email confirmation failed: User not found - {UserId}", model.UserId);
                    return NotFound(new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Không tìm thấy người dùng" }
                    });
                }

                // Check if email is already confirmed
                var isAlreadyConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                _logger.LogInformation("Current email confirmation status for user {UserId}: {IsConfirmed}", model.UserId, isAlreadyConfirmed);

                if (isAlreadyConfirmed)
                {
                    _logger.LogInformation("Email already confirmed for user {UserId}", model.UserId);
                    return Ok(new AuthResponse
                    {
                        Success = true,
                        Message = "Email đã được xác thực trước đó"
                    });
                }

                // Decode the token before using it
                var decodedToken = Uri.UnescapeDataString(model.Token);
                _logger.LogInformation("Attempting to confirm email for user {UserId}", model.UserId);
                
                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                
                if (result.Succeeded)
                {
                    try
                    {
                        // Force update email confirmation status in database
                        var updateResult = await _dbContext.Database.ExecuteSqlRawAsync(
                            "UPDATE AspNetUsers SET EmailConfirmed = 1 WHERE Id = {0}",
                            model.UserId
                        );

                        if (updateResult <= 0)
                        {
                            _logger.LogError("Failed to update email confirmation status in database for user {UserId}", model.UserId);
                            return StatusCode(500, new AuthResponse
                            {
                                Success = false,
                                Errors = new[] { "Xác thực email không thành công. Vui lòng thử lại sau." }
                            });
                        }

                        // Force a refresh of the user from the database
                        await _userManager.UpdateSecurityStampAsync(user);
                        
                        // Verify the confirmation was successful
                        var isConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                        _logger.LogInformation("Email confirmation status after update for user {UserId}: {IsConfirmed}", model.UserId, isConfirmed);

                        if (!isConfirmed)
                        {
                            _logger.LogError("Email confirmation failed to update database for user {UserId}", model.UserId);
                            return StatusCode(500, new AuthResponse
                            {
                                Success = false,
                                Errors = new[] { "Xác thực email không thành công. Vui lòng thử lại sau." }
                            });
                        }
                        
                        _logger.LogInformation("Email confirmed successfully for user {UserId}", model.UserId);
                        return Ok(new AuthResponse
                        {
                            Success = true,
                            Message = "Xác thực email thành công"
                        });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to save changes to database for user {UserId}", model.UserId);
                        return StatusCode(500, new AuthResponse
                        {
                            Success = false,
                            Errors = new[] { "Xác thực email thành công nhưng không thể cập nhật trạng thái. Vui lòng thử lại sau." }
                        });
                    }
                }

                _logger.LogWarning("Email confirmation failed for user {UserId}. Errors: {Errors}", 
                    model.UserId, 
                    string.Join(", ", result.Errors.Select(e => e.Description)));

                return BadRequest(new AuthResponse
                {
                    Success = false,
                    Errors = result.Errors.Select(e => e.Description).ToArray()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email confirmation for user {UserId}", model.UserId);
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "Có lỗi xảy ra trong quá trình xác thực email. Vui lòng thử lại sau." }
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new AuthResponse
                    {
                        Success = false,
                        Errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToArray()
                    });
                }

                var user = await _userManager.FindByEmailAsync(model.Email);

                if (user == null)
                {
                    _logger.LogWarning("Login attempt failed: User not found - {Email}", model.Email);
                    return StatusCode(403, new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Invalid email or password" }
                    });
                }

                _logger.LogInformation("User {Email} email confirmation status from database: {IsConfirmed}", 
                    model.Email, 
                    user.EmailConfirmed);

                var isEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);
                if (!isEmailConfirmed)
                {
                    return StatusCode(401, new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Vui lòng xác thực email trước khi đăng nhập" }
                    });
                }
                var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                if (!result.Succeeded)
                {
                    _logger.LogWarning("Login attempt failed: Invalid password - {Email}", model.Email);
                    return StatusCode(403, new AuthResponse
                    {
                        Success = false,
                        Errors = new[] { "Invalid email or password" }
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var token = GenerateJwtToken(user, roles);

                _logger.LogInformation("User logged in successfully: {Email}", model.Email);

                return Ok(new AuthResponse
                {
                    Success = true,
                    Token = token,
                    User = new UserInfo
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.FullName,
                        Address = user.Address,
                        City = user.City,
                        State = user.State,
                        Province = user.Province,
                        District = user.District,
                        Roles = roles.ToList(),
                        EmailConfirmed = user.EmailConfirmed

                    },
                    Message = "Login successful"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user login");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "An error occurred during login" }
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                await _signInManager.SignOutAsync();
                return Ok(new AuthResponse
                {
                    Success = true,
                    Message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user logout");
                return StatusCode(500, new AuthResponse
                {
                    Success = false,
                    Errors = new[] { "An error occurred during logout" }
                });
            }
        }

        private string GenerateJwtToken(AppUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            // Add roles to claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddDays(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return Unauthorized();

            var roles = await _userManager.GetRolesAsync(user);

            return Ok(new
            {
                user.Id,
                user.Email,
                user.UserName,
                user.FullName,
                user.Address,
                user.City,
                user.State,
                user.Province,
                user.District,
                roles // trả về roles
            });
        }
    }

    public class AuthResponse
    {
        public bool Success { get; set; }
        public string[]? Errors { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; }
        public UserInfo? User { get; set; }
    }

    public class ConfirmEmailModel
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; }
    }
} 