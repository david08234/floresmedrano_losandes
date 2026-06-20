using Microsoft.EntityFrameworkCore;
using ejemplo1.Data;

// Solución para las fechas en PostgreSQL
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// ESTA ES LA LÍNEA QUE FALTABA
var builder = WebApplication.CreateBuilder(args);

// --- CONFIGURACIÓN de MONEDA ---
var cultureInfo = new System.Globalization.CultureInfo("es-BO");
cultureInfo.NumberFormat.CurrencySymbol = "Bs";

System.Globalization.CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;

// Se mantiene el soporte para tus vistas web (.cshtml)
builder.Services.AddControllersWithViews();

// Habilita el soporte para que el sistema funcione como API devolviendo datos
builder.Services.AddControllers();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Mapeo tradicional para que arranque en la pantalla de Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

// Mapeo para los endpoints de la API (ej. /api/productos)
app.MapControllers();

app.Run();