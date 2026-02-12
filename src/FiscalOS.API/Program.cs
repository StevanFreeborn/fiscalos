var builder = WebApplication.CreateBuilder(args);

builder.Services.AddValidation();
builder.Services.AddProblemDetails();

builder.Services.AddInfrastructure();

builder.Services.AddAuthentication(static o =>
  {
    o.DefaultAuthenticateScheme = Schemes.Default;
    o.DefaultChallengeScheme = Schemes.Default;
  })
  .AddJwtBearer(Schemes.Default)
  .AddJwtBearer(Schemes.AllowExpiredTokens);

builder.Services.AddAuthorizationBuilder()
  .AddPolicy(Schemes.Default, static policy =>
  {
    policy.AuthenticationSchemes = [Schemes.Default];
    policy.RequireAuthenticatedUser();
  })
  .AddPolicy(Schemes.AllowExpiredTokens, static policy =>
  {
    policy.AuthenticationSchemes = [Schemes.AllowExpiredTokens];
    policy.RequireAuthenticatedUser();
  });

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseStatusCodePages();

app.MapLoginEndpoint();

app.MapRefreshEndpoint()
  .RequireAuthorization(Schemes.AllowExpiredTokens);

app.MapAccountsEndpoints();

app.Run();