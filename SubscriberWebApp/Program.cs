using Microsoft.EntityFrameworkCore;
using SubscriberWebApp;
using SubscriberWebApp.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MainHostedService
builder.Services.AddHostedService<MainHostedService>();
builder.Services.AddSingleton<MqttSubscriber>();
builder.Services.AddDbContext<ModbusDbContext>(options =>
    options.UseSqlite("Data Source=modbus.db"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


// pipe line

//app.UseHttpsRedirection(); 

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
