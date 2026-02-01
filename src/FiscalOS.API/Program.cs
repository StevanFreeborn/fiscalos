var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapLoginEndpoint();

app.Run();