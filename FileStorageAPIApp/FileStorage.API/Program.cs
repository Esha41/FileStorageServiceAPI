using FileStorage.API.Middleware;
using FileStorage.Application.DTOs.Configurations;
using FileStorage.Application.Interfaces;
using FileStorage.Application.Services;
using FileStorage.Domain.Interfaces;
using FileStorage.Infrastructure;
using FileStorage.Infrastructure.AppDbContext;
using FileStorage.Infrastructure.HealthChecks;
using FileStorage.Infrastructure.LocalFileStorageService;
using FileStorage.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

//UseSerilog configurations
builder.Host.UseSerilog((context, services, configuration) => 
{ 
    configuration.ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext(); 
});

// Add HealthChecks ( Database reachability, Filesystem read/write permission check)
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(name: "database-check")
    .AddCheck<FileSystemHealthCheck>("filesystem-check");

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IFileStorageService, FileStorageService>();
builder.Services.AddScoped<IFileStorageRepository, FileStorageRepository>();
builder.Services.AddScoped<ILocalFileStorageService, LocalFileStorageService>();

//JWT authentication configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JWTConfig>();

// Add Infrastructure services
builder.Services.AddInfrastructure(builder.Configuration);

// Add AutoMapper configuration
builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<FileStorage.Application.DTOs.Mapping.MappingProfile>();
});

builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT Bearer token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings?.Issuer,
        ValidAudience = jwtSettings?.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key))
    };
});



builder.Services.AddAuthorization();
builder.Services.AddControllers();

// define CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200") 
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// apply database migrations
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

app.UseCors("AllowAngularApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//register custom middlewares
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();

app.UseSerilogRequestLogging(options =>   // 2️⃣ Request logging
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
});

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
