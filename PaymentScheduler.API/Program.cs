using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PaymentScheduler.API.Common;
using PaymentScheduler.Application.Interface;
using PaymentScheduler.Application.Services.Hangfire;
using PaymentScheduler.Infrastructure.Common;
using PaymentScheduler.Infrastructure.Context;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.RegisterMapper();

// Add Connection String
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

// Add Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File("logs/notifications.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Scrutor for DI
builder.Services.Scan(scan => scan
                .FromAssemblyOf<IPaymentRepository>()
                .AddClasses(classes => classes
                .InNamespaces("PaymentScheduler.Application.Services"))
                .AsImplementedInterfaces()
                .WithScopedLifetime());

// Add Swagger with JWT support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "RecurringPayments API", Version = "v1" });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]!;
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

// Add Hangfire
builder.Services.AddHangfire(config =>
    config.UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

builder.Services.AddHangfireServer();

var app = builder.Build();

// Database Seeder
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var context = services.GetRequiredService<AppDbContext>();
    var env = services.GetRequiredService<IHostEnvironment>();
    var config = services.GetRequiredService<IConfiguration>();

    await DbSeeder.Initialize(context, env, config);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire/recurring");

RecurringJob.AddOrUpdate<PaymentJob>(
    "ProcessDuePayments",
    job => job.ProcessDuePayments(),
    "0 0 * * *");

app.MapControllers();

app.Run();