using Microsoft.EntityFrameworkCore;
using RealEstate.Infrastructure.Data;
using Serilog;
using RealEstate.API.Extensions; // 추가
using RealEstate.Core.Extensions; // 기존
using RealEstate.Infrastructure.Extensions; // 기존

var builder = WebApplication.CreateBuilder(args);

// Serilog 설정
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
    .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "RealEstate.API")
    .Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        "logs/realestate-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}",
        fileSizeLimitBytes: 10_000_000,
        rollOnFileSizeLimit: true)
    .WriteTo.File(
        "logs/errors/realestate-errors-.log",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

// 서비스 추가
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger 설정을 확장 메서드로 분리
builder.Services.AddSwaggerDocumentation();

// 데이터베이스 설정
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("데이터베이스 연결 문자열이 설정되지 않았습니다.");
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment())
           .EnableDetailedErrors(builder.Environment.IsDevelopment()));

// CORS 설정
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// JWT 인증 설정을 전용 확장 메서드로 분리하여 호출합니다.
builder.Services.AddApiAuthentication(builder.Configuration); // JWT 인증 설정 추가

// 커스텀 서비스 등록
builder.Services.AddCoreServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
// AutoMapper 등록
builder.Services.AddAutoMapper(typeof(RealEstate.Infrastructure.Mapping.MappingProfile));

var app = builder.Build();

// 데이터베이스 초기화
await RealEstate.Infrastructure.Data.DbInitializer.InitializeAsync(app.Services);

// 시작 로그
Log.Information("=== RealEstate API Starting ===");
Log.Information("Environment: {Environment}", app.Environment.EnvironmentName);
Log.Information("ContentRoot: {ContentRoot}", app.Environment.ContentRootPath);

// HTTP 요청 로깅
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) => ex != null
        ? Serilog.Events.LogEventLevel.Error
        : httpContext.Response.StatusCode > 499
            ? Serilog.Events.LogEventLevel.Error
            : Serilog.Events.LogEventLevel.Information;
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("RemoteIP", httpContext.Connection.RemoteIpAddress?.ToString());
    };
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Information("=== RealEstate API Started Successfully ===");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "애플리케이션이 예기치 않게 종료되었습니다");
    throw;
}
finally
{
    Log.Information("=== RealEstate API Shutting Down ===");
    Log.CloseAndFlush();
}
