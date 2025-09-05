// using System.Security.Claims;
// using System.Text;
// using System.Text.Json.Serialization;
// using JobPortalAPI;
// using Microsoft.AspNetCore.Authentication.Cookies;
// using Microsoft.AspNetCore.Authentication.JwtBearer;
// using Microsoft.AspNetCore.Http.Features;
// using Microsoft.AspNetCore.Identity;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.IdentityModel.Tokens;
// using Microsoft.OpenApi.Models;

// var builder = WebApplication.CreateBuilder(args);

// // Add services
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();

// // DB Context
// builder.Services.AddDbContext<AppDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
// );

// // Identity
// builder.Services
//     .AddIdentity<IdentityUser, IdentityRole>(
//         options =>
// {
//     options.SignIn.RequireConfirmedAccount = false; // for dev
// }
//     )
//     .AddEntityFrameworkStores<AppDbContext>()
//     .AddDefaultTokenProviders();

//     builder.Services.ConfigureApplicationCookie(options =>
// {
//     options.LoginPath = "/api/Auth/login";  // redirect if not logged in
//     options.AccessDeniedPath = "/api/Jobs";
//     options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
//     options.SlidingExpiration = true;
//     options.Cookie.SameSite = SameSiteMode.None; // ← Allows cross-site cookies
//     options.Cookie.SecurePolicy = CookieSecurePolicy.None; // ← Requires HTTPS
// });

// builder.Services.AddControllersWithViews();

// builder.Services.Configure<IdentityOptions>(opt =>
// {
//     opt.Password.RequireDigit = false;
//     opt.Password.RequireLowercase = false;
//     opt.Password.RequireNonAlphanumeric = false;
//     opt.Password.RequireUppercase = false;
//     opt.Password.RequiredLength = 6;
// });



// // // Swagger
// // builder.Services.AddSwaggerGen(c =>
// // {
// //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "JobPortalAPI", Version = "v1" });

// //     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
// //     {
// //         Name = "Authorization",
// //         Type = SecuritySchemeType.Http,
// //         Scheme = "Bearer",
// //         BearerFormat = "JWT",
// //         In = ParameterLocation.Header,
// //         Description = "Enter JWT token as: Bearer <token>"
// //     });

// //     c.AddSecurityRequirement(new OpenApiSecurityRequirement
// //     {
// //         {
// //             new OpenApiSecurityScheme
// //             {
// //                 Reference = new OpenApiReference
// //                 {
// //                     Type = ReferenceType.SecurityScheme,
// //                     Id = "Bearer"
// //                 }
// //             },
// //             Array.Empty<string>()
// //         }
// //     });
// // });



// builder.Services.AddCors(options =>
// {
//     options.AddPolicy("AllowAll",
//         policy =>
//         {
//             policy.WithOrigins("https://abayjobportal.netlify.app","http://localhost:5173")
//             // .WithOrigins("https://abayjobportal.netlify.app", "http://localhost:5173") // ✅ Add localhost
//               .AllowAnyMethod()
//               .AllowAnyHeader()
//               .AllowCredentials();
//         });
// });

// builder.Services.AddControllers()
//     .AddJsonOptions(o =>
//     {
//         o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
//     });

//     builder.Services.Configure<FormOptions>(o =>
// {
//     // Allow reasonably sized resumes (adjust as needed)
//     o.MultipartBodyLengthLimit = 10_000_000; // 10MB
// });
// builder.Services.AddSingleton<EmailService>();
// builder.Services.AddHttpsRedirection(options =>
// {
//     options.RedirectStatusCode = StatusCodes.Status307TemporaryRedirect;
//     options.HttpsPort = 443;
// });


// // Add explicit authentication scheme
// builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
//     .AddCookie();

// var app = builder.Build();

// // Middleware
// // if (app.Environment.IsDevelopment())
// // {
//     // app.UseSwagger();
//     // app.UseSwaggerUI();
// // }
// app.UseStaticFiles();
// app.UseCors("AllowAll");
// // app.UseHttpsRedirection();
// app.UseAuthentication();
// app.UseAuthorization();
// app.MapControllers();



// using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     try
//     {
//         var context = services.GetRequiredService<AppDbContext>();
//         context.Database.Migrate(); // This applies pending migrations
//         Console.WriteLine("Database migrations applied successfully!");
//     }
//     catch (Exception ex)
//     {
//         var logger = services.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "An error occurred while migrating the database.");
//     }
    
// }using (var scope = app.Services.CreateScope())
// {
//     var services = scope.ServiceProvider;
//     try
//     {
//         var context = services.GetRequiredService<AppDbContext>();
//         context.Database.Migrate(); // This applies pending migrations
//         Console.WriteLine("Database migrations applied successfully!");
//     }
//     catch (Exception ex)
//     {
//         var logger = services.GetRequiredService<ILogger<Program>>();
//         logger.LogError(ex, "An error occurred while migrating the database.");
//     }
// }
// // Seed roles
// using (var scope = app.Services.CreateScope())
// {
//     var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
//     await SeedRolesAsync(roleManager);
// }

// static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
// {
//     string[] roles = new[] { "Admin", "HR", "Applicant" };
//     foreach (var r in roles)
//         if (!await roleManager.RoleExistsAsync(r))
//             await roleManager.CreateAsync(new IdentityRole(r));
// }

// app.Run();









using System.Text;
using System.Text.Json.Serialization;
using JobPortalAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// DB Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "C1CF4B7DC4C4175B6618DE4F55CA4AAA";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false, // Set to true if you want issuer validation
            ValidateAudience = false, // Set to true if you want audience validation
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };

        options.Events = new JwtBearerEvents
{
    OnAuthenticationFailed = context =>
    {
        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
        return Task.CompletedTask;
    },
    OnTokenValidated = context =>
    {
        Console.WriteLine("Token successfully validated");
        return Task.CompletedTask;
    }
};
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://abayjobportal.netlify.app", "http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Other services
builder.Services.AddControllers().AddJsonOptions(o =>
{
    o.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 10_000_000;
});
builder.Services.AddSingleton<EmailService>();

var app = builder.Build();

// Middleware
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Database migration and seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = new[] { "Admin", "HR", "Applicant" };
        foreach (var r in roles)
            if (!await roleManager.RoleExistsAsync(r))
                await roleManager.CreateAsync(new IdentityRole(r));
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during startup");
    }
}

app.Run();