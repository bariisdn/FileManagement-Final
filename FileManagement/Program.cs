using FileManagement.Data;
using FileManagement.Hubs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Registering services
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>)); // Repository service
builder.Services.AddControllersWithViews();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger Services
builder.Services.AddEndpointsApiExplorer(); // Enables endpoint discovery
builder.Services.AddSwaggerGen(options => // Adds Swagger generator
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "Dosya Yönetim Portalı API",
        Version = "v1",
        Description = "Dosya Yönetim Portalı API Dökümantasyonu",
        
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "Eren Barış",
            Email = "bariseren@gmail.com"
        }
    });
    
});
// Authentication and Cookie setup
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Admin/AccessDenied";

    });

// Database Context
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

builder.Services.AddSignalR(); // Add SignalR service

// Building the app
var app = builder.Build();

// Configure middleware for HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();  // Strict Transport Security
}
else
{
    // Enable Swagger in Development mode
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "API Documentation v1");
        options.RoutePrefix = "api"; // Swagger UI will be at the root (http://localhost:<port>/)
    });
}


app.UseHttpsRedirection();

// Authentication & Authorization
app.UseRouting();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();


// Default route mapping
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}" // Varsayılan rota Home/Index
);
app.MapHub<UserHub>("/userHub");


app.Run();
