var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddValidation();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.ConfigureHttpJsonOptions(static options =>
{
  options.SerializerOptions.Converters.Add(new EnumConverterFactory());
  options.SerializerOptions.Converters.Add(new WebhookBaseConverter());
});

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
app.UseExceptionHandler();

app.MapDefaultEndpoints();
app.MapAuthEndpoints();
app.MapAccountsEndpoints();
app.MapInstitutionsEndpoints();
app.MapTransactionsGroup();

app.Run();