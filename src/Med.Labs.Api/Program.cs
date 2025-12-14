using Med.Labs.Api.Configuration;
using Med.Labs.Api.Middleware;
using Med.Labs.Infrastructure.Outbox;
using Microsoft.OpenApi;



var builder = WebApplication.CreateBuilder(args);   

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Med.Labs.Api",
        Version = "v1"
    });
});
builder.Services.AddHealthChecks();

builder.Services.AddApiDependencies(builder.Configuration);

// Resolve connection string from configuration (matches ServiceCollectionExtensions)
var connectionString = builder.Configuration.GetConnectionString("db")
					   ?? "Host=localhost;Port=5432;Database=med_labs;Username=med;Password=med";

// Outbox publisher registration
builder.Services.Configure<OutboxPublisherOptions>(cfg => cfg.PollIntervalSeconds = 5);
builder.Services.AddSingleton<OutboxPublisher>(_ => new OutboxPublisher(connectionString));
builder.Services.AddHostedService<OutboxPublisherHostedService>();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Med.Labs.Api v1");
    c.RoutePrefix = "swagger"; // URL: /swagger
});

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();



//using Med.Labs.Api.Configuration;
//using Med.Labs.Api.Middleware;

//var builder = WebApplication.CreateBuilder(args);

//builder.Services.AddControllers();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
//builder.Services.AddHealthChecks();

//builder.Services.AddApiDependencies(builder.Configuration);

//var app = builder.Build();

//app.UseMiddleware<ErrorHandlingMiddleware>();

//app.UseSwagger();
//app.UseSwagger();

//app.MapHealthChecks("/health");
//app.MapControllers();

//app.Run();



//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
//builder.Services.AddOpenApi();

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.MapOpenApi();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();
