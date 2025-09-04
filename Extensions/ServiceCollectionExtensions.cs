using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using TimeTraceOne.Data;
using TimeTraceOne.Services;

namespace TimeTraceOne.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<TimeFlowDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        // Add custom services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITimeEntryService, TimeEntryService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ITeamService, TeamService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IReportService, ReportService>();
        services.AddScoped<IValidationService, ValidationService>();
        
        // Add AutoMapper
        services.AddAutoMapper(typeof(Program));
        
        // Add FluentValidation (commented out for now)
        // services.AddFluentValidationAutoValidation();
        
        // Add CORS
        services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigins", policy =>
            {
                var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                if (allowedOrigins != null && allowedOrigins.Length > 0)
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromHours(24)); // Cache preflight for 24 hours
                }
                else
                {
                    // Fallback for development - allow all localhost origins
                    policy.SetIsOriginAllowed(origin => 
                        origin.StartsWith("http://localhost:") || 
                        origin.StartsWith("https://localhost:"))
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                          .SetPreflightMaxAge(TimeSpan.FromHours(24)); // Cache preflight for 24 hours
                }
            });
        });
        
        // Add Response Caching
        services.AddResponseCaching();
        
        // Add Redis Cache
        var redisConnection = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection;
            });
        }
        
        return services;
    }
    
    public static IServiceCollection AddAuthenticationServices(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = configuration.GetSection("Jwt");
        var key = Encoding.ASCII.GetBytes(jwtSettings["Key"]!);
        
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });
        
        services.AddAuthorization(options =>
        {
            options.AddPolicy("OwnerOnly", policy => policy.RequireRole("Owner"));
            options.AddPolicy("ManagerOrOwner", policy => policy.RequireRole("Manager", "Owner"));
            options.AddPolicy("EmployeeOrManagerOrOwner", policy => policy.RequireRole("Employee", "Manager", "Owner"));
        });
        
        return services;
    }
    
    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo 
            { 
                Title = "TimeFlow API", 
                Version = "v1",
                Description = "A comprehensive time tracking and project management API"
            });
            
            // Add JWT authentication to Swagger
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
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
                    Array.Empty<string>()
                }
            });
        });
        
        return services;
    }
    
    public static IHostBuilder AddLoggingServices(this IHostBuilder builder)
    {
        builder.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));
        
        return builder;
    }
}
