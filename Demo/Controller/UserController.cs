using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Demo.Models;
using Demo.Models.ViewModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Demo.Data;

namespace Demo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserController> _logger;
        private readonly AppDbContext _context;

        public UserController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration,
            ILogger<UserController> logger,
            AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Đăng nhập người dùng và trả về JWT token
        /// </summary>
        /// <param name="model">Thông tin đăng nhập</param>
        /// <returns>JWT token và thông tin người dùng</returns>
        [HttpPost("login")]
        [AllowAnonymous]
        // public async Task<IActionResult> Login([FromBody] LoginModel model)
        // {
        //     _logger.LogInformation("Login attempt received for email: {Email}", model?.Email);

        //     if (!ModelState.IsValid)
        //     {
        //         _logger.LogWarning("Invalid login model: {@ModelState}", ModelState);
        //         return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
        //     }

        //     try
        //     {
        //         _logger.LogInformation("Login attempt for email {Email}", model.Email);

        //         // Find user by email
        //         var user = await _userManager.FindByEmailAsync(model.Email);
        //         if (user == null)
        //         {
        //             _logger.LogWarning("User with email {Email} not found", model.Email);
        //             return Unauthorized(new { message = "Invalid email or password" });
        //         }

        //         _logger.LogInformation("User found: {UserId}, EmailConfirmed: {EmailConfirmed}", user.Id, user.EmailConfirmed);

        //         // Check password
        //         var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
        //         _logger.LogInformation("Password validation result: {IsValid}", passwordValid);
                
        //         if (!passwordValid)
        //         {
        //             _logger.LogWarning("Invalid password for user {Email}", model.Email);
        //             return Unauthorized(new { message = "Invalid email or password" });
        //         }

        //         // Check if email is confirmed
        //         if (!user.EmailConfirmed)
        //         {
        //             _logger.LogWarning("User {Email} has not confirmed their email", model.Email);
        //             return Unauthorized(new { message = "Please confirm your email before logging in" });
        //         }

        //         // Generate JWT token
        //         var token = await GenerateJwtToken(user);
        //         if (string.IsNullOrEmpty(token))
        //         {
        //             _logger.LogError("Failed to generate JWT token for user {Email}", model.Email);
        //             return StatusCode(500, new { message = "Failed to generate token" });
        //         }

        //         // Get roles
        //         var roles = await _userManager.GetRolesAsync(user);
        //         _logger.LogInformation("User {Email} logged in successfully with roles: {Roles}", model.Email, roles);

        //         return Ok(new
        //         {
        //             token,
        //             user = new
        //             {
        //                 user.Id,
        //                 user.UserName,
        //                 user.Email,
        //                 roles
        //             }
        //         });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error during login for {Email}: {Message}", model.Email, ex.Message);
        //         return StatusCode(500, new { message = "An error occurred during login", details = ex.Message });
        //     }
        // }

        // /// <summary>
        // /// Đăng ký người dùng mới với vai trò mặc định là "User"
        // /// </summary>
        // /// <param name="model">Thông tin đăng ký</param>
        // /// <returns>Thông tin người dùng đã đăng ký</returns>
        // [HttpPost("register")]
        // [AllowAnonymous]
        // public async Task<IActionResult> Register([FromBody] RegisterModel model)
        // {
        //     try
        //     {
        //         if (!ModelState.IsValid)
        //         {
        //             return BadRequest(ModelState);
        //         }

        //         var existingUser = await _userManager.FindByNameAsync(model.Username);
        //         if (existingUser != null)
        //         {
        //             return BadRequest(new { message = "Username already exists" });
        //         }

        //         existingUser = await _userManager.FindByEmailAsync(model.Email);
        //         if (existingUser != null)
        //         {
        //             return BadRequest(new { message = "Email already exists" });
        //         }

        //         var user = new AppUser
        //         {
        //             UserName = model.Username,
        //             Email = model.Email,
        //             FullName = model.FullName.Trim()
        //         };

        //         var result = await _userManager.CreateAsync(user, model.Password);
        //         if (!result.Succeeded)
        //         {
        //             return BadRequest(new { message = "Failed to create user", errors = result.Errors });
        //         }

        //         await _userManager.AddToRoleAsync(user, "User");

        //         return Ok(new { message = "User registered successfully" });
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error during registration");
        //         return StatusCode(500, new { message = "An error occurred during registration" });
        //     }
        // }

        /// <summary>
        /// Tạo JWT token cho người dùng
        /// </summary>
        /// <param name="user">Người dùng cần tạo token</param>
        /// <returns>JWT token dưới dạng string</returns>
        private async Task<string> GenerateJwtToken(AppUser user)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                };

                var roles = await _userManager.GetRolesAsync(user);
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

                var jwtKey = _configuration["Jwt:Key"];
                if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 32)
                {
                    throw new InvalidOperationException("JWT Key is missing or too short (must be at least 32 characters).");
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddDays(1),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating JWT token for user {UserId}", user.Id);
                throw;
            }
        }

        [HttpGet("current")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    return NotFound("User not found");
                }

                return Ok(new
                {
                    username = user.UserName,
                    email = user.Email,
                    fullName = user.FullName ?? string.Empty,
                    phoneNumber = user.PhoneNumber,
                    address = user.Address,
                    province = user.Province,
                    district = user.District
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting current user");
                return StatusCode(500, new { message = "An error occurred while getting user information" });
            }
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileModel model)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var user = await _userManager.FindByIdAsync(userId.ToString());

                if (user == null)
                {
                    return NotFound("User not found");
                }

                user.FullName = model.FullName;
                user.PhoneNumber = model.PhoneNumber;
                user.Address = model.Address;
                user.Province = model.Province;
                user.District = model.District;

                var result = await _userManager.UpdateAsync(user);

                if (result.Succeeded)
                {
                    return Ok(new { message = "Profile updated successfully" });
                }

                return BadRequest(new { message = "Failed to update profile", errors = result.Errors });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, new { message = "An error occurred while updating profile" });
            }
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                if (userId == 0)
                    return Unauthorized();

                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound();

                var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Failed to change password: {Errors}", errors);
                    return BadRequest(new { message = "Failed to change password", errors });
                }

                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, "An error occurred while changing password");
            }
        }
    }

    public class UpdateProfileModel
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string Province { get; set; }
        public string District { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [MinLength(6)]
        public string NewPassword { get; set; }
    }
}