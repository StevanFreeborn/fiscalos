var builder = DistributedApplication.CreateBuilder(args);

var api = builder.AddProject<Projects.FiscalOS_API>("API")
  .WithHttpHealthCheck("/health");

builder.Build().Run();