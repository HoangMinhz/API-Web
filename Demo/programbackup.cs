// using Demo.Data; // Import the namespace containing your application's data-related classes
// using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database operations
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.AspNetCore.Identity; // Import JWT Bearer authentication for securing API endpoints
// var builder = WebApplication.CreateBuilder(args); // Create a builder for configuring the web application

// // 👉 Add services to the container
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))); 
// // Register the AppDbContext with dependency injection and configure it to use SQLite as the database provider.
// // The connection string is retrieved from the app's configuration (e.g., appsettings.json).
// builder.Services.AddIdentity<IdentityUser<int>, IdentityRole<int>>()// Add Identity services to the DI container
//     .AddEntityFrameworkStores<AppDbContext>() // Configure Identity to use AppDbContext for storing user and role data
//     .AddDefaultTokenProviders(); // Add default token providers for password reset, email confirmation, etc.
// builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)// // Add authentication services to the DI container using JWT Bearer scheme
//     .AddJwtBearer(options =>
//     {
//         options.TokenValidationParameters = new TokenValidationParameters 
//         {
//             ValidateIssuer = true, //Kiem tra Issuer (nguon phat hanh token)
//             ValidateAudience = true, //Kiem tra Audience (nguoi nhan token)
//             ValidateLifetime = true, //Kiem tra thoi gian ton tai cua token
//             ValidateIssuerSigningKey = true, //Kiem tra khoa ky token
//             ValidIssuer = builder.Configuration["Jwt:Issuer"],// // Lay Issuer tu appsettings.json
//             ValidAudience = builder.Configuration["Jwt:Audience"],// // Lay Audience tu appsettings.json
//             IssuerSigningKey = new SymmetricSecurityKey(
//                 System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))// // Lay Key tu appsettings.json
//         };
//     });
// builder.Services.AddControllers(); // Register API controllers for handling HTTP requests.
// builder.Services.AddEndpointsApiExplorer(); // Enable API endpoint exploration (used for Swagger).
// builder.Services.AddSwaggerGen(); // Add Swagger generation for API documentation.'
// // Add CORS policy
// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("VueAppPolicy", builder =>
//     {
//         builder.WithOrigins("http://localhost:8080")  // Vue.js dev server
//                .AllowAnyHeader()
//                .AllowAnyMethod();
//     });
// });


// var app = builder.Build(); // Build the application pipeline.

// // 👉 Configure the HTTP request pipeline
// if (app.Environment.IsDevelopment()) // Check if the app is running in the Development environment.
// {
//     app.UseSwagger(); // Enable Swagger middleware to serve the generated Swagger JSON.
//     app.UseSwaggerUI(options =>
//     {
//         options.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1"); // Define the Swagger JSON endpoint.
//         options.RoutePrefix = string.Empty; // Set Swagger UI as the root page (accessible at "/").
//     });
// }

// app.UseHttpsRedirection(); // Redirect HTTP requests to HTTPS for security.
// app.UseCors("VueAppPolicy"); // Apply the CORS policy defined earlier to allow cross-origin requests from the Vue.js app.
// app.UseAuthentication();
// app.UseAuthorization(); // Add middleware to handle authorization (e.g., checking user permissions).
// app.MapControllers(); // Map controller routes to handle incoming API requests. This is essential for the API to work.
// // Seed admin user on startup
// using (var scope = app.Services.CreateScope())
// {
//     var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser<int>>>();
//     var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();

//     // Create Admin role
//     if (!await roleManager.RoleExistsAsync("Admin"))
//     {
//         await roleManager.CreateAsync(new IdentityRole<int>("Admin"));
//     }

//     // Create admin user
//     var adminUser = await userManager.FindByNameAsync("admin");
//     if (adminUser == null)
//     {
//         adminUser = new IdentityUser<int> { UserName = "admin", Email = "admin@myshop.com" };
//         await userManager.CreateAsync(adminUser, "Admin@123");
//         await userManager.AddToRoleAsync(adminUser, "Admin");
//     }
// }
// app.Run(); // Run the application.
