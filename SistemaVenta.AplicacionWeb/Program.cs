using SistemaVenta.AplicacionWeb.Utilidades.Automapper;
using SistemaVenta.IOC;
using SistemaVenta.AplicacionWeb.Utilidades.Extensiones;

using DinkToPdf;
using DinkToPdf.Contracts;

using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Agregamos los servicios al container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(option => { 
                                                                                                               option.LoginPath = "/Acceso/Login";
                                                                                                               option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
                                                                                                           });

//Inyectamos todas las dependencia que tenemos en la clase de InyectarDependencias
builder.Services.InyectarDependencias(builder.Configuration);

//Inyectamos la dependencia AutoMapper y la clase donde se configuro la conversion de models a ViewModels o viceversa
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

//Uso de libreria externa para poder crear PDF
var context = new CustomAssemblyLoadContext();
context.LoadUnmanagedLibrary(Path.Combine(Directory.GetCurrentDirectory(), "Utilidades/LibreriaPDF/libwkhtmltox.dll"));
//context.LoadUnmanagedLibrary("C:/Users/ace_c/Documents/ProyectosC#/SolutionSistemaVenta/SistemaVenta.AplicacionWeb/Utilidades/LibreriaPDF/libwkhtmltox.dll");
builder.Services.AddSingleton(typeof(IConverter),new SynchronizedConverter(new PdfTools()));

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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();
