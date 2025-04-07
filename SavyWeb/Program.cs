using Amazon.Extensions.NETCore.Setup; // Remove this line
using Amazon.Extensions.NETCore; // Add this line
using Amazon.S3;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Load AWS options from appsettings.json
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions("AWS"));

// Register AWS services
builder.Services.AddAWSService<IAmazonS3>();

// Register your custom S3Service
builder.Services.AddSingleton<S3Service>();

// Register controllers
builder.Services.AddControllers();
IServiceCollection serviceCollection = builder.Services.AddEndpointsApiExplorer();
IServiceCollection serviceCollection1 = builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger setup
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

IApplicationBuilder applicationBuilder = app.UseHttpsRedirection();
IApplicationBuilder applicationBuilder1 = app.UseAuthorization();
app.MapControllers();

app.Run();
