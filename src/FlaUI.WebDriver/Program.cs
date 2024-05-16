using FlaUI.WebDriver;
using FlaUI.WebDriver.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
builder.Services.AddScoped<IActionsDispatcher, ActionsDispatcher>();
builder.Services.AddScoped<IWindowsExtensionService, WindowsExtensionService>();

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options =>
    options.Filters.Add(new WebDriverResponseExceptionFilter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "FlaUI.WebDriver", Version = "v1" });
});

builder.Services.Configure<SessionCleanupOptions>(
    builder.Configuration.GetSection(SessionCleanupOptions.OptionsSectionName));
builder.Services.AddHostedService<SessionCleanupService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "FlaUI.WebDriver v1"));
}

app.UseAuthorization();

app.MapControllers();

app.Run();
