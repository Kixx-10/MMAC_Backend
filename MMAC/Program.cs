using AutoMapper;
using CloudinaryDotNet;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MMAC.Data;
using MMAC.Interfaces;
using MMAC.Profiles;
using MMAC.Repositories;
using MMAC.Repositories.DashboardRepository;
using MMAC.Services;
using MMAC.Services.ArrivalInterface;
using MMAC.Services.AuditLogService;
using MMAC.Services.DashboardService;
using MMAC.Services.PdfService;
using MMAC.Services.PortOfArrivalService;
using MMAC.Services.SearchService;
using MMAC.Services.TokenService;
using MMAC.Services.UpdateService;
using MMAC.Services.UtilityService;
using MMAC.Validations;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

//  Disable ReloadOnChange to fix Linux/Render 'inotify' file watcher error
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                   .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false)
                   .AddEnvironmentVariables();

// Cloudinary Configuration
var cloudinaryAccount = new Account(
    builder.Configuration["Cloudinary:CloudName"],
    builder.Configuration["Cloudinary:ApiKey"],
    builder.Configuration["Cloudinary:ApiSecret"]
);
var cloudinary = new Cloudinary(cloudinaryAccount);
builder.Services.AddSingleton(cloudinary);

// Database 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Hangfire 
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c =>
        c.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));
builder.Services.AddHangfireServer();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Swagger with JWT 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "eArrival Information System", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter JWT token: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

// DI Services 
builder.Services.AddScoped<IPortOfArrivalRepository, PortOfArrivalRepository>();
builder.Services.AddScoped<IPortOfArrivalService, PortOfArrivalService>();
builder.Services.AddScoped<IForeignerSearchService, ForeignerSearchService>();
builder.Services.AddScoped<IMyanmarSearchService, MyanmarSearchService>();
builder.Services.AddScoped<ICompleteArrivalRepository, CompleteArrivalRepository>();
builder.Services.AddScoped<ICompleteArrivalService, CompleteArrivalService>();
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IUtilityService, UtilityService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IPdfService, PdfService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddHttpContextAccessor();

// ── AutoMapper 
var mapperConfig = new MapperConfiguration(cfg => cfg.AddProfile<CompleteArrivalMapper>());
builder.Services.AddSingleton(mapperConfig.CreateMapper());

// ── FluentValidation 
builder.Services.AddValidatorsFromAssemblyContaining<CompleteArrivalDTOValidator>();
builder.Services.AddFluentValidationAutoValidation();

// ── CORS 
builder.Services.AddCors(options =>
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing in configuration");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],

        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],

        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "eArrival Information System v1");
    c.RoutePrefix = "swagger";
});

app.UseRouting();

app.UseCors("AllowAll");
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Hangfire dashboard
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new Hangfire.Dashboard.IDashboardAuthorizationFilter[] { }
});

app.Run();