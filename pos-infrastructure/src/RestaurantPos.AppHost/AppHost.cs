var builder = DistributedApplication.CreateBuilder(args);

var dbConn = builder.AddConnectionString("Database");
var rmqHost = builder.Configuration["Parameters:RabbitMQHost"] ?? "localhost";
var rmqPort = builder.Configuration["Parameters:RabbitMQPort"] ?? "5672";
var rmqUser = builder.Configuration["Parameters:RabbitMQUser"] ?? "guest";
var rmqPass = builder.Configuration["Parameters:RabbitMQPassword"] ?? "guest";

// 1. Identity Service (8081)
var identityService = builder.AddProject<Projects.IdentityService_Api>("identity-service")
    .WithEnvironment("ConnectionStrings__IdentityDb", dbConn);

// 2. Catalog Service (8082)
var catalogService = builder.AddProject<Projects.CatalogService_Api>("catalog-service")
    .WithEnvironment("ConnectionStrings__CatalogDb", dbConn)
    .WaitFor(identityService);

// 3. Tables Service (8083)
var tablesService = builder.AddProject<Projects.TablesService_Api>("tables-service")
    .WithEnvironment("ConnectionStrings__TablesDb", dbConn)
    .WaitFor(identityService);

// 4. Orders Service (8084)
var ordersService = builder.AddProject<Projects.OrdersService_Api>("orders-service")
    .WithEnvironment("ConnectionStrings__OrdersDb", dbConn)
    .WithEnvironment("RabbitMq__HostName", rmqHost)
    .WithEnvironment("RabbitMq__Port", rmqPort)
    .WithEnvironment("RabbitMq__Username", rmqUser)
    .WithEnvironment("RabbitMq__Password", rmqPass)
    .WaitFor(identityService);

// 5. Payments Service (8085)
var paymentsService = builder.AddProject<Projects.PaymentsService_Api>("payments-service")
    .WithEnvironment("ConnectionStrings__PaymentsDb", dbConn)
    .WithEnvironment("RabbitMq__HostName", rmqHost)
    .WithEnvironment("RabbitMq__Port", rmqPort)
    .WithEnvironment("RabbitMq__Username", rmqUser)
    .WithEnvironment("RabbitMq__Password", rmqPass)
    .WaitFor(ordersService);

// 6. Reporting Service (8086)
var reportingService = builder.AddProject<Projects.ReportingService_Api>("reporting-service")
    .WithEnvironment("ConnectionStrings__ReportingDb", dbConn)
    .WithEnvironment("RabbitMQ__Host", rmqHost)
    .WithEnvironment("RabbitMQ__Port", rmqPort)
    .WithEnvironment("RabbitMQ__Username", rmqUser)
    .WithEnvironment("RabbitMQ__Password", rmqPass)
    .WaitFor(ordersService);

// 7. Realtime Hub Service (8087)
var realtimeHubService = builder.AddProject<Projects.RealtimeHubService_Api>("realtime-hub-service")
    .WithEnvironment("RabbitMQ__Host", rmqHost)
    .WithEnvironment("RabbitMQ__Port", rmqPort)
    .WithEnvironment("RabbitMQ__Username", rmqUser)
    .WithEnvironment("RabbitMQ__Password", rmqPass)
    .WaitFor(ordersService);

builder.Build().Run();