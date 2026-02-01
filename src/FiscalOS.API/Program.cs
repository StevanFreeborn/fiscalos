using FiscalOS.API.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.Services.AddOpenApi();

builder.Services.ConfigureOptions<AppDbContextOptionsSetup>();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddHostedService<MigrationService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapLoginEndpoint();

app.Run();