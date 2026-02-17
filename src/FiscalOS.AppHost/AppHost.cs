var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.FiscalOS_API>("API")
  .WithHttpHealthCheck("/health");

builder.AddViteApp("web", "../FiscalOS.Web")
  .WithReference(api)
  .WithHttpsEndpoint(port: 7061, env: "PORT");

builder.Build().Run();