using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.SeedData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Voeg de databasecontext toe aan de dependency-injectiecontainer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Voeg controllers met views toe
builder.Services.AddControllersWithViews();

// Voeg sessieondersteuning toe
builder.Services.AddDistributedMemoryCache(); // Voor sessieopslag in geheugen
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Stel sessietijd in
    options.Cookie.HttpOnly = true; // Beveiliging: alleen toegankelijk via HTTP
    options.Cookie.IsEssential = true; // Essentieel voor GDPR
});
builder.Services.AddHttpContextAccessor();

// Voeg authenticatie toe
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/User/Login"; // Pad naar de loginpagina
        options.LogoutPath = "/User/Logout"; // Pad naar de uitlogpagina
    });

var app = builder.Build();

// Seed de database met testdata
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        SeedData.Initialize(services);
    }
    catch (Exception ex)
    {
        // Log fouten hier
        Console.WriteLine($"Error seeding database: {ex.Message}");
    }
}

// Configure de HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Voor statische bestanden zoals CSS en JS
app.UseRouting();

app.UseSession(); // Activeer sessiemiddleware
app.UseAuthentication(); // Activeer authenticatie middleware
app.UseAuthorization();  // Activeer autorisatie middleware

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
