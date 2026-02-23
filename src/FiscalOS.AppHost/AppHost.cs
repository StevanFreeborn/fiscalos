using FiscalOS.AppHost;

var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.FiscalOS_API>(ProjectNames.API)
  .WithHttpHealthCheck("/health");

builder.AddViteApp(ProjectNames.Web, "../FiscalOS.Web")
  .WithReference(api)
  .WithHttpsEndpoint(port: 7061, env: "PORT");

builder.Build().Run();
