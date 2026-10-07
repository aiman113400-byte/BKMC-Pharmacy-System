var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.BKMC_Pharmacy_Web>("bkmc-pharmacy-web");

builder.Build().Run();
