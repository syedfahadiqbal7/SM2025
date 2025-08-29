var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddConnectionString("DBCS");

var apiService = builder.AddProject<Projects.AFFZ_API>("affz-api")
    .WithExternalHttpEndpoints() //;
    .WithReference(sql);

builder.AddProject<Projects.AFFZ_Admin>("affz-admin")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.AddProject<Projects.AFFZ_Customer>("affz-customer")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

builder.AddProject<Projects.AFFZ_Provider>("affz-provider")
    .WithExternalHttpEndpoints()
    .WithReference(apiService);

//builder.AddProject<Projects.SCAPI_WebFront>("scapi-webfront")
//    .WithExternalHttpEndpoints()
//    .WithReference(apiService);
builder.Build().Run();
