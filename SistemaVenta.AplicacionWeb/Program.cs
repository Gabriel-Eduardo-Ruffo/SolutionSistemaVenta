using SistemaVenta.AplicacionWeb.Utilidades.Automapper;
using SistemaVenta.IOC;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

//Inyectamos todas las dependencia que tenemos en la clase de InyectarDependencias
builder.Services.InyectarDependencias(builder.Configuration);

//Inyectamos la dependencia AutoMapper y la clase donde se configuro la conversion de models a ViewModels o viceversa
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
