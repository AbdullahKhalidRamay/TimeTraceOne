using Microsoft.EntityFrameworkCore;
using Serilog;
using TimeTraceOne.Data;
using TimeTraceOne.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day));

// Add services to the container
builder.Services.AddControllers();

// Add application services
builder.Services.AddApplicationServices(builder.Configuration);

// Add authentication services
builder.Services.AddAuthenticationServices(builder.Configuration);

// Add Swagger services
builder.Services.AddSwaggerServices();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TimeFlow API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI at root
    });
}

// Use CORS
app.UseCors("AllowSpecificOrigins");

// Use response caching
app.UseResponseCaching();

// Use Serilog
app.UseSerilogRequestLogging();

// Use authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TimeFlowDbContext>();
    try
    {
        // Apply any pending migrations
        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
            Log.Information("Database migrations applied successfully");
        }
        
        // Seed the database
        await DatabaseSeeder.SeedAsync(context);
        Log.Information("Database seeded successfully");
    }
    catch (Exception ex)
    {
        Log.Error(ex, "An error occurred while creating or seeding the database");
    }
}

app.Run();
