using RapportinoServer.Components;
using RapportinoServer.Data.Repositories;
using RapportinoServer.Services;

var builder = WebApplication.CreateBuilder(args);

// Razor Components + Blazor Server
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
    });

// Repositories / DI
builder.Services.AddScoped<ClientRepository>();
builder.Services.AddScoped<MachineRepository>();
builder.Services.AddScoped<ReportRepository>();
builder.Services.AddScoped<TypeServiceRepository>();
builder.Services.AddScoped<TechnicianRepository>();
builder.Services.AddScoped<DashboardRepository>();
builder.Services.AddScoped<AuthStateService>();

builder.Services.AddMemoryCache();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
