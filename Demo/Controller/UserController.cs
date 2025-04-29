using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Demo.Models;
using Demo.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

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

        public UserController(
            UserManager<AppUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            IConfiguration configuration,
            ILogger<UserController> logger)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập người dùng và trả về JWT token
        /// </summary>
        /// <param name="model">Thông tin đăng nhập</param>
        /// <returns>JWT token và thông tin người dùng</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid login model: {@ModelState}", ModelState);
                return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                _logger.LogInformation("Login attempt for user {Username}", model.Username);

                // Tìm người dùng theo username
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null)
                {
                    _logger.LogWarning("User {Username} not found", model.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Kiểm tra mật khẩu
                var passwordValid = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!passwordValid)
                {
                    _logger.LogWarning("Invalid password for user {Username}", model.Username);
                    return Unauthorized(new { message = "Invalid username or password" });
                }

                // Kiểm tra email đã xác nhận chưa
                if (!user.EmailConfirmed)
                {
                    _logger.LogWarning("User {Username} has not confirmed their email", model.Username);
                    return Unauthorized(new { message = "Please confirm your email before logging in" });
                }

                // Tạo JWT token
                var token = await GenerateJwtToken(user);
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogError("Failed to generate JWT token for user {Username}", model.Username);
                    return StatusCode(500, new { message = "Failed to generate token" });
                }

                // Lấy danh sách roles
                var roles = await _userManager.GetRolesAsync(user);
                _logger.LogInformation("User {Username} logged in successfully with roles: {Roles}", model.Username, roles);

                return Ok(new
                {
                    token,
                    user = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        roles
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for user {Username}", model.Username);
                return StatusCode(500, new { message = "An error occurred during login", details = ex.Message });
            }
        }

        /// <summary>
        /// Đăng ký người dùng mới với vai trò mặc định là "User"
        /// </summary>
        /// <param name="model">Thông tin đăng ký</param>
        /// <returns>Thông tin người dùng đã đăng ký</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid register model: {@ModelState}", ModelState);
                return BadRequest(new { message = "Invalid input", errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            }

            try
            {
                _logger.LogInformation("Register attempt for user {Username}", model.Username);

                // Kiểm tra username đã tồn tại chưa
                var existingUser = await _userManager.FindByNameAsync(model.Username);
                if (existingUser != null)
                {
                    _logger.LogWarning("Username {Username} already exists", model.Username);
                    return BadRequest(new { message = "Username already exists" });
                }

                // Kiểm tra email đã tồn tại chưa
                existingUser = await _userManager.FindByEmailAsync(model.Email);
                if (existingUser != null)
                {
                    _logger.LogWarning("Email {Email} already exists", model.Email);
                    return BadRequest(new { message = "Email already exists" });
                }

                // Tạo user mới
                var user = new AppUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    EmailConfirmed = false // Yêu cầu xác nhận email (có thể bỏ qua nếu không cần)
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Failed to create user {Username}: {Errors}", model.Username, errors);
                    return BadRequest(new { message = "Failed to register user", errors });
                }

                // Đảm bảo role "User" tồn tại
                if (!await _roleManager.RoleExistsAsync("User"))
                {
                    var roleResult = await _roleManager.CreateAsync(new IdentityRole<int>("User"));
                    if (!roleResult.Succeeded)
                    {
                        var roleErrors = roleResult.Errors.Select(e => e.Description).ToList();
                        _logger.LogError("Failed to create User role: {Errors}", roleErrors);
                        return StatusCode(500, new { message = "Failed to create User role", errors = roleErrors });
                    }
                }

                // Gán role "User" cho user mới
                var addToRoleResult = await _userManager.AddToRoleAsync(user, "User");
                if (!addToRoleResult.Succeeded)
                {
                    var roleErrors = addToRoleResult.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Failed to assign User role to {Username}: {Errors}", model.Username, roleErrors);
                    return BadRequest(new { message = "Failed to assign role to user", errors = roleErrors });
                }

                _logger.LogInformation("User {Username} registered successfully with role User", model.Username);

                return Ok(new
                {
                    message = "User registered successfully",
                    user = new
                    {
                        user.Id,
                        user.UserName,
                        user.Email,
                        roles = new[] { "User" }
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration for user {Username}", model.Username);
                return StatusCode(500, new { message = "An error occurred during registration", details = ex.Message });
            }
        }

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
    }
}