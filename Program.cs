using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartRoom.Data;
using SmartRoom.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// API Documentation (Swagger / OpenAPI)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartRoom REST API",
        Version = "v1",
        Description = "RESTful API terintegrasi untuk Campus & Office Room Booking System (.NET 8). Mendukung pencegahan double-booking, alur persetujuan, dan verifikasi ketersediaan ruangan.",
        Contact = new OpenApiContact
        {
            Name = "SmartRoom Development Team",
            Email = "admin@smartroom.local"
        }
    });

    // Optional: Include XML comments if generated
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure Database Provider (PostgreSQL by default with SQLite fallback support)
var provider = builder.Configuration["DatabaseProvider"] ?? "PostgreSql";
var pgConnStr = builder.Configuration.GetConnectionString("PostgreSql");
var sqliteConnStr = builder.Configuration.GetConnectionString("Sqlite") ?? "Data Source=smartroom.db";

builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
    {
        options.UseNpgsql(pgConnStr);
    }
    else
    {
        options.UseSqlite(sqliteConnStr);
    }
});

// Register Application Services (Service Layer Pattern)
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IBookingService, BookingService>();

// Authentication & RBAC Configuration (Cookie Authentication)
builder.Services.AddAuthentication(Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "SmartRoom.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Enable Swagger UI middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartRoom REST API v1");
    c.RoutePrefix = "swagger";
    c.DocumentTitle = "SmartRoom API Documentation";
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Route mappings: Web API (Attribute Routing) + MVC (Conventional Routing)
app.MapControllers();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Auto-seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        logger.LogInformation("Menjalankan migrasi dan seeding database ({Provider})...", provider);
        await DbSeeder.SeedAsync(context);
        logger.LogInformation("Database berhasil diinisialisasi dan di-seed.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Gagal menginisialisasi database {Provider}: {Message}", provider, ex.Message);
        
        // Graceful fallback for local development if PostgreSQL is offline
        if (provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("PostgreSQL tidak dapat diakses. Anda dapat mengubah 'DatabaseProvider': 'Sqlite' di appsettings.json atau menyalakan PostgreSQL lokal.");
        }
    }
}

app.Run();
