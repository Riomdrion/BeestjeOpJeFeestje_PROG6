using BeestjeOpJeFeestje_PROG6.data.DBcontext;
using BeestjeOpJeFeestje_PROG6.data.SeedData;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Voeg de databasecontext toe aan de dependency-injectiecontainer
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Voeg controllers met views toe
builder.Services.AddControllersWithViews();

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

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();